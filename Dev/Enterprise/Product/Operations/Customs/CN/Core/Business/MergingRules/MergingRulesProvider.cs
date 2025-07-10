using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public interface IMergingRule
	{
		ZString RuleCode { get; }
		ZString RuleName { get; }
		IEnumerable<IZType> GetKeysForLine(JobComInvoiceLine invoiceLine);
	}

	public static class MergingRulesProvider
	{
		public static IEnumerable<IMergingRule> GetAllMergingRules()
		{
			return ImmutableArray.Create<IMergingRule>(
				new MergingRuleAccordingToProductVersion(),
				new MergingRuleAccordingToTradeUnitPrice(),
				new MergingRuleAccordingToSpecModel(),
				new MergingRuleAccordingToSpecialCIQRequired()
			);
		}

		public static IMergingRule GetMergeRule(ZString ruleCode)
		{
			return GetAllMergingRules().FirstOrDefault(x => x.RuleCode == ruleCode);
		}

		public static ICodeDescriptionPairList GetPairList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CN.MergingRules", () =>
			{
				var result = new CodeDescriptionPairList();
				GetAllMergingRules().ForEach(x => result.AddPair(x.RuleCode, x.RuleName));
				return result;
			});
		}
	}
}
