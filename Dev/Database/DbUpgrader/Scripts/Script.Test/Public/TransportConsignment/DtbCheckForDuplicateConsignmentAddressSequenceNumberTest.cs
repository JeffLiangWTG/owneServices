using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransportConsignment;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransportConsignment.Testing
{
	[TestedType(typeof(DtbCheckForDuplicateConsignmentAddressSequenceNumber))]
	class DtbCheckForDuplicateConsignmentAddressSequenceNumberTest : DbCreateScriptTest
	{
		const string CheckForDuplicateConsignmentAddressSequenceNumberTrigger = "TG_DtbConsignmentAddressesDoNotHaveDuplicateSequenceNumbers";

		public void TestCheckDuplicateConsignmentAddressSequenceWhenConsignmentPKInRange()
		{
			var sql = new SqlQueryBuilder();
			using (TestConnection.BeginTransactionWithManager())
			{
				using (TestWhsDataSetupHelper.SuspendTrigger(CheckForDuplicateConsignmentAddressSequenceNumberTrigger, DtbConsignmentAddressSchema.Constants.TableName, TestConnection))
				{
					var consignment1 = CreateTestConsignment("CN1", sql);
					var consignmentAddress1 = new DtbConsignmentAddress(consignment1, "INC") { LTS_InstructionType = "PIC", LTS_Sequence = 1 }.AppendInsertAndReturnObject(sql);
					var consignmentAddress2 = new DtbConsignmentAddress(consignment1, "INC") { LTS_InstructionType = "DLV", LTS_Sequence = 1 }.AppendInsertAndReturnObject(sql);
					TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

					RunCheckForDuplicateSequenceNumber(new[] { consignment1.PK }, true);
				}
			}
		}

		public void TestCheckDuplicateConsignmentAddressSequenceWhenConsignmentPKNotInRange()
		{
			var sql = new SqlQueryBuilder();
			using (TestConnection.BeginTransactionWithManager())
			{
				using (TestWhsDataSetupHelper.SuspendTrigger(CheckForDuplicateConsignmentAddressSequenceNumberTrigger, DtbConsignmentAddressSchema.Constants.TableName, TestConnection))
				{
					var consignment1 = CreateTestConsignment("CN1", sql);
					var consignment2 = CreateTestConsignment("CN2", sql);
					var consignmentAddress1 = new DtbConsignmentAddress(consignment1, "INC") { LTS_InstructionType = "PIC", LTS_Sequence = 1 }.AppendInsertAndReturnObject(sql);
					var consignmentAddress2 = new DtbConsignmentAddress(consignment1, "INC") { LTS_InstructionType = "DLV", LTS_Sequence = 1 }.AppendInsertAndReturnObject(sql);
					TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

					RunCheckForDuplicateSequenceNumber(new[] { consignment2.PK }, false);
				}
			}
		}

		public void TestCheckDuplicateConsignmentAddressSequenceWhenSameSequenceNumberOnDifferentConsignments()
		{
			var sql = new SqlQueryBuilder();
			using (TestConnection.BeginTransactionWithManager())
			{
				using (TestWhsDataSetupHelper.SuspendTrigger(CheckForDuplicateConsignmentAddressSequenceNumberTrigger, DtbConsignmentAddressSchema.Constants.TableName, TestConnection))
				{
					var consignment1 = CreateTestConsignment("CN1", sql);
					var consignment2 = CreateTestConsignment("CN2", sql);
					var consignmentAddress1 = new DtbConsignmentAddress(consignment1, "INC") { LTS_InstructionType = "PIC", LTS_Sequence = 1 }.AppendInsertAndReturnObject(sql);
					var consignmentAddress2 = new DtbConsignmentAddress(consignment2, "INC") { LTS_InstructionType = "PIC", LTS_Sequence = 1 }.AppendInsertAndReturnObject(sql);
					TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

					RunCheckForDuplicateSequenceNumber(new[] { consignment1.PK, consignment2.PK }, false);
				}
			}
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

		void RunCheckForDuplicateSequenceNumber(Guid[] consignmentPKs, bool expectingException)
		{
			const string sql =
@"
EXEC DtbCheckForDuplicateConsignmentAddressSequenceNumber @ConsignmentPKs;
";
			using (var sqlCommand = TestConnection.Command(sql))
			{
				sqlCommand.AddTableValuedParameter("@ConsignmentPKs", "dbo.TVP_uniqueidentifier", consignmentPKs);
				if (expectingException)
				{
					TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message == "There are duplicate consignment address sequence numbers on the same consignment.");
					AssertExceptionThrown(typeof(SqlException), "There are duplicate consignment address sequence numbers on the same consignment.", () => sqlCommand.ExecuteNonQuery());
				}
				else
				{
					AssertNoExceptionThrown(() => sqlCommand.ExecuteNonQuery());
				}
			}
		}
	}
}

