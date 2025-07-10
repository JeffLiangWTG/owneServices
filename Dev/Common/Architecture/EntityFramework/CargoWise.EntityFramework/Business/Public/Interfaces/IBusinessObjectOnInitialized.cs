namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectOnInitialized
	{
		void OnBusinessObjectInitialized(BusinessObject bizo);
	}

	public interface IBusinessObjectOnIntializedFactory
	{
		IBusinessObjectOnInitialized BusinessObjectInitializedHandler { get; }
	}
}
