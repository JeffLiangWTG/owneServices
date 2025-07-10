using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface IRelatedModuleFilterSupportable : IBusiness
	{
		string TablePrefix { get; }
	}
}
