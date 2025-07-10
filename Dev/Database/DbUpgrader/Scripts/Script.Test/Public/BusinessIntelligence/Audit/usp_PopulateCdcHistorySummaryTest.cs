using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	[TestedType(typeof(usp_PopulateCdcHistorySummary))]
	internal class usp_PopulateCdcHistorySummaryTest : BiCreateScriptTest
	{
		public void TestExplicitTransactionProperlyManaged()
		{
			AssertEquals("Script has 'SET XACT_ABORT ON'", true, ScriptToTest.Text.IndexOf("SET XACT_ABORT ON", StringComparison.OrdinalIgnoreCase) >= 0);

			int tranCountBefore = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count before execution", 1, tranCountBefore);
			TestConnection.ExecuteNonQuery($@"
					DECLARE @ErrorCode int, @ErrorNumber int, @ErrorMessage nvarchar(max);
					EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name}
						@print_messages = 1,
						@error_code = @ErrorCode OUTPUT,
						@error_number = @ErrorNumber OUTPUT,
						@error_message = @ErrorMessage OUTPUT;");
			int tranCountAfter = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count after execution", tranCountBefore, tranCountAfter);
		}

		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}
	}
}

