
using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.HRM
{
	[TestedType(typeof(TG_HrlBalanceAffectingLogInsertTrigger))]
	class TG_HrlBalanceAffectingLogInsertTriggerTest : DbCreateScriptTest
	{
		public void TestTriggersCopyToHrlBalanceAffectingQueue()
		{
			var pk = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			var pk3 = Guid.NewGuid();

			var staffPk = Guid.NewGuid();
			CreateStaff(staffPk, "ABC");

			CreateHrlBalanceAffectingLog(pk, 1, "Leave Approved", new DateTimeOffset(new DateTime(2020, 2, 15)), staffPk, new DateTime(2020, 2, 15), "ABC");
			CreateHrlBalanceAffectingLog(pk2, 1, "Cancel Approved Leave", new DateTimeOffset(new DateTime(2020, 2, 16)), staffPk, new DateTime(2020, 2, 16), "ABC");
			CreateHrlBalanceAffectingLog(pk3, 1, "Leave Approved", new DateTimeOffset(new DateTime(2020, 2, 17)), staffPk, new DateTime(2020, 2, 17), "ABC");

			AssertHrlBalanceAffectingQueueCreated(pk, 1, "Leave Approved", new DateTimeOffset(new DateTime(2020, 2, 15)), staffPk, new DateTime(2020, 2, 15), "ABC");
			AssertHrlBalanceAffectingQueueCreated(pk2, 1, "Cancel Approved Leave", new DateTimeOffset(new DateTime(2020, 2, 16)), staffPk, new DateTime(2020, 2, 16), "ABC");
			AssertHrlBalanceAffectingQueueCreated(pk3, 1, "Leave Approved", new DateTimeOffset(new DateTime(2020, 2, 17)), staffPk, new DateTime(2020, 2, 17), "ABC");
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

		void AssertHrlBalanceAffectingQueueCreated(Guid pk, int autoversion, string reference, DateTimeOffset effectiveDate, Guid staffPk, DateTime systemCreateTimeUtc, string systemCreateUser)
		{
			string sqlText = string.Format(@"
SELECT LLQ_PK, LLQ_AutoVersion, LLQ_Reference, LLQ_EffectiveDate, LLQ_GS_Staff, LLQ_SystemCreateTimeUtc, LLQ_SystemCreateUser
FROM dbo.HrlBalanceAffectingQueue WHERE LLQ_PK = '{0}'", pk.ToString());

			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					AssertEquals("AutoVersion", autoversion, Convert.ToInt32(reader["LLQ_AutoVersion"]));
					AssertEquals("Reference", reference, reader["LLQ_Reference"]);
					AssertEquals("EffectiveDate", effectiveDate, reader["LLQ_EffectiveDate"]);
					AssertEquals("GS_Staff", staffPk, reader["LLQ_GS_Staff"]);
					AssertEquals("SystemCreateTimeUtc", systemCreateTimeUtc, reader["LLQ_SystemCreateTimeUtc"]);
					AssertEquals("SystemCreateUser", systemCreateUser, reader["LLQ_SystemCreateUser"]);
				}
				else
				{
					Fail("Log was not copied to HrlBalanceAffectingQueue table.");
				}
			}
		}
	}
}
