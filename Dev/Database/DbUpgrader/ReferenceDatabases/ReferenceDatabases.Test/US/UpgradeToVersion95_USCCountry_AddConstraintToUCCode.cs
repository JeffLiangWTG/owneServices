using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion95_USCCountry_AddConstraintToUCCode : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCCountry()
				});

			var insertCountrySQL = @"
	INSERT INTO USCCountry (UC_PK, UC_ISOCountryCode,UC_RateColumnIndicator,UC_RestrictionIndicator,UC_GSPIndicator,UC_SPICode,UC_Name,UC_CurrencyName,UC_CurrencyCode,UC_DrawbackEligibility,
UC_SheduleCCountryCode,UC_SpecialTradeProgramsIndicator,UC_LesserDevelopedCountry,UC_MiscellaneousSPIIndicator,UC_Code)
    VALUES (NEWID(), 'CN','','','','','','','','','','','','','US');";
			conn.ExecuteNonQuery(insertCountrySQL);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, testConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM [{0}].sys.check_constraints WHERE object_id = OBJECT_ID(N'[{0}].[dbo].[CK_USCCountry_UC_CodeNotEmpty]') AND parent_object_id = OBJECT_ID(N'[{0}].[dbo].[USCCountry]')", refDbUpgrader.DbName)));

			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCCountry WHERE UC_Code = ''"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCCountry"));
		}

		protected override int LatestVersionNumber
		{
			get { return 95; }
		}
	}
}
