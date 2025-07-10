using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

class ProcedureProviderTest : Customs.Business.Testing.DataProviderTestCase<ProcedureProvider>
{
	public void TestAdditionalProcedure()
	{
		invoiceLine.AdditionalProcedureCodes.AddNew();
		AssertEquals(1, provider.AdditionalProcedure.Count);
	}

	public void TestPreviousProcedure()
	{
		invoiceLine.JI_Procedure = "123A";
		AssertEquals("3A", provider.PreviousProcedure);
	}

	public void TestRequestedProcedure()
	{
		invoiceLine.JI_Procedure = "123A";
		AssertEquals("12", provider.RequestedProcedure);
	}

	protected override ProcedureProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
		provider = new ProcedureProvider(invoiceLine);
	}
	JobComInvoiceLine invoiceLine;
	ProcedureProvider provider;
}
