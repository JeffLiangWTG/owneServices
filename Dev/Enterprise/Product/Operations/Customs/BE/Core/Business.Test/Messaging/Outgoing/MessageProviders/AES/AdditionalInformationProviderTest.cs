using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

class AdditionalInformationProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalInformationProvider>
{
	public void TestSequenceNumber() => AssertEquals(1, GetProvider().SequenceNumber);

	public void TestCode()
	{
		additionalInfo.CSI_Code = "123A";
		AssertEquals("123A", provider.Code);
	}

	public void TestText()
	{
		additionalInfo.CSI_Description = "asd123";
		AssertEquals("asd123", provider.Text);
	}

	protected override AdditionalInformationProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		additionalInfo = declaration.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = "INF";
		provider = new AdditionalInformationProvider(additionalInfo, 1);
	}
	EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo additionalInfo;
	AdditionalInformationProvider provider;
}
