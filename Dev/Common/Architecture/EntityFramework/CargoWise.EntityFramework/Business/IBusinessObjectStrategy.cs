namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectStrategy
	{
		void BeforeSuccessfulDelete(BusinessObject businessObject);
		DeleteDetails DeleteDetails(BusinessObject businessObject);
		void OnDelete(BusinessObject businessObject);

		void OnSaving(BusinessObject businessObject);
		void OnSaved(BusinessObject businessObject, bool saveSucceeded);
		void OnFactorySaving(BusinessObject businessObject);
		void OnFactorySaved(BusinessObject businessObject, bool saveSucceeded);
		void OnSaveRollback(BusinessObject businessObject);

		void FetchForLoad(BusinessObject businessObject);
		void OnSavingInObjectsWithLateChanges(BusinessObject businessObject);
	}
}
