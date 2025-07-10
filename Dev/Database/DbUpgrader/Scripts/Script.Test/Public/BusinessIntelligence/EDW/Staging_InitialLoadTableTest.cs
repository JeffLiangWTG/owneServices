using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Testing
{
	[TestedType(typeof(Staging_InitialLoadTable))]
	internal class Staging_InitialLoadTableTest : BiCreateScriptTest
	{
		public void TestExplicitTransactionProperlyManaged()
		{
			int tranCountBefore = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count before execution", 1, tranCountBefore);
			TestConnection.ExecuteNonQuery($@"
					EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name}
						@table_id = 1,
						@server_name = '[{Db.ServerName}]',
						@batch_size = 0,
						@IsInTransaction = 0;");
			int tranCountAfter = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count after execution", tranCountBefore, tranCountAfter);
		}

		public void TestContainsTPTTComments()
		{
			CheckContainsTPTTComments("Ini", 2);
		}

		protected override DbConnection TestConnection // it needs AdminConnection for linked server
		{
			get { return adminConnection ?? (adminConnection = Db.NewAdminConnection()); }
		}
		AdminConnection adminConnection;

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}

