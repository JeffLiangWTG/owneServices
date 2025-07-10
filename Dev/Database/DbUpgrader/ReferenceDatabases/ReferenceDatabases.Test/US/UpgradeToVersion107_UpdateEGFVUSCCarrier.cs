using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion107_UpdateEGFVUSCCarrier : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 107;

		protected override void AssertUpgradeResult()
		{
			string carrierCheck = "SELECT count(*) " +
					"FROM USCCarrier " +
					"WHERE UI_Code = 'EGFV' AND UI_Name = 'EVEREST GLOBAL FREIGHT SERVICES INC' AND UI_ModeOfTransportation = '10' AND UI_Address = '1918 STATE ROUTE 27 EDISON NJ 08 817-3213 US' AND UI_AirwayBillPrefix = ''";
			AssertEquals("USCCarrier should contain this record", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, carrierCheck));
		}
	}
}
