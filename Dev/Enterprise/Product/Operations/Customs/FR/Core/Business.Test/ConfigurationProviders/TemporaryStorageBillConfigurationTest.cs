using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using Enterprise.Customs.FR.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	[TestedType(typeof(TemporaryStorageBillConfiguration))]
	sealed class TemporaryStorageBillConfigurationTest : TemporaryStorageBillConfigurationAbstractTest<TemporaryStorageBillConfiguration>
	{
		[ExpectNoExceptions]
		public override void TestGetValidationDecider()
		{
			AssertType<TemporaryStorageBillValidationDecider>(configuration.GetValidationDecider());
		}
	}
}
