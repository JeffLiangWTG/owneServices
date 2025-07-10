using System;
using System.Data;

namespace Enterprise.DbUpgrader.Data.Testing
{
	class StmSystemDefinedFieldUpgradeTaskTest : StmSystemDefinedFieldTestCase
	{
		public void TestRun()
		{
			DataSet dataSet = InsertData();

			InsertRow(Guid.NewGuid(), "Name3", "Context3");
			DeleteRow(PK1);

			StmSystemDefinedFieldUpgradeTask upgradeTask = new StmSystemDefinedFieldUpgradeTask(new StmSystemDefinedFieldDataFile());
			upgradeTask.Run(dataSet);

			AssertData(dataSet);
		}
	}
}
