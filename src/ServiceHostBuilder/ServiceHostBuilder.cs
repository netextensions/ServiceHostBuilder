﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace NetExtensions
{
    public class ServiceHostBuilder
    {
         public static void LoadProgram<TStartup>(string[] args) where TStartup : class
        {
            var configuration = ServiceConfigurationReader.CreateConfiguration();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("Getting the motors running...");
                CreateHostBuilder<TStartup>(args, configuration).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

         public static async Task LoadProgramAsync<TStartup>(string[] args) where TStartup : class
         {
             var configuration = ServiceConfigurationReader.CreateConfiguration();
             Log.Logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(configuration)
                 .Enrich.FromLogContext()
                 .WriteTo.Debug()
                 .WriteTo.Console(
                     outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                 .CreateLogger();

             try
             {
                 Log.Information("Getting the motors running...");
                 await CreateHostBuilder<TStartup>(args, configuration).Build().RunAsync().ConfigureAwait(false);
             }
             catch (Exception ex)
             {
                 Log.Fatal(ex, "Host terminated unexpectedly");
             }
             finally
             {
                 Log.CloseAndFlush();
             }
         }

        public static IHostBuilder CreateHostBuilder<TStartup>(string[] args, IConfiguration configuration = null) where TStartup : class =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSystemd()
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();
                    webBuilder.UseContentRoot(ServiceContentRoot.GetContentRoot());
                    webBuilder.UseConfiguration(configuration ?? ServiceConfigurationReader.CreateConfiguration());
                    webBuilder.UseIISIntegration();
                    webBuilder.UseStartup<TStartup>();
                });

        private static void Main()
        {
            //workaround
        }

    }
}