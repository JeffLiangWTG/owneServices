using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

class AdditionalReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalReferenceProvider>
{
	public void TestSequenceNumber() => AssertEquals(999, provider.SequenceNumber);

	public void TestType()
	{
		additionalInfo.CSI_Code = "123A";
		AssertEquals("123A", provider.Type);
	}

	public void TestReferenceNumber()
	{
		additionalInfo.CSI_ReferenceNumber = "asd123";
		AssertEquals("asd123", provider.ReferenceNumber);
	}

	protected override AdditionalReferenceProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		additionalInfo = declaration.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = "REF";
		provider = new AdditionalReferenceProvider(additionalInfo, 999);
	}
	EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo additionalInfo;
	AdditionalReferenceProvider provider;
}
