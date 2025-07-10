using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(GlbWorkTime_WithManaged))]
	class GlbWorkTime_WithManagedTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			PrepareTestData();

			var results = Execute(staff0, "HRM,PPL,DRM", DateTimeOffset.Now);
			var dataRows = results.Select();

			DataTable originalDataTable;
			using (var command = TestConnection.Command(SELECT_GlbWorkTime))
			{
				originalDataTable = DataUtils.GetDataTableFromCommand(command);
			}

			var hrmColumns = new[] { "IsSelf", "IsManaged1", "IsManaged2", "IsManaged3" };

			AssertEquals(originalDataTable.Columns.Count + hrmColumns.Length, results.Columns.Count);
			foreach (var column in originalDataTable.Columns)
			{
				Assert(results.Columns.Contains((column as DataColumn).ColumnName));
			}
			foreach (var colName in hrmColumns)
			{
				Assert(results.Columns.Contains(colName));
			}

			AssertEquals(originalDataTable.Rows.Count, dataRows.Length);

			foreach (DataRow originalRow in originalDataTable.Rows)
			{
				var pk = originalRow["GW_PK"];

				var dataRow = dataRows.FirstOrDefault(r => r["GW_PK"].Equals(pk));
				foreach (var column in originalDataTable.Columns)
				{
					var col = column as DataColumn;
					AssertEquals(col.ColumnName, originalRow[col.ColumnName], dataRow[col.ColumnName]);
				}

				var expectedIsSelf = (Guid)dataRow["GW_ParentID"] == workpattern0;
				var expectedIsManaged1 = (Guid)dataRow["GW_ParentID"] == workpattern3 || (Guid)dataRow["GW_ParentID"] == workpattern4;
				var expectedIsManaged2 = (Guid)dataRow["GW_ParentID"] == workpattern1 || (Guid)dataRow["GW_ParentID"] == workpattern2;
				var expectedIsManaged3 = (Guid)dataRow["GW_ParentID"] == workpattern5;

				AssertEquals(expectedIsSelf, dataRow["IsSelf"]);
				AssertEquals(expectedIsManaged1, dataRow["IsManaged1"]);
				AssertEquals(expectedIsManaged2, dataRow["IsManaged2"]);
				AssertEquals(expectedIsManaged3, dataRow["IsManaged3"]);
			}
		}

		void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) 
VALUES
	(@staff0, 'GS0', 'Staff0', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff1, 'GS1', 'Staff1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff2, 'GS2', 'Staff2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff3, 'GS3', 'Staff3', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff4, 'GS4', 'Staff4', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.GlbStaffManager
	(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
VALUES
	(newid(), 'PPL', @staff0, @staff1, '2021-01-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'HRM', @staff0, @staff2, '2021-02-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'DRM', @staff0, @staff3, '2021-03-28', null, GETDATE(), GETDATE(), 'E', 'E');

INSERT INTO dbo.GlbWorkPattern (GWP_PK, GWP_StandardDuration, GWP_GS_Staff, GWP_EffectiveDate, GWP_SystemCreateTimeUtc, GWP_SystemCreateUser, GWP_SystemLastEditTimeUtc, GWP_SystemLastEditUser)
VALUES
	(@workpattern0, '1900-1-1 20:00:00', @staff0, '2019-1-1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@workpattern1, '1900-1-1 20:00:00', @staff1, '2019-1-1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@workpattern2, '1900-1-1 20:00:00', @staff1, '2020-1-1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@workpattern3, '1900-1-1 20:00:00', @staff2, '2020-1-1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@workpattern4, '1900-1-1 20:00:00', @staff2, '2021-1-1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@workpattern5, '1900-1-1 20:00:00', @staff3, '2020-1-1', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser)
VALUES
	(NEWID(), @workpattern0, 'GWP', 'THU', '1900-1-1 06:00:00', '1900-1-1 17:00:00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @workpattern1, 'GWP', 'WED', '1900-1-1 08:00:00', '1900-1-1 13:00:00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @workpattern1, 'GWP', 'WED', '1900-1-1 14:00:00', '1900-1-1 17:00:00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @workpattern2, 'GWP', 'SAT', '1900-1-1 08:00:00', '1900-1-1 17:00:00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @workpattern3, 'GWP', 'SAT', '1900-1-1 08:00:00', '1900-1-1 17:00:00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @workpattern4, 'GWP', 'SAT', '1900-1-1 08:00:00', '1900-1-1 17:00:00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @workpattern5, 'GWP', 'SAT', '1900-1-1 14:00:00', '1900-1-2 17:00:00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff4, 'GS', 'MON', '1900-1-1 07:00:00', '1900-1-1 13:00:00', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

			", p =>
			{
				p.AddParameter("@staff0", SqlDbType.UniqueIdentifier, staff0);
				p.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1);
				p.AddParameter("@staff2", SqlDbType.UniqueIdentifier, staff2);
				p.AddParameter("@staff3", SqlDbType.UniqueIdentifier, staff3);
				p.AddParameter("@staff4", SqlDbType.UniqueIdentifier, staff4);

				p.AddParameter("@workpattern0", SqlDbType.UniqueIdentifier, workpattern0);
				p.AddParameter("@workpattern1", SqlDbType.UniqueIdentifier, workpattern1);
				p.AddParameter("@workpattern2", SqlDbType.UniqueIdentifier, workpattern2);
				p.AddParameter("@workpattern3", SqlDbType.UniqueIdentifier, workpattern3);
				p.AddParameter("@workpattern4", SqlDbType.UniqueIdentifier, workpattern4);
				p.AddParameter("@workpattern5", SqlDbType.UniqueIdentifier, workpattern5);
			});
		}

		DataTable Execute(Guid loggedInStaff, string managerTypes, DateTimeOffset effectiveAsAt)
		{
			using (var command = TestConnection.Command($"SELECT * FROM GlbWorkTime_WithManaged(@loggedInStaff, @managerTypes, @effectiveAsAt)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);
				command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);
				command.AddParameter("@effectiveAsAt", SqlDbType.DateTimeOffset, effectiveAsAt);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		readonly Guid staff0 = Guid.NewGuid();
		readonly Guid staff1 = Guid.NewGuid();
		readonly Guid staff2 = Guid.NewGuid();
		readonly Guid staff3 = Guid.NewGuid();
		readonly Guid staff4 = Guid.NewGuid();

		readonly Guid workpattern0 = Guid.NewGuid();
		readonly Guid workpattern1 = Guid.NewGuid();
		readonly Guid workpattern2 = Guid.NewGuid();
		readonly Guid workpattern3 = Guid.NewGuid();
		readonly Guid workpattern4 = Guid.NewGuid();
		readonly Guid workpattern5 = Guid.NewGuid();

		const string SELECT_GlbWorkTime = @"
	SELECT
      [GW_PK]
      ,[GW_IsValid]
      ,[GW_ParentID]
      ,[GW_ParentTableCode]
      ,[GW_AutoVersion]
      ,[GW_DayOfWeek]
      ,[GW_StartTime]
      ,[GW_EndTime]
      ,[GW_SystemCreateTimeUtc]
      ,[GW_SystemCreateUser]
      ,[GW_SystemLastEditTimeUtc]
      ,[GW_SystemLastEditUser]
	FROM [dbo].[GlbWorkTime]
	WHERE GW_ParentTableCode = 'GWP'";
	}
}
