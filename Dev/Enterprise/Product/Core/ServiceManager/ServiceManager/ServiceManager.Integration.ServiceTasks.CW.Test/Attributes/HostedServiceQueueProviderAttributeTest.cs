using Enterprise.ZArchitecture.Core.Test;
using NUnit.Framework;

namespace ServiceManager.Integration.ServiceTasks.CW.Test
{
	[TestedType(typeof(HostedServiceQueueProviderAttribute))]
	sealed class HostedServiceQueueProviderAttributeTest : AssemblyMetaDataAttributeTestCase<HostedServiceQueueProviderAttribute>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.ServiceTaskCode = "ServiceTaskCode";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ServiceTaskCode = "ServiceTaskCode";
			Assert(attribute1.Equals(attribute2));

			attribute1.QueueName = "QueueName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.QueueName = "QueueName";
			Assert(attribute1.Equals(attribute2));
		}
	}
}
