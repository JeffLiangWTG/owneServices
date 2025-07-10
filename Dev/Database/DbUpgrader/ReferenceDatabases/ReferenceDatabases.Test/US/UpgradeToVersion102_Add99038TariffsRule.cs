using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion102_Add99038TariffsRule : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCTariffRule()
				});
			conn.ExecuteNonQuery(@"
IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = '7973E471-C2F2-4619-8E25-F1A5FB709E65')
BEGIN
INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
	VALUES ('7973E471-C2F2-4619-8E25-F1A5FB709E65', 'I99', '9903', '2018-03-23 00:00:00.000')
END

IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = '3C658E9F-BFDA-4F86-A3FA-3D7F84CCD0E4')
BEGIN
	INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
	VALUES ('3C658E9F-BFDA-4F86-A3FA-3D7F84CCD0E4', 'A99', '990388', '2018-03-23 00:00:00.000')
END
ELSE 
BEGIN
	UPDATE USCTariffRule SET U1_Tariff = '990388' WHERE U1_PK = '3C658E9F-BFDA-4F86-A3FA-3D7F84CCD0E4' AND U1_Tariff <> '990388'
END

IF EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'FDA9E954-6F64-4507-B55A-E2BA5C38762D')
BEGIN
	DELETE USCTariffRule WHERE U1_PK = 'FDA9E954-6F64-4507-B55A-E2BA5C38762D' AND U1_Tariff = '99038505' 
END

IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'FB7D2E0A-D8AE-4D3B-BE53-C4ACFCD64512')
BEGIN
	INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom)
	VALUES ('FB7D2E0A-D8AE-4D3B-BE53-C4ACFCD64512', 'A99', '990385', '2018-03-23 00:00:00.000')
END
BEGIN
	UPDATE USCTariffRule SET U1_Tariff = '990385' WHERE U1_PK = 'FB7D2E0A-D8AE-4D3B-BE53-C4ACFCD64512' AND U1_Tariff <> '990385'
END

IF NOT EXISTS(SELECT null FROM USCTariffRuleException JOIN USCTariffRule ON U2_U1 = U1_PK WHERE U2_Tariff = '99038501' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903')
BEGIN
	INSERT INTO USCTariffRuleException(U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo)
	SELECT NEWID(), U1_PK, '99038501', U1_DateFrom, U1_DateTo
	FROM USCTariffRule
	WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END
ELSE
BEGIN
	UPDATE USCTariffRuleException
	SET U2_Tariff = '99038501'
	FROM USCTariffRuleException
	JOIN USCTariffRule ON U2_U1 = U1_PK
	WHERE U2_Tariff = '99038501' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END 

IF NOT EXISTS(SELECT null FROM USCTariffRuleException JOIN USCTariffRule ON U2_U1 = U1_PK WHERE U2_Tariff = '99038001' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903')
BEGIN
	INSERT INTO USCTariffRuleException(U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo)
	SELECT NEWID(), U1_PK, '99038001', U1_DateFrom, U1_DateTo
	FROM USCTariffRule 
	WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END
ELSE
BEGIN
	UPDATE USCTariffRuleException
	SET U2_Tariff = '99038001'
	FROM USCTariffRuleException
	JOIN USCTariffRule ON U2_U1 = U1_PK
	WHERE U2_Tariff = '99038001' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END 

IF NOT EXISTS(SELECT null FROM USCTariffRuleException JOIN USCTariffRule ON U2_U1 = U1_PK WHERE U2_Tariff = '990388' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903')
BEGIN
	INSERT INTO USCTariffRuleException(U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo)
	SELECT NEWID(), U1_PK, '990388', U1_DateFrom, U1_DateTo
	FROM USCTariffRule
	WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END

IF NOT EXISTS(SELECT null FROM USCTariffRuleException JOIN USCTariffRule ON U2_U1 = U1_PK WHERE U2_Tariff = '990345' AND U1_RuleCode = 'I99' AND U1_Tariff = '9903')
BEGIN
	INSERT INTO USCTariffRuleException(U2_PK, U2_U1, U2_Tariff, U2_DateFrom, U2_DateTo)
	SELECT NEWID(), U1_PK, '990345', U1_DateFrom, U1_DateTo
	FROM USCTariffRule
	WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'
END

");
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals("990385", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode = 'A99' and U1_Tariff = '990385'"));
			AssertEquals("990388", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode = 'A99' and U1_Tariff = '990388'"));
			AssertEquals("99038602", 0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode = 'A99' and U1_Tariff = '99038501'"));
			AssertEquals("99038505", 0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode = 'A99' and U1_Tariff = '99038505'"));
			AssertEquals("99038801", 0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode = 'A99' and U1_Tariff = '99038801'"));
			AssertEquals("990380 USCTariffRuleException", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '990380'"));
			AssertEquals("990388 USCTariffRuleException", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '990388'"));
			AssertEquals("990345 USCTariffRuleException", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '990345'"));
			AssertEquals("990385 USCTariffRuleException", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '990385'"));
			AssertEquals("99038001 USCTariffRuleException", 0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '99038001'"));
			AssertEquals("99038501 USCTariffRuleException", 0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '99038501'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 102; }
		}
	}
}
