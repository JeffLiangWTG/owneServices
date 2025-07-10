using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public class ServiceTaskStatus
	{
		readonly IRunnableServiceTask serviceTask;
		readonly bool blockedAsUnregistered;

		public ServiceTaskStatus(IRunnableServiceTask serviceTask, IEnumerable<IRunnableServiceTask> taskQueueSnapshot, IEnumerable<ServiceTaskCodeWithRunnerProcessId> runnersSnapshot, IEnumerable<IHostedServiceBusinessObjectBinding> serviceTaskBizOBindings, bool blockedAsUnregistered)
		{
			this.serviceTask = serviceTask;
			this.blockedAsUnregistered = blockedAsUnregistered;
			PlaceInQueue = taskQueueSnapshot.IndexOf(serviceTask);
			RunnerPids = runnersSnapshot.Where(r => r.Item1.Equals(serviceTask.Code, StringComparison.OrdinalIgnoreCase)).Select(r => r.Item2).ToList();
			RunningCount = RunnerPids.Count();
			BindingTypes = serviceTaskBizOBindings.Where(binding => binding.ServiceTaskCode.Equals(serviceTask.Code, StringComparison.OrdinalIgnoreCase)).Select(binding => binding.Table).Distinct().ToList();
			Status = CalculateStatusId();
		}

		#region static information
		public string Code => serviceTask.Code;
		public string Branch => serviceTask.Branch;
		public IEnumerable<string> BindingTypes { get; }
		#endregion

		#region dynamic information
		public string Description => serviceTask.ScheduleDescription;
		public string Category => serviceTask.TypeOfDocument;
		public bool IsActive => serviceTask.IsActive;
		public int Status { get; }
		public IEnumerable<int> RunnerPids { get; }
		public int RunningCount { get; }
		public DateTime? NextRunTime => serviceTask.NextScheduledRunTime?.UtcDateTime;
		public TimeSpan SchedulePeriod => serviceTask.SchedulePeriodDuration;
		public int PlaceInQueue { get; }
		public Stopwatch TimeSinceLastEnqueued => serviceTask.TimeSinceLastEnqueued;
		public Stopwatch TimeSinceLastDequeued => serviceTask.TimeSinceLastDequeued;
		public Stopwatch TimeRunning => serviceTask.TimeRunning;
		public DateTime? LastRunTime => serviceTask.LastRunTime?.UtcDateTime;
		public DateTime? LastErrorTime => serviceTask.LastErrorTime?.UtcDateTime;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Not a duration")]
		public int ErrorCountLast24Hours => serviceTask.ErrorCountLast24Hours;
		#endregion

		int CalculateStatusId()
		{
			var status = new BitVector32();
			status[ServiceManagerHelper.IsInitializedMask] = true;
			status[ServiceManagerHelper.IsActiveMask] = serviceTask.IsActive;
			status[ServiceManagerHelper.IsRunningMask] = RunningCount > 0;
			status[ServiceManagerHelper.IsBlockedMask] = blockedAsUnregistered;
			status[ServiceManagerHelper.IsLastRunFailedMask] = serviceTask.IsLastRunFailed;
			return status.Data;
		}
	}
}
