using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SendGridWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                using var scope = _serviceScopeFactory.CreateScope();

                var sendGridClient = scope.ServiceProvider.GetRequiredService<ISendGridClient>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var message = new SendGridMessage
                {
                    Subject = "Greetings",
                    PlainTextContent = "Hello from an ASP.NET Core worker service!",
                    From = new EmailAddress(configuration["Email:From"]),
                };

                message.AddTo(configuration["Email:Recipient"]);

                await sendGridClient.SendEmailAsync(message, stoppingToken);

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
