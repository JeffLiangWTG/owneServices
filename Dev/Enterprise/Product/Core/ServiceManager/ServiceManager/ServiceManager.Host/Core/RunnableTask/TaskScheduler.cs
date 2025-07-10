using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Types;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class TaskScheduler : ITaskScheduler
	{
		public static readonly TimeSpan FailedScheduleRetryTime = TimeSpan.FromSeconds(60);

		static class XmlDataSerializer
		{
			public static IEnumerable<(string taskCode, ZDateTime nextRuntime)> ReadTasks(Stream fileStream, IDatabaseAspectVersions aspectVersions)
			{
				try
				{
					var xmlDoc = XDocument.Load(fileStream);
					var result = xmlDoc.Root.Elements()
						.FirstOrDefault(e => e.Name == Constants.AllTasksNode
										&& e.Attribute("version")?.Value == Constants.GetLatestXMLFileVersion(aspectVersions))
						?.Elements()
						.Select(taskNode =>
						(
							taskCode: taskNode.Attributes().FirstOrDefault((a) => a.Name == Constants.TaskCodeAttribute)?.Value,
							nextRunTimeString: taskNode.Attributes().FirstOrDefault((a) => a.Name == Constants.TaskNextRunTimeAttribute)?.Value
						))
						.Where(tuple => tuple.taskCode != null && tuple.nextRunTimeString != null)
						.Select(tuple =>
						(
							tuple.taskCode,
							parsed: ZDateTime.TryParseExact(tuple.nextRunTimeString, out var nextRunTime, Constants.XMLDateTimeFormat),
							nextRunTime: new ZDateTime(nextRunTime, DateTimeKind.Utc)
						))
						.Where(tuple => tuple.parsed)
						.Select(tuple => (tuple.taskCode, tuple.nextRunTime))
						.ToList();
					return result ?? Enumerable.Empty<(string, ZDateTime)>();
				}
				catch (XmlException)
				{
					// File was not valid, ignore.
					return Enumerable.Empty<(string, ZDateTime)>();
				}
			}

			public static void WriteTasks(IEnumerable<Tuple<string, ZDateTime>> data, Stream fileStream, IDatabaseAspectVersions aspectVersions)
			{
				var allTasksNode = new XElement(Constants.AllTasksNode,
					new XAttribute("version", Constants.GetLatestXMLFileVersion(aspectVersions)));
				foreach (var task in data)
				{
					var taskCodeAttribute = new XAttribute(Constants.TaskCodeAttribute, task.Item1);
					var nextRuntimeString = task.Item2.ToString(Constants.XMLDateTimeFormat, CultureInfo.InvariantCulture);
					var taskNextRuntimeAttribute = new XAttribute(Constants.TaskNextRunTimeAttribute, nextRuntimeString);
					var taskNode = new XElement(Constants.TaskNode, taskCodeAttribute, taskNextRuntimeAttribute);
					allTasksNode.Add(taskNode);
				}

				var rootNode = new XElement(Constants.RootNode, allTasksNode);
				var xmlDoc = new XDocument(rootNode);
				xmlDoc.Save(fileStream);
			}
		}

		readonly IAllTasksConsumer allTasks;
		readonly IHostLogger hostLogger;
		readonly ITransactionAdapter transactionAdapter;
		readonly IProductRegistrationPeriodicChecker regChecker;
		readonly IBackgroundDataSaver dataSaver;
		readonly IClientHostedServiceAttributeProvider hostedServiceAttributeProvider;
		readonly IDatabaseAspectVersions aspectVersions;

		bool disposed;

		internal static class Constants
		{
			public const string PersistenceFileName = "TaskScheduler.xml";
			public const string RootNode = "Root";
			public const string AllTasksNode = "AllTasks";
			public const string TaskNode = "Task";
			public const string TaskCodeAttribute = "Code";
			public const string TaskNextRunTimeAttribute = "NextRunTime";
			public const string XMLDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

			public static string GetLatestXMLFileVersion(IDatabaseAspectVersions aspectVersions)
			{
				return $"{aspectVersions.SchemaVersion}.{aspectVersions.TransformationVersion}";
			}
		}

		public TaskScheduler(
			ITransactionAdapter transactionAdapter,
			IAllTasksConsumer allTasks,
			IHostLogger hostLogger,
			IProductRegistrationPeriodicChecker regChecker,
			IBackgroundDataSaverFactory dataSaverFactory,
			IErrorReporterProxy errorReporterProxy,
			IDatabaseAspectVersions aspectVersions,
			IClientHostedServiceAttributeProvider hostedServiceAttributeProvider)
			: this(transactionAdapter, allTasks, hostLogger, regChecker, dataSaverFactory, null, TimeSpan.FromMinutes(5), errorReporterProxy, aspectVersions, hostedServiceAttributeProvider)
		{
		}

		internal TaskScheduler(ITransactionAdapter transactionAdapter,
			IAllTasksConsumer allTasks,
			IHostLogger hostLogger,
			IProductRegistrationPeriodicChecker regChecker,
			IBackgroundDataSaverFactory dataSaverFactory,
			string filePath,
			TimeSpan saveFrequency,
			IErrorReporterProxy errorReporterProxy,
			IDatabaseAspectVersions aspectVersions,
			IClientHostedServiceAttributeProvider hostedServiceAttributeProvider)
		{
			this.transactionAdapter = transactionAdapter;
			this.allTasks = allTasks;
			this.hostLogger = hostLogger;
			this.regChecker = regChecker;
			dataSaverFactory = dataSaverFactory ?? new BackgroundDataSaverFactory();
			this.hostedServiceAttributeProvider = hostedServiceAttributeProvider;
			var persistFilePath = filePath ?? Path.Combine(ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName), Constants.PersistenceFileName);
			this.aspectVersions = aspectVersions;

			void HandleDataSaverException(Exception e)
			{
				using (Db.DisposableActionForDbConnection())
				{
					errorReporterProxy.ReportOnce("Task Scheduler Background Data Saver", e);
				}
			}

			dataSaver = dataSaverFactory.Create(persistFilePath, saveFrequency, BackupData, HandleDataSaverException);
			this.transactionAdapter.Committing += OnTransactionAdapterCommit;
		}

		void OnTransactionAdapterCommit(object sender, EventArgs e)
		{
			foreach (var task in allTasks.GetAll())
			{
				task.UpdateUnderlyingScheduleNextRunTime();
			}
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || disposed)
			{
				return;
			}

			disposed = true;
			transactionAdapter.Committing -= OnTransactionAdapterCommit;
			dataSaver.Dispose();
		}

		#endregion

		public void InitialiseTasks()
		{
			dataSaver.Load(fileStream =>
			{
				var tasks = XmlDataSerializer.ReadTasks(fileStream, aspectVersions);
				foreach (var (taskCode, nextRuntime) in tasks)
				{
					if (allTasks.TryGetByCode(taskCode, out var task)
						&& task.HasSchedule
						&& (task.NextScheduledRunTime == null || nextRuntime.ToNullableDateTimeOffset() > task.NextScheduledRunTime))
					{
						task.SetNextRunTime(nextRuntime.ToNullableDateTimeOffset(), SetNextRuntimeReason.LoadingFromLocalXml);
					}
				}
			});
		}

		public ITaskRunRequest ReconstructRequest(Func<IRunnableServiceTask, ITaskRunRequest> createRequest, string taskCode)
		{
			return allTasks.TryGetByCode(taskCode, out var target) ? createRequest(target) : null;
		}

		public TasksActionResultDTO RequestReloadOfTaskConfiguration(IEnumerable<TaskCodeDTO> taskCodes)
		{
			return ProcessTasksActionRequest(taskCodes, (knownTask) => knownTask.ReloadConfigurationAsync());
		}

		public TasksActionResultDTO SetNextRuntime(IEnumerable<TaskCodeDTO> taskCodes, DateTimeOffset? value, bool? revertingAfterFailedRun = null)
		{
			return ProcessTasksActionRequest(taskCodes, (knownTask) => knownTask.SetNextRunTime(value, SetNextRuntimeReason.SetRequestReceived, revertingAfterFailedRun));
		}

		public void TrackServiceTaskError(string taskCode)
		{
			if (allTasks.TryGetByCode(taskCode, out var task) && task.HasSchedule)
			{
				task.OnErrorReported();
			}
		}

		TasksActionResultDTO ProcessTasksActionRequest(IEnumerable<TaskCodeDTO> taskCodes, Action<IRunnableServiceTask> taskAction)
		{
			var results = new Dictionary<TaskCodeDTO, TaskActionResultDTO>();
			var knownTasks = allTasks.GetAll().Where(task => taskCodes.Any(code => code.Code.Equals(task.Code, StringComparison.OrdinalIgnoreCase))).ToList();
			foreach (var knownTask in knownTasks)
			{
				if (knownTask.HasSchedule)
				{
					taskAction(knownTask);
					results.Add(new TaskCodeDTO(knownTask.Code), TaskActionResultDTO.Succeeded);
				}
				else
				{
					results.Add(new TaskCodeDTO(knownTask.Code), TaskActionResultDTO.TaskDisabledOrInactive);
				}
			}

			foreach (var unknownTask in taskCodes.Where(code => !knownTasks.Exists(item => item.Code.Equals(code.Code, StringComparison.OrdinalIgnoreCase))))
			{
				results[new TaskCodeDTO(unknownTask.Code)] = TaskActionResultDTO.UnknownTask;
			}

			return new TasksActionResultDTO(results);
		}

		public TasksActionResultDTO ScheduleTasks(IEnumerable<TaskCodeDTO> taskCodes, bool echoes = true, TimeSpan? delay = null, string userCode = null)
		{
			var result = new Dictionary<TaskCodeDTO, TaskActionResultDTO>();
			var isLicensed = regChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			foreach (var taskCode in taskCodes)
			{
				var scheduleResult = ScheduleTask(taskCode, isLicensed, echoes, delay, userCode);
				result.Add(taskCode, scheduleResult);
			}

			return new TasksActionResultDTO(result);
		}

		TaskActionResultDTO ScheduleTask(TaskCodeDTO taskCode, bool isLicensed, bool echoes = true, TimeSpan? delay = null, string userCode = null)
		{
			var taskFound = allTasks.TryGetByCode(taskCode.Code, out var task);
			if (!taskFound && hostedServiceAttributeProvider.GetClientHostedServiceAttribute(taskCode.Code) == null)
			{
				hostLogger.Log(LogLevel.Warning, $"Failed to Nudge task [{taskCode.Code}] due to reason: {TaskActionResultDTO.UnknownTask}");
				return TaskActionResultDTO.UnknownTask;
			}

			if (!taskFound || !task.HasSchedule || !task.IsActive)
			{
				hostLogger.Log(LogLevel.Debug, $"Failed to Nudge task [{taskCode.Code}] due to reason: {TaskActionResultDTO.TaskDisabledOrInactive}");
				return TaskActionResultDTO.TaskDisabledOrInactive;
			}

			if (!isLicensed)
			{
				hostLogger.Log(LogLevel.Warning, $"Failed to Nudge task [{taskCode.Code}] due to reason: {TaskActionResultDTO.RequiresProductRegistration}");
				return TaskActionResultDTO.RequiresProductRegistration;
			}

			if (delay != null)
			{
				task.EnqueueDelayed(delay.Value, echoes);
				return TaskActionResultDTO.EnqueuedDelayed;
			}

			if (!task.Enqueue(echoes))
			{
				return TaskActionResultDTO.EnqueuedAlready;
			}

			if (!string.IsNullOrEmpty(userCode))
			{
				task.Log(LogLevel.Information, $"{task.Info.Description} is scheduled manually by user {userCode}");
			}
			return TaskActionResultDTO.EnqueuedNow;
		}

		void BackupData(Stream fileStream)
		{
			var tasks = allTasks.GetAll().Where(t => t.HasSchedule).Select(t => new Tuple<string, ZDateTime>(t.Code, new ZDateTime(t.NextScheduledRunTime?.UtcDateTime)));
			XmlDataSerializer.WriteTasks(tasks, fileStream, aspectVersions);
		}

		public void Save()
		{
			transactionAdapter.Commit();
			dataSaver.DeleteFileOnDispose = true; // We have persisted scheduling data to the database, no need to keep the backup file.
		}
	}
}

