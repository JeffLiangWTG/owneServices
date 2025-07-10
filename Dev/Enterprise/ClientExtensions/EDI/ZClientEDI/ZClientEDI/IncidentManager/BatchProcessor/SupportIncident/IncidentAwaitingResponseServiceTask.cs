using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"IHP",
	"Incident Awaiting Response Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.IncidentManager.BatchProcessor.IncidentAwaitingResponseServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Minutes",
	DefaultScheduleRunEvery = "1hours",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true)
]

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	[NeedsDataRefresh]
	class IncidentAwaitingResponseServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var incidents = GetAwaitingResponseIncidents();

			foreach (var incident in incidents)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					SupportIncident loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
					var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						ProcessIncident(loadedIncident);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture,
																	"Failed to process incident {0}. Exception Details : {1}",
																	incident.IM_IncidentNumber,
																	ex.ToString()));
					ErrorReporter.ReportOnce("Failed to process Incident", FormattableString.Invariant($"{incident.IM_IncidentNumber} {ex.Message}"), ex);
				}
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		IEnumerable<SupportIncident> GetAwaitingResponseIncidents()
		{
			ZQuery filter = new ZQuery(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.SupportIncident);
			filter.AddToFilter(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed);
			filter.AddToFilter(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse);
			return new BusinessObjectFactory().Load<SupportIncident>(filter);
		}

		void ProcessIncident(SupportIncident incident)
		{
			bool isProcessed = false;
			string logText = null;

			var targetCWREvent = incident.GetLatestAwaitingResponseEvent();
			if (targetCWREvent == null)
			{
				var statusChangeLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
				statusChangeLogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.EndsWith, SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse);
				statusChangeLogQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
				targetCWREvent = incident.Logs.Find(statusChangeLogQuery)[0];
			}

			var lastAwaitingResponseTime = targetCWREvent.SL_PostedTimeUtc;
			var noticeLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.IncidentEmailSentCode);
			noticeLogQuery.AddToFilter(StmALogSchema.SL_Reference, NoticeSentLogReference);
			noticeLogQuery.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, lastAwaitingResponseTime);
			noticeLogQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			var noticeLogs = incident.Logs.Find(noticeLogQuery);
			int noticeCount = noticeLogs.Length;
			var lastNoticeTime = noticeCount > 0 ? noticeLogs[0].SL_PostedTimeUtc : ZDateTime.Empty;
			var now = ZDateTime.UtcNow;

			var expireDays = GetDaysPendingCustomerToClosed(incident);

			if (lastAwaitingResponseTime.AddDays(expireDays) <= now)
			{
				if (noticeCount == 0 || lastNoticeTime.AddDays(3) <= now)
				{
					var resolvedTimeUtc = targetCWREvent?.SL_EventTimeUtc ?? targetCWREvent?.SL_EventTime.ToUniversalBranchTime(incident.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, targetCWREvent.SL_GB_NKBranch)) ?? ZDateTime.Empty;
					incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, string.Empty, resolvedTimeUtc);
					incident.IM_CloseTimeUtc = ZDateTime.UtcNow;
					//No eConversation update or email to client on legacy system in this case - incident should be closed silently.
					incident.AddPublicSystemLogMessage("Support Incident Closed: " + incident.Lookups.StatusDispositionList.GetDescriptionFromCode(SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient));
					logText = "No response from client for " + expireDays + " days - incident closed";
					isProcessed = true;
				}
			}
			else if (lastAwaitingResponseTime.AddHours(expireDays * 24 - 72) <= now)
			{
				if (noticeCount == 0 || (noticeCount == 1 && lastNoticeTime.AddHours(48) <= now))
				{
					string noticeMessage = GetNoticeMessage(incident, "3 days");
					incident.AddSystemMessageToCustomer(noticeMessage);
					incident.Logs.AddLog(Events.IncidentEmailSent, NoticeSentLogReference);
					logText = "No response from client with 3 days until automatic closure - notice sent";
					isProcessed = true;
				}
			}
			else if (lastAwaitingResponseTime.AddHours(expireDays * 24 - 120) <= now && noticeCount == 0)
			{
				string noticeMessage = GetNoticeMessage(incident, "5 days");
				incident.AddSystemMessageToCustomer(noticeMessage);
				incident.Logs.AddLog(Events.IncidentEmailSent, NoticeSentLogReference);
				logText = "No response from client with 5 days until automatic closure - notice sent";
				isProcessed = true;
			}

			if (isProcessed)
			{
				incident.Factory.Save();
				ServiceLogger.Log(LogType.Information, string.Format("Processed incident {0}. {1}", incident.IM_IncidentNumber, logText));
			}
		}

		string GetNoticeMessage(SupportIncident incident, string timeLeft)
		{
			string result = new SupportIncidentParser(incident.Factory).Parse(incident, EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody);
			return result.Replace(TimeLeftMacro, timeLeft);
		}

		int GetDaysPendingCustomerToClosed(SupportIncident incident)
		{
			return incident.GetResolutionAndClosureBehaviour().DaysPendingCustomerToClosed;
		}

		const string TimeLeftMacro = "(*TimeLeft*)";
		const string NoticeSentLogReference = "Awaiting Response Notice Sent";
	}
}
