using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	// This is commonly known as Tagilator or Tailgator by BAS
	public class TagRuleRunner : BMServiceTaskProcessor
	{
		public TagRuleRunner(ILogger logger)
			: base(logger, new ServiceTaskFactoryProviderWrapper(logger, TagServiceTask.Code))
		{
		}

		#region ProcessHeaderProcessor Overrides

		protected override bool IsSufficientWorkflowManagementModeEnabled => BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled;

#if DEBUG

		public void Process()
		{
			Process(CancellationToken.None);
		}

#endif

		public override void ProcessCore(CancellationToken token)
		{
			var tagRules = GetTagRules();

			if (tagRules.Any())
			{
				var tagRulesToRun = GetTagRulesToRun(tagRules);
				var orderedValidTagRules = OrderTagRules(tagRulesToRun).ToArray();
				var connectionProvider = SecondaryServerConnectionProviderProvider.GetProvider();

				var branchPK = Guid.Empty;
				var departmentPK = Guid.Empty;
				IDisposable context = null;

				try
				{
					foreach (var rule in orderedValidTagRules)
					{
						token.ThrowIfCancellationRequested();
						var newBranchPK = rule.TGR_GB_Branch.ToGuid();
						var newDepartmentPK = rule.TGR_GE_Department.ToGuid();

						if (branchPK != newBranchPK || departmentPK != newDepartmentPK)
						{
							branchPK = newBranchPK;
							departmentPK = newDepartmentPK;
							context = SwitchBranchAndDepartment(context, branchPK, departmentPK);
						}

						var rowsChanged = ProcessRule(rule, connectionProvider);

						if (rowsChanged >= 0)
						{
							Logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Processed rule [{0}], rows modified [{1}], branch [{2}], department [{3}]", rule.TGR_Name, rowsChanged, Env.CurrentBranch.Code, Env.CurrentDepartment.Code));
						}

						rule.Schedule.UpdateNextScheduledDate();
					}
				}
				finally
				{
					context?.Dispose();
				}

				SaveRuleChanges(orderedValidTagRules);
			}
		}

		protected virtual IDisposable SwitchBranchAndDepartment(IDisposable context, Guid branchPK, Guid departmentPK)
		{
			context?.Dispose();
			return Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK, departmentPK);
		}

		static string GetBranchCode(TagRule rule)
		{
			return rule.TGR_GB_Branch.IsEmpty ? Env.CurrentBranch.Code : rule.Branch.GB_Code.ToString();
		}

		static string GetDepartmentCode(TagRule rule)
		{
			return rule.TGR_GE_Department.IsEmpty ? Env.CurrentDepartment.Code : rule.Department.GE_Code.ToString();
		}

		protected virtual TagRule[] GetTagRules()
		{
			var rulesFactory = FactoryProvider.Current;
			var query = new ZQuery(TagRuleSchema.TGR_IsActive, true) { OrderBy = TagRuleSchema.Constants.TGR_Name };

			return rulesFactory.Load<TagRule>(query);
		}

		protected virtual IEnumerable<TagRule> GetTagRulesToRun(TagRule[] tagRules)
		{
			LoadSchedules(tagRules);

			if (ShouldRunAllRules)
			{
				Logger.Log(LogType.Information, "Processing all active rules regardless of schedule recurrence and frequency throttling.");
				return tagRules;
			}
			else
			{
				Logger.Log(LogType.Information, "Processing active rules allowed by schedule recurrence and frequency throttling. To run all active rules regardless of schedule and throttling, run the service task using the command 'TAG -configString:ALL' (without quotes).");
			}

			var scheduledRules = tagRules.Where(x => x.Schedule.S5_NextScheduledPrintRunTimeUtc <= ZDateTime.UtcNow);

			var validTagRules = new List<TagRule>();
			var throttlingRegistryItem = BMSRegistry.Instance.TagRuleThrottling.Value;

			foreach (var rule in scheduledRules)
			{
				if (IsTagRuleValid(rule))
				{
					if (DoesRuleMeetThrottlingRequirements(rule, throttlingRegistryItem))
					{
						validTagRules.Add(rule);
					}
				}
				else
				{
					var isReaderEnabled = IsReaderConnectionOpen();
					var ruleError = isReaderEnabled ? $@"the rule is invalid:
{rule.TagTemplate.Notifications.ToUniqueMessageListString()}"
						: ReaderAccountDoesNotExistMessage; // Error messages for logging and reporting should be in English
					var errorMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)"Could not run rule [{0}], because {1}", rule.TGR_Name, ruleError); // Error messages for logging and reporting should be in English

					Logger.Log(LogType.Warning, errorMessage);

					if (isReaderEnabled)
					{
						var factoryName = (NoResString)"Invalid Tag Rule Factory"; // Error messages for logging and reporting should be in English
						LogFailureOnRuleAndMaybeDisableRule(rule, errorMessage, factoryName);
					}
				}
			}
			return validTagRules;
		}

		public bool ShouldRunAllRules { get; set; }

		void LoadSchedules(TagRule[] tagRules)
		{
			var rulePks = tagRules.Select(x => x.PK);
			var factory = tagRules.First().Factory;
			var query = new ZQuery(StmScheduleTaskSchema.S5_ParentID, rulePks);
			query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, TagRuleSchema.Constants.Prefix);

			var schedules = factory.Load<StmScheduleTask>(query);
			var scheduleParentPks = schedules.Select(x => x.S5_ParentID);
			var rulesWithMissingSchedules = tagRules.Where(x => !scheduleParentPks.Contains(x.PK));

			var rulesWithMissingSchedulesByBranch = rulesWithMissingSchedules.GroupBy(x => x.TGR_GB_Branch);

			foreach (var branchGroup in rulesWithMissingSchedulesByBranch)
			{
				using (DisposableEnvironment.ForBranch(branchGroup.Key.ToGuid()))
				{
					foreach (var rule in branchGroup)
					{
						_ = rule.Schedule; // Creates a new schedule.
					}
				}
			}
		}

		static bool IsTagRuleValid(TagRule rule)
		{
			var wasValidationSuspended = rule.Factory.IsValidationSuspended;

			if (wasValidationSuspended)
			{
				rule.Factory.ResumeValidation();
			}

			try
			{
				rule.ValidateForRunningRule();
			}
			finally
			{
				if (wasValidationSuspended)
				{
					rule.Factory.SuspendValidation();
				}
			}

			return !rule.HasErrors;
		}

#if DEBUG
		protected virtual
#endif
		bool DoesRuleMeetThrottlingRequirements(TagRule rule, TagRuleThrottlingHeader registryItem)
		{
			if (!registryItem.IsThrottlingEnabled || !rule.TGR_LastRunStartTimeUtc.IsValid)
			{
				return true;
			}

			var minutes = registryItem.GetRunIntervalMinutesForRunTime(rule.TGR_LastRunDurationInSeconds, rule.TGR_Name);

			return minutes == TagRuleThrottlingThreshold.NotSpecifiedValue || rule.TGR_LastRunStartTimeUtc.AddMinutes(minutes) <= ZDateTime.UtcNow;
		}

		static IEnumerable<TagRule> OrderTagRules(IEnumerable<TagRule> validTagRules)
		{
			return validTagRules
				.OrderBy(r => r.TagTemplate.Magnitude.TGM_RuleRunSequence)
				.ThenBy(GetBranchCode)
				.ThenBy(GetDepartmentCode)
				.ThenBy(r => r.TGR_Name);
		}

		void SaveRuleChanges(IEnumerable<TagRule> rules)
		{
			try
			{
				var firstRule = rules.FirstOrDefault();
				if (firstRule != null)
				{
					using (DisposableEnvironment.ForBranch(firstRule.TGR_GB_Branch.ToGuid()))
					{
						firstRule.Factory.Save();
					}
				}
			}
			catch (ZSaveConcurrencyException ex)
			{
				Logger.Log(LogType.Warning, "Concurrency error: " + ex.Message);
			}
		}

		#endregion

		#region ProcessRule

		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid if critical exceptions are excluded")]
		long ProcessRule(TagRule rule, IConnectionProvider connectionProvider)
		{
			var ruleRunStrategy = GetStrategy(rule, connectionProvider);

			try
			{
				return ProcessRuleCore(rule, ruleRunStrategy);
			}
			catch (DroppedDbConnectionException ex)
			{
				var errorMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)"Error occurred while processing rule [{0}]. Error message: {1}", rule.TGR_Name, ex.Message); // Error messages for logging and reporting should be in English
				Logger.Log(LogType.Error, errorMessage);
				return -1;
			}
			catch (SqlLockLostException ex)
			{
				LogInfrastructureError(rule, ex);
				return -1;
			}
			catch (InvalidFilterConfigurationException ex)
			{
				HandleException(rule, ex, isTimeout: false);
				return -1;
			}
			catch (Exception ex) when (!ex.IsCriticalException() && (IsExecuteNonQueryEnvironmentalError(ex) || IsConsideredInfrastructureErrorForTagRuleRunnerPurposes(ex)))
			{
				LogInfrastructureError(rule, ex);
				return -1;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is SqlException sqlException)
				{
					if (ZExceptionExtensions.IsInfrastructureDbError(sqlException))
					{
						LogInfrastructureError(rule, ex);
						return -1;
					}
					else
					{
						var isTimeout = sqlException.IsTimeoutExpired() || sqlException.IsLockTimeoutExpired();
						HandleException(rule, ex, isTimeout);
						return -1;
					}
				}

				var isReaderEnabled = IsReaderConnectionOpen();
				if (isReaderEnabled)
				{
					ErrorReporter.ReportOnce("Tag rule", ex);
					Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Tag rule [{0}] failed to complete.", rule.TGR_Name), ex);
				}
				else
				{
					var errorMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)"Error occurred while processing rule [{0}]. {1}. Error message: {2}", rule.TGR_Name, ReaderAccountDoesNotExistMessage, ex.Message); // Error messages for logging and reporting should be in English
					Logger.Log(LogType.Error, errorMessage, ex);
				}

				return -1;
			}
		}

		bool ShouldDisableFromTimeout(TagRule rule, string message)
		{
			var consecutiveFailuresBeforeDeactivation = BMSRegistry.Instance.ConsecutiveTagRuleTimeoutsBeforeDeactivation.Value;

			if (!rule.Notes.HasNotes)
			{
				return false;
			}

			var notes = rule.Notes.GetAllNotes().Cast<StmNote>().Where(n => n.ST_NoteText == message).ToArray();

			if (notes.Length < consecutiveFailuresBeforeDeactivation)
			{
				return false;
			}

			var mostRecentFailures = notes.OrderByDescending(n => n.ST_CreatedDateUtc)
											.Take(consecutiveFailuresBeforeDeactivation)
											.ToArray();

			var interval = mostRecentFailures[0].ST_CreatedDateUtc - mostRecentFailures[1].ST_CreatedDateUtc;
			var delta = new TimeSpan(0, 1, 0);

			for (var i = 1; i < consecutiveFailuresBeforeDeactivation - 1; ++i)
			{
				var current = mostRecentFailures[i].ST_CreatedDateUtc - mostRecentFailures[i + 1].ST_CreatedDateUtc;
				if (current < interval.Subtract(delta) || current > interval.Add(delta))
				{
					return false;
				}
			}

			return true;
		}

		void HandleException(TagRule rule, Exception ex, bool isTimeout)
		{
			var factoryName = GetType().Name + (NoResString)": Deactivated Rule Factory"; // Error messages for logging and reporting should be in English
			var ruleType = rule.TGR_IsSystem ? (NoResString)"system rule" : (NoResString)"rule"; // Error messages for logging and reporting should be in English
			var errorMessage = FormattableString.Invariant($"Error occurred while processing {ruleType} [{rule.TGR_Name}].{(isTimeout ? string.Empty : (NoResString)" Rule is now deactivated.")} Error message: {ex.Message}"); // Error messages for logging and reporting should be in English
			errorMessage = DbCommand.SanitizeExecuteAsReaderFlags(errorMessage);
			Logger.Log(LogType.Error, errorMessage);

			var shouldDisable = !isTimeout || ShouldDisableFromTimeout(rule, errorMessage);
			LogFailureOnRuleAndMaybeDisableRule(rule, errorMessage, factoryName, reportError: !isTimeout, shouldDisable: shouldDisable);

			if (!isTimeout || !shouldDisable)
			{
				return;
			}

			var deactivationMessage = FormattableString.Invariant($"Repeated timeout exceptions occurred while processing {ruleType} [{rule.TGR_Name}]. Rule is now deactivated.");
			Logger.Log(LogType.Error, deactivationMessage);
		}

		protected virtual long ProcessRuleCore(TagRule rule, TagRuleRunStrategyBase ruleRunStrategy)
		{
			rule.TGR_LastRunStartTimeUtc = ZDateTime.UtcNow;

			var sw = new Stopwatch();
			sw.Start();

			var rowsProcessed = ruleRunStrategy.Execute(rule);

			sw.Stop();

			rule.TGR_LastRunDurationInSeconds = (int)Math.Ceiling(sw.Elapsed.TotalSeconds);

			return rowsProcessed;
		}

		void LogInfrastructureError(TagRule rule, Exception ex)
		{
			var errorMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)"Environmental error occurred while processing rule [{0}]. Error message: {1}", rule.TGR_Name, ex.Message); // Error messages for logging and reporting should be in English
			Logger.Log(LogType.Warning, errorMessage);
		}

		static bool IsExecuteNonQueryEnvironmentalError(Exception ex)
		{
			return ex.TargetSite.Name == "ExecuteNonQuery"
				&& ex is InvalidOperationException; // The SqlConnection closed or dropped during a streaming operation
		}

		static bool IsConsideredInfrastructureErrorForTagRuleRunnerPurposes(Exception exception)
		{
			return exception is SqlException sqlException && (sqlException.IsDeadlock() || (sqlException.IsTimeoutExpired() && sqlException.Message.Contains((NoResString)"The timeout period elapsed during the post-login phase."))); // Error messages for logging and reporting should be in English
		}

		protected virtual TagRuleRunStrategyBase GetStrategy(TagRule rule, IConnectionProvider connectionProvider)
		{
			return TagRuleRunStrategyProvider.GetStrategy(rule, this, connectionProvider, Logger);
		}

		#endregion

		#region Error Handling

		bool IsReaderConnectionOpen()
		{
			using (var readerConnection = Db.NewExtraConnectionToMainDbWithReaderCredentials())
			{
				var testSql = "--";

				var open = true;

				try
				{
					readerConnection.ExecuteNonQuery(testSql);
				}
				catch (SqlException)
				{
					open = false;
				}
				return open;
			}
		}

		public void LogFailureOnRuleAndMaybeDisableRule(TagRule rule, string message, string factoryName, bool reportError = true, bool shouldDisable = true)
		{
			var ruleFactory = new BusinessObjectFactory { NameForDebugging = factoryName };
			var loadedRule = ruleFactory.Load<TagRule>(rule.PK);
			if (loadedRule != null)
			{
				using (DisposableEnvironment.ForBranch(loadedRule.TGR_GB_Branch.ToGuid()))
				{
					loadedRule.LogFailureOnRuleAndMaybeDisableRule(message, reportError: reportError, shouldDisable: shouldDisable);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error messages for logging and reporting should be in English")]
		const string ReaderAccountDoesNotExistMessage = "the Reader Account does not exist.";

		#endregion
	}
}
