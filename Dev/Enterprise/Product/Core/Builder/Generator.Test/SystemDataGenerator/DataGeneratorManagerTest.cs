using CargoWise.BuildTools.Testing;
using Enterprise.DbUpgrader.Data;
using NUnit.Framework;

namespace Enterprise.Builder.Generator
{
	sealed class DataGeneratorManagerTest : TestCase
	{
		public void TestGetUpgradeTasks()
		{
			DataGeneratorManager testManager = new DataGeneratorManager();
			UpgradeTask[] testTaskList = testManager.GetUpgradeTasks();

			Assert("List should not be empty", testTaskList.Length > 0);

			bool foundDocumentsUpgradeTask = false;

			foreach (UpgradeTask task in testTaskList)
			{
				if (task.GetType() == typeof(DocumentsUpgradeTask))
				{
					foundDocumentsUpgradeTask = true;
				}
			}

			Assert("DocumentsUpgradeTask not found in the task list", foundDocumentsUpgradeTask);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}
	}
}
