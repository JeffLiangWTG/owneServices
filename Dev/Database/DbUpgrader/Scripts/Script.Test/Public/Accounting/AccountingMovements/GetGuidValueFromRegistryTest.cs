using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(GetGuidValueFromRegistry))]
	class GetGuidValueFromRegistryTest : DbCreateScriptTest
	{
		public void TestSimpleRun()
		{
			const string arControlAccount = "GL_AR_CONTROL_ACCOUNT";

			TestConnection.ExecuteNonQuery($@"DELETE FROM dbo.StmData WHERE SD_Name = '{arControlAccount}'");
			var result = TestConnection.ExecuteScalar($@"SELECT SD_GuidValue FROM dbo.StmData WHERE SD_Name = '{arControlAccount}'");
			AssertNull(result);

			var expected = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue)
VALUES (NEWID(), 'GL_AR_CONTROL_ACCOUNT', NULL, NULL, 'GID', null, '{expected}');");
			result = TestConnection.ExecuteScalar($@"SELECT SD_GuidValue FROM dbo.StmData WHERE SD_Name = '{arControlAccount}'");
			AssertEquals(expected, result);
		}
	}
}

