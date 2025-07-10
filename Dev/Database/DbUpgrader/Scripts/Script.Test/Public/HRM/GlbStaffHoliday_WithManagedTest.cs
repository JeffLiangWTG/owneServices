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
	[TestedType(typeof(GlbStaffHoliday_WithManaged))]
	class GlbStaffHoliday_WithManagedTest : ManagedTypeTest<GlbStaffHolidaySchema>
	{
		protected override string StaffFKColumnName => "GA_GS";

		public void TestExecute()
		{
			var staff0 = Guid.NewGuid();
			var staff1 = Guid.NewGuid();
			var staff2 = Guid.NewGuid();
			var staff3 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES
	(@staff0, 'S00', 'Staff00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff1, 'S01', 'Staff01', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff2, 'S02', 'Staff02', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff3, 'S03', 'Staff03', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.GlbStaffManager
	(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
VALUES
	(NEWID(), 'PPL', @staff0, @staff1, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(NEWID(), 'HRM', @staff0, @staff2, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(NEWID(), 'DRM', @staff0, @staff3, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E');

INSERT INTO dbo.GlbStaffHoliday
	(GA_PK, GA_IsValid, GA_RecordType, GA_WorkHolidayType, GA_IsWorkingAway, GA_ApprovalStatus, GA_StartTime, GA_EndTime, GA_AvailabilityPercentage, GA_DaysLeaveTaken, GA_ParentID, GA_ParentTableCode, GA_GS, GA_LeaveComment, GA_SystemCreateTimeUtc, GA_SystemCreateUser, GA_SystemLastEditTimeUtc, GA_SystemLastEditUser, GA_OverrideLeaveTaken)
VALUES
	(NEWID(), 0, 'LEV', 'ANN', 0, 'APP', '2023-01-15 00:00:00', '2023-05-15 00:00:00', 0, 1.0, NULL, '', @staff0, 'Comment 1', GETDATE(), 'E', GETDATE(), 'E', NULL),
	(NEWID(), 0, 'LEV', 'SIC', 0, 'REQ', '2023-02-15 00:00:00', '2023-06-15 00:00:00', 0, 2.0, NULL, '', @staff1, 'Comment 2', GETDATE(), 'E', GETDATE(), 'E', NULL),
	(NEWID(), 0, 'LEV', 'OTH', 0, 'APP', '2023-03-15 00:00:00', '2023-07-15 00:00:00', 0, 3.0, NULL, '', @staff2, 'Comment 3', GETDATE(), 'E', GETDATE(), 'E', NULL),
	(NEWID(), 0, 'LEV', 'LWO', 0, 'REQ', '2023-04-15 00:00:00', '2023-08-15 00:00:00', 0, 4.0, NULL, '', @staff3, 'Comment 4', GETDATE(), 'E', GETDATE(), 'E', NULL);
",
		p =>
		{
			p.AddParameter("@staff0", SqlDbType.UniqueIdentifier, staff0);
			p.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1);
			p.AddParameter("@staff2", SqlDbType.UniqueIdentifier, staff2);
			p.AddParameter("@staff3", SqlDbType.UniqueIdentifier, staff3);
		});

			var results = Execute(staff0, "HRM,PPL,DRM", DateTimeOffset.Now);
			var dataRows = results.Select();

			DataTable originalDataTable;
			using (var command = TestConnection.Command($"SELECT * FROM {Schema.SqlSchemaName}.{Schema.TableName}"))
			{
				originalDataTable = DataUtils.GetDataTableFromCommand(command);
			}

			var tableColumns = Schema.All.Select(c => c.Name).ToList();
			var expectedColumns = new List<string>();
			expectedColumns.AddRange(tableColumns);
			expectedColumns.AddRange(["GA_AutoVersion", "IsSelf", "IsManaged1", "IsManaged2", "IsManaged3"]);

			AssertEquals(expectedColumns.Count, results.Columns.Count);
			foreach (var colName in expectedColumns)
			{
				Assert(results.Columns.Contains(colName));
			}
			AssertEquals(originalDataTable.Rows.Count, dataRows.Length);

			for (var i = 0; i < originalDataTable.Rows.Count; i++)
			{
				AssertRowValues(originalDataTable.Rows[i], dataRows[i], tableColumns);

				var expectedIsSelf = (Guid)dataRows[i][StaffFKColumnName] == staff0;
				var expectedIsManaged1 = (Guid)dataRows[i][StaffFKColumnName] == staff2;
				var expectedIsManaged2 = (Guid)dataRows[i][StaffFKColumnName] == staff1;
				var expectedIsManaged3 = (Guid)dataRows[i][StaffFKColumnName] == staff3;

				AssertEquals(expectedIsSelf, dataRows[i]["IsSelf"]);
				AssertEquals(expectedIsManaged1, dataRows[i]["IsManaged1"]);
				AssertEquals(expectedIsManaged2, dataRows[i]["IsManaged2"]);
				AssertEquals(expectedIsManaged3, dataRows[i]["IsManaged3"]);
			}
		}
	}
}
