using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion73_ModifySTNRule : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCRule(),
				new USCTariffRule(),
				new USCTariffRuleException(),
				new USCRuleSecondaryTariff(),
				new USCRuleSecondaryTariffException(),
				});

			string insertScript = @"
			IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'A60AB456-80B0-4257-9AB8-9386800B4FFC')
				INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('A60AB456-80B0-4257-9AB8-9386800B4FFC', 'STN', '8206000000', '2000-01-01 00:00:00.000')

			IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_U1 = 'A60AB456-80B0-4257-9AB8-9386800B4FFC')
				INSERT INTO USCRuleSecondaryTariff (U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom) VALUES ('BA0A2A39-9E7D-4688-AEF4-D3C2559D9D31', 'A60AB456-80B0-4257-9AB8-9386800B4FFC', '8202', '2000-01-01 00:00:00.000')

			IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = '799B28B2-0D35-4D65-A036-06D484E3E295')
				INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('799B28B2-0D35-4D65-A036-06D484E3E295', 'STN', '98220515', '2000-01-01 00:00:00.000')

			IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = 'E386B9AD-6CAD-4DA2-889B-9CC4F76A0231')
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom)
					VALUES ('E386B9AD-6CAD-4DA2-889B-9CC4F76A0231', '799B28B2-0D35-4D65-A036-06D484E3E295', '17011110', '2000-01-01 00:00:00.000')";

			conn.ExecuteNonQuery(insertScript);
		}

		protected override void AssertUpgradeResult()
		{
			//tariff 8206000000 deleted from STN rule - incident CS00323215
			AssertEquals(null, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select * from USCTariffRule where U1_RuleCode = 'STN' and U1_Tariff = '8206000000'"));
			AssertEquals(null, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select * from USCRuleSecondaryTariff where U3_U1 = 'A60AB456-80B0-4257-9AB8-9386800B4FFC'"));

			//tariff 1701.11.10 is not associated with 9822.05.15 in STN rule, incident CS00332314
			AssertEquals(new DateTime(2013, 12, 31), UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select U3_DateTo from USCRuleSecondaryTariff where U3_TariffFrom = '17011110' and U3_U1 = '799B28B2-0D35-4D65-A036-06D484E3E295'"));

			//new tariffs are added for STN rule, tariff 9822.05.15 - incident CS00332314
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCRuleSecondaryTariff where U3_TariffFrom = '17011310' and U3_U1 = '799B28B2-0D35-4D65-A036-06D484E3E295'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from USCRuleSecondaryTariff where U3_TariffFrom = '17011410' and U3_U1 = '799B28B2-0D35-4D65-A036-06D484E3E295'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 73; }
		}
	}
}
