using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public abstract class BaseUpgraderTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestEstimatedNumberOfTasks()
		{
			var dummyUpgradeManager = new BaseUpgraderUpgradeManagerForTesting();
			var upgrader = GetNewUpgrader(dummyUpgradeManager);
			using (Db.DisableSchemaVersionCheck())
			{
				upgrader.RunUpgrade();
			}

			var estimatedNumber = upgrader.EstimatedNumberOfTasks;
			var actualNumber = dummyUpgradeManager.RunTaskCallsNumber;

			var tasks = string.Join("\r\n", dummyUpgradeManager.tasks.Select((s, i) => $"{i + 1,2}: {s}"));

			Assert($"Expected up to: {estimatedNumber} tasks, but was {actualNumber}\r\nTasks:\r\n{tasks}", actualNumber <= estimatedNumber);
		}

		public abstract BaseUpgrader GetNewUpgrader(BaseUpgraderUpgradeManagerForTesting dummyUpgradeManager);
	}
}
