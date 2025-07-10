using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business
{
	class AutoAdminBusinessObjectLogger : IBusinessObjectLogger
	{
		public AutoAdminBusinessObjectLogger()
		{
		}

		public BusinessObject CreateSaveLog(BusinessObject loggingBizo) => CreateSaveLogCore(loggingBizo, false);

		public BusinessObject CreateModifiedSaveLog(BusinessObject loggingBizo) => CreateSaveLogCore(loggingBizo, true);

		BusinessObject CreateSaveLogCore(BusinessObject loggingBizo, bool forceModifiedLog)
		{
			if (StaticCurrentFetcher.Instance.CurrentUser == null)
			{
				return null;
			}

			var logTarget = loggingBizo as IAutoAdminLogTarget;

			if (logTarget == null)
			{
				return null;
			}

			var logObj = logTarget.Logs.CreateAutoAdminLog(forceModifiedLog);

			logTarget.OnCreateAutoAdminLog();
			//don't create a new empty auto-admin log if one with the same or more information already exists
			//(we waited until after OnCreateAutoAdminLog so it has a chance to add to the SL_Reference and so it doesn't NRE)
			if (logObj != null && !logObj.IsDeleted && logObj.SL_Reference.IsEmpty)
			{
				if (logTarget.Logs.HasNonEmptyLogNotInDatabase(logObj.SL_SE_NKEvent))
				{
					logObj.Delete();
					return null;
				}
			}
			return logObj;
		}

		public BusinessObject CreateDeleteLog(BusinessObject loggingBizo)
		{
			var logTarget = loggingBizo as IAutoAdminLogTarget;

			if (logTarget == null)
			{
				return null;
			}

			var log = logTarget.Logs.CreateAutoDeleteLog();
			return log;
		}

		public void RemoveLog(BusinessObject loggingBizo)
		{
			if (loggingBizo is IAutoAdminLogTarget logTarget) 
			{
				logTarget.Logs.RemoveAutoAdminLog();
			}
		}

		public void OnSaved(BusinessObject businessObject, bool isSavedSucceeded)
		{
		}

		public void FetchForSaving(BusinessObject loggingBizo)
		{
			if (!(loggingBizo is IAutoAdminLogTarget logTarget))
			{
				return;
			}

			if (!(loggingBizo is IWorkflowProviderCore) && !(loggingBizo is IWorkflowTriggerEventSource) ||
				StaticCurrentFetcher.Instance.CurrentUser == null)
			{
				return;
			}

			var eventCode = loggingBizo.IsInDatabase ? Events.EditedARecordCode : Events.AddedARecordToTheSystemCode;
			var logsParent = (IStmALogParent)loggingBizo;

			ObjectFactory.Get<ITriggerProvider>().AddFetchHintsForTriggeringEvent(loggingBizo.Factory, logsParent, eventCode);
		}

		public bool RunAfterOnSavingForAllBizos { get; }
	}
}
