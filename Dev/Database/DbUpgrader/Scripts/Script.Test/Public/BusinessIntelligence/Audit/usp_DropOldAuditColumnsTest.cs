using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	[TestedType(typeof(usp_DropOldAuditColumns))]
	internal class usp_DropOldAuditColumnsTest : BiCreateScriptTest
	{
		public void TestExplicitTransactionProperlyManaged()
		{
			AssertEquals("Script has 'SET XACT_ABORT ON'", true, ScriptToTest.Text.IndexOf("SET XACT_ABORT ON", StringComparison.OrdinalIgnoreCase) >= 0);

			AssertExceptionThrown(
				"Attempt to execute procedure within an existing transaction context",
				typeof(SqlException),
				"This procedure must not run in a transaction context.",
				() => TestConnection.ExecuteNonQuery($@"
					DECLARE @InfoMessage varchar(max);
					DECLARE @ErrorCode int;
					EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name}
						@AuditRetentionPeriod = 0,
						@InfoMessage = @InfoMessage OUTPUT,
						@ErrorCode = @ErrorCode OUTPUT;")
			);
		}

		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}
	}
}

