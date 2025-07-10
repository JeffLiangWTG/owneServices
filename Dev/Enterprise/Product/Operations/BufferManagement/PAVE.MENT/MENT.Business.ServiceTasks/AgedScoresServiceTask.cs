using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.PAVE.MENT.Business.ServiceTasks;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	AgedScoresServiceTask.Code,
	AgedScoresServiceTask.Description,
	BMSServiceTaskBase.Category,
	typeof(AgedScoresServiceTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.PAVE.MENT.Business.ServiceTasks
{
	public class AgedScoresServiceTask : BMSServiceTaskBase
	{
		public const string Code = BMConstants.AgedScoresServiceTaskCode; // Service Task Code
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Aged Scores Metrics";

		protected override string TaskDescription
		{
			get { return Description; }
		}

		volatile LogType serviceTaskState = LogType.Information;

#if DEBUG

		protected void RunTaskCore()
		{
			RunTaskCore(CancellationToken.None);
		}

#endif

		protected override void RunTaskCore(CancellationToken token)
		{
			var factory = new ReadOnlyBusinessObjectFactory
			{
				NameForDebugging = "AgedScoreMetricsServiceTask.QueriesToRunFactory",
				RefreshEnabled = false
			};
			var messages = new List<string>();
			using (new BMSServiceTaskHelper().GetTemporaryEnvironmentForServiceTaskBranch())
			{
				foreach (var query in GetAndOrderActiveAgedScoreQueries(factory))
				{
					var code = query.MAQ_Code;
					var continueAttemptingToRunQuery = true;
					var attemptNumber = 0;

					while (continueAttemptingToRunQuery)
					{
						attemptNumber++;
						token.ThrowIfCancellationRequested();

						var queryFactory = new BusinessObjectFactory
						{
							NameForDebugging = string.Format(CultureInfo.InvariantCulture, "AgedScoreMetricsServiceTask.ParallelQueryFactory.{0}.Attempt{1}", code, attemptNumber),
							RefreshEnabled = false
						};
						var importedQuery = (MENTAgedScoreQuery)queryFactory.ImportFromAnotherFactory(query);

						var inserter = new AgedScoreInserter();
						var queryResult = inserter.RunInsertSqlAndLogErrors(importedQuery);

						messages.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)"On attempt {0}: \r\n{1}", attemptNumber, queryResult.Messages)); // Service task logging

						if (queryResult.Result == InserterResult.Success)
						{
							importedQuery.QuerySchedule.UpdateNextScheduledDate();
							continueAttemptingToRunQuery = false;

							if (serviceTaskState != LogType.Error && attemptNumber > 1)
							{
								serviceTaskState = LogType.Warning;
							}
						}
						else if (queryResult.Result == InserterResult.Error && attemptNumber >= MENTConstants.MaxNumberOfQueryAttempts)
						{
							importedQuery.MAQ_IsFaulty = true;
							importedQuery.MAQ_IsActive = false;
							continueAttemptingToRunQuery = false;
							serviceTaskState = LogType.Error;
						}
						else if (queryResult.Result == InserterResult.InfrastructureFailure && attemptNumber >= MENTConstants.MaxNumberOfQueryAttempts)
						{
							continueAttemptingToRunQuery = false;

							if (serviceTaskState != LogType.Error)
							{
								serviceTaskState = LogType.Warning;
							}
						}

						queryFactory.Save();
					}
				}

				ReportAndLogErrors(messages);
			}
		}

		void ReportAndLogErrors(List<string> messages)
		{
			Argument.NotNull(messages, nameof(messages));

			if (serviceTaskState == LogType.Error)
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "All queries run. A query was marked faulty after the maximum number of attempts. Log Messages: \r\n{0}", string.Join("\r\n", messages))); // Service task logging
			}
			else if (serviceTaskState == LogType.Warning)
			{
				ServiceLogger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "All queries run. A query failed but was run successfully in a later attempt. Log Messages: \r\n{0}", string.Join("\r\n", messages))); // Service task logging
			}
			else
			{
				ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "All queries run successfully. Log Messages: \r\n{0}", string.Join("\r\n", messages))); // Service task logging
			}
		}

		static IEnumerable<MENTAgedScoreQuery> GetAndOrderActiveAgedScoreQueries(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));

			var filter = new ZQuery(MENTAgedScoreQuerySchema.MAQ_IsActive, true);
			filter?.AddToFilter(MENTAgedScoreQuerySchema.MAQ_IsFaulty, false);
			return factory.Load<MENTAgedScoreQuery>(filter).Where(q => q.QuerySchedule.S5_NextScheduledPrintRunTimeUtc <= ZDateTime.UtcNow && q.QuerySchedule.S5_IsActive).OrderByDescending(q => q.QuerySchedule.S5_NextScheduledPrintRunTimeUtc).ToArray();
		}

		[HostedServiceRequirement]
		public static string CheckSufficientWorkflowModeEnabled()
		{
			return CheckBufferManagementWorkflowModeOrBetterEnabledCore();
		}
	}
}
