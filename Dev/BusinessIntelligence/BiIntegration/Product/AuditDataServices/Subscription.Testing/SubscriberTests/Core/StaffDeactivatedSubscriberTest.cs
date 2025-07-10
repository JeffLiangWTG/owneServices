using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.Core.Subscribers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Subscription.Testing;

[TestedType(typeof(StaffDeactivatedSubscriber))]
public class StaffDeactivatedSubscriberTest : ActualDataChangesAuditSubscriberTest
{
	[UseSnapshotProtection(skipTransaction: true)]
	public void TestConnectionShouldBeKilledIfStaffIsDeactivated()
	{
		var factory = new BusinessObjectFactory();

		var staff = factory.NewWithValidTestData<GlbStaff>();
		staff.GS_IsActive = false;

		var changeTable = new DataTable();
		changeTable.Columns.Add(GlbStaffSchema.Constants.PK, typeof(Guid));
		changeTable.Columns.Add(GlbStaffSchema.Constants.GS_IsActive, typeof(bool));

		var row = changeTable.NewRow();
		row[GlbStaffSchema.Constants.PK] = staff.PK.ToGuid();
		row[GlbStaffSchema.Constants.GS_IsActive] = (bool)staff.GS_IsActive;
		changeTable.Rows.Add(row);

		row.AcceptChanges();
		row.SetModified();

		var subscriber = NewDataChangeSubscriber() as StaffDeactivatedSubscriber;
		var svPk = Guid.NewGuid();
		var expectStatement = new StringBuilder();
		expectStatement.AppendLine(@$"DELETE FROM dbo.StmServiceHeartBeat WHERE SV_ParentId IN ('{staff.PK.ToString()}') AND SV_ParentTableCode = 'GS'");

		var dbManager = new SemaphoreDbManager();
		dbManager.CreateHeartbeatInDatabase(svPk, "Test", Process.GetCurrentProcess().Id, staff.PK.ToGuid(), GlbStaffSchema.Constants.Prefix, 60, HeartbeatTypes.Enterprise, null);

		using (var adminConnection = Db.NewAdminConnection())
		{
			using (var cmd = adminConnection.Command("SELECT session_id FROM sys.dm_exec_sessions WHERE host_process_id = @ProcessID"))
			{
				cmd.AddParameter("@ProcessID", SqlDbType.Int, Process.GetCurrentProcess().Id);
				var spids = new List<int>();
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						spids.Add(reader.GetInt16(0));
					}
					spids.Sort();
				}

				foreach (var spid in spids)
				{
					expectStatement.AppendLine(@$"KILL {spid}");
				}
			}

			var actualCommandStatement = subscriber.GetCommandStatement(changeTable);
			AssertEquals(expectStatement.ToString(), actualCommandStatement);
		}

		AssertNoExceptionThrown("This should not throw an exception when sql query is empty.", () => subscriber.ProcessChanges(new LoggerForTest(), changeTable));

		Assert("The heartbeat should be removed after the subscriber has completed processing.", dbManager.CheckHeartbeatExpiredTimeIsExpired(svPk));
	}

	public override void TestCustomFilter()
	{
		var subscriber = new StaffDeactivatedSubscriber();
		AssertNull(subscriber.CustomFilter);
	}

	public void TestProcessChangesIfNoDeactivateStaffFound_EmptyKillCommand()
	{
		var factory = new BusinessObjectFactory();

		var staff = factory.NewWithValidTestData<GlbStaff>();
		staff.GS_IsActive = true;

		var changeTable = new DataTable();
		changeTable.Columns.Add(GlbStaffSchema.Constants.PK, typeof(Guid));
		changeTable.Columns.Add(GlbStaffSchema.Constants.GS_IsActive, typeof(bool));

		var row = changeTable.NewRow();
		row[GlbStaffSchema.Constants.PK] = staff.PK.ToGuid();
		row[GlbStaffSchema.Constants.GS_IsActive] = (bool)staff.GS_IsActive;
		changeTable.Rows.Add(row);

		row.AcceptChanges();
		row.SetModified();

		var subscriber = NewDataChangeSubscriber() as StaffDeactivatedSubscriber;
		var logger = new LoggerForTest();

		AssertNoExceptionThrown("This should not throw an exception when sql query is empty.",
			() => subscriber.ProcessChanges(logger, changeTable));
	}

	protected override DataTable GetTestDataTable() => null;
}
