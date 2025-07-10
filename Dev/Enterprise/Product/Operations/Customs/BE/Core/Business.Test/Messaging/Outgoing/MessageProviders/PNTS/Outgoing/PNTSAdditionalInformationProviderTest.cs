using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSAdditionalInformationProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSAdditionalInformationProvider>
{
	public void TestText()
	{
		additionalInfo.CSI_Description = "Description";
		AssertEquals("Description", provider.Text);
	}

	public void TestCode()
	{
		additionalInfo.CSI_Code = "Code";
		AssertEquals("Code", provider.Code);
	}

	protected override PNTSAdditionalInformationProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		additionalInfo = Factory.New<TemporaryStorageAdditionalInfo>();
		additionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalInformation;
		provider = new PNTSAdditionalInformationProvider(additionalInfo);
	}

	TemporaryStorageAdditionalInfo additionalInfo;
	PNTSAdditionalInformationProvider provider;
}
