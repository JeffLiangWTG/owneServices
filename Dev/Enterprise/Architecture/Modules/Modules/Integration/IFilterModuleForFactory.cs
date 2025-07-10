using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IFilterModuleForFactory
	{
		BusinessObjectFactory GetNewFactoryForFilterModule();
	}
}
