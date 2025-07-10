using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class MergingRuleAccordingToTradeUnitPrice : MergingRuleAccordingToInvoiceLineProperty, IMergingRule
	{
		const string ruleCode = "TUP";

		public ZString RuleCode => ruleCode;
		public ZString RuleName => Res.GetString("15940480-54AF-4252-A15E-EF1E21E87A22", "Trade Unit Price");

		public MergingRuleAccordingToTradeUnitPrice() : base(JobComInvoiceLine.Schema.TradeUnitPrice)
		{
		}
	}
}
