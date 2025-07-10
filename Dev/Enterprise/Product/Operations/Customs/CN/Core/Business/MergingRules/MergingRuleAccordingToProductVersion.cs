using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class MergingRuleAccordingToProductVersion : MergingRuleAccordingToInvoiceLineProperty, IMergingRule
	{
		const string ruleCode = "PDV";

		public ZString RuleCode => ruleCode;
		public ZString RuleName => Res.GetString("FE52D06C-0A75-496F-B297-45C1F76CB294", "Product Version");

		public MergingRuleAccordingToProductVersion() : base(AutoCNJobComInvoiceLine.Schema.JI_ProductVersion)
		{
		}
	}
}
