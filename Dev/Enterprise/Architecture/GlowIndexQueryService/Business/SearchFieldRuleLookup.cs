using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace GlowIndexQueryService.Business
{
	public class SearchFieldRuleLookup
	{
		public SearchFieldRuleLookup(Guid ruleId)
		{
			RuleId = ruleId;
		}

		public Guid RuleId { get; }

		public CodeDescriptionPairList GetList()
		{
			var glowIndexQueryEngine = ObjectFactory.Get<IGlowIndexQueryEngine>();
			return glowIndexQueryEngine.GetListByRuleId(RuleId);
		}
	}
}
