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
	[TestedType(typeof(GlbEmploymentHistory_WithManaged))]
	class GlbEmploymentHistory_WithManagedTest : ManagedTypeTest<GlbEmploymentHistorySchema>
	{
		protected override string StaffFKColumnName => "GEH_GS_Staff";

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

INSERT INTO dbo.GlbEmploymentHistory
	(GEH_PK, GEH_GS_Staff, GEH_EffectiveDate, GEH_JobTitle, GEH_HJ_JobRole, GEH_JobFamily, GEH_IsInternalPosition, GEH_WorksOutsideBranch, GEH_JobDescription, GEH_EmploymentType, GEH_CompanyName, GEH_DepartureReason, GEH_DepartureComments, GEH_SystemCreateTimeUtc, GEH_SystemCreateUser, GEH_SystemLastEditTimeUtc, GEH_SystemLastEditUser, GEH_AutoEffectiveEndDate, GEH_GCR_ChangeRequest, GEH_IsApproved)
VALUES
	(NEWID(), @staff0, '2021-10-16 00:00:00', 'JT1', NULL, 'JF1', 0, 0, 'Description 1', '', 'CompanyName 1', '', '', GETDATE(), 'E', GETDATE(), 'E', NULL, NULL, 0),
	(NEWID(), @staff1, '2021-05-14 00:00:00', 'JT2', NULL, 'JF2', 0, 0, 'Description 2', '', 'CompanyName 2', '', '', GETDATE(), 'E', GETDATE(), 'E', NULL, NULL, 0),
	(NEWID(), @staff2, '2021-03-03 00:00:00', 'JT3', NULL, 'JF3', 0, 0, 'Description 3', '', 'CompanyName 3', '', '', GETDATE(), 'E', GETDATE(), 'E', NULL, NULL, 0),
	(NEWID(), @staff3, '2021-10-10 00:00:00', 'JT4', NULL, 'JF4', 0, 0, 'Description 4', '', 'CompanyName 4', '', '', GETDATE(), 'E', GETDATE(), 'E', NULL, NULL, 0);
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
			expectedColumns.AddRange(["GEH_AutoVersion", "IsSelf", "IsManaged1", "IsManaged2", "IsManaged3"]);

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
