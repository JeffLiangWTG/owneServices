using System.Collections.Generic;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class DummyLoggerForTest : IUpgradeTaskWorkflowLogger
	{
		public void ActivateSubtaskProgress(int numOfSubtasks) { }
		public void ActivateTaskProgress(int numOfTasks) { }
		public void ShowInfoMessage(string infoMessage) { Logs.Add(infoMessage); }
		public void ShowTaskError(string errorMessage) { Logs.Add(errorMessage); }
		public void StartSubtask(string subtask) { Logs.Add(subtask); }
		public void StartTask(string task) { Logs.Add(task); }

		public List<string> Logs = new List<string>();
	}
}
