using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Elasticsearch.Net;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Nest;

namespace Enterprise.ServiceManager.Shared
{
	public interface IElasticLogClientFactory
	{
		IElasticClient CreateKafkaClient();
		IElasticClient CreateElasticClient();
	}

	public class ElasticLogClientFactory : IElasticLogClientFactory
	{
		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "ConnectionSettings is used in ElasticClient")]
		IElasticClient GetElasticClient(
			string server,
			string basicAuthUsername,
			string basicAuthPassword,
			string index)
		{
			try
			{
				var settings = new ConnectionSettings(new Uri(server));
				settings.BasicAuthentication(basicAuthUsername, basicAuthPassword)
					.DefaultIndex(index + "*");
				return GetElasticClient(settings);
			}
			catch (UriFormatException ex)
			{
				Globals.Message.ShowError($"Could not connect to elastic client, check registry configuration:\r\n{ex.Message}");
				return new ElasticClient();
			}
			catch (ElasticsearchClientException ex)
			{
				Globals.Message.ShowError($"Could not connect to elastic client, check registry configuration:\r\n{ex.Message}");
				return new ElasticClient();
			}
		}

		internal IElasticClient GetElasticClient(ConnectionSettings connectionSettings)
		{
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
			return new ElasticClient(connectionSettings);
		}

		public IElasticClient CreateKafkaClient()
		{
			var registry = SystemDataRegistry.Instance;
			var elasticClient = GetElasticClient(
				registry.KafkaElasticsearchServiceUri.Value,
				registry.KafkaElasticsearchServerUserName.Value,
				registry.KafkaElasticsearchServerPassword.Value,
				registry.KafkaElasticsearchIndex.Value);
			return elasticClient;
		}

		public IElasticClient CreateElasticClient()
		{
			var registry = SystemDataRegistry.Instance;
			var elasticClient = GetElasticClient(
				registry.ElasticsearchServiceUri.Value,
				registry.ElasticsearchServerUserName.Value,
				registry.ElasticsearchServerPassword.Value,
				registry.ElasticsearchIndex.Value);
			return elasticClient;
		}
	}
}
