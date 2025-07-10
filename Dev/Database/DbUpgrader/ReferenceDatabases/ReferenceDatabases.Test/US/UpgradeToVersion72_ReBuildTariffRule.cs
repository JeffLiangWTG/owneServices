using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion72_ReBuildTariffRule : USReferenceDbUpgraderVersionTest
	{
		struct ExpectedRuleData
		{
			public string RuleCode;
			public int TariffRuleCount;
			public int TariffRuleExceptionCount;
			public int SecondaryTariffRuleCount;
			public int SecondaryTariffRuleExceptionCount;
		}

		protected override void AssertUpgradeResult()
		{
			AssertTariffRule();
		}

		void AssertTariffRule()
		{
			var expectedRulesData = new[]
				{
					new ExpectedRuleData() { RuleCode = "A99", TariffRuleCount = 13, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "AAU", TariffRuleCount = 2, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "AGO", TariffRuleCount = 2, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "ATP", TariffRuleCount = 2, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "CAF", TariffRuleCount = 2, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "CBT", TariffRuleCount = 3, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "CFE", TariffRuleCount = 13, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "CLP", TariffRuleCount = 1, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "FDE", TariffRuleCount = 158, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "FME", TariffRuleCount = 30, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "HTH", TariffRuleCount = 11, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "I99", TariffRuleCount = 11, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "LCY", TariffRuleCount = 26, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "NSP", TariffRuleCount = 12, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "R98", TariffRuleCount = 3, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "SGL", TariffRuleCount = 1, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "SGP", TariffRuleCount = 1, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "STN", TariffRuleCount = 1676, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 4863, SecondaryTariffRuleExceptionCount = 87 },
					new ExpectedRuleData() { RuleCode = "TEM", TariffRuleCount = 60, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "VLT", TariffRuleCount = 5, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 },
					new ExpectedRuleData() { RuleCode = "WLE", TariffRuleCount = 2, TariffRuleExceptionCount = 0, SecondaryTariffRuleCount = 0, SecondaryTariffRuleExceptionCount = 0 }
				};

			AssertEquals("USCRule count", expectedRulesData.Length, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(*) FROM USCRule"));
			foreach (var expectedRuleData in expectedRulesData)
			{
				AssertEquals(string.Format("USCRule '{0}' should exist", expectedRuleData.RuleCode), 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, string.Format("SELECT COUNT(*) FROM USCRule WHERE U0_Code = '{0}'", expectedRuleData.RuleCode)));
				AssertEquals(string.Format("USCTariffRule count for '{0}'", expectedRuleData.RuleCode), expectedRuleData.TariffRuleCount, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, string.Format("SELECT COUNT(*) FROM USCTariffRule WHERE U1_RuleCode = '{0}'", expectedRuleData.RuleCode)));
				AssertEquals(string.Format("USCTariffRuleException count for '{0}'", expectedRuleData.RuleCode), expectedRuleData.TariffRuleExceptionCount, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, string.Format("SELECT COUNT(*) FROM USCTariffRuleException WHERE U2_U1 IN (SELECT U1_PK FROM USCTariffRule WHERE U1_RuleCode = '{0}')", expectedRuleData.RuleCode)));
				AssertEquals(string.Format("USCRuleSecondaryTariff count for '{0}'", expectedRuleData.RuleCode), expectedRuleData.SecondaryTariffRuleCount, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, string.Format("SELECT COUNT(*) FROM USCRuleSecondaryTariff WHERE U3_U1 IN (SELECT U1_PK FROM USCTariffRule WHERE U1_RuleCode = '{0}')", expectedRuleData.RuleCode)));
				AssertEquals(string.Format("USCRuleSecondaryTariffException count for '{0}'", expectedRuleData.RuleCode), expectedRuleData.SecondaryTariffRuleExceptionCount, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, string.Format("SELECT COUNT(*) FROM USCRuleSecondaryTariffException WHERE U4_U3 IN (SELECT U3_PK FROM USCRuleSecondaryTariff WHERE U3_U1 IN (SELECT U1_PK FROM USCTariffRule WHERE U1_RuleCode = '{0}'))", expectedRuleData.RuleCode)));
			}
		}

		protected override void PrepareTestData(DbConnection conn)
		{
			base.PrepareTestData(conn);

			string insertRule = @"
IF NOT EXISTS (SELECT U0_Code FROM dbo.USCRule WHERE U0_Code = 'ALH')
BEGIN
	insert into USCRule (U0_Code) values ('ALH')

	insert into USCTariffRule (U1_RuleCode, U1_Tariff, U1_DateFrom) values ('ALH', '2203', '2000-01-01')
	insert into USCTariffRule (U1_RuleCode, U1_Tariff, U1_DateFrom) values ('ALH', '2204', '2000-01-01')
	insert into USCTariffRule (U1_RuleCode, U1_Tariff, U1_DateFrom) values ('ALH', '2205', '2000-01-01')
	insert into USCTariffRule (U1_RuleCode, U1_Tariff, U1_DateFrom) values ('ALH', '2206', '2000-01-01')
	insert into USCTariffRule (U1_RuleCode, U1_Tariff, U1_DateFrom) values ('ALH', '2207', '2000-01-01')
	insert into USCTariffRule (U1_RuleCode, U1_Tariff, U1_DateFrom) values ('ALH', '2208', '2000-01-01')
END
";
			conn.ExecuteNonQuery(insertRule);

			string insertRecords = @"
if not exists (select 1 from USCRule where U0_Code = 'ESB')
begin
	insert into USCRule (U0_Code) values ('ESB')
	insert into USCTariffRule (U1_RuleCode, U1_Tariff, U1_DateFrom) values ('ESB', '61032100', '2000-01-01')
end
";
			conn.ExecuteNonQuery(insertRecords);
		}

		protected override int LatestVersionNumber
		{
			get { return 72; }
		}
	}
}
