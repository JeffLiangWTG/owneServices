using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(GlbStaffHolidayWorkflowTasks_WithManaged))]
	class GlbStaffHolidayWorkflowTasks_WithManagedTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			var staff0 = Guid.NewGuid();
			var staff1 = Guid.NewGuid();
			var staff2 = Guid.NewGuid();
			var staff3 = Guid.NewGuid();

			var leave0 = Guid.NewGuid();
			var leave1 = Guid.NewGuid();
			var leave2 = Guid.NewGuid();
			var leave3 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES
	(@staff0, 'S00', 'Staff00', GETDATE(), 'E', GETDATE(), 'E'),
	(@staff1, 'S01', 'Staff01', GETDATE(), 'E', GETDATE(), 'E'),
	(@staff2, 'S02', 'Staff02', GETDATE(), 'E', GETDATE(), 'E'),
	(@staff3, 'S03', 'Staff03', GETDATE(), 'E', GETDATE(), 'E');


INSERT INTO dbo.GlbStaffManager
	(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
VALUES
	(newid(), 'PPL', @staff0, @staff1, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'HRM', @staff0, @staff2, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'DRM', @staff0, @staff3, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E');

INSERT INTO dbo.GlbStaffHoliday
	(GA_PK, GA_GS, GA_WorkHolidayType, GA_ApprovalStatus, GA_StartTime, GA_EndTime, GA_SystemCreateTimeUtc, GA_SystemCreateUser, GA_SystemLastEditTimeUtc, GA_SystemLastEditUser)
VALUES
	(@leave0, @staff0, 'ANN', 'APP', '2020-01-01 01:00:00', '2020-01-11 01:00:00', GETDATE(), 'E', GETDATE(), 'E'),
	(@leave1, @staff1, 'ANN', 'APP', '2020-02-01 11:00:00', '2020-02-11 01:00:00', GETDATE(), 'E', GETDATE(), 'E'),
	(@leave2, @staff2, 'ANN', 'APP', '2020-03-01 03:00:00', '2020-03-11 01:00:00', GETDATE(), 'E', GETDATE(), 'E'),
	(@leave3, @staff3, 'ANN', 'APP', '2020-04-01 17:00:00', '2020-04-11 01:00:00', GETDATE(), 'E', GETDATE(), 'E');

INSERT INTO dbo.ProcessTasks
	(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type)
VALUES
	(NEWID(), @leave0, 'GA', 'APP'),
	(NEWID(), @leave1, 'GA', 'APP'),
	(NEWID(), @leave2, 'GA', 'APP'),
	(NEWID(), @leave3, 'GA', 'APP'),
	(NEWID(), @leave3, 'GA', 'EXC'),
	(NEWID(), @staff0, 'GS', 'APP'),
	(NEWID(), @staff1, 'GS', 'APP');

			", p =>
			{
				p.AddParameter("@staff0", SqlDbType.UniqueIdentifier, staff0);
				p.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1);
				p.AddParameter("@staff2", SqlDbType.UniqueIdentifier, staff2);
				p.AddParameter("@staff3", SqlDbType.UniqueIdentifier, staff3);

				p.AddParameter("@leave0", SqlDbType.UniqueIdentifier, leave0);
				p.AddParameter("@leave1", SqlDbType.UniqueIdentifier, leave1);
				p.AddParameter("@leave2", SqlDbType.UniqueIdentifier, leave2);
				p.AddParameter("@leave3", SqlDbType.UniqueIdentifier, leave3);
			});

			var results = Execute(staff0, "HRM,PPL,DRM", DateTimeOffset.Now);
			var dataRows = results.Select();

			DataTable originalDataTable;
			using (var command = TestConnection.Command($"SELECT * FROM dbo.WorkflowTask WHERE P9_ParentTableCode = '{GlbStaffHolidaySchema.Constants.Prefix}'"))
			{
				originalDataTable = DataUtils.GetDataTableFromCommand(command);
			}

			var tableColumns = originalDataTable.Columns;
			var expectedColumns = new List<string>();
			expectedColumns.AddRange(tableColumns.Cast<DataColumn>().Select(c => c.ColumnName));
			expectedColumns.AddRange(["IsSelf", "IsManaged1", "IsManaged2", "IsManaged3"]);

			AssertEquals(expectedColumns.Count, results.Columns.Count);
			foreach (var colName in expectedColumns)
			{
				Assert(results.Columns.Contains(colName));
			}
			AssertEquals(originalDataTable.Rows.Count, dataRows.Length);

			for (var i = 0; i < originalDataTable.Rows.Count; i++)
			{
				foreach (var colName in tableColumns.Cast<DataColumn>())
				{
					AssertEquals(colName.ColumnName, originalDataTable.Rows[i][colName.ColumnName], dataRows[i][colName.ColumnName]);
				}

				var expectedIsSelf = (Guid)dataRows[i][ProcessTasksSchema.P9_ParentID.Name] == leave0;
				var expectedIsManaged1 = (Guid)dataRows[i][ProcessTasksSchema.P9_ParentID.Name] == leave2;
				var expectedIsManaged2 = (Guid)dataRows[i][ProcessTasksSchema.P9_ParentID.Name] == leave1;
				var expectedIsManaged3 = (Guid)dataRows[i][ProcessTasksSchema.P9_ParentID.Name] == leave3;

				AssertEquals(expectedIsSelf, dataRows[i]["IsSelf"]);
				AssertEquals(expectedIsManaged1, dataRows[i]["IsManaged1"]);
				AssertEquals(expectedIsManaged2, dataRows[i]["IsManaged2"]);
				AssertEquals(expectedIsManaged3, dataRows[i]["IsManaged3"]);
			}
		}

		DataTable Execute(Guid loggedInStaff, string managerTypes, DateTimeOffset effectiveAsAt)
		{
			using (var command = TestConnection.Command($"SELECT * FROM GlbStaffHolidayWorkflowTasks_WithManaged(@loggedInStaff, @managerTypes, @effectiveAsAt)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);
				command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);
				command.AddParameter("@effectiveAsAt", SqlDbType.DateTimeOffset, effectiveAsAt);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;
	}
}
