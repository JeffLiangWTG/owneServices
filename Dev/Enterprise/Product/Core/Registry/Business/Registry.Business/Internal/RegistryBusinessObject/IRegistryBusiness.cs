using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public interface IRegistryBusiness : IBusiness
	{
		FallbackLevel CurrentFallbackLevel { get; set; }
		IRegistryBusiness Clone(FallbackLevel currentFallbackLevel, BusinessObjectFactory factory);
	}
}
