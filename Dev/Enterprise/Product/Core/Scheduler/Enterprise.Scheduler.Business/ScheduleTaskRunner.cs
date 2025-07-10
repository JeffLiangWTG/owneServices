using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Core.Constants;

namespace Enterprise.Scheduler.Business
{
	[Serializable]
	public class ScheduleTaskRunner : IScheduleTaskRunner
	{
		public void Process(
			ZString parentTableCode,
			bool useStmReportRun,
			INotifications notifications,
			CancellationToken token)
		{
			this.notifications = new NotificationsWrapper(notifications);
			RunStmScheduleTasks(parentTableCode, useStmReportRun, token, mustRunLocked: true);
		}

		public void ProcessWithoutLock(
			ZString parentTableCode,
			bool useStmReportRun,
			INotifications notifications,
			CancellationToken token)
		{
			this.notifications = new NotificationsWrapper(notifications);
			RunStmScheduleTasks(parentTableCode, useStmReportRun, token, mustRunLocked: false);
		}

		public QueueResult GetPendingJobsQueue(ZString parentTableCode)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = $"{nameof(ScheduleTaskRunner)}_{nameof(GetPendingJobsQueue)}" };
			var taskCollection = new StmScheduleTaskCollection(factory, parentTableCode);
			return taskCollection.GetPendingTasksQueue(ZDateTime.UtcNow);
		}

		void RunStmScheduleTasks(ZString parentTableCode, bool useStmReportRun, CancellationToken token, bool mustRunLocked)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(ScheduleTaskRunner) + "_InitialLoad" };
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(factory))
			{
				var tasks = LoadTasksToRun(factory, parentTableCode)
					.OfType<StmScheduleTask>()
					.OrderBy(x => x.S5_IsPrivate ? 1 : -1)
					.ThenBy(x => x.S5_NextScheduledPrintRunTimeUtc)
					.ThenByDescending(x => x.Priority)
					.ToArray();

				notifications.Notify(new InfoNotification(Res.GetString("2b1edc18-3235-441f-9295-15348de251fb", "{0} scheduled tasks to run", tasks.Length)));

				var newFactoryToLoadTaskFromDatabase = new BusinessObjectFactory { NameForDebugging = nameof(ScheduleTaskRunner) + "_SecondaryLoad" };
				for (var taskIndex = 0; taskIndex < tasks.Length; ++taskIndex)
				{
					token.ThrowIfCancellationRequested();
					RunStmScheduleTaskWithOptionalLock(tasks, useStmReportRun, taskIndex, newFactoryToLoadTaskFromDatabase, token, mustRunLocked);
				}
			}
		}

		void RunStmScheduleTaskWithOptionalLock(StmScheduleTask[] tasks, bool useStmReportRun, int taskIndex, BusinessObjectFactory newFactoryToLoadTaskFromDatabase, CancellationToken token, bool mustRunLocked)
		{
			var task = tasks[taskIndex];
			var retriesMax = SystemDataRegistry.Instance.SRRMaximumRetryCount.Value;
			if (useStmReportRun && retriesMax > 0)
			{
				if (HasContinuousFailuresSinceReactive(task, retriesMax))
				{
					task.S5_IsActive = false;
					task.Factory.Save();
					notifications.AddError(Res.GetString("3df21df8-c213-4dcf-a138-084998341206",
						"Task aborted and scheduled report {0} is de-activated as it has failed more than {1} times, more than the failure threshold value set in the registry: {2}. Please check the reasons for failure before enabling the scheduled report again.",
						task.DescriptionForLog, retriesMax, SystemDataRegistry.Instance.SRRMaximumRetryCount.HumanReadableRegistryPath()));
					return;
				}
			}

			if (mustRunLocked)
			{
				var key = "ScheduleTaskRunner:" + task.PK.ToString();

				var result = Db.Connection.RunLocked(key, isFirstAttempt => ProcessSingleTask(task, useStmReportRun, taskIndex, tasks.Length, isFirstAttempt, token), () => IsFinished(task, newFactoryToLoadTaskFromDatabase));

				if (result == LockedProcessResult.Error)
				{
					notifications.Notify(new InfoNotification(Res.GetString("EE1C03AE-F106-43C2-8F19-93367A6A45BD", "Task {0} could not be completed, maximum retries exceeded.", taskIndex + 1)));
				}
				else if (result == LockedProcessResult.AlreadyBeingProcessed)
				{
					notifications.Notify(new InfoNotification(Res.GetString("CCE45E5C-5E93-492B-B4CB-10A11BDD4919", "Task {0} of {1} is already being run.", taskIndex + 1, tasks.Length)));
				}
			}
			else
			{
				ProcessSingleTask(task, useStmReportRun, taskIndex, tasks.Length, /*isFirstAttempt*/ true, token);
			}
		}

		void ProcessSingleTask(StmScheduleTask task, bool useStmReportRun, int taskIndex, int numberOfTasks, bool isFirstAttempt, CancellationToken token)
		{
			var log = isFirstAttempt ?
				Res.GetString("A26A5A0D-EFFF-432C-8BDE-43979EB141EA", "Running {0} of {1} scheduled tasks: {2}", taskIndex + 1, numberOfTasks, task.DescriptionForLog) :
				Res.GetString("E7754404-B81F-4959-A1AE-03E5384244F0", "The connection to the database has been reset. Rerunning {0} of {1} scheduled tasks: {2}", taskIndex + 1, numberOfTasks, task.DescriptionForLog);

			notifications.Notify(new InfoNotification(log));

			var stopwatch = ObjectFactory.Get<IStopwatch>();
			stopwatch.Start();
			string description = task.DescriptionForLog;
			var result = RunStmScheduleTask(task, useStmReportRun, token);
			if (result)
			{
				notifications.Notify(new InfoNotification(Res.GetString("9dd913ed-26c1-469b-9a18-316b2d72f40a", "Task completed. {0}. Time taken: {1} seconds", description, stopwatch.ElapsedMilliseconds / 1000)));
			}
		}

		bool IsFinished(StmScheduleTask task, BusinessObjectFactory newFactory)
		{
			var query = new ZDBOnlyQuery(task.GetType());
			query.ReLoadExistingRows = true;
			query.AddToFilter(StmScheduleTaskSchema.PK, task.PK);

			var taskInAnotherFactory = newFactory.Load(task.GetType(), query).FirstOrDefault();

			if (taskInAnotherFactory == null)
			{
				return true;
			}

			query.AddToFilter(StmScheduleTaskSchema.S5_IsActive, true);
			query.AddToFilter(StmScheduleTaskSchema.S5_NextScheduledPrintRunTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			taskInAnotherFactory = newFactory.Load(task.GetType(), query).FirstOrDefault();
			return taskInAnotherFactory == null;
		}

		Process CurrentProcess
		{
			get
			{
				if (currentProcess != null)
				{
					currentProcess.Refresh();
				}
				else
				{
					currentProcess = System.Diagnostics.Process.GetCurrentProcess();
				}
				return currentProcess;
			}
		}
		Process currentProcess;

#if DEBUG
		virtual protected Action<StmScheduleTask, StmScheduleTask> betweenScheduleTasksTestAction => null;
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception needs to be passed out before rethrown.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Useless CA rule that will be deprecated soon.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1071:DoNotUseGCWaitForPendingFinalizersOrGetTotalMemory", Justification = "It's this or immediately blow everything up")]
		bool RunStmScheduleTask(StmScheduleTask task, bool useStmReportRun, CancellationToken token)
		{
			var result = true;
			var timeMax = SystemDataRegistry.Instance.SRRMaximumTimeElapsed.Value;
			var memoryMax = SystemDataRegistry.Instance.SRRMaximumMemory.Value;
			var memoryBefore = memoryMax > 0 ? CurrentProcess.PrivateMemorySize64 : 0;

			var time = new Stopwatch();
			notifications.Reset();
			var stmReportRun = useStmReportRun ? CreateStmReportRun(task) : null;

			Exception taskException = null;
			var taskDescription = task.DescriptionForLog;

			using (var tokenSource = new CancellationTokenSource())
			{
				tokenSource.Token.Register(() =>
				{
					notifications.Notify(new InfoNotification(Res.GetString("64fcbc07-789d-41e4-8328-e1199aefc989", "Scheduled task is canceled successfully. {0}", taskDescription)));
					OnCancelRunningReport(task);
				});

				time.Start();
				var runnerTask = AsyncHelper.RunTask(() =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							var taskFactory = new BusinessObjectFactory { NameForDebugging = $@"{nameof(ScheduleTaskRunner)}_{nameof(RunStmScheduleTask)}" };
							var runningTask = (StmScheduleTask)taskFactory.Load(task.GetType(), task.PK);
							if (useStmReportRun)
							{
								var report = taskFactory.Load<StmReportRun>(stmReportRun.PK);
								runningTask.StmReportRun = report;
							}
#if DEBUG
							betweenScheduleTasksTestAction?.Invoke(task, runningTask);
#endif

							try
							{
								runningTask.RunSafe(notifications, tokenSource.Token);
							}
							catch (Exception ex) when (!(ex is OperationCanceledException))
							{
								if (!tokenSource.IsCancellationRequested)
								{
									taskException = ex;
								}
							}
							finally
							{
								if (useStmReportRun && !tokenSource.IsCancellationRequested)
								{
									var reportRun = runningTask.StmReportRun;
									reportRun.RRI_EndTimeUtc = ZDateTime.UtcNow;
									if (reportRun.RRI_Status != StmReportRunState.Finished)
									{
										reportRun.RRI_Status = StmReportRunState.Error;
									}
									reportRun.Factory.Save();
								}
							}

#if DEBUG
							betweenScheduleTasksTestAction?.Invoke(runningTask, task);
#endif
						}
					}
					catch (OperationCanceledException)
					{
					}
					return true;
				}, tokenSource.Token, "runnerTask");

				try
				{
					using (var newDbConnection = Db.NewExtraConnectionToMainDb())
					{
						var cancellationIsAlreadyReported = false;
						var aborted = false;

						do
						{
							if (useStmReportRun && timeMax > 0 && time.Elapsed.TotalSeconds > timeMax)
							{
								var error = Res.GetString("b927f5eb-f67d-4762-ab3d-54cd54feff82", "Task aborted; {0} took longer than {1} seconds, more than the timeout value set in the registry: {2}", taskDescription, timeMax, SystemDataRegistry.Instance.SRRMaximumTimeElapsed.HumanReadableRegistryPath());
								notifications.AddError(error);
								taskException = new InvalidOperationException(error);
								tokenSource.Cancel();
								aborted = true;
							}
							else if (useStmReportRun && memoryMax > 0 && ((currentProcess.PrivateMemorySize64 - memoryBefore) / (1024L * 1024L * 1024L)) >= memoryMax)
							{
								GC.Collect();
								GC.WaitForPendingFinalizers();
								GC.Collect();
								if (((currentProcess.PrivateMemorySize64 - memoryBefore) / (1024L * 1024L * 1024L)) >= memoryMax)
								{
									var error = Res.GetString("af75402a-fecd-4e47-8e22-b37744cca946", "Task aborted; {0} used more than {1} GB of memory, more than the memory limit set in the registry: {2}", taskDescription, memoryMax, SystemDataRegistry.Instance.SRRMaximumMemory.HumanReadableRegistryPath());
									notifications.AddError(error);
									taskException = new InvalidOperationException(error);
									tokenSource.Cancel();
									aborted = true;
								}
							}
							else if (useStmReportRun && !tokenSource.IsCancellationRequested && RequireCancelRunningReport(stmReportRun.PK.ToGuid(), newDbConnection))
							{
								notifications.Notify(new InfoNotification(Res.GetString("9d7937e8-c121-4a87-8358-8cdfb8d7e898", "Canceling scheduled task. {0}", taskDescription)));
								tokenSource.Cancel();
								aborted = true;
							}
							else if (token.IsCancellationRequested && !cancellationIsAlreadyReported)
							{
								// The task must be reloaded from a new factory to synchronize the changes from the above running thread.
								task = GetReloadedTaskFromNewFactory(task.GetType(), task.PK);

								var reportName = Res.GetString(
									"72BEA9B0-D6B1-4118-A702-D0F290750E50",
									"{0}",
									task.S5_ParentTableCode == "SU"
										? task.Factory.Load<StmMenuItem>(task.S5_ParentID).SU_MenuName.ToString()
										: $"unknown report name - unexpected parent table code \"{task.S5_ParentTableCode}\" (should be SU)");

								notifications.Notify(new InfoNotification(
									Res.GetString("{6FAC4C46-52B5-4712-8C41-B0C7B8746B53}",
										"Report name \"{0}\" with description \"{1}\" received a cancellation request from Runner.",
										reportName,
										taskDescription)));
								cancellationIsAlreadyReported = true;
							}

							try
							{
								runnerTask.Wait(1000);
							}
							catch (AggregateException ae)
							{
								ae.Handle(ex => ex is TaskCanceledException);
							}
						} while (!aborted && !runnerTask.IsCompleted);
					}

					// The task must be reloaded from a new factory to synchronize the changes from the above running thread.
					if (taskException != null)
					{
						task = GetReloadedTaskFromNewFactory(task.GetType(), task.PK);
						throw new InvalidOperationException("Exception occurred during scheduled task", taskException);
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					if (task != null)
					{
						if (task.S5_IsPrivate || e is ScheduleTaskExcessiveUsageException || task.S5_ParentTableCode == ScheduleUniversalCopyTask.ParentTableCode)
						{
							NotifyUserOfException(task, e);
						}
					}
					result = false;
				}
				finally
				{
					if (useStmReportRun)
					{
						OnAfterRunStmScheduleTask(stmReportRun, taskException);
					}
				}
			}
			return result;
		}

		protected virtual StmScheduleTask GetReloadedTaskFromNewFactory(Type type, ZGuid pk)
		{
#if DEBUG
			if (type == typeof(DummyReloadedStmScheduleTask))
			{
				return null;
			}
#endif
			var reloadFactory = new BusinessObjectFactory { NameForDebugging = $"{nameof(ScheduleTaskRunner)}_{nameof(GetReloadedTaskFromNewFactory)}" };
			return (StmScheduleTask)reloadFactory.Load(type, pk);
		}

		StmReportRun CreateStmReportRun(StmScheduleTask task)
		{
			SetAllPreviousStmReportRunRowsFromRUNtoSTP(task);

			var stmReportRun = task.Factory.New<StmReportRun>();
			stmReportRun.RRI_S5_Schedule = task.PK;
			stmReportRun.RRI_ReportDescription = task.S5_ScheduleDescription;
			//We assume that it's been waiting in queue since which ever of these dates is the most recent; when we next expect it to run, when we last created it, when we last modified it.
			stmReportRun.RRI_StartTimeInQueueUtc = new[] { task.S5_NextScheduledPrintRunTimeUtc, task.S5_SystemCreateTimeUtc, task.S5_SystemLastEditTimeUtc }.Max();
			if (stmReportRun.RRI_StartTimeInQueueUtc == ZDateTime.Empty)
			{
				stmReportRun.RRI_StartTimeInQueueUtc = ZDateTime.UtcNow;
			}
			//Kind of by definition, these are both equal to when we start processing it.
			stmReportRun.RRI_EndTimeInQueueUtc = ZDateTime.UtcNow;
			stmReportRun.RRI_StartTimeUtc = ZDateTime.UtcNow;
			stmReportRun.RRI_GS_NKPrintUser = task.S5_GS_NKPrintUser;
			stmReportRun.RRI_Status = StmReportRunState.Running;
			stmReportRun.RRI_RunningServer = System.Environment.MachineName;

			StmMenuItem command = task.Factory.Load<StmMenuItem>(task.S5_ParentID);
			stmReportRun.RRI_ReportName = command?.SU_MenuName ?? ZString.Empty;
			stmReportRun.RRI_IsSystemDefined = command?.SU_IsSystemDefined ?? true;

			task.StmReportRun = stmReportRun;

			stmReportRun.Factory.Save();

			return stmReportRun;
		}

		bool RequireCancelRunningReport(Guid stmReportRunPK, DbConnection newDbConnection)
		{
			const string selectScript = "SELECT RRI_Status FROM dbo.StmReportRun WHERE RRI_PK = @RRI_PK";

			using (var command = newDbConnection.Command(selectScript))
			{
				command.AddParameterBasedOnDbColumn("@RRI_PK", stmReportRunPK, StmReportRunSchema.PK);

				var status = command.ExecuteScalar();

				return status?.ToString() == StmReportRunState.Cancelled;
			}
		}

		void OnCancelRunningReport(StmScheduleTask task)
		{
			// The task must be reloaded from a new factory to synchronize the changes from the above running thread.
			var reloadFactory = new BusinessObjectFactory { NameForDebugging = $"{nameof(ScheduleTaskRunner)}_{nameof(OnCancelRunningReport)}" };
			var taskReloadedInNewFactory = (StmScheduleTask)reloadFactory.Load(task.GetType(), task.PK);
			taskReloadedInNewFactory.OnAfterRun();
		}

		void OnAfterRunStmScheduleTask(StmReportRun stmReportRun, Exception taskException)
		{
			stmReportRun.Reload();
			if (taskException != null)
			{
				stmReportRun.RRI_Status = Enterprise.Core.Constants.StmReportRunState.Error;
				stmReportRun.RRI_EndTimeUtc = ZDateTime.UtcNow;

				var note = stmReportRun.Notes.AddNew();
				note.ST_Description = (NoResString)"Exception";
				note.ST_NoteDataAsText = taskException.ToString();
				note.ST_NoteType = nameof(StmNoteVisibility.PUB);
			}

			foreach (var notification in notifications.currentNotifications.Events)
			{
				if (notification.Type.IsFatal)
				{
					stmReportRun.RRI_Status = Enterprise.Core.Constants.StmReportRunState.Error;
					stmReportRun.RRI_EndTimeUtc = ZDateTime.UtcNow;
				}

				var note = stmReportRun.Notes.AddNew();
				note.ST_Description = notification.Type.EnumValueName;
				note.ST_NoteDataAsText = notification.Message;
				note.ST_NoteType = nameof(StmNoteVisibility.PUB);
			}

			stmReportRun.Factory.Save();
		}

		void NotifyUserOfException(StmScheduleTask task, Exception exception)
		{
			task.S5_IsActive = false;
			task.Factory.Save();
			notifications.Notify(new InfoNotification(Res.GetString("c5a15446-f634-49a7-8f0b-834b99e30d8f", "Ad-hoc scheduled task '{0}' threw an exception when ran. It has been disabled and the submitter notified.", task.S5_ScheduleDescription)));

			var subject = Res.GetString("d2c5334f-9671-4db2-bf55-66da3439deee",
					"One-off report has failed to run: {0}", task.S5_ScheduleDescription);
			var body = Res.GetString("719ED884-57DA-4E6F-8BAC-3E085B94603A",
				@"While attempting to run your report {0}, an exception occurred. It has been deactivated. Ensure that the parameters used for the report are valid and do not create too much data, then activate it again. Exception details follow: \r\n{1}",
				task.S5_ScheduleDescription, exception.ToString());

			var sender = task.ErrorNotificationSender ?? new ScheduleReportErrorNotificationSender(task, notifications);
			sender.SendErrorNotification(subject, body, () =>
			{
				var emailOfLastEditingUser = GetEmailofLastEditingUser(task);
				if (!string.IsNullOrEmpty(emailOfLastEditingUser))
				{
					var mail = new EmailDef();
					mail.Subject = subject;
					mail.Body = body;
					mail.AddRecipientForUserCommunication(emailOfLastEditingUser);
					EnvProxy.Instance.OutgoingMailManager.CreateAndSave(mail, task.Factory);
					return new[] { emailOfLastEditingUser };
				}

				return null;
			});
		}

		string GetEmailofLastEditingUser(StmScheduleTask task)
		{
			var emailAddress = string.Empty;
			if (!task.S5_SystemLastEditUser.IsEmpty)
			{
				emailAddress = GlbStaff.GetEmailAddressFromUserCode(task.Factory, task.S5_SystemLastEditUser);
			}

			if (string.IsNullOrEmpty(emailAddress) && !task.S5_SystemLastEditUser.Equals(task.S5_SystemCreateUser) && !task.S5_SystemCreateUser.IsEmpty)
			{
				emailAddress = GlbStaff.GetEmailAddressFromUserCode(task.Factory, task.S5_SystemCreateUser);
			}

			return emailAddress;
		}

		bool HasContinuousFailuresSinceReactive(StmScheduleTask task, int retriesMax)
		{
			var log = task.Logs.MostRecentLogByPostedTime(Events.SetToActive);
			var failuresCount = task.StmReportRuns
				.OrderByDescending(t => t.RRI_StartTimeUtc)
				.Take(retriesMax)
				.Count(r => (r.RRI_Status == StmReportRunState.Error || r.RRI_Status == StmReportRunState.Stopped) && (log == null || r.RRI_EndTimeUtc > log.SL_PostedTimeUtc));

			return failuresCount == retriesMax;
		}

		void SetAllPreviousStmReportRunRowsFromRUNtoSTP(StmScheduleTask task)
		{
			var sql = @"UPDATE dbo.StmReportRun SET RRI_Status = 'STP', RRI_EndTimeUtc = getutcdate() WHERE RRI_Status = 'RUN' AND RRI_S5_Schedule = @TaskPK";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@TaskPK", task.PK.ToGuid(), StmScheduleTaskSchema.PK);
				command.ExecuteNonQuery();
			}
		}

		NotificationsWrapper notifications;

		[Serializable]
		class NotificationsWrapper : INotifications
		{
			public NotificationsWrapper(INotifications notifications)
			{
				this.notifications = notifications;
				this.currentNotifications = new NotificationBuffer();
			}
			public INotifications notifications;
			[NonSerialized]
			public NotificationBuffer currentNotifications;

			public void Add(INotification notification)
			{
				notifications.Add(notification);
				((INotifications)currentNotifications).Add(notification);
			}

			public void Reset()
			{
				currentNotifications = new NotificationBuffer();
			}
		}

		protected virtual StmScheduleTaskCollection LoadTasksToRun(BusinessObjectFactory factory, ZString parentTableCode)
		{
			var tasksToRun = new StmScheduleTaskCollection(factory, parentTableCode);
			tasksToRun.LoadTasksToRun(ZDateTime.UtcNow);
			return tasksToRun;
		}
	}
}
