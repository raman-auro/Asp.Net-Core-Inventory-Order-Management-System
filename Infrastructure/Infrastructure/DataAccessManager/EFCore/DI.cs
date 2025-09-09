using Application.Common.CQS.Commands;
using Application.Common.CQS.Queries;
using Application.Common.Repositories;
using Infrastructure.DataAccessManager.EFCore.Contexts;
using Infrastructure.DataAccessManager.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // ✅ Needed for MySQL
using Serilog;

namespace Infrastructure.DataAccessManager.EFCore
{
    public static class DI
    {
        public static IServiceCollection RegisterDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var databaseProvider = configuration["DatabaseProvider"]?.ToLower();

            // Register Context
            switch (databaseProvider)
            {
                case "mysql":
                    services.AddDbContext<DataContext>(options =>
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                            .LogTo(Log.Information, LogLevel.Information)
                            .EnableSensitiveDataLogging()
                    );
                    services.AddDbContext<CommandContext>(options =>
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                            .LogTo(Log.Information, LogLevel.Information)
                            .EnableSensitiveDataLogging()
                    );
                    services.AddDbContext<QueryContext>(options =>
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                            .LogTo(Log.Information, LogLevel.Information)
                            .EnableSensitiveDataLogging()
                    );
                    break;

                case "sqlserver":
                default:
                    services.AddDbContext<DataContext>(options =>
                        options.UseSqlServer(connectionString)
                            .LogTo(Log.Information, LogLevel.Information)
                            .EnableSensitiveDataLogging()
                    );
                    services.AddDbContext<CommandContext>(options =>
                        options.UseSqlServer(connectionString)
                            .LogTo(Log.Information, LogLevel.Information)
                            .EnableSensitiveDataLogging()
                    );
                    services.AddDbContext<QueryContext>(options =>
                        options.UseSqlServer(connectionString)
                            .LogTo(Log.Information, LogLevel.Information)
                            .EnableSensitiveDataLogging()
                    );
                    break;
            }

            // Register repositories
            services.AddScoped<ICommandContext, CommandContext>();
            services.AddScoped<IQueryContext, QueryContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));

            return services;
        }

        public static IHost CreateDatabase(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var dataContext = serviceProvider.GetRequiredService<DataContext>();
            dataContext.Database.EnsureCreated(); // only for dev/test

            return host;
        }
    }
}
