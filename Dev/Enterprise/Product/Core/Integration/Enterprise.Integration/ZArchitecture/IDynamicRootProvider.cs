using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public interface IDynamicRootProvider : IRootTypeProvider
	{
		BusinessObject[] AugmentedRoots(BusinessObject parent);
	}
}
