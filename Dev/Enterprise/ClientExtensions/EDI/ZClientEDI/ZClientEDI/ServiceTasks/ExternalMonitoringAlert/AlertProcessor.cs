using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	class AlertProcessor
	{
		public AlertProcessor(ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}

		protected readonly ILogger serviceLogger;

		BaseExternalAlertProvider alertProvider;

		public void Run(CancellationToken token)
		{
			var wiseGridReportingConnectionString = EDIDataRegistry.Instance.ExternalMonitoringWiseGridReportingConnectionString.Value;
			if (!string.IsNullOrWhiteSpace(wiseGridReportingConnectionString))
			{
				using (var wiseGridReportingConnectionForConfig = GetDbConnection(wiseGridReportingConnectionString))
				using (var wiseGridReportingConnectionForReader = GetDbConnection(wiseGridReportingConnectionString))
				{
					alertRuleAndCapabilityManager = new AlertRuleAndCapabilityManager(wiseGridReportingConnectionForConfig);
					var alertDefaults = LoadAlertDefaults();

					alertProvider = BuildSqlCpuUsageAlertProvider(serviceLogger ,wiseGridReportingConnectionForReader, alertDefaults);

					var alertHandleResult = new AlertHandleResult();
					var alertRules = LoadAlertRules(alertProvider.RuleType);

					var capabilityJobsList = alertProvider.GetCapabilityJobsList(alertRules);
					if (capabilityJobsList != null && capabilityJobsList.Count > 0)
					{
						alertProvider.GetAlertAndProcess(capabilityJobsList?.Values.SelectMany(v => v.Item2), (alert) => ProcessAlert(alertRules, alert, alertHandleResult, capabilityJobsList, token));
					}

					serviceLogger.Information(FormattableString.Invariant($@"Finished processing {alertProvider.AlertTypeName} alerts.
Totally alerts processed: {alertHandleResult.TotalProcessedAlerts}
{alertProvider.AlertTargetName} generated: {alertHandleResult.TotalGeneratedTargets}
Tasks added: {alertHandleResult.TotalGeneratedTasks}
Ignored because same {alertProvider.AlertTargetName} created recently: {alertHandleResult.TotalRecentlyCreatedTargets}
Ignored because {alertProvider.AlertTargetName} still open: {alertHandleResult.TotalStillOpenTargets}
Ignored because {alertProvider.AlertTargetName} reached maximum amount per capability: {alertHandleResult.TotalReachedMaximumTargets}
Ignored because no matched rule: {alertHandleResult.TotalNoMatchedRuleTargets}
Failed to create: {alertHandleResult.TotalFailedToCreateTargets}"));
				}
			}
		}

		protected internal virtual BaseExternalAlertProvider BuildSqlCpuUsageAlertProvider(ILogger serviceLogger, DbConnection wiseGridReportingConnection, IEnumerable<AlertDefault> alertDefaults)
		{
			return new SqlCpuUsageAlertProvider(serviceLogger, wiseGridReportingConnection, alertDefaults);
		}

		ZGuid ObtainGlbCapabilityId(string capability, Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)> capabilityJob)
		{
			return capabilityJob[capability].Item4;
		}

		public const string DefaultCapabilityCode = "DDV";
		internal const string DefaultCapabilityMissingMessage = $"Default capability '{DefaultCapabilityCode}' does not exist";

		bool ProcessAlert(IList<AlertRule> alertRules, BaseAlert alert, AlertHandleResult alertHandleResult, Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)> capabilityJobsList, CancellationToken token)
		{
			try
			{
				AddAlertTargetResult result;
				alertHandleResult.TotalProcessedAlerts++;
				var matchedRule = alertProvider.DetermineAlertRule(alertRules, alert);

				if (matchedRule != null)
				{
					var glbCapablityId = ObtainGlbCapabilityId(matchedRule.Capability, capabilityJobsList);
					var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						(result, _) = CreateTargetAndUpdateAlertAndSource(alert, matchedRule, glbCapablityId, capabilityJobsList);
					}
				}
				else
				{
					//ok, something is not right here... what should I do?? pulling my hairs
					result = AddAlertTargetResult.NoRuleMatched;
				}

				var failedReason = string.Empty;
				switch (result)
				{
					case AddAlertTargetResult.TargetAdded:
						alertHandleResult.TotalGeneratedTargets++;
						break;
					case AddAlertTargetResult.TaskAdded:
						alertHandleResult.TotalGeneratedTasks++;
						break;
					case AddAlertTargetResult.AlreadyCreatedRecently:
						failedReason = "item recently created";
						alertHandleResult.TotalRecentlyCreatedTargets++;
						break;
					case AddAlertTargetResult.TargetStillOpen:
						failedReason = "item is still open";
						alertHandleResult.TotalStillOpenTargets++;
						break;
					case AddAlertTargetResult.ReachedMaxAmount:
						failedReason = "maxium amount reached";
						alertHandleResult.TotalReachedMaximumTargets++;
						break;
					case AddAlertTargetResult.NoRuleMatched:
						failedReason = "no matched rule";
						alertHandleResult.TotalNoMatchedRuleTargets++;
						break;
					default:
						break;
				}
				if (result != AddAlertTargetResult.TargetAdded && result != AddAlertTargetResult.TaskAdded)
				{
					serviceLogger.Debug(FormattableString.Invariant($@"Did not generate item for {alertProvider.AlertTypeName} alert because {failedReason}.
{GetAlertDetails(alert, alertProvider)}"));
				}
				if (token.IsCancellationRequested)
				{
					serviceLogger.Information("Stopped processing because of cancellation");
					return true;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("AlertProcessorTopLevelHandler", FormattableString.Invariant($"Something went wrong when generating {alertProvider} for {alertProvider.AlertTypeName} alert"), ex);
				serviceLogger.Error(FormattableString.Invariant($@"Error in processing {alertProvider.AlertTypeName} alert.
{GetAlertDetails(alert, alertProvider)}"), ex);
				alertHandleResult.TotalFailedToCreateTargets++;
			}
			return false;
		}

		static string GetAlertDetails(BaseAlert alert, BaseExternalAlertProvider alertProvider)
		{
			return FormattableString.Invariant($@"
Alert Name:{alert.AlertName ?? string.Empty}
Rule Id {alert.RuleId}
Path: {alert.Path ?? string.Empty}
Alert Id: {alert.Id}
Time Added: {(object)alert.TimeAdded ?? "-"}
Priority: {alert.Priority}
Severity: {alert.Severity}
Occurrences: {alert.Quantity}
Entity Name: {alert.EntityName}
{alertProvider.GetAdditionalFields(alert)}");
		}

		protected internal const string DefaultCriticalityName = "DefaultCriticality";
		protected internal const string FallbackCapabilityName = "FallbackCapability";
		protected internal const string ClientOrgIdName = "ClientOrgId";
		protected internal const string ClientCompanyIdName = "ClientCompanyId";
		protected internal const string ClientDbIdName = "ClientDbId";
		protected internal const string ClientContactIdName = "ClientContactId";

		protected internal virtual (
			AddAlertTargetResult result, string jobNumber
			) CreateTargetAndUpdateAlertAndSource(BaseAlert alert, AlertRule matchedRule, ZGuid glbCapabilityId, Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)> capabilityJobsList)
		{
			var boFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var (result, alertTarget, descriptivePath, task) = alertProvider.FindAlertTargetCreateIfNotExists(alert, matchedRule, boFactory, glbCapabilityId, capabilityJobsList);
			if (result == AddAlertTargetResult.TargetAdded || result == AddAlertTargetResult.TaskAdded)
			{
				var newTargetCreated = !alertTarget.IsInDatabase;
				if (task == null || newTargetCreated)
				{
					var notes = alertProvider.GetNotes(alert, matchedRule.IsDefault);
					CreatOrUpdateWorkflowTask(glbCapabilityId, (IWorkflowProvider)alertTarget, boFactory, descriptivePath, task, notes);
					boFactory.Save();
					var targetKey = alertProvider.GetTargetKey(alertTarget);
					AddAlertIncident(alert.RuleId, alert.Id, matchedRule.PK, targetKey);
					alertProvider.UpdateAlert(alert, targetKey, matchedRule.Owner);
					return (newTargetCreated ? AddAlertTargetResult.TargetAdded : AddAlertTargetResult.TaskAdded, targetKey);
				}
				else
				{
					return (AddAlertTargetResult.TargetStillOpen, null);
				}
			}
			return (result, null);
		}

		static void CreatOrUpdateWorkflowTask(ZGuid glbCapabilityId, IWorkflowProvider taskParent, BusinessObjectFactory factory, string descriptivePath, ProcessTask task, string notes)
		{
			if (task == null)
			{
				task = taskParent.WorkflowItems.Tasks.AddNew();
			}
			task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task.P9_Type = BaseExternalAlertProvider.ProcessTaskInvestigationType;
			task.P9_G4_RequiredCapability = glbCapabilityId;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Description = descriptivePath;
			task.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf(notes));

			var now = ZDateTime.UtcNow;
			var incidentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(taskParent, factory);
			var mostRecentHeader = incidentJobHeader?.ProcessHeaders.OrderByDescending(p => p.FH_SystemCreateTimeUtc).FirstOrDefault();
			if (mostRecentHeader == null || (mostRecentHeader.FH_SystemCreateTimeUtc.IsValid && mostRecentHeader.FH_SystemCreateTimeUtc.AddDays(1) < now))
			{
				mostRecentHeader = incidentJobHeader?.ProcessHeaders.AddNew();
			}
			if (mostRecentHeader != null)
			{
				if (mostRecentHeader.FH_CompletionStatement.IsEmpty)
				{
					mostRecentHeader.FH_CompletionStatement = now.ToShortDateString();
				}
				if (!mostRecentHeader.IsInDatabase)
				{
					factory.Save();
				}
			}

			task.P9_FH_ProcessHeader = mostRecentHeader?.PK ?? ZGuid.Empty;
			task.P9_EstDuration = TimeSpan.FromMinutes(DefaultEstimatedDurationInMinutes);
			task.P9_EstimateVariationFactor = TaskEstimateVariationFactor;
		}

		const int TaskEstimateVariationFactor = 1;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		const int DefaultEstimatedDurationInMinutes = 10;

		protected internal virtual void AddAlertIncident(Guid sourceRuleId, Guid sourceAlertId, Guid alertRuleId, string incidentNumber)
		{
			alertRuleAndCapabilityManager.AddAlertIncident(sourceRuleId, sourceAlertId, alertRuleId, incidentNumber);
		}

		protected internal virtual DbConnection GetDbConnection(string connectionString)
		{
			return AlertUtils.GetDbConnection(connectionString);
		}

		#region Alert Rules / Capabilities

		AlertRuleAndCapabilityManager alertRuleAndCapabilityManager;

		protected internal virtual IEnumerable<AlertDefault> LoadAlertDefaults()
		{
			return alertRuleAndCapabilityManager.LoadAlertDefaults();
		}

		protected internal virtual IList<AlertRule> LoadAlertRules(AlertRuleType type)
		{
			return alertRuleAndCapabilityManager.LoadAlertRules(type);
		}
		#endregion

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}

	public enum AddAlertTargetResult
	{
		None = 0,
		TargetAdded = 1,
		TaskAdded = 2,
		AlreadyCreatedRecently = 3,
		TargetStillOpen = 4,
		NoRuleMatched = 5,
		ReachedMaxAmount = 6
	}
}
