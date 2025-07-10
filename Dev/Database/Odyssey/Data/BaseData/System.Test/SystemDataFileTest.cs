using System.Data;
using NUnit.Framework;
using Stm = System;

namespace Enterprise.DbUpgrader.Data.BaseData.System
{
	sealed class SystemDataFileTest : TransactionedTestCase
	{
		[Stm.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestMustCleanDataBeforeSetup()
		{
			SystemDataFile testDataFile = new SystemDataFile();
			AssertEquals("MustCleanDataBeforeSetup", true, testDataFile.MustCleanDataBeforeSetup);

			DataSet data = testDataFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 2, data.Tables.Count);
		}
	}
}
