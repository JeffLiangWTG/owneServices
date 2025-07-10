using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Shared
{
	#region User Interaction and Task Workflow interfaces

	public interface IUpgradeUserInteraction
	{
		bool GetUserConfirmation(string title, string message, string[] detailLines);
		bool ShowErrorWithRetry(string errorMessage);
	}

	public interface IUpgradeTaskWorkflowControl : IUpgradeTaskWorkflowLogger
	{
		void StartNonEstimatedTask(string task);
		void IncrementNumberOfTasks(int numOfTasksToAdd);
		void UpdateCurrentProgress(int currentProgress);
	}

	#endregion

	public interface ITemplateManager
	{
		bool ManagerKeyExists(DbConnection connection, string dbName);
		void SetManagerKey(DbConnection connection, string dbName);
	}

	public interface IUpgradeManager : IUpgradeUserInteraction, IUpgradeTaskWorkflowControl, ITemplateManager, IUpgradeContext
	{
		VersionLabel SchemaVersionBeforeUpgrade { get; }
		VersionLabel TransformationVersionBeforeUpgrade { get; }
	}
}
