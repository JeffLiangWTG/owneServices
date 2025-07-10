using Enterprise.Client.EDI.ServiceTasks.GraphAPI;
using NUnit.Framework;

namespace ZClientEDI.Test.ServiceTasks.Test
{
	public class GraphServiceFactoryTest : TestCase
	{
		public void TestCreateGraphService()
		{
			var graphService = GraphServiceFactory.Instance.CreateGraphService("tenantId", "graphClientId");

			AssertNotNull(graphService);
			AssertEquals(1, GraphServiceFactory.Instance.graphServices.Count);
			AssertEquals(graphService, GraphServiceFactory.Instance.graphServices["graphClientId"]);

			GraphServiceFactory.Instance.CreateGraphService("tenantId", "graphClientId");
			AssertEquals(1, GraphServiceFactory.Instance.graphServices.Count);

			GraphServiceFactory.Instance.CreateGraphService("tenantId", "graphClientId");
			AssertEquals(1, GraphServiceFactory.Instance.graphServices.Count);
		}
	}
}
