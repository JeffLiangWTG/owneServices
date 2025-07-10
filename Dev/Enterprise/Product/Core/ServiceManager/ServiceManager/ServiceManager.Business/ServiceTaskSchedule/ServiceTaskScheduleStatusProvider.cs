using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Integration.ServiceHostClient.Abstractions.Exceptions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Business
{
	public sealed class ServiceTaskScheduleStatusProvider : IServiceTaskScheduleStatusProvider
	{
		public ServiceTaskScheduleStatusProvider() : this(ObjectFactory.Get<IServiceHostsCache>())
		{
		}

		internal ServiceTaskScheduleStatusProvider(IServiceHostsCache serviceHostsCache)
		{
			statusStringProvider = new StatusStringProvider();
			this.serviceHostsCache = serviceHostsCache;
			taskBindings = new Lazy<IReadOnlyDictionary<string, (int Count, string List)>>(() =>
				HostedServiceBusinessObjectBindingsProvider
					.Instance
					.BusinessObjectBindings
					.GroupBy(binding => binding.ServiceTaskCode, binding => binding.Table)
					.ToDictionary(
						bindings => bindings.Key,
						bindings => (bindings.Count(), string.Join(",", bindings.Distinct(StringComparer.OrdinalIgnoreCase))),
						StringComparer.OrdinalIgnoreCase));
		}

		public IDictionary<string, TaskInstanceStatus> GetServiceStatus()
		{
			var result = QueryServiceStatusFromWebService()
				.GroupBy(status => status.TaskStatus.Code)
				.Select(grouping => (grouping.Key, instanceStatus: CombineHostStatuses(grouping, grouping.Key)))
				.Where(tuple => tuple.instanceStatus != null)
				.ToDictionary(tuple => tuple.Key, tuple => tuple.instanceStatus, StringComparer.OrdinalIgnoreCase);

			return result;
		}

		public TaskInstanceStatus GetServiceTaskStatus(string code)
		{
			var hostStatus = QueryServiceStatusFromWebService(taskCode: code);
			return CombineHostStatuses(hostStatus, code);
		}

		public void RequestTaskConfigurationReload(string taskCode)
		{
			InvokeActionOnTask(taskCode, "requestconfigreload");
		}

		public void SetServiceTaskNextRuntime(string taskCode, DateTime? nextRunTime, bool? revertingAfterFailedRun = null)
		{
			var nextRunTimeString = nextRunTime.HasValue
				? nextRunTime.Value.ToString(ServiceManagerConstants.JsonDateTimeFormat, CultureInfo.InvariantCulture)
				: string.Empty;
			var options = new List<string>
			{
				$"nextruntime={nextRunTimeString}"
			};
			if (revertingAfterFailedRun != null)
			{
				options.Add($"reverting={revertingAfterFailedRun.Value}");
			}
			InvokeActionOnTask(taskCode, "setnextruntime", options.ToArray());
		}

		public void WaitForPendingTasks(TimeSpan timeout)
		{
			var currentTasks = new List<Task>();
			while (tasks.TryTake(out var task))
			{
				currentTasks.Add(task);
			}
			Task.WaitAll(currentTasks.ToArray(), timeout);
		}

		void InvokeActionOnTask(string taskCode, string action, params string[] options)
		{
			if (string.IsNullOrWhiteSpace(taskCode))
			{
				return;
			}

			var hosts = serviceHostsCache.AvailableServiceHosts;
			foreach (var host in hosts)
			{
				var task = Task.Run(() =>
				{
					try
					{
						host.InvokeActionOnTasks(action, new[] { taskCode }, options);
					}
					catch (ServiceHostCommunicationException)
					{
						// do nothing, it should be an intermittent infrastructure issue
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("ServiceTaskScheduleStatusProvider|InvokeActionOnTask", "Problem invoking action", ex);
					}
				}); // Do not wait, we do not need to know the result.

				tasks.Add(task);
			}
		}

		IEnumerable<HostTaskStatus> QueryServiceStatusFromWebService(string taskCode = null)
		{
			var hosts = serviceHostsCache.AvailableServiceHosts;
			var stack = new ConcurrentStack<HostTaskStatus>();

			if (ObjectFactory.Get<ISystemDataRegistry>().ServiceTaskParallelWebRequestsEnabled)
			{
				Parallel.ForEach(hosts, host => QueryServiceStatus(stack, host, taskCode));
			}
			else
			{
				hosts.ForEach(host => QueryServiceStatus(stack, host, taskCode));
			}

			var result = stack.ToList();
			stack.Clear();

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
		static void QueryServiceStatus(ConcurrentStack<HostTaskStatus> incomingStack, IServiceHostClient serviceHostClient, string taskCode = null)
		{
			try
			{
				if (!string.IsNullOrEmpty(taskCode))
				{
					var result = serviceHostClient.GetTaskStatus(taskCode);
					if (result != null)
					{
						incomingStack.Push(new HostTaskStatus(serviceHostClient.HostName.Hostname, result));
					}
				}
				else
				{
					var result = serviceHostClient.GetTaskStatusList();
					if (result?.StatusList?.Any() ?? false)
					{
						incomingStack.PushRange(result.StatusList.Select(status => new HostTaskStatus(serviceHostClient.HostName.Hostname, status)).ToArray());
					}
				}
			}
			catch (ServiceHostCommunicationException)
			{
				// do nothing, it should be an intermittent infrastructure issue
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ServiceTaskScheduleStatusProvider|QueryServiceStatusFromWebService", "Problem requesting status", ex);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		TaskInstanceStatus CombineHostStatuses(IEnumerable<HostTaskStatus> thisTaskInstances, string code)
		{
			var instances = thisTaskInstances.ToList();
			if (instances.Count == 0)
			{
				return null;
			}

			var (bindingCount, bindingsList) = taskBindings.Value
				.TryGetValue(code, out var bindings)
				? bindings
				: (0, string.Empty);

			var tasksInQueue = instances
				.Where(e => e.TaskStatus.PlaceInQueue != -1);

			return new TaskInstanceStatus
			{
				StatusString = statusStringProvider.GetStatusString(instances.Max(t => t.TaskStatus.Status)),
				PlaceInQueueString = string.Join(";", tasksInQueue.OrderBy(task => task.TaskStatus.PlaceInQueue).Select(e => $"[{e.HostName}: {e.TaskStatus.PlaceInQueue}]")),
				SecondsInQueueString = string.Join(";", tasksInQueue.OrderBy(task => (int)task.TaskStatus.TimeInQueue.TotalSeconds).Select(e => $"[{e.HostName}: {(int)e.TaskStatus.TimeInQueue.TotalSeconds}]")),
				RunningCount = instances.Sum(e => e.TaskStatus.RunningCount),
				ProcessIDsString = string.Join(";", instances.Where(e => e.TaskStatus.RunnerPids.Any()).OrderBy(task => task.TaskStatus.RunnerPids.Min()).Select(e => $"[{e.HostName}: {string.Join(",", e.TaskStatus.RunnerPids)}]")),
				SecondsRunningString = string.Join(";", instances.Where(e => e.TaskStatus.RunningCount != 0).OrderBy(task => (int)task.TaskStatus.TimeRunning.TotalSeconds).Select(e => $"[{e.HostName}: {(int)e.TaskStatus.TimeRunning.TotalSeconds}]")),
				RegisteredOnHosts = string.Join(";", instances.Select(e => e.HostName)),
				BindingCount = bindingCount,
				BindingTypes = bindingsList,
				NextRunTime = instances.Max(t => t.TaskStatus.NextRunTime),
				LastRunTime = instances.Max(t => t.TaskStatus.LastRunTime),
				LastErrorTime = instances.Max(t => t.TaskStatus.LastErrorTime),
				ErrorCountLast24Hours = instances.Sum(t => t.TaskStatus.ErrorCountLast24Hours),
			};
		}

		public void Dispose()
		{
			WaitForPendingTasks(TimeSpan.FromMinutes(1));
		}

		readonly IServiceHostsCache serviceHostsCache;
		readonly StatusStringProvider statusStringProvider;
		readonly ConcurrentBag<Task> tasks = new ();
		readonly Lazy<IReadOnlyDictionary<string, (int Count, string List)>> taskBindings;
	}

	public class HostTaskStatus
	{
		public HostTaskStatus(string hostName, TaskStatusDTO taskStatus)
		{
			HostName = hostName;
			TaskStatus = taskStatus;
		}

		public string HostName;
		public TaskStatusDTO TaskStatus;
	}
}
