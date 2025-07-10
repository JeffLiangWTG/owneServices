using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	public class SharedRefDbUpgradeLogger : IUpgradeTaskWorkflowLogger
	{
		public void ActivateSubtaskProgress(int numOfSubtasks)
		{
		}

		public void ActivateTaskProgress(int numOfTasks)
		{
		}

		public void ShowInfoMessage(string infoMessage)
		{
		}

		public void ShowTaskError(string errorMessage)
		{
		}

		public void StartSubtask(string subtask)
		{
		}

		public void StartTask(string task)
		{
		}
	}
}
