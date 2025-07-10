using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	StaggeredReleaseDelayCalculatorTask.Code,
	StaggeredReleaseDelayCalculatorTask.Description,
	BMSServiceTaskBase.Category,
	typeof(StaggeredReleaseDelayCalculatorTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.BufferManagement.Business
{
	public class StaggeredReleaseDelayCalculatorTask : BMSServiceTaskBase
	{
		public const string Code = "BMD";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Staggered Release Delay Calculator";

		protected override string TaskDescription
		{
			get { return Description; }
		}

		[HostedServiceRequirement]
		public static string CheckCapacityCalculationsNotDisabled()
		{
			return CheckCapacityCalculationsNotDisabledCore();
		}

		[HostedServiceRequirement]
		public static string CheckSufficientWorkflowModeEnabled()
		{
			return CheckBufferManagementWorkflowModeOrBetterEnabledCore();
		}

		protected override bool ShouldNotRunIfCapacityCalculationsDisabled => true;

		protected override bool IsSufficientWorkflowManagementModeEnabled => BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled;

		protected override void RunTaskCore(CancellationToken token)
		{
			var query = GetWorkflowsQuery();
			var logger = new BatchLogger(ServiceLogger, Description);

			BusinessObject lastBizoRead = null;
			IEnumerable<ProcessHeader> workflowBatch = null;

			var reader = new FilteredBusinessObjectReaderWithLogger(FactoryProvider, query, typeof(ProcessHeader), logger) { BatchSize = BMSRegistry.Instance.StaggeredReleaseCalculatorBatchSize.Value, SaveBeforeLoadNextEnabled = true };

			using (new BMSServiceTaskHelper().GetTemporaryEnvironmentForServiceTaskBranch())
			{
				try
				{
					while ((workflowBatch = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<ProcessHeader>()).Any())
					{
						token.ThrowIfCancellationRequested();
						ProcessBatch(workflowBatch.ToArray());
						lastBizoRead = workflowBatch.LastOrDefault();
					}
				}
				catch (ZSaveConcurrencyException ex)
				{
					ServiceLogger.Log(LogType.Warning, "Concurrency error: " + ex.Message); // Service Task Logging
				}
			}
		}

		void ProcessBatch(ProcessHeader[] batch)
		{
			foreach (var workflow in batch)
			{
				try
				{
					workflow.CalculateReleaseDelayExpiry();
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}
					else
					{
						ReportFailedWorkflow(workflow, ex);
					}
				}
			}
		}

#if DEBUG
		public
#endif
		BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (factoryProvider == null)
				{
					factoryProvider = new BusinessObjectFactoryProvider();
				}
				return factoryProvider;
			}
		}
		BusinessObjectFactoryProvider factoryProvider;

		void ReportFailedWorkflow(ProcessHeader failedWorkflow, Exception ex)
		{
			var message = string.Format(CultureInfo.InvariantCulture,
(NoResString)@"Failed to process workflow:
Job: {0}
Description: {1}
Error details:
{2}", // Service task log
failedWorkflow.ProviderJobDescription,
failedWorkflow.FH_CompletionStatement,
ex.ToString());

			ServiceLogger.Log(LogType.Error, message);
		}

		static ZQuery GetWorkflowsQuery()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
				FH_PK in
				(
					SELECT FH_PK 
					FROM dbo.ProcessHeader
					WHERE 
						FH_Status = '{0}'
						AND FH_FC_CurrentComponent IS NOT NULL
						AND FH_StaggeredReleaseDelayExpiry IS NOT NULL
					UNION ALL
					SELECT ph.FH_PK 
					FROM dbo.ProcessHeader AS ph
					LEFT JOIN dbo.BMComponent AS component ON component.FC_PK = ph.FH_FC_CurrentComponent
					LEFT JOIN dbo.ProcessHeader AS parent ON parent.FH_PK = ph.FH_FH_ParentHeader AND parent.FH_Status IN ('OPN', 'BLK')
					LEFT JOIN dbo.ProcessHeaderLink AS link ON link.FP_FH_HeaderTo = ph.FH_PK
					LEFT JOIN dbo.ProcessHeader AS ph2 ON ph2.FH_PK = link.FP_FH_HeaderFrom
					LEFT JOIN dbo.BMComponent AS component2 ON component2.FC_PK = ph2.FH_FC_CurrentComponent
					WHERE 
						ph.FH_Status = '{0}'
						AND ph.FH_FC_CurrentComponent IS NOT NULL
						AND
						(
							component.FC_Type = '{1}'
							AND component2.FC_Type = '{2}'
							AND
							(
								parent.FH_TimeDelayFactor > 0 
								OR parent.FH_TimeDelayMinutes > 0
								OR link.FP_TimeDelayFactor > 0 
								OR link.FP_TimeDelayMinutes > 0
							)
						)
				)
				",
				WorkflowStatusList.Codes.Blocked, // Staggered release is only relevant on workflows with open prereqs
				BMComponentTypeList.Codes.Bucket, // No need to process workflows already released to a buffer
				BMComponentTypeList.Codes.Buffer); // Pre-req should already be released, otherwise there can't be a valid release delay calculated

			return new ZDBOnlyQuery(typeof(ProcessHeader)).AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());
		}
	}
}
