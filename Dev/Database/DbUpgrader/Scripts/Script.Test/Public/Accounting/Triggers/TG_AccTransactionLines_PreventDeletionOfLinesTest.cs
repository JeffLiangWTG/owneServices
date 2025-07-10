using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccTransactionLines_PreventDeletionOfLines))]
	class TG_AccTransactionLines_PreventDeletionOfLinesTest : DbCreateScriptTest
	{
		[UseSnapshotProtection]
		public void TestTriggerPreventedAccTransactionLinesFromBeingDeletedWithTypeREV() =>
			AssertTriggerPreventedAccTransactionLinesFromBeingDeleted("REV", () => DbHelper.InsertTransactionLine(lineType: "REV"));

		[UseSnapshotProtection]
		public void TestTriggerPreventedAccTransactionLinesFromBeingDeletedWithTypeCST() =>
			AssertTriggerPreventedAccTransactionLinesFromBeingDeleted("CST", () => DbHelper.InsertTransactionLine(lineType: "CST"));

		[UseSnapshotProtection]
		public void TestTriggerPreventedAccTransactionLinesFromBeingDeletedWithTypeWIP() =>
			AssertTriggerPreventedAccTransactionLinesFromBeingDeleted("WIP", () =>
			{
				var glAccountPK = DbHelper.InsertGLAccount("111.222.01", "TestGLAccount 1");
				return DbHelper.InsertTransactionLine(lineType: "WIP", glAccountPK: glAccountPK);
			});

		[UseSnapshotProtection]
		public void TestTriggerPreventedAccTransactionLinesFromBeingDeletedWithTypeACR() =>
			AssertTriggerPreventedAccTransactionLinesFromBeingDeleted("ACR", () =>
			{
				var glAccountPK = DbHelper.InsertGLAccount("111.222.01", "TestGLAccount 1");
				return DbHelper.InsertTransactionLine(lineType: "ACR", glAccountPK: glAccountPK);
			});

		void AssertTriggerPreventedAccTransactionLinesFromBeingDeleted(string lineType, Func<Guid> lineInsertion)
		{
			var invoiceLinePK = lineInsertion.Invoke();

			TestConnection.CommitTransaction();

			var getRecordCountSQL = $"SELECT COUNT(1) FROM dbo.Acctransactionlines WHERE AL_PK = '{invoiceLinePK}'";
			AssertEquals("Acctransactionline should be in database", 1, (int)TestConnection.ExecuteScalar(getRecordCountSQL));

			using (TestConnection.BeginTransactionWithManager())
			{
				var deleteSQL = $"DELETE FROM dbo.Acctransactionlines WHERE AL_PK = '{invoiceLinePK}'";
				var sqlException = AssertExceptionThrown<SqlException>("Should have an error when deleting Acctransactionlines with type not allowed to delete.", () => TestConnection.ExecuteNonQuery(deleteSQL));
				AssertContains($"Attempting to delete transaction line of type {lineType}", sqlException.Message);

				var transactionException = AssertExceptionThrown<TransactionException>("Transaction should be rolled back", () => TestConnection.ExecuteScalar(getRecordCountSQL));
				AssertContains("Transaction has been rolled back in the server", transactionException.Message);
			}

			TestConnection.BeginTransaction();
			AssertEquals("Acctransactionline should be in database", 1, (int)TestConnection.ExecuteScalar(getRecordCountSQL));
		}

		public void TestTriggerAllowAccTransactionLinesToBeDeletedWithAllowedType()
		{
			var invoiceLinePK = DbHelper.InsertTransactionLine(lineType: "UCT");
			var getRecordCountSQL = $"SELECT COUNT(1) FROM dbo.Acctransactionlines WHERE AL_PK = '{invoiceLinePK}'";
			AssertEquals("Acctransactionline should be in database", 1, (int)TestConnection.ExecuteScalar(getRecordCountSQL));

			AssertNoExceptionThrown("Other types can be deleted.", () => TestConnection.ExecuteNonQuery($"DELETE FROM dbo.Acctransactionlines WHERE AL_PK = '{invoiceLinePK}'"));
			AssertEquals("Acctransactionlines should not be in database", 0, (int)TestConnection.ExecuteScalar(getRecordCountSQL));
		}
	}
}

