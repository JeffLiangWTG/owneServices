using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	public interface IActiveServiceLevelProvider
	{
		RegistryServiceLevelCollection GetActiveServiceLevels(BusinessObjectFactory factory);
	}
}
