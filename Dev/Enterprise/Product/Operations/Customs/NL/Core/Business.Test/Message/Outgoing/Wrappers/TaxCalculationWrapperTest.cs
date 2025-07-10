using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class TaxCalculationWrapperTest : DataProviderTestCase<TaxCalculationWrapper>
{
	public void TestTaxAssessedAmount()
	{
		AssertEquals(0m, wrapper.TaxAssessedAmount);
	}

	public void TestQuotaOrderID()
	{
		invoiceLine.JI_ConcessionOrder = "ID";
		AssertEquals("ID", wrapper.QuotaOrderID);
	}

	public void TestDutyRegimeCode()
	{
		invoiceLine.JI_PrimaryPreference = "2PR";
		AssertEquals("2PR", wrapper.DutyRegimeCode);
	}

	public void TestDutyTaxFees()
	{
		invoiceLine.CusEntryLine.Fees.AddNew();
		AssertEquals(1, wrapper.DutyTaxFees.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var cusEntryLine = Factory.New<CusEntryLine>();
		invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_CL = cusEntryLine.PK;
		wrapper = new TaxCalculationWrapper(invoiceLine);
	}
	JobComInvoiceLine invoiceLine;
	TaxCalculationWrapper wrapper;

	protected override TaxCalculationWrapper GetProvider() => wrapper;
}
