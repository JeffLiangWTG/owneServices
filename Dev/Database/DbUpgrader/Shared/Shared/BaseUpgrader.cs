namespace Enterprise.DbUpgrader.Shared
{
	using System;
	using System.Collections.Generic;
	using CargoWise.Common;
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Resource.Version;

	public abstract class BaseUpgrader : IUpgradeActionProvider
	{
		public BaseUpgrader(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade)
		{
			this.Manager = manager;
			this.upgConnection = upgConnection;
			this.versionBeforeUpgrade = versionBeforeUpgrade;
		}

	public
#if DEBUG
	virtual
#endif
		 void RunUpgrade()
		{
			StartTask("Starting " + Name);
			StartTask(String.Format("Version: {0} => {1}", versionBeforeUpgrade.ToString(), LatestVersion.ToString()));
			Manager.ShowInfoMessage(".");

			if (IsUpgradeRequired)
			{
				try
				{
					if (upgConnection.IsInTransaction || !RequiresTransaction)
					{
						DoUpgrade();
					}
					else
					{
						DoUpgradeInTransaction();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is InvalidOperationException && ex.Message.Contains("The connection is closed."))
					{
						throw new Exception(Name + " failed.\r\nDatabase connection was lost. Rolling back upgrade to ensure data consistency.", ex);
					}
					else
					{
						throw new Exception(Name + " failed.\r\n" + ex.Message, ex);
					}
				}

				StartTask(Name + " completed.");
			}
			else
			{
				StartTask(Name + " not required.");
			}
		}

		protected void DoUpgradeInTransaction()
		{
			using (var manager = upgConnection.BeginTransactionWithManager())
			{
				DoUpgrade();
				manager.CommitTransaction();
			}
		}

		public virtual bool IsUpgradeRequired => true;

		protected abstract void DoUpgrade();

		public abstract int EstimatedNumberOfTasks { get; }

		public abstract string Name { get; }

		public virtual bool RequiresApplicationLockout
		{
			get { return true; }
		}

		protected virtual bool RequiresTransaction
		{
			get { return true; }
		}

		/// <summary>
		/// MUST NOT return NULL.
		/// Returns an empty array, if no secondary DBs.
		/// </summary>
		public abstract IEnumerable<string> SecondaryDatabasesToUpgrade { get; }

		protected abstract VersionLabel LatestVersion { get; }

#region IUpgradeActionProvider Interface

		public IUpgradeAction UpgradeAction
		{
			get
			{
				var action = new UpgradeAction(RunUpgrade);
				action.Name = Name;
				action.RequiresApplicationLockout = RequiresApplicationLockout;
				action.EstimatedNumberOfTasks = EstimatedNumberOfTasks;
				action.SecondaryDatabasesToUpgrade = SecondaryDatabasesToUpgrade;

				return action;
			}
		}

#endregion // IUpgradeActionProvider Interface

		protected readonly IUpgradeManager Manager;
		protected readonly DbConnection upgConnection;
		protected readonly VersionLabel versionBeforeUpgrade;

#region Notification Methods

		protected void StartTask(string task)
		{
			Manager.StartTask(task);
		}

		protected void StartNonEstimatedTask(string task)
		{
			Manager.StartNonEstimatedTask(task);
		}

		protected void StartSubtask(string subtask)
		{
			Manager.StartSubtask(subtask);
		}

		protected void ShowInfoMessage(string infoMessage)
		{
			Manager.ShowInfoMessage(infoMessage);
		}

		protected void ShowTaskError(string errorMessage)
		{
			Manager.ShowTaskError(errorMessage);
		}

		protected void ActivateTaskProgress(int numOfTasks)
		{
			Manager.ActivateTaskProgress(numOfTasks);
		}

		protected void ActivateSubtaskProgress(int numOfSubtasks)
		{
			Manager.ActivateSubtaskProgress(numOfSubtasks);
		}

		protected void IncrementNumberOfTasks(int numOfTasksToAdd)
		{
			Manager.IncrementNumberOfTasks(numOfTasksToAdd);
		}

#endregion
	}
}
