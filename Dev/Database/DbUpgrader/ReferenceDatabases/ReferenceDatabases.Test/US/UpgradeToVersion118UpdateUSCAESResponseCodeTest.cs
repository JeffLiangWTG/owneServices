using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion118UpdateUSCAESResponseCodeTest : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertEquals("Db has USCForeignPort.UH_Code", true, DbObjectCreator.ColumnExists(testConnection, "USCForeignPort", "UH_Code"));
			AssertEquals("Db has USCForeignPort.UH_Name", true, DbObjectCreator.ColumnExists(testConnection, "USCForeignPort", "UH_Name"));
			AssertEquals("Db has USCForeignPort.UH_ValidForType", true, DbObjectCreator.ColumnExists(testConnection, "USCForeignPort", "UH_ValidForType"));
			AssertEquals("Db has NR_IX__UH_Code", true, DbObjectCreator.IndexExists(testConnection, "USCForeignPort", "NR_IX__UH_Code"));
			AssertEquals("Db has NR_IX__UH_ValidForType", true, DbObjectCreator.IndexExists(testConnection, "USCForeignPort", "NR_IX__UH_ValidForType"));

			// Assert AES Foreign Port
			string sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '4901' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4901' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '4904' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4904' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '4907' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4907' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '4908' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4908' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '4909' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4909' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '4913' AND UH_ValidForType = 'AES'";
			AssertEquals("Code '4913' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			// Assert INB Foreign Port codes
			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_ValidForType = 'INB'";
			AssertEquals("45 INB type records should not be affected & should still exist.", 45, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			// Assert InBond Foreign Port
			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80101' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80101' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80102' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80102' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80103' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80103' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80104' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80104' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80105' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80105' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80106' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80106' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80108' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80108' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80109' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80109' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80110' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80110' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80111' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80111' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80112' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80112' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80113' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80113' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '80107' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '80107' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97101' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97101' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97102' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97102' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97103' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97103' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97104' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97104' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97105' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97105' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97106' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97106' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97107' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97107' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97108' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97108' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97109' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97109' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97110' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97110' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97111' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97111' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97112' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97112' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97113' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97113' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97114' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97114' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97115' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97115' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97116' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97116' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97117' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97117' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97118' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97118' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97119' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97119' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97120' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97120' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97121' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97121' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97122' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97122' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97123' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97123' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97124' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97124' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97125' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97125' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97126' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97126' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97127' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97127' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97128' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97128' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97129' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97129' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97130' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97130' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97131' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97131' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));

			sql = "SELECT COUNT(*) FROM USCForeignPort WHERE UH_Code = '97132' AND UH_ValidForType = 'INB'";
			AssertEquals("Code '97132' should exists.", 1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));
		}

		protected override int LatestVersionNumber
		{
			get { return 118; }
		}
	}
}
