namespace Enterprise.BufferManagement.Business
{
	public class TagDefinitionLookups : AutoTagDefinitionLookups
	{
		public TagDefinitionLookups(AutoTagDefinition parent)
			: base(parent)
		{
		}

		public TagUsageScopeList UsageScopeList
		{
			get { return Factory.GetCachedValue<TagUsageScopeList>(); }
		}

		public TagScopeList ScopeList
		{
			get { return Factory.GetCachedValue<TagScopeList>(); }
		}
	}
}
