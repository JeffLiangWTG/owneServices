using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	interface IBusinessObjectLogger
	{
		BusinessObject CreateSaveLog(BusinessObject loggingBizo);

		BusinessObject CreateModifiedSaveLog(BusinessObject loggingBizo);

		BusinessObject CreateDeleteLog(BusinessObject loggingBizo);

		void RemoveLog(BusinessObject loggingBizo);

		void OnSaved(BusinessObject loggingBizo, bool isSavedSucceeded);

		void FetchForSaving(BusinessObject loggingBizo);

		bool RunAfterOnSavingForAllBizos { get; }
	}
}
