using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageBillConfiguration))]
sealed class TemporaryStorageBillConfigurationTest : TemporaryStorageBillConfigurationAbstractTest<TemporaryStorageBillConfiguration>
{
	public override void TestGetValidationDecider()
	{
		AssertType<TemporaryStorageBillValidationDecider>(configuration.GetValidationDecider());
	}
}
