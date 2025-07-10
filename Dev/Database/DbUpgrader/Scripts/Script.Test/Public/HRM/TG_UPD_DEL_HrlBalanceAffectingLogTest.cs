using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.HRM
{
	[TestedType(typeof(TG_UPD_DEL_HrlBalanceAffectingLog))]
	class TG_UPD_DEL_HrlBalanceAffectingLogTest : DbCreateScriptTest
	{
		public void TestUpdateAndDelete()
		{
			var staffPk = Guid.NewGuid();
			var balanceAffectingLogPk = Guid.NewGuid();
			var llb_pk = balanceAffectingLogPk.ToString().QuoteName('\'');

			CreateStaff(staffPk, "ABC");
			CreateHrlBalanceAffectingLog(balanceAffectingLogPk, 1, "Leave Approved", new DateTimeOffset(new DateTime(2020, 2, 15)), staffPk, new DateTime(2020, 2, 15), "ABC");
			AssertExceptionThrown<SqlException>("Could not delete", "Update/Delete operation is NOT allowed on HrlBalanceAffectingLog", () => Db.Connection.ExecuteNonQuery($"DELETE FROM dbo.HrlBalanceAffectingLog WHERE LLB_PK = {llb_pk};"));
			AssertExceptionThrown<SqlException>("Could not update", "Update/Delete operation is NOT allowed on HrlBalanceAffectingLog", () => Db.Connection.ExecuteNonQuery($"UPDATE dbo.HrlBalanceAffectingLog SET LLB_Reference = 'Cancel Leave' WHERE LLB_PK = {llb_pk};"));
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

		void CreateHrlBalanceAffectingLog(Guid pk, int autoversion, string reference, DateTimeOffset effectiveDate, Guid staffPk, DateTime systemCreateTimeUtc, string systemCreateUser)
		{
			const string sqlText = @"
INSERT INTO dbo.HrlBalanceAffectingLog (LLB_PK, LLB_AutoVersion, LLB_Reference, LLB_EffectiveDate, LLB_GS_Staff, LLB_SystemCreateTimeUtc, LLB_SystemCreateUser)
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
