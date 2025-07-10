using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Workflow.StmALog;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class StmALogWriterTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestStmALogSql()
		{
			var parameters = new StmALogProperties
			{
				TableName = "TestTable",
				ParentId = Guid.NewGuid(),
				UserCode = "USR",
				BranchCode = "BR1",
				DepartmentCode = "DEP",
				FireWorkflow = true,
				EventCode = "Z01",
				Reference = "TestReference",
				DataSource = "C",
				EventTime = new DateTimeOffset(new DateTime(2025, 1, 10, 20, 9, 9), TimeSpan.FromHours(10)),
				IsCancelled = false,
				IsEstimate = true
			};

			var sql = StmALogWriter.GetStmALogSQLText(parameters);
			using (var cmd = Db.Connection.Command(sql.CommandText))
			{
				sql.Parameters.ForEach(p =>
				{
					cmd.AddParameter(p.Name, p.Type, p.Size, p.Value);
				});
				cmd.ExecuteNonQuery();
				CheckForStmALogRecord(parameters);
			}
		}

		[UseSnapshotProtection]
		public void TestStmALogQueueSql()
		{
			var parameters = new StmALogQueueProperties
			{
				TableName = "TestTable",
				ParentId = Guid.NewGuid(),
				UserCode = "USR",
				BranchCode = "BR1",
				DepartmentCode = "DEP",
				FireWorkflow = true,
				EventCode = "Z01",
				Reference = "TestReference",
				EventTime = new DateTimeOffset(new DateTime(2025, 1, 10, 20, 9, 9), TimeSpan.FromHours(10)),
				IsCancelled = false,
				IsEstimate = true,
				ALogReference = Guid.NewGuid()
			};

			var sql = StmALogWriter.GetStmALogQueueSQLText(parameters);
			using (var cmd = Db.Connection.Command(sql.CommandText))
			{
				sql.Parameters.ForEach(p =>
				{
					cmd.AddParameter(p.Name, p.Type, p.Size, p.Value);
				});
				cmd.ExecuteNonQuery();
			}
			CheckForStmALogQueueRecord(parameters);
		}

		void CheckForStmALogRecord(StmALogProperties stmALogProperties)
		{
			var sql = $"SELECT * FROM StmALog WHERE SL_Table = '{stmALogProperties.TableName}'";

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					CombineAssertions(() =>
					{
						AssertEquals("ParentID", stmALogProperties.ParentId, reader["SL_Parent"]);
						AssertEquals("TableName", stmALogProperties.TableName, reader["SL_Table"]);
						AssertEquals("UserCode", stmALogProperties.UserCode, reader["SL_GS_NKUser"]);
						AssertEquals("BranchCode", stmALogProperties.BranchCode, reader["SL_GB_NKBranch"]);
						AssertEquals("DepartmentCode", stmALogProperties.DepartmentCode, reader["SL_GE_NKDepartment"]);
						AssertEquals("FireWorkflow", stmALogProperties.FireWorkflow, reader["SL_FireWorkflow"]);
						AssertEquals("EventCode", stmALogProperties.EventCode, reader["SL_SE_NKEvent"]);
						AssertEquals("Reference", stmALogProperties.Reference, reader["SL_Reference"]);
						AssertEquals("DataSource", stmALogProperties.DataSource, reader["SL_DataSource"]);
						AssertEquals("EventTime", stmALogProperties.EventTime.Value.DateTime, reader["SL_EventTime"]);
						AssertEquals("IsCancelled", stmALogProperties.IsCancelled ? "Y" : "N", reader["SL_IsCancelled"]);
						AssertEquals("IsEstimate", stmALogProperties.IsEstimate ? "Y" : "N", reader["SL_IsEstimate"]);
					});
				}
				else
				{
					Fail("Should be StmALog row in the table but not found.");
				}
			}
		}

		public static void CheckForStmALogQueueRecord(StmALogQueueProperties stmALogQueueProperties, bool shouldExist = true)
		{
			var sql = $"SELECT * FROM StmALogQueue WHERE SLQ_SE_NKEvent = '{stmALogQueueProperties.EventCode}' AND SLQ_ParentTableName = '{stmALogQueueProperties.TableName}'";
			var found = false;
			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					if (!shouldExist)
					{
						Fail("Should not be StmALogQueue row in the table but found.");
					}
					else if (found)
					{
						Fail("Should not be more than one StmALogQueue row in the table but found more than one.");
					}
					else
					{
						CombineAssertions(() =>
						{
							AssertEquals("ParentID", stmALogQueueProperties.ParentId, reader["SLQ_ParentID"]);
							AssertEquals("TableName", stmALogQueueProperties.TableName, reader["SLQ_ParentTableName"]);
							AssertEquals("UserCode", stmALogQueueProperties.UserCode, reader["SLQ_GS_NKUser"]);
							AssertEquals("BranchCode", stmALogQueueProperties.BranchCode, reader["SLQ_GB_NKBranch"]);
							AssertEquals("DepartmentCode", stmALogQueueProperties.DepartmentCode, reader["SLQ_GE_NKDepartment"]);
							AssertEquals("FireWorkflow", stmALogQueueProperties.FireWorkflow, reader["SLQ_FireWorkflow"]);
							AssertEquals("EventCode", stmALogQueueProperties.EventCode, reader["SLQ_SE_NKEvent"]);
							AssertEquals("Reference", stmALogQueueProperties.Reference, reader["SLQ_Reference"]);
							AssertEquals("EventTime", stmALogQueueProperties.EventTime.Value.DateTime, reader["SLQ_EventTime"]);
							AssertEquals("IsCancelled", stmALogQueueProperties.IsCancelled, reader["SLQ_IsCancelled"]);
							AssertEquals("IsEstimate", stmALogQueueProperties.IsEstimate, reader["SLQ_IsEstimate"]);
							AssertEquals("ALogReference", stmALogQueueProperties.ALogReference, reader["SLQ_ALogReference"]);
						});
					}
					found = true;
				}
				if (shouldExist && !found)
				{
					Fail("Should be StmALogQueue row in the table but not found.");
				}
			}
		}
	}
}
