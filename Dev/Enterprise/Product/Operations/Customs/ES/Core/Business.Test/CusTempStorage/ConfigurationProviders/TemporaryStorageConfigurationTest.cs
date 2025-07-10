using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

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
		AssertType<EU.Business.CusTempStorage.TemporaryStorageHeaderValidationDecider>(configuration.GetValidationDecider());
	}

	public override void TestBillConfiguration()
	{
		AssertType<TemporaryStorageBillConfiguration>(configuration.BillConfiguration);
	}

	public override void TestPackedItemConfiguration()
	{
		AssertType<EU.Business.CusTempStorage.TemporaryStoragePackedItemConfiguration>(configuration.PackedItemConfiguration);
	}

	[ExpectNoExceptions]
	public new void TestTemporaryStorageHeaderDocumentWrapperClass()
	{
		AssertEquals(ESConstants.DocumentWrapperConstants.TemporaryStorageHeaderDocumentWrapperType, configuration.TemporaryStorageHeaderDocumentWrapperClass);
	}

	protected override bool ExpectedSupportLRNGeneration => true;

	protected override bool ExpectedSupportAgentDefaulting => true;

	protected override string TemporaryStorageHeaderDocumentWrapperClass => ESConstants.DocumentWrapperConstants.TemporaryStorageHeaderDocumentWrapperType;
}
