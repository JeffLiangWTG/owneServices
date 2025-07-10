using System.Text;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class MemoryLoggerForTest : IUpgradeTaskWorkflowLogger
	{
		readonly StringBuilder logs = new StringBuilder();

		public void ActivateSubtaskProgress(int numOfSubtasks) { }
		public void ActivateTaskProgress(int numOfTasks) { }
		public void ShowInfoMessage(string infoMessage) { }
		public void ShowTaskError(string errorMessage) { }

		public void StartSubtask(string info)
		{
			logs.AppendLine("> " + info);
		}

		public void StartTask(string info)
		{
			logs.AppendLine(info);
		}

		public void Clear()
		{
			logs.Clear();
		}

		public override string ToString()
		{
			return logs.ToString();
		}
	}
}
