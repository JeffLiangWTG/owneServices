using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Testing;

class AdditionalProcedureProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalProcedureProvider>
{
	public void TestSequenceNumber() => AssertEquals("1", GetProvider().SequenceNumber);

	public void TestAdditionalProcedure()
	{
		additionalProcedureCode.CY_Code = "123A";
		AssertEquals("123A", provider.AdditionalProcedure);
	}

	protected override AdditionalProcedureProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		additionalProcedureCode = Factory.NewWithValidTestData<AdditionalProcedureCode>();
		provider = new AdditionalProcedureProvider(additionalProcedureCode, 1);
	}
	AdditionalProcedureCode additionalProcedureCode;
	AdditionalProcedureProvider provider;
}
