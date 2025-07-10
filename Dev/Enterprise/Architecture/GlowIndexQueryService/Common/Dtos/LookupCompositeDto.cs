namespace GlowIndexQueryService.Common
{
	class LookupCompositeDto
	{
		public LookupCompositeDto(LookupEntityDto entityLookup, LookupRuleDto ruleLookup)
		{
			EntityLookup = entityLookup;
			RuleLookup = ruleLookup;
		}

		public LookupEntityDto EntityLookup { get; }
		public LookupRuleDto RuleLookup { get; }
	}
}
