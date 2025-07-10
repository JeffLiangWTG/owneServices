using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Common.Testing
{
	[TestedType(typeof(usp_SetMasterStateParameter))]
	internal class usp_SetMasterStateParameterEdwTest : BiCreateScriptTest
	{
		public void TestExplicitTransactionProperlyManaged()
		{
			int tranCountBefore = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count before execution", 1, tranCountBefore);
			TestConnection.ExecuteNonQuery($@"
					DECLARE @ParamName NVARCHAR(MAX);
					DECLARE @ParamValue NVARCHAR(MAX);
					EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name}
						@ParamName = @ParamName,
						@ParamValue = @ParamValue;
			");
			int tranCountAfter = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count after execution", tranCountBefore, tranCountAfter);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}

	[TestedType(typeof(usp_SetMasterStateParameter))]
	internal class usp_SetMasterStateParameterAuditTest : BiCreateScriptTest
	{
		public void TestExplicitTransactionProperlyManaged()
		{
			int tranCountBefore = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count before execution", 1, tranCountBefore);
			TestConnection.ExecuteNonQuery($@"
					DECLARE @ParamName NVARCHAR(MAX);
					DECLARE @ParamValue NVARCHAR(MAX);
					EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name}
						@ParamName = @ParamName,
						@ParamValue = @ParamValue;
			");
			int tranCountAfter = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count after execution", tranCountBefore, tranCountAfter);
		}

		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}
	}
}

