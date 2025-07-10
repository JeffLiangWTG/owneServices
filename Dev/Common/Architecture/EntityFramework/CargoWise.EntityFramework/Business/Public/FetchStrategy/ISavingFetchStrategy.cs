namespace CargoWise.EntityFramework
{
	public interface ISavingFetchStrategy
	{
		void FetchForSaving(BusinessObject businessObject);
	}
}
