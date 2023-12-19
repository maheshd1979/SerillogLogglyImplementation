using Loggly.Config;
using Loggly;
using SerillogLogglyImplementation;
using Serilog;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.IO;
using Serilog.Core;

var builder = WebApplication.CreateBuilder(args);
//builder.Host.UseSerilog();

var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json")
                      // .AddJsonFile($"appsettings.{_environmentName}.json", optional: true, reloadOnChange: true)
                       .Build();

var logglySettings = new LogglySettings();
configuration.GetSection("Serilog:Loggly").Bind(logglySettings);

var config = LogglyConfig.Instance;
config.CustomerToken = logglySettings.CustomerToken;
config.ApplicationName = logglySettings.ApplicationName;
config.Transport = new TransportConfiguration()
{
    EndpointHostname = logglySettings.EndpointHostname,
    EndpointPort = logglySettings.EndpointPort,
    LogTransport = logglySettings.LogTransport
};
config.ThrowExceptions = logglySettings.ThrowExceptions;

//Define Tags sent to Loggly
config.TagConfig.Tags.AddRange(new ITag[]{
                new ApplicationNameTag {Formatter = "Application-{0}"},
                new HostnameTag { Formatter = "Host-{0}" }
            });

Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(configuration)
               .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();
