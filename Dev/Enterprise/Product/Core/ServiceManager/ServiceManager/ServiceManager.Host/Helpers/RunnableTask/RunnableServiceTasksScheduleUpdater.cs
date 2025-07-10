using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class RunnableServiceTasksScheduleUpdater : IRunnableServiceTasksScheduleUpdater
	{
		public RunnableServiceTasksScheduleUpdater(IMemoryCache memoryCache, IHostLogger hostLogger, IHostRegistrySettings hostRegistry)
		{
			this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
			lastReloadTimeUtc = ZDateTime.TruncateSeconds(ZDateTime.UtcNow);
		}

		public void ReloadUpdatedFromDatabase(
			IEnumerable<IRunnableServiceTask> allTasks,
			IServiceTasksReloader updatedTasksReloader,
			ITaskScheduler taskScheduler)
		{
			if (hostRegistry.ServiceTaskConfigurationPollingInterval == TimeSpan.Zero)
			{
				return;
			}

			_ = allTasks ?? throw new ArgumentNullException(nameof(allTasks));
			_ = updatedTasksReloader ?? throw new ArgumentNullException(nameof(updatedTasksReloader));
			_ = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));

			memoryCache.AddOrGetExisting(
				MemoryCacheKey,
				() =>
				{
					ReloadUpdatedTaskSchedules();
					return new object();
				},
				hostRegistry.ServiceTaskConfigurationPollingInterval);

			void ReloadUpdatedTaskSchedules()
			{
				var currentTime = ZDateTime.TruncateSeconds(ZDateTime.UtcNow);
				var cachedTasks = allTasks
					.ToDictionary(x => x.Code, x => x, StringComparer.OrdinalIgnoreCase);

				var businessObjectFactory = new BusinessObjectFactory();
				var updatedSchedules = updatedTasksReloader
					.Reload(lastReloadTimeUtc.UtcToDateTimeOffset().ToDateTimeOffset())
					.GovernedTasks
					.Where(governedTask =>
						cachedTasks.TryGetValue(governedTask.Code, out var task)
						&& task.HasScheduleUpdates(governedTask));

				var updatedTaskCodes =
					updatedSchedules
						.Select(schedule => new TaskCodeDTO(schedule.Code))
						.ToArray();

				var result = taskScheduler.RequestReloadOfTaskConfiguration(updatedTaskCodes);
				LogResult(result);

				lastReloadTimeUtc = currentTime;
			}

			void LogResult(TasksActionResultDTO tasksActionResultDto)
			{
				foreach (var result in tasksActionResultDto.Results)
				{
					hostLogger.Log(LogLevel.Debug, $"Schedule reloaded due to external changes: [{result.Key}]");
				}
			}
		}

		const string MemoryCacheKey = nameof(RunnableServiceTasksScheduleUpdater);
		readonly IHostLogger hostLogger;
		readonly IMemoryCache memoryCache;
		ZDateTime lastReloadTimeUtc;
		readonly IHostRegistrySettings hostRegistry;
	}
}
