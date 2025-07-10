using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.Accounting.Business
{
	class JCDStateTransitionValidation
	{
		public static bool IsValidTransition(string state, ref string msg)
		{
			var objectsChecker = new JCDStateTransitionValidation(new JCDDBObjectsChecker(Db.Connection));
			if ((state == JCDActionList.Codes.NotInitialized || state == JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask) && objectsChecker.IsJCDServiceTaskInitialized)
			{
				msg = ResString.GetMultilingualString("858c1cfc-e644-4e61-a871-4cb9cb714221", "You cannot change the registry value to {0}, as Job Costing Data Queue Service task is already initialized.", state);
			}
			else if ((state == JCDActionList.Codes.RemoveJobCostingDataQueueServiceTask || state == JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask) && !objectsChecker.IsJCDServiceTaskInitialized)
			{
				msg = ResString.GetMultilingualString("fbe8bd8c-dc78-48d1-914f-1f4d0d1be0ed", "You cannot change the registry value to {0}, as Job Costing Data Queue Service task is not initialized.", state);
			}
			else if (state == JCDActionList.Codes.CompletedAllDatabaseObjectsForJobCostingDataQueueHaveBeenCreated && !objectsChecker.DoesDBStateAllowTransitionToCDBState)
			{
				msg = ResString.GetMultilingualString("6f8ff073-315f-4b97-bb48-e58476889e59", "You cannot change the registry value to {0}, as either there are unprocessed old transaction records in Database or Job Costing Data Queue service task is not initialized.", state);
			}
			else if (state == JCDActionList.Codes.ProcessingOldTransactionLines && !objectsChecker.DoesDBStateAllowTransitionToPORState)
			{
				msg = ResString.GetMultilingualString("f4de130d-b926-4cb0-8354-8036c9546d87", "You cannot change the registry value to {0}, as either there is no unprocessed transaction record or Job Costing Data Queue service task is not initialized.", state);
			}
			else if (state == JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed && !objectsChecker.DoesDBStateAllowTransitionToCUPState)
			{
				msg = ResString.GetMultilingualString("515f4317-f1a2-41f3-8e82-f84cce7c098a", "You cannot change the registry value to {0}, as either there are unprocessed old transaction records or Job Costing Data Queue service task is not initialized.", state);
			}
			return msg.IsNullOrEmpty();
		}

		JCDStateTransitionValidation(JCDDBObjectsChecker checker)
		{
			this.checker = Argument.NotNull(checker, nameof(checker));
		}
		readonly JCDDBObjectsChecker checker;

		bool IsJCDServiceTaskInitialized => checker.DoesAnyJCDDBObjectExist();

		bool DoesDBStateAllowTransitionToCDBState => checker.DoesAllJCDDBObjectExist() &&
											!checker.IsThereAnyUnprocessedOldALRecord() &&
											checker.IsQueueEmpty() &&
											checker.IsReportTableEmpty();

		bool DoesDBStateAllowTransitionToPORState
		{
			get
			{
				var result = checker.DoesAllJCDDBObjectExist();
				if (result)
				{
					var anyUnprocessedALRecord = checker.IsThereAnyUnprocessedOldALRecord();
					var anyALRecord = checker.IsThereAnyALRecord();
					result = (anyUnprocessedALRecord == anyALRecord);
					if (result)
					{
						result = checker.IsReportTableEmpty();
					}
				}
				return result;
			}
		}

		bool DoesDBStateAllowTransitionToCUPState => checker.DoesAllPermanentJCDDBObjectExist() && !checker.IsThereAnyUnprocessedOldALRecord();
	}
}
