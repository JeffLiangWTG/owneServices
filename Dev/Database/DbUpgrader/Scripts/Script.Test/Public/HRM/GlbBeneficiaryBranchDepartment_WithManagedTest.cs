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
	[TestedType(typeof(GlbBeneficiaryBranchDepartment_WithManaged))]
	class GlbBeneficiaryBranchDepartment_WithManagedTest : ManagedTypeTest<GlbBeneficiaryBranchDepartmentSchema>
	{
		protected override string StaffFKColumnName => "GBB_GS_Staff";

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

INSERT INTO dbo.GlbBeneficiaryBranchDepartment
	(GBB_PK, GBB_GS_Staff, GBB_EffectiveDate, GBB_GB_Branch, GBB_GE_Department, GBB_SystemCreateTimeUtc, GBB_SystemCreateUser, GBB_SystemLastEditTimeUtc, GBB_SystemLastEditUser)
VALUES
	(NEWID(), @staff0, '2020-01-01 01:00:00 +01:00', NULL, NULL, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff1, '2020-02-01 11:00:00 +07:00', NULL, NULL, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff2, '2020-03-01 11:00:00 +07:00', NULL, NULL, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff3, '2020-04-01 11:00:00 +07:00', NULL, NULL, GETUTCDATE(), 'E', GETUTCDATE(), 'E');
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
			expectedColumns.AddRange(["GBB_AutoVersion", "IsSelf", "IsManaged1", "IsManaged2", "IsManaged3"]);

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
