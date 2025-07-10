using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Testing
{
	[TestedType(typeof(Transform_MasterTransform))]
	internal class Transform_MasterTransformTest : BiCreateScriptTest
	{
		public void TestExplicitTransactionProperlyManaged()
		{
			AssertEquals("Script has 'SET XACT_ABORT ON'", true, ScriptToTest.Text.IndexOf("SET XACT_ABORT ON", StringComparison.OrdinalIgnoreCase) >= 0);

			int tranCountBefore = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count before execution", 1, tranCountBefore);
			TestConnection.ExecuteNonQuery($@"
					DECLARE @ErrorCode int;
					EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name}
						@truncate_model_tables = 0,
						@print_messages = 0,
						@error_code = @ErrorCode OUTPUT;");
			int tranCountAfter = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count after execution", tranCountBefore, tranCountAfter);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}

