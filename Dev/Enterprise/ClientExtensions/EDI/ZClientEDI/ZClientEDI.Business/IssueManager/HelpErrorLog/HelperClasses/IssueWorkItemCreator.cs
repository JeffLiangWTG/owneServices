using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	static class IssueWorkItemCreator
	{
		public static void LinkOrCreateIssueForWorkItemIfRequired(EdiHelpErrorLog log, DataFormatter formatter, IIssueAssignmentCalculator issueAssignmentCalculator, ZDateTime timeToReferForCutoff, bool isNewIssue)
		{
			var occurrence = log.Factory.LoadTop1<HelpErrorLogOccurrence>(new ZQuery(HelpErrorLogOccurrenceSchema.HO_HE, log.PK) { OrderBy = HelpErrorLogOccurrenceSchema.Constants.HO_EXEDateTime + " DESC" });
			LinkOrCreateIssueForWorkItemIfRequired(log, formatter, issueAssignmentCalculator, timeToReferForCutoff, occurrence, isNewIssue);
		}

		public static void LinkOrCreateIssueForWorkItemIfRequired(EdiHelpErrorLog log, DataFormatter formatter, IIssueAssignmentCalculator issueAssignmentCalculator, ZDateTime timeToReferForCutoff, HelpErrorLogOccurrence occurrence, bool isNewIssue)
		{
			if (EDIDataRegistry.Instance.UseTestRigOriginInIssueWorkItemCreation.Value)
			{
				LinkOrCreateIssueForWorkItemIfRequiredWithTestRigIssues(log, formatter, issueAssignmentCalculator, timeToReferForCutoff, occurrence, isNewIssue);
			}
			else
			{
				LinkOrCreateIssueForWorkItemIfRequiredCore(log, formatter, issueAssignmentCalculator, timeToReferForCutoff, occurrence);
			}
		}

		static void LinkOrCreateIssueForWorkItemIfRequiredCore(EdiHelpErrorLog log, DataFormatter formatter, IIssueAssignmentCalculator issueAssignmentCalculator, ZDateTime timeToReferForCutoff, HelpErrorLogOccurrence occurrence)
		{
			if (IssueDoesNotYetHaveWorkItem(log) && IssueNeedsWorkItem(log, timeToReferForCutoff, occurrence))
			{
				var isFallbackAssignment = GetAssignment(issueAssignmentCalculator, log, formatter, out var assignment);

				if (RelevantWorkItemExistsForIssue(log, assignment, out var relevantWorkItem))
				{
					relevantWorkItem.RelatedItems.Add(log);
				}
				else
				{
					CreateNewIssueWorkItem(log, assignment, occurrence, isFallbackAssignment, formatter);
				}
			}
		}

		static void LinkOrCreateIssueForWorkItemIfRequiredWithTestRigIssues(EdiHelpErrorLog log, DataFormatter formatter, IIssueAssignmentCalculator issueAssignmentCalculator, ZDateTime timeToReferForCutoff, HelpErrorLogOccurrence processedOccurrence, bool isNewIssue)
		{
			var fromTestRig = OccurrenceFromTestRig(processedOccurrence);

			if (isNewIssue)
			{
				if (fromTestRig && AttachIssueToTestRigOrigin(log, processedOccurrence))
				{
					return;
				}
			}
			else
			{
				if (!fromTestRig && log.HE_LogType == "EXT")
				{
					var isFallbackAssignment = GetAssignment(issueAssignmentCalculator, log, formatter, out var assignment);
					log.HE_LogType = "EXC";
					CreateNewIssueWorkItem(log, assignment, processedOccurrence, isFallbackAssignment, formatter);
					return;
				}
			}
			LinkOrCreateIssueForWorkItemIfRequiredCore(log, formatter, issueAssignmentCalculator, timeToReferForCutoff, processedOccurrence);
		}

		static bool GetAssignment(IIssueAssignmentCalculator assignmentCalculator, EdiHelpErrorLog log, DataFormatter formatter, out IssueAssignment assignment)
		{
			formatter.LogMessage("Getting issue assignment...");
			assignment = assignmentCalculator.GetAssignment(log, formatter);
			var useFallbackAssignment = assignment == null;
			if (useFallbackAssignment)
			{
				formatter.LogMessage("No assignment was found, getting default issue assignment...");
				assignment = GetDefaultIssueAssignment(log);
			}

			formatter.LogMessage($"Final assignment: {assignment}");

			return useFallbackAssignment;
		}

		static bool AttachIssueToTestRigOrigin(EdiHelpErrorLog log, HelpErrorLogOccurrence occurrence)
		{
			var workItem = log.Factory.LoadTop1<NewWorkItem>(new ZQuery(WorkItemSchema.WKI_WorkItemNumber, occurrence.TestRigOrigin));

			if (workItem == null)
			{
				return false;
			}

			workItem.RelatedItems.Add(log);
			log.HE_LogType = "EXT";

			return true;
		}

		static bool OccurrenceFromTestRig(HelpErrorLogOccurrence occurrence)
		{
			return occurrence.TestRigOrigin != "From Prod";
		}

		public static bool IsFromOldSystem(ZDateTime exeDate)
		{
			var earliestExeToProcess = EDIDataRegistry.Instance.EarliestExeDateToProcessInIssueManager;
			return (!earliestExeToProcess.IsEmpty && exeDate < earliestExeToProcess);
		}

		static void CreateNewIssueWorkItem(EdiHelpErrorLog log, IssueAssignment assignment, HelpErrorLogOccurrence occurrence, bool useFallbackAssignment, ILogFormatProvider formatProvider)
		{
			var newWorkItem = log.Factory.New<NewWorkItem>();
			var diagnosticData = Encoding.ASCII.GetBytes(formatProvider.AsString());
			newWorkItem.DocManagerInfo.AddFileOrDocument(diagnosticData, "Issue Assignment Log.txt", "LTR", description: "Data on how this issue WI was classified and assigned.");
			newWorkItem.DocManagerInfo.Save();
			newWorkItem.RelatedItems.Add(log);
			newWorkItem.WKI_WorkItemType = assignment.Product;
			newWorkItem.WKI_WorkItemArea = assignment.ProductArea;
			newWorkItem.WKI_ActivityType = assignment.Module;
			newWorkItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.IssueFix;
			newWorkItem.WKI_Priority = ReleaseRings.Codes.GPR;
			if (useFallbackAssignment)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				newWorkItem.Logs.AddNew(AutoEvents.EditedARecord, $"Created from issue, assigned with fallback criteria: {assignment.Product}/{assignment.ProductArea}/{assignment.Module}");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			log.AfterIssueNumberSet(() =>
			{
				ZString description = string.Empty;
				if (occurrence.ErrorType == HelpErrorLogOccurrenceLookups.ErrorType.DatabaseUpgradeFailure)
				{
					description += "Upgrade Failure ";
				}
				description += log.ExceptionMessageFirstLine;
				if (newWorkItem.WKI_Summary.IsEmpty)
				{
					newWorkItem.WKI_Summary = description.SubstringSafe(0, IncidentMainSchema.IM_Description.MaxLength);
					newWorkItem.WKI_Details = ZBlob.FromUTF8(log.HE_ExceptionMessage);
				}

				var suffixApiIndication = assignment.IsRetrievedFromAPI ? " by MLTA service" : "";
				var eventMessage = $"Created from issue {log.HE_IssueNumber}, assigned with {assignment}{suffixApiIndication}";
				newWorkItem.Logs.AddNew(Events.AutoMatchDone, eventMessage);
			});

			var dictionary = GetErrorDictionary(log);
			dictionary.SaveSimilar(newWorkItem.PK, log.HE_ExceptionMessage, assignment);
		}

		static ErrorDictionary GetErrorDictionary(EdiHelpErrorLog log)
		{
			var dictionary = log.Factory.ServiceContainer.GetService<ErrorDictionary>();
			if (dictionary == null)
			{
				dictionary = new ErrorDictionary();
				log.Factory.ServiceContainer.AddService(dictionary);
			}
			return dictionary;
		}

		static bool IssueDoesNotYetHaveWorkItem(EdiHelpErrorLog log)
		{
			return (!log.HasWorkItems || log.RelatedWorkItems.Cast<WorkItem>().All(x => x.IsClosedOrCancelled));
		}

		static bool IssueNeedsWorkItem(EdiHelpErrorLog log, ZDateTime timeToReferForCutoff, HelpErrorLogOccurrence occurrence)
		{
			return IssueIsOverThreshold(log, timeToReferForCutoff, occurrence);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]

		static bool IssueIsOverThreshold(EdiHelpErrorLog log, ZDateTime timeToReferForCutoff, HelpErrorLogOccurrence occurrence)
		{
			var thresholdRegistryItem = log.HE_IsClientVisible ? EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible : EDIDataRegistry.Instance.IssueWorkItemCreationThresholdNonClientVisible;
			var thresholdOccurrences = thresholdRegistryItem.Value.Cast<IssueWorkItemCreationThreshold>().First().IssueOccurrenceThreshold;

			if (occurrence.ErrorType == HelpErrorLogOccurrenceLookups.ErrorType.DatabaseUpgradeFailure)
			{
				thresholdOccurrences = 1;
			}

			var thresholdTimespan = thresholdRegistryItem.Value.Cast<IssueWorkItemCreationThreshold>().First().ThresholdTimespan;
			var exceptionTimeCutoff = timeToReferForCutoff.AddDays(-thresholdTimespan);
			var exeDateCutoff = ZDateTime.Today.AddDays(-30);

			var latestReleaseBuildsDictionary = new ReleaseBuilds.LatestReleaseBuildsDictionary(log.Factory);
			latestReleaseBuildsDictionary.Load();
			var latestGP1ReleaseBuildVersion = latestReleaseBuildsDictionary.GetLatestAvailableBuildVersion(ReleaseRings.Codes.GP1, takeWeeklyBuildsInsteadOfLatest: false);
			int majorVersion = latestGP1ReleaseBuildVersion.Major;
			int minorVersion = latestGP1ReleaseBuildVersion.Minor;
			int releaseVersion = latestGP1ReleaseBuildVersion.Release;

			int count;

			var sqlText = @"
                    SELECT Count(*)
                    FROM dbo.HelpErrorLogOccurrence
						LEFT JOIN dbo.ReleaseBuild on HO_HL = HL_PK
                    Where 1=1
						AND HO_HE = @Pk
                        AND HO_ExceptionDateTime > @ExceptionTimeCutoff
                        AND (	HO_EXEDateTime > @ExeDateCutoff 
							OR	HO_EXEDateTime is null
							OR	(
									HL_MajorVersion = @LatestMajor
								AND	HL_MinorVersion = @LatestMinor
								AND	HL_Release = @LatestRelease
								)
							)";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@Pk", SqlDbType.UniqueIdentifier, log.PK.ToGuid());
				cmd.AddParameter("@ExeDateCutoff", SqlDbType.DateTime, exeDateCutoff.ToDateTime());
				cmd.AddParameter("@ExceptionTimeCutoff", SqlDbType.DateTime, exceptionTimeCutoff.ToDateTime());
				cmd.AddParameter("@LatestMajor", SqlDbType.Int, majorVersion);
				cmd.AddParameter("@LatestMinor", SqlDbType.Int, minorVersion);
				cmd.AddParameter("@LatestRelease", SqlDbType.Int, releaseVersion);

				count = (int)cmd.ExecuteScalar();
			}

			count += log.Factory.GetChanges().GetAddedObjects()
				.Where(added => added is HelpErrorLogOccurrence oc && oc.HO_HE == log.PK && oc.HO_ExceptionDateTime > exceptionTimeCutoff &&
					(oc.HO_EXEDateTime > exeDateCutoff || oc.HO_EXEDateTime.Date.IsEmpty ||
					(oc.ReleaseBuild != null && oc.ReleaseBuild.HL_MajorVersion == majorVersion && oc.ReleaseBuild.HL_MinorVersion == minorVersion && oc.ReleaseBuild.HL_Release == releaseVersion)))
				.Count();

			return count >= thresholdOccurrences;
		}

		static IssueAssignment GetDefaultIssueAssignment(EdiHelpErrorLog log)
		{
			var dotnetIssueAssignment = new Lazy<IssueAssignment>(() =>
			{
				var fallbackItem = EDIDataRegistry.Instance.FallbackWorkItemCriteriaRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				if (fallbackItem.IsNullOrEmpty())
				{
					return new IssueAssignment(string.Empty, string.Empty, string.Empty);
				}
				var fallbackAssignment = fallbackItem.Split('/');
				return new IssueAssignment(fallbackAssignment[0], fallbackAssignment[1], fallbackAssignment[2]);
			});

			return GetAssignmentForNonDotNetIssue(log) ?? dotnetIssueAssignment.Value;
		}

		static IssueAssignment GetAssignmentForNonDotNetIssue(EdiHelpErrorLog log)
		{
			if (log.HE_ExceptionType.StartsWith("VFP", StringComparison.OrdinalIgnoreCase)) // Sapphire
			{
				return new IssueAssignment("SPH", string.Empty, "SYS");
			}
			return null;
		}

		static bool RelevantWorkItemExistsForIssue(EdiHelpErrorLog log, IssueAssignment assignment, out NewWorkItem openWorkItem)
		{
			openWorkItem = null;

			var dictionary = GetErrorDictionary(log);

			if (!ShouldFindSimilarWorkItem(log))
			{
				return false;
			}

			var workItemPk = dictionary.FindSimilar(log.HE_ExceptionMessage, assignment);

			if (workItemPk.IsValid)
			{
				var foundWorkItem = log.Factory.Load<NewWorkItem>(workItemPk);
				var exeDate = RetrieveLatestExeDate(log);
				if (!foundWorkItem.IsClosedOrCancelled || (foundWorkItem.IsClosed && foundWorkItem.JobCloseDateUtc > exeDate))
				{
					openWorkItem = foundWorkItem;
				}
			}

			return openWorkItem != null;
		}

		static ZDateTime RetrieveLatestExeDate(EdiHelpErrorLog log)
		{
			return log.Occurrences.Select(errorOccurrence => errorOccurrence.HO_EXEDateTime).Max();
		}

		static bool ShouldFindSimilarWorkItem(EdiHelpErrorLog log)
		{
			return !new ZString[]
			{
				typeof(NullReferenceException).FullName
			}.Contains(log.HE_ExceptionType);
		}

		class ErrorDictionary : IService
		{
			readonly Dictionary<(string errorDescription, IssueAssignment assignment), ZGuid> previousSearches = new Dictionary<(string errorDescription, IssueAssignment assignment), ZGuid>();

			public ZGuid FindSimilar(ZString errorDescription, IssueAssignment assignment)
			{
				ZGuid result = ZGuid.Empty;
				if (previousSearches.TryGetValue((errorDescription, assignment), out result))
				{
					return result;
				}

				// find similar workitem that is open
				const string sqlText = @"SELECT TOP 1 WKI_PK FROM dbo.WorkItem
INNER JOIN dbo.GenPivot ON WorkItem.WKI_Status NOT IN ('CLS','CAN') AND (WorkItem.WKI_PK = GenPivot.XX_Relation1ID)
INNER JOIN dbo.HelpErrorLog ON (GenPivot.XX_Relation2ID = HelpErrorLog.HE_PK)
AND HelpErrorLog.HE_ExceptionMessage = @ExceptionMessage 
WHERE WKI_WorkItemType = @Product AND WKI_WorkItemArea = @ProductArea AND WKI_ActivityType = @Module";

				using (var command = Db.Connection.Command(sqlText))
				{
					command.AddParameterBasedOnDbColumn("@ExceptionMessage", errorDescription.ToString(), HelpErrorLogSchema.HE_ExceptionMessage);
					command.AddParameterBasedOnDbColumn("@Product", assignment.Product, WorkItemSchema.WKI_WorkItemType);
					command.AddParameterBasedOnDbColumn("@ProductArea", assignment.ProductArea, WorkItemSchema.WKI_WorkItemArea);
					command.AddParameterBasedOnDbColumn("@Module", assignment.Module, WorkItemSchema.WKI_ActivityType);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							result = reader.GetGuid(0);
						}
					}
				}
				SaveSimilar(result, errorDescription, assignment);
				return result;
			}

			internal void SaveSimilar(ZGuid workitemPK, ZString errorDescription, IssueAssignment assignment)
			{
				previousSearches[(errorDescription, assignment)] = workitemPK;
			}
		}
	}
}
