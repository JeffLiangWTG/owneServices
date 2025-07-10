using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransportConsignment.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransportConsignment.Triggers.Testing
{
	[TestedType(typeof(TG_DtbConsignmentAddressesDoNotHaveDuplicateSequenceNumbers))]
	class TG_DtbConsignmentAddressesDoNotHaveDuplicateSequenceNumbersTest : DBCreateTriggerScriptTest
	{
	}

	class TG_DtbConsignmentAddressesDoNotHaveDuplicateSequenceNumbersNonTransactionedTest : TestCase
	{
		const string ErrorMessage = "There are consignment addresses with duplicate sequence numbers.";
		const string TriggerName = "TG_DtbConsignmentAddressesDoNotHaveDuplicateSequenceNumbers";
		const string ProcedureName = "DtbCheckForDuplicateConsignmentAddressSequenceNumber";

		[UseSnapshotProtection]
		public void TestTriggerOnInsertCallsStoredProcIfNotDeferred()
		{
			var consignment1 = SetupConsignmentForInsertTest();

			var insertSql = new SqlQueryBuilder();
			var consignmentAddress1 = new DtbConsignmentAddress(consignment1, "INC") { LTS_InstructionType = "PIC", LTS_Sequence = 1 }.AppendInsertAndReturnObject(insertSql);

			AssertCheckProcedureRanForConsignments("If trigger is not suspended then check procedure should be run.", insertSql.ToString(), new[] { consignment1 });
		}

		[UseSnapshotProtection]
		public void TestTriggerOnInsertDoesNotCallStoredProcIfDeferred()
		{
			var consignment = SetupConsignmentForInsertTest();

			var insertSql = new SqlQueryBuilder();
			var consignmentAddress = new DtbConsignmentAddress(consignment, "INC") { LTS_InstructionType = "PIC", LTS_Sequence = 1 }.AppendInsertAndReturnObject(insertSql);

			insertSql.Prepend($"SuspendTrigger '{TriggerName}'\r\n");

			AssertNoExceptionThrown(
				"If trigger is suspended then check procedure should not be run.",
				() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(Db.Connection, ProcedureName, insertSql.ToString()));
		}

		[UseSnapshotProtection]
		public void TestTriggerOnUpdateCallsStoredProcIfNotDeferredAndSequenceUpdated()
		{
			var (consignment1, consignment2, consignmentAddress1) = SetupConsignmentAddressForUpdateTest();

			var updateSql = $@"
UPDATE {DtbConsignmentAddressSchema.Constants.SqlSchemaName}.{DtbConsignmentAddressSchema.Constants.TableName} 
SET {DtbConsignmentAddressSchema.Constants.LTS_Sequence} = 2 
WHERE LTS_PK = '{consignmentAddress1.PK}'
";
			AssertCheckProcedureRanForConsignments(
				"If trigger is not suspended and we have an update and sequence is updated then check procedure should be run",
				updateSql,
				new[] { consignment1 });
		}

		[UseSnapshotProtection]
		public void TestTriggerOnUpdateCallsStoredProcIfNotDeferredAndConsignmentUpdated()
		{
			var (consignment1, consignment2, consignmentAddress1) = SetupConsignmentAddressForUpdateTest();

			var updateSql = $@"
UPDATE {DtbConsignmentAddressSchema.Constants.SqlSchemaName}.{DtbConsignmentAddressSchema.Constants.TableName} 
SET {DtbConsignmentAddressSchema.Constants.LTS_LTC_Consignment} = '{consignment2.PK}'
WHERE LTS_PK = '{consignmentAddress1.PK}'
";
			AssertCheckProcedureRanForConsignments(
				"If trigger is not suspended and we have an update and parent consignment is updated then check procedure should be run",
				updateSql,
				new[] { consignment2 });
		}

		[UseSnapshotProtection]
		public void TestTriggerOnUpdateDoesNotCallStoredProcIfNotDeferredAndSequenceAndConsignmentNotUpdated()
		{
			var (consignment1, consignment2, consignmentAddress1) = SetupConsignmentAddressForUpdateTest();

			var updateSql = $@"
UPDATE {DtbConsignmentAddressSchema.Constants.SqlSchemaName}.{DtbConsignmentAddressSchema.Constants.TableName} 
SET {DtbConsignmentAddressSchema.Constants.LTS_Notes} = 'test update'
WHERE LTS_PK = '{consignment1.PK}'
";
			AssertNoExceptionThrown(
				"If trigger is not suspended and we have an update and neither sequence nor parent consignment are updated then check procedure should not be run",
				() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(Db.Connection, ProcedureName, updateSql));
		}

		[UseSnapshotProtection]
		public void TestTriggerOnUpdateDoesNotCallStoredProcIfDeferred()
		{
			var (consignment1, consignment2, consignmentAddress1) = SetupConsignmentAddressForUpdateTest();

			var updateSql = $@"
SuspendTrigger {TriggerName};
UPDATE {DtbConsignmentAddressSchema.Constants.SqlSchemaName}.{DtbConsignmentAddressSchema.Constants.TableName} 
SET {DtbConsignmentAddressSchema.Constants.LTS_Notes} = 'test update'
WHERE LTS_PK = '{consignment1.PK}'
";
			AssertNoExceptionThrown(
				"If trigger is suspended and we have an update and neither sequence nor parent consignment are updated then check procedure should not be run",
				() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(Db.Connection, ProcedureName, updateSql));
		}

		void AssertCheckProcedureRanForConsignments(string errorMessage, string sqlToRun, params DtbConsignment[] expectedConsignmentsInTheProcedure)
		{
			TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message == ErrorMessage);
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(Db.Connection, ProcedureName, sqlToRun, expectedConsignmentsInTheProcedure);
			AssertNotNull(errorMessage, actualPKsInTheProcedure);
			AssertContainsExactElementsInAnyOrder(errorMessage, expectedConsignmentsInTheProcedure.Select(l => l.PK), actualPKsInTheProcedure);
		}

		void ExecuteSqlInTransaction(string sql)
		{
			using (Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery(sql);
				Db.Connection.CommitTransaction();
			}
		}

		DtbConsignment SetupConsignmentForInsertTest()
		{
			var sql = new SqlQueryBuilder();
			var consignment1 = CreateTestConsignment("CN1", sql);

			ExecuteSqlInTransaction(sql.ToStringWithNewLineBetweenAppends());

			return consignment1;
		}

		DtbConsignment CreateTestConsignment(string jobID, SqlQueryBuilder sql)
		{
			return new DtbConsignment(jobID, "LTL")
			{
				LTC_Direction = "LOC",
				LTC_Status = "BKD",
				LTC_SystemCreateTimeUtc = DateTime.UtcNow,
				LTC_SystemCreateUser = "XXX",
				LTC_SystemLastEditTimeUtc = DateTime.UtcNow,
				LTC_SystemLastEditUser = "XXX"
			}.AppendInsertAndReturnObject(sql);
		}

		(DtbConsignment consignment, DtbConsignment otherConsignment, DtbConsignmentAddress consignmentAddress) SetupConsignmentAddressForUpdateTest()
		{
			var sql = new SqlQueryBuilder();
			var consignment = CreateTestConsignment("CN1", sql);
			var otherConsignment = CreateTestConsignment("CN2", sql);
			var consignmentAddress = new DtbConsignmentAddress(consignment, "INC") { LTS_InstructionType = "PIC", LTS_Sequence = 1 }.AppendInsertAndReturnObject(sql);
			ExecuteSqlInTransaction(sql.ToStringWithNewLineBetweenAppends());

			return (consignment, otherConsignment, consignmentAddress);
		}
	}
}

