using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(TG_CP_INS_StmALogQueue))]
	class TG_CP_INS_StmALogQueueTest : DbCreateScriptTest
	{
		public void TestTriggersCopyToStmALogQueue()
		{
			var pk = Guid.NewGuid();
			var parent = Guid.NewGuid();
			string table = "ProcessTasks";
			string user = "USR";
			string ev = "LGI";
			string branch = "BRA";
			string department = "DEP";
			int fireWorkflow = 0;

			var pk2 = Guid.NewGuid();
			string ev2 = "WTE";

			CreateStmALog(pk, table, parent, user, ev, branch, department, fireWorkflow);
			CreateStmALog(pk2, table, parent, user, ev2, branch, department, fireWorkflow);

			AssertStmALogQueueCreated(pk, table, parent, user, ev, branch, department, fireWorkflow);
			AssertStmALogQueueWTECreated(pk2, table, parent, user, branch, department, fireWorkflow);
		}

		public void TestDontAddRowsWithEmptyTable()
		{
			string table = "";
			AssertExceptionThrown(typeof(SqlException), () => CreateStmALog(Guid.NewGuid(), table, Guid.NewGuid(), "USR", "EVT", "BRA", "DEP", 1));
		}

		void CreateStmALog(Guid pk, string table, Guid parent, string user, string ev, string branch, string department, int fireWorkflow)
		{
			const string sqlText = @"
INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_EventTimeUtc, SL_GS_NKUser, SL_SE_NKEvent, SL_GB_NKBranch, SL_GE_NKDepartment, SL_FireWorkflow)
VALUES (@PK, @Table, @Parent, GETDATE(), GETDATE(), GETUTCDATE(), @User, @Event, @Branch, @Department, @FireWorkflow)
";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@Table", SqlDbType.VarChar, table);
				cmd.AddParameter("@Parent", SqlDbType.UniqueIdentifier, parent);
				cmd.AddParameter("@User", SqlDbType.VarChar, user);
				cmd.AddParameter("@Event", SqlDbType.VarChar, ev);
				cmd.AddParameter("@Branch", SqlDbType.VarChar, branch);
				cmd.AddParameter("@Department", SqlDbType.VarChar, department);
				cmd.AddParameter("@FireWorkflow", SqlDbType.Bit, fireWorkflow);

				cmd.ExecuteNonQuery();
			}
		}

		void AssertStmALogQueueCreated(Guid pk, string table, Guid parent, string user, string ev, string branch, string department, int fireWorkflow)
		{
			string sqlText = string.Format(@"
SELECT SLQ_ParentTableName, SLQ_ParentID, SLQ_GS_NKUser, SLQ_SE_NKEvent, SLQ_GB_NKBranch, SLQ_GE_NKDepartment, SLQ_FireWorkflow, SLQ_EventTime, SLQ_EventTimeUtc
FROM dbo.StmALogQueue WHERE SLQ_ALogReference = '{0}'", pk.ToString());

			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					AssertEquals("ParentTableName", table, Convert.ToString(reader["SLQ_ParentTableName"]));
					AssertEquals("ParentId", parent.ToString(), Convert.ToString(reader["SLQ_ParentID"]));
					AssertEquals("User", user, Convert.ToString(reader["SLQ_GS_NKUser"]));
					AssertEquals("Event", ev, Convert.ToString(reader["SLQ_SE_NKEvent"]));
					AssertEquals("Branch", branch, Convert.ToString(reader["SLQ_GB_NKBranch"]));
					AssertEquals("Department", department, Convert.ToString(reader["SLQ_GE_NKDepartment"]));
					AssertEquals("FireWorkflow", fireWorkflow, Convert.ToInt32(reader["SLQ_FireWorkflow"]));
					Assert("EventTimeUtc", CompareDateTimeToMinutes(((DateTime)reader["SLQ_EventTime"]).ToUniversalTime(), (DateTime)reader["SLQ_EventTimeUtc"]));
				}
				else
				{
					Fail("Log was not copied to StmALogQueue table.");
				}
			}
		}

		public static bool CompareDateTimeToMinutes(DateTime dt1, DateTime dt2)
		{
			return ((int)(dt1 - dt2).TotalMinutes) == 0;
		}

		void AssertStmALogQueueWTECreated(Guid pk, string table, Guid parent, string user, string branch, string department, int fireWorkflow)
		{
			string sqlText = string.Format(@"
SELECT WTE_ParentTableName, WTE_ParentID, WTE_GS_NKUser, WTE_GB_NKBranch, WTE_GE_NKDepartment, WTE_FireWorkflow, WTE_EventTime, WTE_EventTimeUtc
FROM dbo.StmALogQueueWTE WHERE WTE_ALogReference = '{0}'", pk.ToString());

			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					AssertEquals("ParentTableName", table, Convert.ToString(reader["WTE_ParentTableName"]));
					AssertEquals("ParentId", parent.ToString(), Convert.ToString(reader["WTE_ParentID"]));
					AssertEquals("User", user, Convert.ToString(reader["WTE_GS_NKUser"]));
					AssertEquals("Branch", branch, Convert.ToString(reader["WTE_GB_NKBranch"]));
					AssertEquals("Department", department, Convert.ToString(reader["WTE_GE_NKDepartment"]));
					AssertEquals("FireWorkflow", fireWorkflow, Convert.ToInt32(reader["WTE_FireWorkflow"]));
					Assert("EventTimeUtc", CompareDateTimeToMinutes(((DateTime)reader["WTE_EventTime"]).ToUniversalTime(), (DateTime)reader["WTE_EventTimeUtc"]));
				}
				else
				{
					Fail("Log was not copied to StmALogQueueWTE table.");
				}
			}
		}
	}
}

