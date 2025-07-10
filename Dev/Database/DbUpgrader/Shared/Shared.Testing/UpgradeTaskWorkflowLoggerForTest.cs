using System.Collections.Generic;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Shared
{
	public sealed class UpgradeTaskWorkflowLoggerForTest : IUpgradeTaskWorkflowLogger
	{
		public List<string> LogEntries = new List<string>();

		public void ActivateSubtaskProgress(int numOfSubtasks)
		{
		}

		public void ActivateTaskProgress(int numOfTasks)
		{
		}

		public void ShowInfoMessage(string infoMessage)
		{
			LogEntries.Add(infoMessage);
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
