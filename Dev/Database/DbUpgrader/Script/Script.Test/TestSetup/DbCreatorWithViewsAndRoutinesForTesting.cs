using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Script.Test.TestSetup
{
	sealed class DbCreatorWithViewsAndRoutinesForTesting : AuxiliaryDbCreator
	{
		public DbCreatorWithViewsAndRoutinesForTesting(string dbName)
			: base(dbName)
		{
		}

		protected override void SetupDatabaseAfterCreation(DbConnection conn)
		{
			CreateTestTableViewsAndRoutines(conn);
		}

		void CreateTestTableViewsAndRoutines(DbConnection conn)
		{
			conn.ExecuteNonQuery(createTestTableScript);
			conn.ExecuteNonQuery(createTestDbViewScript);
			conn.ExecuteNonQuery(createTestDbFunctionScript);
			conn.ExecuteNonQuery(createTestDbClientFunctionScript);
			conn.ExecuteNonQuery(createTestDbProcedureScript);
			conn.ExecuteNonQuery(CreateAlienSchema);
			conn.ExecuteNonQuery(CreateDuplicatedAlienProcedure);
		}

		#region Create Scripts

		const string createTestTableScript = @"
			-- This table is needed to get/set version info
			CREATE TABLE dbo.StmData
			( 
				[SD_PK] UNIQUEIDENTIFIER NOT NULL,
				[SD_Name] VARCHAR(300) NOT NULL DEFAULT '',
				[SD_Owner] UNIQUEIDENTIFIER NULL,
				[SD_DepartmentGuid] UNIQUEIDENTIFIER NULL,
				[SD_Type] CHAR(3) NOT NULL DEFAULT '',
				[SD_IsLogged] BIT NOT NULL DEFAULT 0,
				[SD_BinaryValue] VARBINARY(MAX) NULL,
				[SD_GuidValue] UNIQUEIDENTIFIER NULL,
				[SD_IsCancelled] BIT NOT NULL DEFAULT 0,
				[SD_PreserveTestValue] BIT NOT NULL DEFAULT 0,
				[SD_SystemCreateTimeUtc] SMALLDATETIME NULL,
				[SD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
				[SD_SystemLastEditTimeUtc] SMALLDATETIME NULL,
				[SD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
			)";

		const string createTestDbViewScript = @"
			-- View with special character (eg.&) in the name. Dropping it should not blow up.
			CREATE VIEW [VW_&_Match_DefaultDiff] AS
				SELECT * FROM dbo.StmData";

		const string createTestDbFunctionScript = @"
			-- This Function should be removed
			CREATE FUNCTION Test_Function_01() RETURNS TABLE AS
				RETURN (SELECT 0 col1)";

		const string createTestDbClientFunctionScript = @"
			-- Function name starts with [Client]. Should NOT be droppedd.
			CREATE FUNCTION Client_TestClientFunction() RETURNS TABLE AS
				RETURN (SELECT 0 col1)";

		const string createTestDbProcedureScript = @"
			-- This Proc should be modified
			CREATE PROCEDURE XT_Test_Proc_01 AS
				SELECT 'nada'";

		const string CreateAlienSchema = "CREATE SCHEMA Alien;";
		const string CreateDuplicatedAlienProcedure = "CREATE PROCEDURE Alien.XT_Test_Proc_01 AS RETURN 0;";

		#endregion
	}
}
