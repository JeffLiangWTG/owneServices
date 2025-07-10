using System.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.Accounting
{
	sealed class AccountingDataFileTest : TransactionedTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestMustCleanDataBeforeSetup()
		{
			AccountingDataFile testDataFile = new AccountingDataFile();
			AssertEquals("MustCleanDataBeforeSetup", true, testDataFile.MustCleanDataBeforeSetup);

			DataSet data = testDataFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 6, data.Tables.Count);
		}
	}
}
