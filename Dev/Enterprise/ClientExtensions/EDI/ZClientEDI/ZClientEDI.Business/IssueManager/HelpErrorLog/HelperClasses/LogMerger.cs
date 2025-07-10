using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class LogMerger
	{
		#region Merge

		public ZGuid Merge(EdiHelpErrorLog[] selectedLogs)
		{
			try
			{
				if (selectedLogs.Length > 1)
				{
					BusinessObjectFactory savingFactory = new BusinessObjectFactory();

					ZGuid masterPK = FindMasterPK(selectedLogs);
					EdiHelpErrorLog masterLog = savingFactory.Load<EdiHelpErrorLog>(masterPK);

					PerformMerge(savingFactory, masterLog, selectedLogs);
					UpdateRelatedWorkItems(masterLog, selectedLogs);
					DeleteChildlessLogs(savingFactory, selectedLogs, masterPK);

					savingFactory.Save();
					CreateNewWorkItemIfNeeded(savingFactory, masterLog);
					return masterPK;
				}
				else
				{
					errorMessage = CannotMergeNeedToSelectMoreThanOneIssueErrorMessage;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errorMessage = "An Exception occurred while merging:" + System.Environment.NewLine + ex.ToString();
			}
			return ZGuid.Empty;
		}

		ZDateTime RetriveLatestExceptionDate(EdiHelpErrorLog masterLog)
		{
			return masterLog.Occurrences.Select(errorOccurrence => errorOccurrence.HO_ExceptionDateTime).Max();
		}

		void CreateNewWorkItemIfNeeded(BusinessObjectFactory savingFactory, EdiHelpErrorLog masterLog)
		{
			var exeDate = RetriveEarliestExeDate(masterLog);
			if (!IssueWorkItemCreator.IsFromOldSystem(exeDate) && masterLog.HE_FixedDate.IsEmpty)
			{
				IssueWorkItemCreator.LinkOrCreateIssueForWorkItemIfRequired(masterLog, new DataFormatter(), IssueAssignmentCalculator, RetriveLatestExceptionDate(masterLog), false);
				savingFactory.Save();
			}
		}

		ZDateTime RetriveEarliestExeDate(EdiHelpErrorLog masterLog)
		{
			return masterLog.Occurrences.Select(errorOccurrence => errorOccurrence.HO_EXEDateTime).Min();
		}

		void PerformMerge(BusinessObjectFactory savingFactory, EdiHelpErrorLog masterLog, EdiHelpErrorLog[] selectedLogs)
		{
			HelpErrorLogOccurrence[] occurrencesToMerge = LoadOccurrencesToMerge(savingFactory, selectedLogs);
			HelpErrorLogKey[] keysToMerge = LoadKeysToMerge(savingFactory, selectedLogs);
			ZDateTime latestEXEDate = ZDateTime.MinSmallDateTimeValue;
			ZDateTime latestFixedDate = ZDateTime.MinSmallDateTimeValue;

			foreach (var errorOccurrence in occurrencesToMerge)
			{
				if (errorOccurrence.HO_HE != masterLog.PK)
				{
					masterLog.Occurrences.Add(errorOccurrence);
					masterLog.HE_FailCount++;
				}
				if (errorOccurrence.HO_EXEDateTime > latestEXEDate)
				{
					latestEXEDate = errorOccurrence.HO_EXEDateTime;
				}
			}

			foreach (var errorKey in keysToMerge)
			{
				if (!masterLog.Keys.ContainsKey(errorKey.HK_Key, errorKey.HK_HashCode))
				{
					masterLog.Keys.Add(errorKey);
				}
			}

			foreach (var log in selectedLogs)
			{
				if (log.PK != masterLog.PK)
				{
					masterLog.Logs.AddNew(Events.Transferred, "Merged Issue: " + log.HE_IssueNumber, ZDateTimeOffset.UtcNow);
				}
				if (log.HE_FixedDate > latestFixedDate)
				{
					latestFixedDate = log.HE_FixedDate;
				}
				masterLog.HE_IsClientVisible = (!masterLog.HE_IsClientVisible && log.HE_IsClientVisible) ? ZBool.True : masterLog.HE_IsClientVisible;
				masterLog.HE_LastEXEVersionDate = (log.HE_LastEXEVersionDate > masterLog.HE_LastEXEVersionDate) ? log.HE_LastEXEVersionDate : masterLog.HE_LastEXEVersionDate;
			}

			masterLog.HE_FixedDate = latestEXEDate > latestFixedDate ? ZDateTime.Empty : latestFixedDate;
		}

		#endregion

		#region Update Related Items

		void UpdateRelatedWorkItems(EdiHelpErrorLog masterLog, EdiHelpErrorLog[] selectedLogs)
		{
			foreach (var errorLog in selectedLogs)
			{
				if (errorLog.PK != masterLog.PK)
				{
					masterLog.RelatedWorkItems.AddRange(errorLog.RelatedWorkItems);
				}
			}
		}

		void DeleteChildlessLogs(BusinessObjectFactory savingFactory, EdiHelpErrorLog[] selectedLogs, ZGuid masterPK)
		{
			foreach (EdiHelpErrorLog errorLog in selectedLogs)
			{
				if (errorLog.PK != masterPK)
				{
					EdiHelpErrorLog logToDelete = savingFactory.Load<EdiHelpErrorLog>(errorLog.PK);
					if (logToDelete != null)
					{
						logToDelete.Delete();
					}
				}
			}
		}

		#endregion

		#region Implementation

		string errorMessage;

		ZGuid FindMasterPK(EdiHelpErrorLog[] selectedLogs)
		{
			ZGuid masterPK = ZGuid.Empty;
			ZDateTime earliestOccurrence = ZDateTime.MaxSmallDateTime;

			foreach (var errorLog in selectedLogs)
			{
				if (errorLog.HE_FirstProcessed < earliestOccurrence)
				{
					earliestOccurrence = errorLog.HE_FirstProcessed;
					masterPK = errorLog.PK;
				}
			}

			return masterPK;
		}

		HelpErrorLogOccurrence[] LoadOccurrencesToMerge(BusinessObjectFactory savingFactory, BusinessObject[] selectedElements)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JoinCondition.Or, HelpErrorLogOccurrenceSchema.HO_HE, SQLComparisonOperator.Equal, GetSelectedElementGuids(selectedElements));
			return savingFactory.Load<HelpErrorLogOccurrence>(query);
		}

		HelpErrorLogKey[] LoadKeysToMerge(BusinessObjectFactory savingFactory, BusinessObject[] selectedElements)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JoinCondition.Or, HelpErrorLogKeySchema.HK_HE, SQLComparisonOperator.Equal, GetSelectedElementGuids(selectedElements));
			return savingFactory.Load<HelpErrorLogKey>(filter);
		}

		List<ZGuid> GetSelectedElementGuids(BusinessObject[] selectedElements)
		{
			List<ZGuid> guids = new List<ZGuid>();
			foreach (BusinessObject element in selectedElements)
			{
				guids.Add(element.PK);
			}
			return guids;
		}

		public string ErrorMessage
		{
			get { return errorMessage; }
		}

		public const string CannotMergeNeedToSelectMoreThanOneIssueErrorMessage = "You must select more than one issue to merge.";
		#endregion
		public IIssueAssignmentCalculator IssueAssignmentCalculator
		{
			get;
			set;
		} = new StackLinesWeightsLogAutoAssigner();
	}
}
