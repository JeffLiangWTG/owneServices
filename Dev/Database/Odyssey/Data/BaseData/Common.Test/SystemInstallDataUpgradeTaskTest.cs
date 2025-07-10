using System.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.Common
{
	sealed class SystemInstallDataUpgradeTaskTest : TransactionedTestCase
	{
		public void TestOnlyRequiredIfNewSystem()
		{
			DummyBaseDataFile testDataFile = new DummyBaseDataFile(2);
			SystemInstallDataUpgradeTaskForTesting testUpgradeTask = new SystemInstallDataUpgradeTaskForTesting(testDataFile);

			testDataFile.VersionInDatabase = 1;
			AssertEquals("[Version 1 to 2] Is Required?", false, testUpgradeTask.IsRequired);

			testDataFile.VersionInDatabase = 0;
			AssertEquals("[Version 0 to 2]Is Required?", true, testUpgradeTask.IsRequired);

			testDataFile = new DummyBaseDataFile(0);
			testUpgradeTask = new SystemInstallDataUpgradeTaskForTesting(testDataFile);

			testDataFile.VersionInDatabase = 1;
			AssertEquals("[Version 1 to 0] Is Required?", false, testUpgradeTask.IsRequired);

			testDataFile.VersionInDatabase = 0;
			AssertEquals("[Version 0 to 0] Is Required?", false, testUpgradeTask.IsRequired);
		}

		class SystemInstallDataUpgradeTaskForTesting : SystemInstallDataUpgradeTask
		{
			public SystemInstallDataUpgradeTaskForTesting(BaseDataFile resourceFile)
				: base(resourceFile)
			{
			}
		}

		class DummyBaseDataFile : BaseDataFile
		{
			public DummyBaseDataFile(int dataFileVersion)
				: base("DummyBaseDataFile")
			{
				dummyDataSet = new DataSet(dataFileVersion.ToString());
			}

			protected override DataSet LoadDataSet()
			{
				return dummyDataSet;
			}

			readonly DataSet dummyDataSet;
		}
	}
}
