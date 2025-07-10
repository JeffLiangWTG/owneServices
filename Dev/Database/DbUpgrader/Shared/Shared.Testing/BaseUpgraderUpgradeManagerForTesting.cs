using System.Collections.Generic;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public class BaseUpgraderUpgradeManagerForTesting : DummyUpgradeManager
	{
		public readonly ICollection<string> tasks = new List<string>();

		public int RunTaskCallsNumber => tasks.Count;

		public override void StartTask(string task)
		{
			tasks.Add(task);
		}
	}
}
