using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.TimeEngineScheduler.ServiceTask;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using BusinessConstants = Enterprise.TimeEngineScheduler.Integration.Constants;

[assembly: HostedService(
	ActionSchedulerTask.Code,
	ActionSchedulerTask.Description,
	ActionSchedulerTask.Category,
	typeof(ActionSchedulerTask),
	IsMandatory = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute",
	DefaultScheduleStartAtLocal = "0seconds"
	)]

[assembly: HostedServiceQueueProvider(ActionSchedulerTask.Code, ActionSchedulerTask.Description, typeof(ActionSchedulerTaskQueue))]
namespace Enterprise.TimeEngineScheduler.ServiceTask
{
	public class ActionSchedulerTaskQueue : IHostedServiceQueueProvider
	{
		public QueueResult QueueResult
		{
			get
			{
				var sqlQuery = @"
select count(*), isnull(max(datediff(second, TAS_ExecutionDateTimeUtc, @utcNow)), 0)
from dbo.TimeActionSchedule
where TAS_ExecutionDateTimeUtc <= @utcNow and TAS_ExecutionStatus = 'SCH'";

				var result = QueueResult.Error;
				Db.Connection.ExecuteReader(
					sqlQuery,
					parameters => parameters.AddParameter("@utcNow", System.Data.SqlDbType.DateTime, ZDateTime.UtcNow),
					reader => result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1)))
				);
				return result;
			}
		}
	}

	public class ActionSchedulerTask : ServiceProviderImpl
	{
		public const string Code = "TAS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
		public const string Description = "Task Action Scheduler";
		public const string Category = "BMS";

		protected internal class Processor : ManagedBatchProcessor<IActionSchedule>
		{
			public Processor(IActionScheduleProvider provider, ILogger logger)
			{
				this.provider = provider;
				this.logger = logger;
			}
			readonly IActionScheduleProvider provider;
			readonly ILogger logger;

			public override int BatchSize => ObjectFactory.Get<IBMSRegistry>().TaskActionSchedulerBatchSize;
			protected override ZQuery GetQuery() => provider.GetRunnableSchedulesQuery();
			protected override ZQuery GetSingularQuery(IActionSchedule row) => new ZQuery(TimeActionScheduleSchema.PK, row.PK);
			protected override IList<IActionSchedule> LoadBatchCore(BusinessObjectFactory factory, ZQuery query) => factory.Load<IActionSchedule>(query);

			protected override void MarkRowAsBadCore(INotifications notifications, IActionSchedule row, BusinessObjectFactory factory)
			{
				row.ExecutionStatus = BusinessConstants.TimeActionScheduleStatus.Failed;
			}

			protected override IBatchGrouper GetGrouper() => ScheduleGrouper.Instance;

			struct BranchAndDepartment
			{
				public BranchAndDepartment(ZGuid branch, ZGuid department)
				{
					Branch = branch;
					Department = department;
				}
				public ZGuid Branch { get; }
				public ZGuid Department { get; }

				public override bool Equals(object obj) => obj is BranchAndDepartment o && o.Branch.Equals(Branch) && o.Department.Equals(Department);
				public override int GetHashCode() => Branch.GetHashCode() ^ Department.GetHashCode();
			}

			class ScheduleGrouper : IBatchGrouper
			{
				internal static ScheduleGrouper Instance { get; } = new ScheduleGrouper();

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
				public IEnumerable<(BusinessObjectFactory groupedBatchFactory, IList<IActionSchedule> groupedBatch, object groupKey)> GetGroups(BusinessObjectFactory factory, IList<IActionSchedule> batch)
				{
					BusinessObjectFactory GetFactory(string name) => new BusinessObjectFactory
					{
						NameForDebugging = FormattableString.Invariant($"{nameof(ActionSchedulerTask)}.{name}"),
						RefreshEnabled = false,
					};
					IActionSchedule SwitchFactory(BusinessObjectFactory f, IActionSchedule s) => (IActionSchedule)f.ImportFromAnotherFactory((BusinessObject)s);

					// For rows where a failure has occurred, we process one at a time
					foreach (var row in batch.Where(b => b.RetryAttempts > 0))
					{
						var singleFactory = GetFactory("Single");
						var singleFactoryRow = SwitchFactory(singleFactory, row);
						yield return (singleFactory, new[] { singleFactoryRow }, new BranchAndDepartment(singleFactoryRow.ExecutionBranch, singleFactoryRow.ExecutionDepartment));
					}

					// Everything else is done in batches
					foreach (var group in batch.Where(b => b.RetryAttempts == 0).GroupBy(s => new BranchAndDepartment(s.ExecutionBranch, s.ExecutionDepartment)))
					{
						var groupFactory = GetFactory("Group");
						var groupedBatch = group.Select(s => SwitchFactory(groupFactory, s)).ToList();
						yield return (groupFactory, groupedBatch, group.Key);
					}
				}
			}

			protected override IDisposable WithBatch(BusinessObjectFactory factory, IList<IActionSchedule> batch, object groupKey)
			{
				var branchAndDepartment = (BranchAndDepartment)groupKey;
				var branch = factory.Load<GlbBranch>(branchAndDepartment.Branch);
				var department = factory.Load<GlbDepartment>(branchAndDepartment.Department);

				if (branch != null && department != null)
				{
					return Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), department.PK.ToGuid());
				}

				return ObjectFactory.Get<IBMSServiceTaskHelper>().GetTemporaryEnvironmentForServiceTaskBranch();
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
			protected override void ProcessRowCore(INotifications notifications, CancellationToken token, IActionSchedule schedule, BusinessObjectFactory factory, (int index, int count) batchPosition)
			{
				schedule.RetryAttempts += 1;
				if (schedule.RetryAttempts > BusinessConstants.MaxRetryAttempts)
				{
					notifications.AddError(FormattableString.Invariant($"Schedule {schedule.PK} for {schedule.ActionCode} was loaded with more than the maximum number of retry attempts!"));
					schedule.ExecutionResult = "Exceeded maximum retry attempts";
					schedule.ExecutionStatus = BusinessConstants.TimeActionScheduleStatus.Failed;
					return;
				}

				var action = GetAction(schedule.ActionCode);
				if (action == null)
				{
					schedule.ExecutionStatus = BusinessConstants.TimeActionScheduleStatus.Failed;
					schedule.ExecutionResult = "Action handler not registered";
					notifications.AddError(FormattableString.Invariant($"Error running scheduled action {schedule.ActionCode}. Action handler not registered."));
					return;
				}

				var result = action.Execute(token, factory, logger, schedule.TargetPK, schedule.TargetTableCode, schedule.JsonParameter, schedule.SystemCreateTimeUtc);
				notifications.AddInfo(FormattableString.Invariant($"Executed scheduled action {schedule.ActionCode}"));
				schedule.ExecutionStatus = BusinessConstants.TimeActionScheduleStatus.Closed;
				schedule.ExecutionResult = result;
			}

			ISchedulerActionFactory ActionFactory => actionFactory ?? (actionFactory = ObjectFactory.Get<ISchedulerActionFactory>(nameof(ISchedulerActionFactory)));
			ISchedulerActionFactory actionFactory;

			internal ISchedulerAction GetAction(string code) => IDictionaryExtensions.GetOrAdd(actions, code, () => ActionFactory.GetSchedulerAction(code));

			readonly Dictionary<string, ISchedulerAction> actions = new Dictionary<string, ISchedulerAction>();
		}

		protected virtual Processor GetProcessor(IActionScheduleProvider scheduleProvider, ILogger logger) => new Processor(scheduleProvider, logger);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task logging")]
		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Debug("Started executing scheduled actions");
			var processor = GetProcessor(ScheduleProvider, ServiceLogger);
			processor.Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
			ServiceLogger.Debug("Finished executing scheduled actions");
		}

		internal bool SaveAction(IActionSchedule schedule, string state, string result)
		{
			schedule.ExecutionStatus = state;
			schedule.ExecutionResult = result;

			var shouldRetry = false;
			if (schedule.ExecutionStatus == BusinessConstants.TimeActionScheduleStatus.Failed)
			{
				schedule.RetryAttempts++;
				if (schedule.RetryAttempts < BusinessConstants.MaxRetryAttempts)
				{
					schedule.ExecutionStatus = BusinessConstants.TimeActionScheduleStatus.Scheduled;
					schedule.ExecutionDateTimeUtc = ZDateTime.UtcNow + BusinessConstants.RetryAfterPeriod;
					shouldRetry = true;
				}
			}

			ScheduleProvider.ChangeState(schedule);
			return shouldRetry;
		}

		#region Implementation

		IActionScheduleProvider ScheduleProvider => scheduleProvider ?? (scheduleProvider = ObjectFactory.Get<IActionScheduleProvider>());
		IActionScheduleProvider scheduleProvider;

		#endregion
	}
}
