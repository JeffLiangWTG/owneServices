namespace CargoWise.EntityFramework
{
	public interface IBOIsSavedByFactoryService : IService
	{
		bool IsBOSavedByFactory(BusinessObject businessObjects);
	}
}
