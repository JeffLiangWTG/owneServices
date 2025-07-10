using System.Text;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class IncidentConstants
	{
		public const string SupportIncidentDocManagerCode = Core.Constants.DocManagerCodes.IncidentRequest;
		public const string ProfessionalServicesQuoteDocManagerCode = "INC";
		public const string IncidentManagementGroupDocManagerCode = "ING";
		public const string CriticalityChangedMessageTemplate = "Criticality changed from {0} to {1}";
		public const string StageChangedMessageTemplate = "Escalated as {0}";

		#region Resources
		/// <summary>
		/// Resource MUST be stored in ASCII format
		/// </summary>
		/// <param name="resourceName"></param>
		/// <returns></returns>
		public static string GetTextFromResource(string resourceName)
		{
			return new EmbeddedResourceRetriever().GetString(resourceName, Encoding.ASCII);
		}
		#endregion

		#region Record Type

		public static class IncidentType
		{
			public const string SupportIncident = "INC";
			public const string ProfessionalServicesQuote = "PSQ";
		}

		public static string GetIncidentTypeDescription(string incidentTypeCode)
		{
			string result = "";

			if (incidentTypeCode == IncidentType.ProfessionalServicesQuote)
			{
				result = "Professional Services Quote";
			}
			else if (incidentTypeCode == IncidentType.SupportIncident)
			{
				result = "Customer Service Incident";
			}

			return result;
		}

		#endregion

		#region Status

		public static class IncidentStatus
		{
			public const string Unassigned = "UAS";
			public const string Assigned = "ASI";
			public const string InProgress = "INP";
			public const string FinishedPendingCodeReview = "FPR";
			public const string FinishedDeployedAndPendingVerification = "FIX";
			public const string FinishedPendingCheckIn = "FPC";
			public const string FinishedCheckedInAndPendingDeploy = "FPD";
			public const string Closed = "CLO";
			public const string Waiting = "WTG";
			public const string Deferred = "DEF";
			public const string Cancelled = "CAN";
			public const string AwaitingFeatureAnalysis = "AFA";
			public const string AwaitingDevelopmentWork = "ADW";
			public const string AwaitingSchedulingTeam = "WST";
			public const string AwaitingTeamScheduler = "ATS";

			public const string AllOpen = "OPE";
			public const string OpenAndNotFinished = "ONF";
			public const string OpenAndNotFinishedAndNotWaiting = "ONW";
			public const string OpenAndNotFinishedAndNotWaitingAndNotDeferred = "OND";
		}

		public static CodeDescriptionPairList GetStatusCodeDescPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			result.AddPair(IncidentStatus.Unassigned, "Unassigned");
			result.AddPair(IncidentStatus.Assigned, "Assigned, but Not Started");
			result.AddPair(IncidentStatus.InProgress, "In Progress");
			result.AddPair(IncidentStatus.Waiting, "Waiting on Someone Else");
			result.AddPair(IncidentStatus.AwaitingFeatureAnalysis, "Awaiting Feature Analysis");
			result.AddPair(IncidentStatus.AwaitingSchedulingTeam, "Awaiting Scheduling Team");
			result.AddPair(IncidentStatus.AwaitingTeamScheduler, "Awaiting Team Scheduler");
			result.AddPair(IncidentStatus.AwaitingDevelopmentWork, "Awaiting Development Work");
			result.AddPair(IncidentStatus.FinishedPendingCodeReview, "Finished - Pending Code Review");
			result.AddPair(IncidentStatus.FinishedPendingCheckIn, "Finished - Pending Check In");
			result.AddPair(IncidentStatus.FinishedCheckedInAndPendingDeploy, "Finished - Checked In and Pending Deploy");
			result.AddPair(IncidentStatus.FinishedDeployedAndPendingVerification, "Finished - Deployed and Pending Verification");
			result.AddPair(IncidentStatus.Closed, "Closed");
			result.AddPair(IncidentStatus.Deferred, "Deferred");
			result.AddPair(IncidentStatus.Cancelled, "Cancelled");

			return result;
		}

		#endregion

		#region Priority

		public static class Priority
		{
			public const string Low = "LOW";
			public const string Medium = "MED";
			public const string High = "HI";
			public const string Critical = "CRT";
			public const string All = "ALL";
		}

		public static CodeDescriptionPairList GetPriorityCodeDescPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			result.AddPair(Priority.Low, "Low");
			result.AddPair(Priority.Medium, "Medium");
			result.AddPair(Priority.High, "High");
			result.AddPair(Priority.Critical, "Critical");

			return result;
		}

		public static bool IsDefect(ZString criticality)
		{
			return criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown ||
				criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown ||
				criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround ||
				criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
		}

		#endregion

		#region Email Constants

		public const string SupportDisplayName = "CargoWise Customer Service";
		public const string ManagerEmailAddress = "PleaseDoNotReply@wisetechglobal.com";

		public const string ImplementationDefaultReplyToName = "Implementation";
		public const string MarkName = "CustomerServiceIdentifier";
		public const string MarkContent = "<div name='CustomerServiceIdentifier' id='{0}'>Please don't edit the content of this email as your message may not be processed</div>";

		#endregion

		#region Program Area

		public static CodeDescriptionPairList GetProgramAreaList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			result.AddPair(ProgramArea.General, "General");
			result.AddPair(ProgramArea.Documents, "Documents");
			result.AddPair(ProgramArea.Reports, "Reports");
			result.AddPair(ProgramArea.Graphics, "Graphics");
			result.AddPair(ProgramArea.Interface, "Interface");
			result.AddPair(ProgramArea.DataFix, "Data Fix");

			return result;
		}

		public static class ProgramArea
		{
			public const string General = "GEN";
			public const string Documents = "DOC";
			public const string Reports = "REP";
			public const string Graphics = "GRA";
			public const string Interface = "INT";
			public const string DataFix = "DAT";
		}

		#endregion

		#region Event Log Free Text

		public static class LogFreeText
		{
			public const string CloseAsResolvedClosureResolutionLog = "Incident resolved with method";

			public const string CloseAsResolvedChangedUserLog = "Incident resolved by user";

			public const string CloseSkipResolvedChangedUserLog = "Incident closed by user";

			public const string CloseIncidentFromResolvedByIRPLog = "Incident is being closed by IRP service task";

			public const string RevertIncidentToClosed = "Revert resolution code from {0} to {1}.";

			public const string CR5ResolutionAssistantResult = "CR5 Resolution Assistant Result";
		}

		#endregion

		public static class DelayedMessageKey
		{
			public const string WorkCompletedButNoUpgrades = "WorkCompletedButNoUpgrades";
			public const string AllWorkItemsCancelled = "AllWorkItemsCancelled";
		}

		public static class CR5ContentAvailable
		{
			public const string CompletelySolved = "Y";
			public const string PartlySolved = "PARTLY";
			public const string ContentNotBeFound = "N";
		}

		public static class CR5ContentPriority
		{
			public const string DoNotDevelop = "0";
			public const string LowPriority = "1";
			public const string NormalPriority = "2";
		}
	}
}

