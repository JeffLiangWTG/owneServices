namespace Enterprise.ZArchitecture.Environment
{
	public interface IGlowRegistry
	{
		string GlowServiceUri { get; }
		bool IsGlowIndexSearchAllowedForModule(string module);
		int MaximumNumberOfModuleFiltersSearchResults { get; }
		bool IndexSearchUsageCollector { get; }
	}
}
