using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public class DummyUpgradeManager : IUpgradeManager
	{
		#region IUpgradeManager Members

		public virtual bool IsHosted => false;

		public virtual bool? IsInternalSystem => throw new NotImplementedException();

		public virtual bool? IsUATSystem => throw new NotImplementedException();

		public virtual void StartTask(string task)
		{
		}

		public virtual void StartNonEstimatedTask(string task)
		{
		}

		public virtual void StartSubtask(string subtask)
		{
		}

		public virtual void ShowInfoMessage(string infoMessage)
		{
		}

		public virtual void ShowTaskError(string errorMessage)
		{
		}

		public virtual bool ShowErrorWithRetry(string errorMessage)
		{
			return false;
		}

		public void ActivateTaskProgress(int numOfTasks)
		{
		}

		public void ActivateSubtaskProgress(int numOfSubtasks)
		{
		}

		public void IncrementNumberOfTasks(int numOfTasksToAdd)
		{
		}

		public void UpdateCurrentProgress(int currentProgress)
		{
		}

		public virtual VersionLabel SchemaVersionBeforeUpgrade
		{
			get { return new VersionLabel(0, 0); }
		}

		public virtual VersionLabel TransformationVersionBeforeUpgrade
		{
			get { return new VersionLabel(0, 0); }
		}

		#region ITemplateManager

		const string DUMMY_UPGRADE_MANAGER_KEY_PROPERTY_NAME = "DummyUpgradeManagerKey";

		public bool ManagerKeyExists(DbConnection connection, string dbName)
		{
			try
			{
				if (connection.DatabaseExists(dbName))
				{
					Guid currentKey;
					if (Guid.TryParse(DataUtils.LoadDbExtendedProperty(connection, DUMMY_UPGRADE_MANAGER_KEY_PROPERTY_NAME, dbName), out currentKey))
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
			DataUtils.AddDbExtendedProperty(connection, DUMMY_UPGRADE_MANAGER_KEY_PROPERTY_NAME, UpgradeManagerKey.ToString(), dbName);
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

		public bool GetUserConfirmation(string title, string message, string[] detailLines)
		{
			return true;
		}

		#endregion // IUpgradeManager Members
	}
}
