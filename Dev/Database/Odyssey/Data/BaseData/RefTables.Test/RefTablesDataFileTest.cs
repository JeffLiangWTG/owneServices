using System.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.RefTables
{
	sealed class RefTablesDataFileTest : TransactionedTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestMustCleanDataBeforeSetup()
		{
			RefTablesDataFile testDataFile = new RefTablesDataFile();
			AssertEquals("MustCleanDataBeforeSetup", true, testDataFile.MustCleanDataBeforeSetup);

			DataSet data = testDataFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 5, data.Tables.Count);
		}
	}
}
