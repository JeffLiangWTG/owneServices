using System;

namespace GlowIndexQueryService.Common
{
	class LookupRuleDto
	{
		public LookupRuleDto(Guid ruleId)
		{
			RuleId = ruleId;
		}
		public Guid RuleId { get; }
	}
}
