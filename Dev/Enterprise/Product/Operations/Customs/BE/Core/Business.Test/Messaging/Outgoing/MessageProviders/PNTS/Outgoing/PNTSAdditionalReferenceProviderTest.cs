using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSAdditionalReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSAdditionalReferenceProvider>
{
	public void TestReferenceNumber()
	{
		additionalReference.CSI_ReferenceNumber = "Reference";
		AssertEquals("Reference", provider.ReferenceNumber);
	}

	public void TestType()
	{
		additionalReference.CSI_Code = "Type";
		AssertEquals("Type", provider.Type);
	}

	protected override PNTSAdditionalReferenceProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		additionalReference = Factory.New<TemporaryStorageAdditionalInfo>();
		additionalReference.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
		provider = new PNTSAdditionalReferenceProvider(additionalReference);
	}

	TemporaryStorageAdditionalInfo additionalReference;
	PNTSAdditionalReferenceProvider provider;
}
