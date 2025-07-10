using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class MergingRuleAccordingToSpecModel : IMergingRule
	{
		const string ruleCode = "SPM";

		public ZString RuleCode => ruleCode;
		public ZString RuleName => Res.GetString("9EC3C39A-565B-4091-8675-628B71F47F2F", "Specification & Model");

		public IEnumerable<IZType> GetKeysForLine(JobComInvoiceLine invoiceLine)
		{
			yield return invoiceLine.XC_GoodsSpecModel;

			if (invoiceLine.ChildInstruction != null)
			{
				yield return invoiceLine.XC_GoodsSpecModel2;
			}
		}
	}
}
