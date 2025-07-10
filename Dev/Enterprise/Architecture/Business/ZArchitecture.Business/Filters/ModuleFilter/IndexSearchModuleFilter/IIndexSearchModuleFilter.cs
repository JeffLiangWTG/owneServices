namespace Enterprise.ZArchitecture.Business
{
	public interface IIndexSearchModuleFilter
	{
		GlowIndexQueryService.Business.IGlowQuery GetGlowIndexQuery();
	}
}
