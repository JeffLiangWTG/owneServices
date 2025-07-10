using System;
using System.ComponentModel;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Shared
{
	#region Event And Delegates

	public enum UpgradeEventType
	{
		TaskStarted,
		SubtaskStarted,
		TaskFailed,
		InfoMessage,
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
	public delegate void UpgraderEvent(UpgradeEventType eventType, string message);
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
	public delegate bool UpgraderWithRetryEvent(string message);
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
	public delegate void UpgraderNumOfStepsEstimatedEvent(int numOfSteps);

	#endregion

	public abstract class BaseUpgradeManager : IUpgradeManager
	{
		public event UpgraderEvent UpgradeEvent;
		public event UpgraderWithRetryEvent OnErrorWithRetry;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event UpgraderNumOfStepsEstimatedEvent OnTasksEstimated;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event UpgraderNumOfStepsEstimatedEvent OnSubtasksEstimated;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event UpgraderNumOfStepsEstimatedEvent OnIncrementNumberOfTasks;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event UpgraderNumOfStepsEstimatedEvent OnUpdateCurrentProgress;

		public ISynchronizeInvoke SyncInvoke
		{
			get { return syncInvoke; }
			set { syncInvoke = value; }
		}
		ISynchronizeInvoke syncInvoke;

		#region IUpgradeManager Members

		#region Notification Methods (Messaging)

		void IUpgradeTaskWorkflowLogger.StartTask(string task)
		{
			OnUpgradeEvent(UpgradeEventType.TaskStarted, task);
		}

		void IUpgradeTaskWorkflowControl.StartNonEstimatedTask(string task)
		{
			((IUpgradeManager)this).IncrementNumberOfTasks(1);
			((IUpgradeManager)this).StartTask(task);
		}

		void IUpgradeTaskWorkflowLogger.StartSubtask(string subtask)
		{
			OnUpgradeEvent(UpgradeEventType.SubtaskStarted, subtask);
		}

		void IUpgradeTaskWorkflowLogger.ShowInfoMessage(string infoMessage)
		{
			OnUpgradeEvent(UpgradeEventType.InfoMessage, infoMessage);
		}

		void IUpgradeTaskWorkflowLogger.ShowTaskError(string errorMessage)
		{
			OnUpgradeEvent(UpgradeEventType.TaskFailed, errorMessage);
		}

		void OnUpgradeEvent(UpgradeEventType eventType, string message)
		{
			if (UpgradeEvent != null && !string.IsNullOrEmpty(message))
			{
				if (syncInvoke == null)
				{
					UpgradeEvent(eventType, message);
				}
				else
				{
					syncInvoke.BeginInvoke(new UpgraderEvent(UpgradeEvent), new object[] { eventType, message });
				}
			}
		}

		bool IUpgradeUserInteraction.ShowErrorWithRetry(string errorMessage)
		{
			bool retry = false;
			if ((OnErrorWithRetry != null) && (!string.IsNullOrEmpty(errorMessage)))
			{
				if (syncInvoke == null)
				{
					retry = OnErrorWithRetry(errorMessage);
				}
				else
				{
					retry = (bool)syncInvoke.Invoke(new UpgraderWithRetryEvent(OnErrorWithRetry), new object[] { errorMessage });
				}
			}
			else
			{
				((IUpgradeManager)this).ShowTaskError(errorMessage);
			}
			return retry;
		}

		void IUpgradeTaskWorkflowLogger.ActivateTaskProgress(int numOfTasks)
		{
			if (OnTasksEstimated != null)
			{
				if (syncInvoke == null)
				{
					OnTasksEstimated(numOfTasks);
				}
				else
				{
					syncInvoke.BeginInvoke(new UpgraderNumOfStepsEstimatedEvent(OnTasksEstimated), new object[] { numOfTasks });
				}
			}
		}

		void IUpgradeTaskWorkflowLogger.ActivateSubtaskProgress(int numOfSubtasks)
		{
			if (OnSubtasksEstimated != null)
			{
				if (syncInvoke == null)
				{
					OnSubtasksEstimated(numOfSubtasks);
				}
				else
				{
					syncInvoke.BeginInvoke(new UpgraderNumOfStepsEstimatedEvent(OnSubtasksEstimated), new object[] { numOfSubtasks });
				}
			}
		}

		void IUpgradeTaskWorkflowControl.IncrementNumberOfTasks(int numOfTasksToAdd)
		{
			if (OnIncrementNumberOfTasks != null)
			{
				if (syncInvoke == null)
				{
					OnIncrementNumberOfTasks(numOfTasksToAdd);
				}
				else
				{
					syncInvoke.BeginInvoke(new UpgraderNumOfStepsEstimatedEvent(OnIncrementNumberOfTasks), new object[] { numOfTasksToAdd });
				}
			}
		}

		public void UpdateCurrentProgress(int currentProgress)
		{
			if (OnUpdateCurrentProgress != null)
			{
				if (syncInvoke == null)
				{
					OnUpdateCurrentProgress(currentProgress);
				}
				else
				{
					syncInvoke.BeginInvoke(new UpgraderNumOfStepsEstimatedEvent(OnUpdateCurrentProgress), new object[] { currentProgress });
				}
			}
		}

		#endregion

		public abstract bool IsHosted { get; }

		public virtual bool? IsInternalSystem => throw new NotImplementedException();

		public virtual bool? IsUATSystem => throw new NotImplementedException();

		public abstract VersionLabel SchemaVersionBeforeUpgrade { get; }

		public abstract VersionLabel TransformationVersionBeforeUpgrade { get; }

		bool IUpgradeUserInteraction.GetUserConfirmation(string title, string message, string[] detailLines)
		{
			return GetConfirmationIfUserAttended(title, message, detailLines);
		}

		protected virtual bool GetConfirmationIfUserAttended(string title, string message, string[] detailLines)
		{
			return UpgUtils.Instance.ShowConfirmationMessageBox(title, message, detailLines);
		}

		#region ITemplateManager

		const string UPGRADE_MANAGER_KEY_PROPERTY_NAME = "UpgradeManagerKey";

		public bool ManagerKeyExists(DbConnection connection, string dbName)
		{
			try
			{
				if (connection.DatabaseExists(dbName))
				{
					Guid currentKey;
					if (Guid.TryParse(DataUtils.LoadDbExtendedProperty(connection, UPGRADE_MANAGER_KEY_PROPERTY_NAME, dbName), out currentKey))
					{
						if (currentKey.Equals(UpgradeManagerKey))
						{
							return true;
						}
					}
				}
			}
			catch (SqlException)
			{
				// just drop old and create new TemplateDB
			}

			return false;
		}

		public void SetManagerKey(DbConnection connection, string dbName)
		{
			DataUtils.AddDbExtendedProperty(connection, UPGRADE_MANAGER_KEY_PROPERTY_NAME, UpgradeManagerKey.ToString(), dbName);
		}

		public Guid UpgradeManagerKey
		{
			get
			{
				if (!upgradeManagerKey.HasValue)
				{
					upgradeManagerKey = Guid.NewGuid();
				}

				return upgradeManagerKey.Value;
			}
		}

		Guid? upgradeManagerKey;

		#endregion // ITemplateManager

		#endregion // IUpgradeManager Members

		public abstract ValidationResponse Run();
	}
}
