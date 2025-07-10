using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class SelectionCriteriaTest<T, TInvoiceLine> : TestCaseWithFactory
		where T : IZZApplicabilitySelectionCriteria
		where TInvoiceLine : JobComInvoiceLine
	{
		public void TestTradeGroupCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = (TInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "IQ";
			invoiceLine.ZG_CountryOfDestination = "DE";
			declaration.JE_GoodsDestination = "FR";

			declaration.JE_MessageType = "IMP";
			var applicabilitySelectionCriteria = GetNewApplicabilitySelectionCriteria(invoiceLine);
			AssertEquals("When Declaration Type is IMP, TradeGroupCountry", "IQ", applicabilitySelectionCriteria.TradeGroupCountry);

			declaration.JE_MessageType = "EXP";
			applicabilitySelectionCriteria = GetNewApplicabilitySelectionCriteria(invoiceLine);
			AssertEquals("When Declaration Type is EXP, TradeGroupCountry", "DE", applicabilitySelectionCriteria.TradeGroupCountry);

			invoiceLine.ZG_CountryOfDestination = "";
			applicabilitySelectionCriteria = GetNewApplicabilitySelectionCriteria(invoiceLine);
			AssertEquals("When Declaration Type is EXP but ZG_CountryOfDestination is Empty, TradeGroupCountry", "FR", applicabilitySelectionCriteria.TradeGroupCountry);
		}

		protected abstract T GetNewApplicabilitySelectionCriteria(TInvoiceLine invoiceLine);
	}

	class RateSelectionCriteriaTest : SelectionCriteriaTest<IZZRateSelectionCriteria, JobComInvoiceLine>
	{
		protected override IZZRateSelectionCriteria GetNewApplicabilitySelectionCriteria(JobComInvoiceLine invoiceLine)
		{
			return new JobComInvoiceLine.RateSelectionCriteria<JobComInvoiceLine>(invoiceLine, "RT0", "RC0");
		}
	}

	class ZZConditionSelectionCriteriaTest : SelectionCriteriaTest<IZZConditionSelectionCriteria, JobComInvoiceLine>
	{
		protected override IZZConditionSelectionCriteria GetNewApplicabilitySelectionCriteria(JobComInvoiceLine invoiceLine)
		{
			return new JobComInvoiceLine.ZZConditionSelectionCriteria<JobComInvoiceLine>(invoiceLine);
		}
	}
}
