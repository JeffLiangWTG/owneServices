using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class AdditionalFiscalReferencesProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalFiscalReferencesProvider>
{
	public void TestSequenceNumber() => AssertEquals("1", GetProvider().SequenceNumber);

	public void TestRole()
	{
		fiscalReference.CFR_Code = "123";
		AssertEquals("123", provider.Role);
	}

	public void TestVatIdentificationNumber()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = organisation.MainAddress;
		fiscalReference.CFR_OA_Owner = orgAddress.PK;
		MessageProviderDataHelper.SetupEORI(orgAddress, "R1234");

		AssertEquals("R1234", provider.VatIdentificationNumber);
	}

	protected override AdditionalFiscalReferencesProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		fiscalReference = Factory.New<CusFiscalReference>();
		provider = new AdditionalFiscalReferencesProvider(fiscalReference, 1);
	}
	CusFiscalReference fiscalReference;
	AdditionalFiscalReferencesProvider provider;
}
