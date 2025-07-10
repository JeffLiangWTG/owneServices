using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ChinaVoucher;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ChinaVoucher
{
	[TestedType(typeof(GetControlAccountFromRegistry))]
	class GetControlAccountFromRegistryTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var arrangingDataSql =
				$@"INSERT INTO dbo.StmData VALUES('{Guid.NewGuid()}', 'TestName', NULL, NULL, 'DT', 1, CONVERT(varbinary(max), '0x390065006500610039006200390066002D0065006400350034002D0034003100380064002D0062006500320034002D00640062006300320030003300620062006200350037006500', 1), NULL, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E')";

			TestConnection.ExecuteNonQuery(arrangingDataSql);

			AssertEquals(Guid.Parse("9EEA9B9F-ED54-418D-BE24-DBC203BBB57E"), TestConnection.ExecuteScalar("SELECT [Value] FROM [dbo].[GetControlAccountFromRegistry]('TestName')"));
		}
	}
}

