using System.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.Company
{
	sealed class CompanyDataFileTest : TransactionedTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestMustCleanDataBeforeSetup()
		{
			CompanyDataFile testDataFile = new CompanyDataFile();
			AssertEquals("MustCleanDataBeforeSetup", true, testDataFile.MustCleanDataBeforeSetup);

			DataSet data = testDataFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 7, data.Tables.Count);
		}
	}
}
