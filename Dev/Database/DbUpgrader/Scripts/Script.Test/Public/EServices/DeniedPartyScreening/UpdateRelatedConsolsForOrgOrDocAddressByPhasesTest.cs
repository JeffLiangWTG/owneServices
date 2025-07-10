using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.EServices.DeniedPartyScreening;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.EServices.DeniedPartyScreening.Testing
{
	[TestedType(typeof(UpdateRelatedConsolsForOrgOrDocAddressByPhases))]
	class UpdateRelatedConsolsForOrgOrDocAddressByPhasesTest : DbCreateScriptTest
	{
		//Further tested in ScreeningUpdaterTest.

		public void TestTransactionCountBeforeExecutionIsTheSameAsAfterExecution()
		{
			DpsUpdateRelatedJobsByPhasesTestHelper.AssertTransactionCountBeforeExecutionIsTheSameAsAfterExecution(TestConnection, ScriptToTest.Name, JobConsolSchema.Constants.TableName);
		}
	}

	static class DpsUpdateRelatedJobsByPhasesTestHelper
	{
		public static void AssertTransactionCountBeforeExecutionIsTheSameAsAfterExecution(DbConnection connection, string procedureName, string jobTable)
		{
			Assertion.AssertEquals("Connection must be in a transaction to execute this test. Please use a TransactionedTestCase.", 1, connection.AppTransactionCount);

			var forceErrorTriggerSql = $@"
				CREATE TRIGGER TG_TestTransactionCountBeforeExecutionIsTheSameAsAfterExecution ON dbo.{jobTable}
				AFTER UPDATE AS
				BEGIN
					RAISERROR('FORCED_ERROR_FOR_TEST', 16, 1);
				END";

			connection.ExecuteNonQuery(forceErrorTriggerSql);

			var executeProcWithErrorHandlingSql = $@"
				DECLARE @TrancountBefore int = @@TRANCOUNT;
				DECLARE @TrancountAfter int;
				DECLARE @PhaseListTvp dbo.TVP_char_3;
				DECLARE @UserCode varchar(3) = 'TST';
		
				BEGIN TRY
					EXEC {procedureName}
						@entityPK = '00000000-0000-0000-0000-000000000000',
						@companyPK = '00000000-0000-0000-0000-000000000000',
						@phaseList = @PhaseListTvp,
						@userCode = @UserCode;
				END TRY
				BEGIN CATCH
					IF (ERROR_MESSAGE() <> 'FORCED_ERROR_FOR_TEST') THROW;
					SET @TrancountAfter = @@TRANCOUNT;
					ROLLBACK;
				END CATCH

				IF (@TrancountAfter is null)
				BEGIN
					THROW 50000, 'Test Forced Exception Was Not Thrown By Procedure. Modify Test To Ensure An Error Occurs.', 1;
				END
				ELSE IF (@TrancountAfter = @TrancountBefore)
				BEGIN
					SELECT @TrancountAfter;
				END
				ELSE
				BEGIN
					DECLARE @DiffTrancountError VARCHAR(100) = 'Transaction Count Changed => Before = ' + CONVERT(char(1), @TrancountBefore) + ', After = ' + CONVERT(char(1), @TrancountAfter) + '.';
					THROW 50000, @DiffTrancountError, 1;
				END";

			var transactionCount = connection.ExecuteScalar(executeProcWithErrorHandlingSql);
			Assertion.AssertEquals("Transaction count before and after execution", 1, transactionCount);
		}
	}
}

