using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.HRM
{
	[TestedType(typeof(TG_UPD_HrlBalanceAffectingQueue))]
	class TG_UPD_HrlBalanceAffectingQueueTest : DbCreateScriptTest
	{
		public void TestUpdateAndDelete()
		{
			var staffPk = Guid.NewGuid();
			var balanceAffectingQueuePk = Guid.NewGuid();
			var llq_pk = balanceAffectingQueuePk.ToString().QuoteName('\'');

			CreateStaff(staffPk, "ABC");
			CreateHrlBalanceAffectingQueue(balanceAffectingQueuePk, 1, "Leave Approved", new DateTimeOffset(new DateTime(2020, 2, 15)), staffPk, new DateTime(2020, 2, 15), "ABC");
			AssertExceptionThrown<SqlException>("Could not update", "Update operation is NOT allowed on HrlBalanceAffectingQueue", () => Db.Connection.ExecuteNonQuery($"UPDATE dbo.HrlBalanceAffectingQueue SET LLQ_Reference = 'Cancel Leave' WHERE LLQ_PK = {llq_pk};"));
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery($"DELETE FROM dbo.HrlBalanceAffectingQueue WHERE LLQ_PK = {llq_pk};"));
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

		void CreateHrlBalanceAffectingQueue(Guid pk, int autoversion, string reference, DateTimeOffset effectiveDate, Guid staffPk, DateTime systemCreateTimeUtc, string systemCreateUser)
		{
			const string sqlText = @"
INSERT INTO dbo.HrlBalanceAffectingQueue (LLQ_PK, LLQ_AutoVersion, LLQ_Reference, LLQ_EffectiveDate, LLQ_GS_Staff, LLQ_SystemCreateTimeUtc, LLQ_SystemCreateUser)
VALUES (@PK, @AutoVersion, @Reference, @EffectiveDate, @GS_Staff, @SystemCreateTimeUtc, @SystemCreateUser)
";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@AutoVersion", SqlDbType.SmallInt, autoversion);
				cmd.AddParameter("@Reference", SqlDbType.NVarChar, reference);
				cmd.AddParameter("@EffectiveDate", SqlDbType.DateTimeOffset, effectiveDate);
				cmd.AddParameter("@GS_Staff", SqlDbType.UniqueIdentifier, staffPk);
				cmd.AddParameter("@SystemCreateTimeUtc", SqlDbType.DateTime, systemCreateTimeUtc);
				cmd.AddParameter("@SystemCreateUser", SqlDbType.VarChar, systemCreateUser);

				cmd.ExecuteNonQuery();
			}
		}
	}
}
