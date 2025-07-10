using Enterprise.Integration.ZArchitecture;

namespace Enterprise.Customs.Forwarding.Module
{
	public interface IModuleCustomFiltersProvider
	{
		void AddFilters(IModuleFilterCollection filters);
	}
}
