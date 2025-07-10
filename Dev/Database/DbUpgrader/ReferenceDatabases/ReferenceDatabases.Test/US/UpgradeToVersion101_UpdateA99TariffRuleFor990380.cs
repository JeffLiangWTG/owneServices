using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion101_UpdateA99TariffRuleFor990380 : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 101;

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCTariffRule()
				});
			conn.ExecuteNonQuery(@"IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'C0CF62CC-A1D3-4BB0-A149-791C15642AAB')
BEGIN
	INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
	VALUES ('C0CF62CC-A1D3-4BB0-A149-791C15642AAB', 'A99', '99038001', '2018-03-23 00:00:00.000')
END

else
begin
UPDATE USCTariffRule SET U1_Tariff = '99038001' WHERE U1_PK = 'C0CF62CC-A1D3-4BB0-A149-791C15642AAB'
end");
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule where U1_Tariff = '990380'"));
		}
	}
}
