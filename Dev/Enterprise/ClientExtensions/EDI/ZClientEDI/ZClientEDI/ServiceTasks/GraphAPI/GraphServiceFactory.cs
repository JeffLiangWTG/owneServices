using System;
using System.Collections.Generic;
using Azure.Identity;
using Enterprise.Client.EDI.OAuth2;
using WTG.AzureApplicationIntegration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.ServiceTasks.GraphAPI
{
	public class GraphServiceFactory : IGraphServiceFactory
	{
		[ThreadSafe]
		static readonly Lazy<GraphServiceFactory> LazyInstance = new(() => new GraphServiceFactory());

		public static GraphServiceFactory Instance => LazyInstance.Value;

		GraphServiceFactory()
		{
		}

		public GraphService CreateGraphService(string tenantId, string graphClientId)
		{
			if (!graphServices.TryGetValue(graphClientId, out var graphService))
			{
				graphService = new GraphService(new ClientAssertionCredential(tenantId, graphClientId, () => AccessTokenProvider.Instance.GetAccessToken()));
				graphServices.Add(graphClientId, graphService);
			}

			return graphService;
		}

#if DEBUG
		internal
#endif
		readonly Dictionary<string, GraphService> graphServices = new();
	}
}
