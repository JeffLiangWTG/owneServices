using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.Asycuda;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Asycuda.Testing
{
	[TestedType(typeof(IncrementBGMReferenceCounter))]
	internal class IncrementBGMReferenceCounterTest : DbCreateScriptTest
	{
		public void TestExplicitTransactionProperlyManaged()
		{
			AssertExceptionThrown(
				"Attempt to execute procedure within an existing transaction context",
				typeof(SqlException),
				"This procedure must not run in a transaction context.",
				() => TestConnection.ExecuteNonQuery($@"
					DECLARE @LockName NVARCHAR(255);
					DECLARE @XA_Name VARCHAR(32);
					DECLARE @XA_ParentID UNIQUEIDENTIFIER;
					DECLARE @XA_SystemLastEditUser VARCHAR(3);
					DECLARE @incrementStep INT;
					EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name}
						@LockName = 'TestLock',
						@XA_Name = 'TestName',
						@XA_ParentID = 'D7CF5356-4266-42D5-8770-346F5F29E557',
						@XA_SystemLastEditUser = 'E',
						@incrementStep = '1'")
			);
		}

		public void TestInvalidParametersProperlyManaged()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var errorMsg = "Invalid Parameters. Any of the parameters @LockName or @XA_Name or @XA_ParentID should not be null or empty.";

				AssertExceptionThrown("Attempt to pass null parameters to the stored procedure", typeof(SqlException), errorMsg,
										() => connection.ExecuteNonQuery($"EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name} null, null, null, null, 1"));
				AssertExceptionThrown("Attempt to pass null @LockName to the stored procedure", typeof(SqlException), errorMsg,
										() => connection.ExecuteNonQuery($"EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name} null, 'TestName', 'D7CF5356-4266-42D5-8770-346F5F29E557', 'E', 1"));
				AssertExceptionThrown("Attempt to pass empty @LockName to the stored procedure", typeof(SqlException), errorMsg,
										() => connection.ExecuteNonQuery($"EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name} '', 'TestName', 'D7CF5356-4266-42D5-8770-346F5F29E557', 'E', 1"));
				AssertExceptionThrown("Attempt to pass null @XA_Name to the stored procedure", typeof(SqlException), errorMsg,
										() => connection.ExecuteNonQuery($"EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name} '', null, 'D7CF5356-4266-42D5-8770-346F5F29E557', 'E', 1"));
				AssertExceptionThrown("Attempt to pass empty @XA_Name to the stored procedure", typeof(SqlException), errorMsg,
										() => connection.ExecuteNonQuery($"EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name} 'TestLock', '', 'D7CF5356-4266-42D5-8770-346F5F29E557', 'E', 1"));
				AssertExceptionThrown("Attempt to pass empty @XA_ParentID to the stored procedure", typeof(SqlException), errorMsg,
										() => connection.ExecuteNonQuery($"EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name} 'TestLock', 'TestName', null, 'E', 1"));
				AssertExceptionThrown("Attempt to pass empty @XA_ParentID to the stored procedure", typeof(SqlException), errorMsg,
										() => connection.ExecuteNonQuery($"EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name} 'TestLock', 'TestName', '00000000-0000-0000-0000-000000000000', 'E', 1"));

				AssertExceptionThrown("Attempt to pass negative @incrementStep to the stored procedure", typeof(SqlException), "The parameter @incrementStep should not be negative.",
						() => connection.ExecuteNonQuery($"EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name} 'TestLock', 'TestName', 'D7CF5356-4266-42D5-8770-346F5F29E557', 'E', -1"));
			}
		}

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestInvalidXADataProperlyManaged()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.ExecuteNonQuery(@"
IF NOT EXISTS(Select 1 from dbo.GenAddOnColumn where XA_Name = 'TestName' and XA_ParentID = 'D7CF5356-4266-42D5-8770-346F5F29E557')
INSERT INTO dbo.GenAddOnColumn ([XA_PK],[XA_Name] ,[XA_Type],[XA_Data],[XA_ParentTableCode],[XA_ParentID])
VALUES(newid(), 'TestName', 'TST', 'NotIntData', 'JE', 'D7CF5356-4266-42D5-8770-346F5F29E557')");

				AssertExceptionThrown("Have Invalid XAData", typeof(SqlException), "Conversion failed when converting the varchar value 'NotIntData' to data type int.",
						() => connection.ExecuteNonQuery($"EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name} 'TestLock', 'TestName', 'D7CF5356-4266-42D5-8770-346F5F29E557', 'E', 1"));
			}
		}
	}
}

