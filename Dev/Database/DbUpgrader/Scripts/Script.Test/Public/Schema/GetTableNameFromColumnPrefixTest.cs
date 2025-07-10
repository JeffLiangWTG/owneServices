using CargoWise.DbUpgrader.Scripts.Definitions.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Schema.Testing
{
	[TestedType(typeof(GetTableNameFromColumnPrefix))]
	sealed class GetTableNameFromColumnPrefixTest : DbCreateScriptTest
	{
		public void TestFunction()
		{
			AssertGetTableName("TableDoesExist", ProcessHeaderSchema.Constants.Prefix, ProcessHeaderSchema.Constants.TableName);
			AssertGetTableName("TableDoesNotReturnAView", JobShipmentSchema.Constants.Prefix, JobShipmentSchema.Constants.TableName);
			AssertGetTableName("TableDoesntExist", "F_H", null);
		}

		void AssertGetTableName(string message, string tablePrefix, string expectedTableName)
		{
			var sql = string.Format("SELECT TableName FROM dbo.GetTableNameFromColumnPrefix('{0}')", tablePrefix);

			var result = TestConnection.ExecuteScalar(sql);

			if (expectedTableName != null)
			{
				AssertEquals(message, expectedTableName, (string)result);
			}
			else
			{
				AssertEquals(message, null, result);
			}
		}
	}
}

