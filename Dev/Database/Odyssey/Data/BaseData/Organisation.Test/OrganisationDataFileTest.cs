using System.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.Organisation
{
	sealed class OrganisationDataFileTest : TransactionedTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestMustCleanDataBeforeSetup()
		{
			OrganisationDataFile testDataFile = new OrganisationDataFile();
			AssertEquals("MustCleanDataBeforeSetup", true, testDataFile.MustCleanDataBeforeSetup);

			DataSet data = testDataFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 4, data.Tables.Count);
		}
	}
}
