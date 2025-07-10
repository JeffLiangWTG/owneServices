using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using Res = ZClientEDI.Res;

[assembly: HostedService(
	"ICP",
	"Incident Close Prompt Task Creator",
	"CSP",
	typeof(Enterprise.Client.EDI.IncidentManager.BatchProcessor.IncidentClosePromptServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "15minutes",
	DefaultScheduleRunEvery = "1hour",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true)
]

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	class IncidentClosePromptServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var taskProcessTime = ZDateTime.UtcNow;
				var incidents = GetOpenIncidentsWithNoOpenTasks(taskProcessTime);

				foreach (var incident in incidents)
				{
					token.ThrowIfCancellationRequested();
					try
					{
						SupportIncident loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
						if (!loadedIncident.IsGroupControlled)
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
						ErrorReporter.ReportOnce("Failed to create new task for Incident", FormattableString.Invariant($"{incident.IM_IncidentNumber} {ex.Message}"), ex);
					}
				}
				EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taskProcessTime.ToDateTime());
			}
		}

		IEnumerable<SupportIncident> GetOpenIncidentsWithNoOpenTasks(ZDateTime runTimeUtc)
		{
			var filter = new ZDBOnlyQuery(typeof(SupportIncident));
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@MinDateUtc", EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value, IncidentMainSchema.IM_SystemLastEditTimeUtc);
			parameters.Add("@UtcNow", runTimeUtc, IncidentMainSchema.IM_SystemLastEditTimeUtc);
			filter.AddFilterAndZSQLParameterCollection(@"
IM_PK IN
(
	SELECT 
		IM_PK
	FROM 
		dbo.IncidentMain 
		LEFT JOIN dbo.ProcessTasks ON P9_ParentID = IM_PK AND P9_Type NOT IN ('MIL', 'TRG', 'EXC') AND P9_Status NOT IN ('CLS','CAN')
	WHERE 
		P9_PK IS NULL
		AND IM_Status NOT IN ('CAN', 'CLS')
		AND IM_ResolutionCode NOT IN ('UPO', 'UDO')
		AND IM_IncidentType = 'INC'
		AND IM_SystemLastEditTimeUtc > @MinDateUtc
		AND IM_SystemLastEditTimeUtc < @UtcNow
		AND IM_PK NOT IN
		(
			SELECT IM_PK FROM
			dbo.IncidentMain
			JOIN dbo.GenPivot P1 ON IM_PK = P1.XX_Relation1ID
			JOIN dbo.WorkItem ON WKI_PK = P1.XX_Relation2ID
			WHERE WKI_Status NOT IN ('CLS','CAN')

			UNION ALL

			SELECT IM_PK FROM
			dbo.IncidentMain
			JOIN dbo.GenPivot P2 ON IM_PK = P2.XX_Relation2ID
			JOIN dbo.WorkItem ON WKI_PK = P2.XX_Relation1ID
			WHERE WKI_Status NOT IN ('CLS','CAN')
		)
)
", parameters);
			return new BusinessObjectFactory().Load<SupportIncident>(filter);
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		string CloseIncidentMessage => Res.GetString("4328cdef-15a3-4bed-b8b5-1e578d761489", "Review Incident status and close");

		void ProcessIncident(SupportIncident incident)
		{
			var isProcessed = false;
			string logText = null;

			if (incident.WorkflowItems.Any() || incident.RelatedWorkItems.Any())
			{
				var newTaskSequence = 5;
				var completedTaskList = new SortedDictionary<ZDateTime, ProcessTask>();
				var latestCompletedTaskOnIncident = incident.WorkflowItems.Cast<ProcessTask>().Where(x => !(x.P9_GS_NKAssignedStaffMember.IsEmpty && x.P9_G4_RequiredCapability.IsEmpty)).OrderByDescending(x => x.P9_CompletedTimeUtc).ThenByDescending(x => x.P9_Sequence).FirstOrDefault();

				if (latestCompletedTaskOnIncident != null)
				{
					completedTaskList.Add(latestCompletedTaskOnIncident.P9_CompletedTimeUtc, latestCompletedTaskOnIncident);
					newTaskSequence = latestCompletedTaskOnIncident.P9_Sequence + 5;
				}

				foreach (var task in incident.RelatedWorkItems.Cast<NewWorkItem>().Select(x => x.WorkflowItems.Cast<ProcessTask>().Where(z => !(z.P9_GS_NKAssignedStaffMember.IsEmpty && z.P9_G4_RequiredCapability.IsEmpty)).OrderByDescending(y => y.P9_CompletedTimeUtc).ThenByDescending(y => y.P9_Sequence).FirstOrDefault()))
				{
					if (task != null && !completedTaskList.ContainsKey(task.P9_CompletedTimeUtc))
					{
						completedTaskList.Add(task.P9_CompletedTimeUtc, task);
					}
				}

				var latestTask = completedTaskList.LastOrDefault().Value;

				if (latestTask != null)
				{
					var newTask = incident.WorkflowItems.Tasks.AddNew();
					newTask.P9_GS_NKAssignedStaffMember = latestTask.P9_GS_NKAssignedStaffMember;
					newTask.P9_G4_RequiredCapability = latestTask.P9_G4_RequiredCapability;
					newTask.P9_Description = CloseIncidentMessage;
					newTask.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf(@"This Incident has an open status and all workflow tasks were closed or cancelled.

To transition the Incident to a closed state, close this task and when prompted decide if its appropriate to send the Incident closure email notification to the customer.Cancel the email notification if its not appropriate."));
					newTask.P9_Type = "INV";
					newTask.P9_Sequence = newTaskSequence;
					newTask.P9_EstDuration = TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(0, 10, 0));

					var jobHeader = ProcessJobHeaderProvider.GetForParent(incident, incident.Factory);
					var workflows = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Where(x => x.FH_CompletionStatement == CloseIncidentMessage);
					if (workflows.IsNullOrEmpty())
					{
						var newWorkflow = jobHeader.ProcessHeaders.AddNew();
						newWorkflow.FH_CompletionStatement = CloseIncidentMessage;
						newTask.P9_FH_ProcessHeader = newWorkflow.PK;
					}
					else
					{
						newTask.P9_FH_ProcessHeader = workflows.FirstOrDefault().PK;
					}

					isProcessed = true;
					logText = FormattableString.Invariant($"Created task: Sequence:{newTask.P9_Sequence} Staff:{newTask.P9_GS_NKAssignedStaffMember}");
				}
			}

			if (isProcessed)
			{
				incident.Factory.Save();
				ServiceLogger.Log(LogType.Information, FormattableString.Invariant($"Processed incident {incident.IM_IncidentNumber}. {logText}"));
			}
		}
	}
}
