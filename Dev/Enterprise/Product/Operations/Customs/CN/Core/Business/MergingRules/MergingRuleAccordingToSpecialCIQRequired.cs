using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class MergingRuleAccordingToSpecialCIQRequired : IMergingRule
	{
		const string ruleCode = "CIR";

		public ZString RuleCode => ruleCode;

		public ZString RuleName => Res.GetString("AC596ED9-31F2-44C0-9A7E-485391A3AFC8", "Specification & Model of Special CIQ Required Goods");

		public IEnumerable<IZType> GetKeysForLine(JobComInvoiceLine invoiceLine)
		{
			var tariff = invoiceLine.UniversalTariff;
			var declaration = invoiceLine.Declaration;
			if (tariff != null && declaration != null)
			{
				var hasSpecialCIQRequirements = (declaration.WillGenerateEnteringEntry && tariff.HasSpecialCIQImportRequirement()) || (declaration.WillGenerateExitingEntry && tariff.HasSpecialCIQExportRequirement());
				if (hasSpecialCIQRequirements)
				{
					yield return invoiceLine.XC_GoodsSpecModel;

					if (invoiceLine.ChildInstruction != null)
					{
						yield return invoiceLine.XC_GoodsSpecModel2;
					}
				}
			}
		}
	}
}
