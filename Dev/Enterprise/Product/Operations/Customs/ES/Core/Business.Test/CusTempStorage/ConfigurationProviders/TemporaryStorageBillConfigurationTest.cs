using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageBillConfiguration))]
sealed class TemporaryStorageBillConfigurationTest : EU.Business.CusTempStorage.Testing.TemporaryStorageBillConfigurationAbstractTest<TemporaryStorageBillConfiguration>
{
	public override void TestGetValidationDecider()
	{
		AssertType<TemporaryStorageBillValidationDecider>(configuration.GetValidationDecider());
	}
}
