using System;
using System.Collections.Generic;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class DummyClusterKeyStrategyLogger : IUpgradeTaskWorkflowLogger
	{
		public void ActivateSubtaskProgress(int numOfSubtasks)
		{
			throw new NotImplementedException();
		}

		public void ActivateTaskProgress(int numOfTasks)
		{
			throw new NotImplementedException();
		}

		public void ShowInfoMessage(string infoMessage)
		{
			Logs.Add(infoMessage);
		}

		public void ShowTaskError(string errorMessage)
		{
			throw new NotImplementedException();
		}

		public void StartSubtask(string subtask)
		{
			throw new NotImplementedException();
		}

		public void StartTask(string task)
		{
			throw new NotImplementedException();
		}

		public List<string> Logs { get; } = new List<string>();
	}
}
