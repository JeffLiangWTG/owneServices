using CargoWise.eHub.MessageEvent.KafkaProducerService.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.DI
{
	static class ConfigureOptions
	{
		public static IServiceCollection AddAppOptions(this IServiceCollection services)
		{
			var config = services.BuildServiceProvider().GetService<IConfiguration>();

			services.Configure<KafkaOptions>(config.GetRequiredSection(KafkaOptions.Section));
			services.Configure<ServiceBrokerOptions>(config.GetRequiredSection(ServiceBrokerOptions.Section));

			return services;
		}
	}
}
