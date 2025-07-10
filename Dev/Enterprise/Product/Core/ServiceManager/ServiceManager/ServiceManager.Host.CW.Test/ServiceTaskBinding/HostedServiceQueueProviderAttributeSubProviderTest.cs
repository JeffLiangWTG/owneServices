using System.Linq;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using ServiceManager.Host.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Business.Testing
{
	sealed class HostedServiceQueueProviderAttributeSubProviderTest : TestCase
	{
		public void TestQueues()
		{
			IHostedServiceQueuesSubProvider provider = new HostedServiceQueueProviderAttributeSubProvider();
			var expectedBindingsCount = AssemblyMetaDataReader.GetAttributes<HostedServiceQueueProviderAttribute>().ToArray().Length;
			AssertEquals("Queues Count", expectedBindingsCount, provider.Queues.Count());
		}
	}
}
