using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion120Test : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			string addExtraForeignPort = @"
-- Foreign Port for AES

	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4901', 'AGUADILLA, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4904', 'FAJARDO, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4907', 'MAYAGUEZ, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4908', 'PONCE, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4909', 'SAN JUAN, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '4913', 'INTL AIRPORT, PR', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '57042', 'QINZHOU, CHINA', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '5101', 'CHARLOTTE AMALIE, USVI', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '5102', 'CRUZ BAY, VI', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '5104', 'CHRISTIANSTED, VI', 'AES')
	INSERT INTO USCForeignPort (UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES (NEWID(), '5105', 'FREDERIKSTED, VI', 'AES')

";
			conn.ExecuteNonQuery(addExtraForeignPort);
		}

		protected override void AssertUpgradeResult()
		{
			string sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_ValidForType = 'AES'";
			AssertEquals("11 AES type records should exist.", 11, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '4901' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4901' should exist & should be for Port AGUADILLA, PR.", "AGUADILLA, PR", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '4904' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4904' should exist & should be for Port FAJARDO, PR.", "FAJARDO, PR", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '4905' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4905' deleted GUANICA, PR.", null, (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '4906' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4906' deleted", null, (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '4907' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4907' should exist & should be for Port MAYAGUEZ, PR.", "MAYAGUEZ, PR", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '4908' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4908' should exist & should be for Port PONCE, PR.", "PONCE, PR", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '4909' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4909' should exist & should be for Port SAN JUAN, PR.", "SAN JUAN, PR", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '4911' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4911' deleted", null, (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '4912' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4912' deleted - GUAYANILLA, PR.", null, (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '4913' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4913' should exist & should be for INTL AIRPORT, PR", "INTL AIRPORT, PR", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '57042' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '57042' should exist & should be for Port QINZHOU, CHINA", "QINZHOU, CHINA", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '5101' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '5101' should exist & should be for Port CHARLOTTE AMALIE, USVI.", "CHARLOTTE AMALIE, USVI", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '5102' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '5102' should exist & should be for Port CRUZ BAY, VI.", "CRUZ BAY, VI", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '5104' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '5104' should exist & should be for Port CHRISTIANSTED, VI.", "CHRISTIANSTED, VI", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '5105' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '5105' should exist & should be for Port FREDERIKSTED, VI.", "FREDERIKSTED, VI", (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT UH_Name FROM USCForeignPort WHERE UH_Code = '5108' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '5108' deleted", null, (string)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));
		}

		protected override int LatestVersionNumber => 120;
	}
}
