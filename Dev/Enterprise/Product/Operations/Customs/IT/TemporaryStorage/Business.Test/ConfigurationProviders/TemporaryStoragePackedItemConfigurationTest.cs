using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStoragePackedItemConfiguration))]
sealed class TemporaryStoragePackedItemConfigurationTest : TemporaryStoragePackedItemConfigurationAbstractTest<TemporaryStoragePackedItemConfiguration>
{
	public override void TestGetValidationDecider()
	{
		AssertType<TemporaryStoragePackedItemValidationDecider>(configuration.GetValidationDecider());
	}
}
