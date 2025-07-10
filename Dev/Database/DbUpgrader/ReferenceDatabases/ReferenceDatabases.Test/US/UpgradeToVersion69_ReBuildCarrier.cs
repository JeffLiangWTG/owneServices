using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion69_ReBuildCarrier : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertEquals("Db has USCDataVersion.UZ_UpdateTime", true, DbObjectCreator.ColumnExists(testConnection, "USCDataVersion", "UZ_UpdateTime"));
			AssertCarrier();
		}

		void AssertCarrier()
		{
			var sqlText = @"
SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'USCCarrier'
AND COLUMN_NAME = 'UI_AirwayBillPrefix'
AND DATA_TYPE = 'VARCHAR'
AND CHARACTER_MAXIMUM_LENGTH = 3";
			AssertEquals("UI_AirwayBillPrefix is VARCHAR(3)", 1, testConnection.ExecuteScalar(sqlText));
			sqlText = @"SELECT count(*) from sys.objects WHERE NAME = N'DF_USCCarrier_UI_AirwayBillPrefix' AND TYPE = 'D'";
			AssertEquals("DF_USCCarrier_UI_AirwayBillPrefix exists", 1, testConnection.ExecuteScalar(sqlText));

			sqlText = @"
SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'USCCarrier'
AND COLUMN_NAME = 'UI_ModeOfTransportation'
AND DATA_TYPE = 'VARCHAR'
AND CHARACTER_MAXIMUM_LENGTH = 2";
			AssertEquals("UI_ModeOfTransportation is VARCHAR(2)", 1, testConnection.ExecuteScalar(sqlText));
			sqlText = @"SELECT count(*) from sys.objects WHERE NAME = 'DF_USCCarrier_UI_ModeOfTransportation' AND TYPE = 'D'";
			AssertEquals("DF_USCCarrier_UI_ModeOfTransportation exists", 1, testConnection.ExecuteScalar(sqlText));

			string carrierCheck = @"SELECT COUNT(*) FROM #TempUSCCarrierSystem INNER JOIN USCCarrier ON UI_PK = US_PK";
			AssertEquals("#TempUSCCarrierSystem should contain system records", 6428, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, carrierCheck));

			carrierCheck =
					"SELECT count(*) " +
					"FROM USCCarrier " +
					"WHERE UI_Code = 'LTAN' AND UI_Name = 'LASER TRANSPORT' AND UI_ModeOfTransportation = '30' AND UI_Address = '3380 WHEELTON DR WINDSOR ON 857 CA' AND UI_AirwayBillPrefix = ''";

			AssertEquals("USCCarrier should contain this record", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, carrierCheck));

			carrierCheck =
					"SELECT count(*) " +
					"FROM USCCarrier " +
					"WHERE UI_Code = 'BW' AND UI_Name = 'CARIBBEAN AIRLINES LIMITED' AND UI_ModeOfTransportation = '40' AND UI_Address = 'GOLDEN GROVE ROAD IERE HOUSE TRINIDAD 00000 TT' AND UI_AirwayBillPrefix = '106'";

			AssertEquals("USCCarrier should contain this record", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, carrierCheck));

			carrierCheck =
					"SELECT count(*) " +
					"FROM USCCarrier " +
					"WHERE UI_Code = 'TFSG' AND UI_Name = 'TFT SHIPPING APS' AND UI_ModeOfTransportation = '10' AND UI_Address = 'BROGADE 31 SVENDBORG 5700 DENMARK' AND UI_AirwayBillPrefix = ''";

			AssertEquals("USCCarrier should contain this record", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, carrierCheck));
		}

		protected override int LatestVersionNumber
		{
			get { return 69; }
		}
	}
}
