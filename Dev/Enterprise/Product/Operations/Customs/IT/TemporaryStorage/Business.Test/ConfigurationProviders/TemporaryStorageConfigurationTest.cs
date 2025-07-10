using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageConfiguration))]
sealed class TemporaryStorageConfigurationTest : EU.Business.CusTempStorage.Testing.TemporaryStorageConfigurationAbstractTest<TemporaryStorageConfiguration>
{
	public override void TestPreviousDocumentConfiguration()
	{
		AssertType<TemporaryStoragePreviousDocumentConfiguration>(configuration.PreviousDocumentConfiguration);
	}

	public override void TestSupportingDocumentConfiguration()
	{
		AssertType<EU.Business.CusTempStorage.TemporaryStorageSupportingDocumentConfiguration>(configuration.SupportingDocumentConfiguration);
	}

	public override void TestTemporaryStorageHeaderValidationDecider()
	{
		AssertType<TemporaryStorageHeaderValidationDecider>(configuration.GetValidationDecider());
	}

	public override void TestBillConfiguration()
	{
		AssertType<TemporaryStorageBillConfiguration>(configuration.BillConfiguration);
	}

	public override void TestPackedItemConfiguration()
	{
		AssertType<TemporaryStoragePackedItemConfiguration>(configuration.PackedItemConfiguration);
	}

	protected override bool ExpectedSupportLRNGeneration => false;

	protected override bool ExpectedSupportAgentDefaulting => false;

	protected override string TemporaryStorageHeaderDocumentWrapperClass => "Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeaderWrapper, Enterprise.Customs.IT.TemporaryStorage.Business";
}
