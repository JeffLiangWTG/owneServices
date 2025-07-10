using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.HRM
{
	[TestedType(typeof(TG_UPD_DEL_HrlBalanceTransaction))]
	class TG_UPD_DEL_HrlBalanceTransactionTest : DbCreateScriptTest
	{
		public void TestUpdateAndDelete()
		{
			var balanceTransactionPk = Guid.NewGuid();
			var processingRunPk = Guid.NewGuid();
			var staffPk = Guid.NewGuid();

			CreateStaff(staffPk, "ABC");
			CreateHrlProcessingRun(processingRunPk, 1, "QUE", new DateTime(2020, 2, 16), "AU", new DateTime(2020, 3, 16), "ABC",
				new DateTime(2020, 3, 16), "ABC");
			CreateHrlBalanceTransaction(balanceTransactionPk, 1, staffPk, "ANN", new DateTimeOffset(new DateTime(2020, 2, 15)),
				new DateTimeOffset(new DateTime(2020, 2, 15)), processingRunPk, "123", 1.23m, new DateTime(2020, 3, 16), "ABC",
				new DateTime(2020, 3, 16), "ABC", "PRO");

			var llt_pk = balanceTransactionPk.ToString().QuoteName('\'');
			AssertExceptionThrown<SqlException>("Could not delete", "Update/Delete operation is NOT allowed on HrlBalanceTransaction", () => Db.Connection.ExecuteNonQuery($"DELETE FROM dbo.HrlBalanceTransaction WHERE LLT_PK = {llt_pk};"));
			AssertExceptionThrown<SqlException>("Could not update", "Update/Delete operation is NOT allowed on HrlBalanceTransaction", () => Db.Connection.ExecuteNonQuery($"UPDATE dbo.HrlBalanceTransaction SET LLT_Comment = '456' WHERE LLT_PK = {llt_pk};"));
		}

		void CreateStaff(Guid pk, string staffCode)
		{
			var sql = "INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@PK, @StaffCode, @StaffCode, GETUTCDATE(), 'E', GETUTCDATE(), 'E')";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@StaffCode", SqlDbType.VarChar, staffCode);
				cmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseDateTimeForDuration", Justification = "Baseline")]
		void CreateHrlBalanceTransaction(Guid pk, int autoversion, Guid staffPk, string leaveType, DateTimeOffset forfeiture,
			DateTimeOffset accrual, Guid processingRunPk, string comment, decimal deltaValueHours,  DateTime systemCreateTimeUtc,
			string systemCreateUser, DateTime systemLastEditTimeUtc, string systemLastEditUser, string transactionType)
		{
			const string sqlText = @"
INSERT INTO dbo.HrlBalanceTransaction (LLT_PK, LLT_AutoVersion, LLT_GS_Staff, LLT_LeaveType, LLT_Forfeiture, LLT_Accrual,
	LLT_LLR_ProcessingRun, LLT_Comment, LLT_DeltaValueHours, LLT_SystemCreateTimeUtc, LLT_SystemCreateUser,
	LLT_SystemLastEditTimeUtc, LLT_SystemLastEditUser, LLT_TransactionType)
VALUES (@PK, @AutoVersion, @GS_Staff, @LeaveType, @Forfeiture, @Accrual, @LLR_ProcessingRun, @Comment, @DeltaValueHours,
	@SystemCreateTimeUtc, @SystemCreateUser, @SystemLastEditTimeUtc, @SystemLastEditUser, @TransactionType)
";
			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@AutoVersion", SqlDbType.SmallInt, autoversion);
				cmd.AddParameter("@GS_Staff", SqlDbType.UniqueIdentifier, staffPk);
				cmd.AddParameter("@LeaveType", SqlDbType.VarChar, leaveType);
				cmd.AddParameter("@Forfeiture", SqlDbType.DateTimeOffset, forfeiture);
				cmd.AddParameter("@Accrual", SqlDbType.DateTimeOffset, accrual);
				cmd.AddParameter("@LLR_ProcessingRun", SqlDbType.UniqueIdentifier, processingRunPk);
				cmd.AddParameter("@Comment", SqlDbType.NVarChar, comment);
				cmd.AddParameter("@DeltaValueHours", SqlDbType.Decimal, deltaValueHours);
				cmd.AddParameter("@SystemCreateTimeUtc", SqlDbType.DateTime, systemCreateTimeUtc);
				cmd.AddParameter("@SystemCreateUser", SqlDbType.VarChar, systemCreateUser);
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.DateTime, systemLastEditTimeUtc);
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, systemLastEditUser);
				cmd.AddParameter("@TransactionType", SqlDbType.Char, transactionType);
				cmd.ExecuteNonQuery();
			}
		}

		void CreateHrlProcessingRun(Guid pk, int autoversion, string status,
			DateTime processTo, string rn_NKCountry, DateTime systemCreateTimeUtc,
			string systemCreateUser, DateTime systemLastEditTimeUtc, string systemLastEditUser)
		{
			const string sqlText = @"
INSERT INTO dbo.HrlProcessingRun (LLR_PK, LLR_AutoVersion, LLR_Status, LLR_ProcessTo, LLR_RN_NKCountry,
	LLR_SystemCreateTimeUtc, LLR_SystemCreateUser,
	LLR_SystemLastEditTimeUtc, LLR_SystemLastEditUser)
VALUES (@PK, @AutoVersion, @Status, @ProcessTo, @RN_NKCountry,@SystemCreateTimeUtc, @SystemCreateUser,
	@SystemLastEditTimeUtc, @SystemLastEditUser)
";
			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@AutoVersion", SqlDbType.SmallInt, autoversion);
				cmd.AddParameter("@Status", SqlDbType.VarChar, status);
				cmd.AddParameter("@ProcessTo", SqlDbType.DateTime, processTo);
				cmd.AddParameter("@RN_NKCountry", SqlDbType.VarChar, rn_NKCountry);
				cmd.AddParameter("@SystemCreateTimeUtc", SqlDbType.DateTime, systemCreateTimeUtc);
				cmd.AddParameter("@SystemCreateUser", SqlDbType.VarChar, systemCreateUser);
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.DateTime, systemLastEditTimeUtc);
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, systemLastEditUser);

				cmd.ExecuteNonQuery();
			}
		}
	}
}
