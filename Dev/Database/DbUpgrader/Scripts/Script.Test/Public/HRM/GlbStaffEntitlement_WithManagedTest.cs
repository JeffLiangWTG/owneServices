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
	[TestedType(typeof(GlbStaffEntitlement_WithManaged))]
	class GlbStaffEntitlement_WithManagedTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			var staff0 = Guid.NewGuid();
			var staff1 = Guid.NewGuid();
			var staff2 = Guid.NewGuid();
			var staff3 = Guid.NewGuid();

			var gsr0 = Guid.NewGuid();
			var gsr1 = Guid.NewGuid();
			var gsr2 = Guid.NewGuid();
			var gsr3 = Guid.NewGuid();

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
	(newid(), 'PPL', @staff0, @staff1, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'HRM', @staff0, @staff2, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'DRM', @staff0, @staff3, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E');

INSERT INTO [hrm].[GlbStaffRemuneration]
	(GSR_PK, GSR_GS_Staff, GSR_EffectiveDate, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemLastEditTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditUser)
VALUES
	(@gsr0, @staff0, GETDATE(), 'AU', 'AUD', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(@gsr1, @staff1, GETDATE(), 'AU', 'AUD', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(@gsr2, @staff2, GETDATE(), 'AU', 'AUD', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(@gsr3, @staff3, GETDATE(), 'AU', 'AUD', GETDATE(), GETDATE(), 'ZZN', 'ZZN');

INSERT INTO [hrm].[GlbStaffEntitlement]
	(GSI_PK, GSI_GSR_Remuneration, GSI_EntitlementCode, GSI_SystemCreateTimeUtc, GSI_SystemLastEditTimeUtc, GSI_SystemCreateUser, GSI_SystemLastEditUser)
VALUES
	(NEWID(), @gsr0, 'EN0', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(NEWID(), @gsr1, 'EN1', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(NEWID(), @gsr2, 'EN2', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(NEWID(), @gsr3, 'EN3', GETDATE(), GETDATE(), 'ZZN', 'ZZN');

			", p =>
			{
				p.AddParameter("@staff0", SqlDbType.UniqueIdentifier, staff0);
				p.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1);
				p.AddParameter("@staff2", SqlDbType.UniqueIdentifier, staff2);
				p.AddParameter("@staff3", SqlDbType.UniqueIdentifier, staff3);

				p.AddParameter("@gsr0", SqlDbType.UniqueIdentifier, gsr0);
				p.AddParameter("@gsr1", SqlDbType.UniqueIdentifier, gsr1);
				p.AddParameter("@gsr2", SqlDbType.UniqueIdentifier, gsr2);
				p.AddParameter("@gsr3", SqlDbType.UniqueIdentifier, gsr3);
			});

			var results = Execute(staff0, "HRM,PPL,DRM", DateTimeOffset.Now);
			var dataRows = results.Select();

			DataTable originalDataTable;
			using (var command = TestConnection.Command("SELECT * FROM [hrm].[GlbStaffEntitlement]"))
			{
				originalDataTable = DataUtils.GetDataTableFromCommand(command);
			}

			var schema = (GlbStaffEntitlementSchema)typeof(GlbStaffEntitlementSchema).GetField("Instance").GetValue(null);
			var tableColumns = GlbStaffEntitlementSchema.All.Select(c => c.Name).ToList();
			var expectedColumns = new List<string>();
			expectedColumns.AddRange(tableColumns);
			expectedColumns.AddRange(["GSI_AutoVersion", "IsSelf", "IsManaged1", "IsManaged2", "IsManaged3"]);

			AssertEquals(expectedColumns.Count, results.Columns.Count);
			foreach (var colName in expectedColumns)
			{
				Assert(results.Columns.Contains(colName));
			}
			AssertEquals(originalDataTable.Rows.Count, dataRows.Length);

			for (var i = 0; i < originalDataTable.Rows.Count; i++)
			{
				foreach (var colName in tableColumns)
				{
					AssertEquals(colName, originalDataTable.Rows[i][colName], dataRows[i][colName]);
				}

				var expectedIsSelf = (Guid)dataRows[i][GlbStaffEntitlementSchema.GSI_GSR_Remuneration.Name] == gsr0;
				var expectedIsManaged1 = (Guid)dataRows[i][GlbStaffEntitlementSchema.GSI_GSR_Remuneration.Name] == gsr2;
				var expectedIsManaged2 = (Guid)dataRows[i][GlbStaffEntitlementSchema.GSI_GSR_Remuneration.Name] == gsr1;
				var expectedIsManaged3 = (Guid)dataRows[i][GlbStaffEntitlementSchema.GSI_GSR_Remuneration.Name] == gsr3;

				AssertEquals(expectedIsSelf, dataRows[i]["IsSelf"]);
				AssertEquals(expectedIsManaged1, dataRows[i]["IsManaged1"]);
				AssertEquals(expectedIsManaged2, dataRows[i]["IsManaged2"]);
				AssertEquals(expectedIsManaged3, dataRows[i]["IsManaged3"]);
			}
		}

		DataTable Execute(Guid loggedInStaff, string managerTypes, DateTimeOffset effectiveAsAt)
		{
			using (var command = TestConnection.Command($"SELECT * FROM hrm.GlbStaffEntitlement_WithManaged(@loggedInStaff, @managerTypes, @effectiveAsAt)"))
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
