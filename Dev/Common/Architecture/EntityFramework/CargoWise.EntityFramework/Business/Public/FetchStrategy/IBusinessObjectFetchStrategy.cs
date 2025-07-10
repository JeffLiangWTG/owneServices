namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectFetchStrategy : IFetchStrategy
	{
		void FetchForLoad();
		void FetchForLoadChildEditableObjects();
		void FetchForFactorySave();
		void FetchForFactorySaveBeforeTransaction();
		void FetchForValidate();
		void FetchForDelete();
		void FetchForView(TableColumn[] columns);
	}
}
