using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(GetRelevantHiringRequests))]
	class GetRelevantHiringRequestsTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			var loggedInStaff = Guid.NewGuid();
			var myDirectReport = Guid.NewGuid();
			var anotherDirectReport = Guid.NewGuid();
			var myIndirectReport = Guid.NewGuid();
			var requestWithTask1 = Guid.NewGuid();
			var requestWithTask2 = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{loggedInStaff}', 'bos', 'bos', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{myDirectReport}', 'mid', 'mid', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaffManager (GSM_PK, GSM_GS_Manager, GSM_GS_Staff, GSM_ManagerType, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_EffectiveDate)
VALUES (NEWID(), '{loggedInStaff}', '{myDirectReport}', 'XXX', GETUTCDATE(), GETUTCDATE(), 'X', 'X', DATEADD(day, -1, GETUTCDATE()))

INSERT INTO dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES('{myIndirectReport}', 'new', 'new', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaffManager (GSM_PK, GSM_GS_Manager, GSM_GS_Staff, GSM_ManagerType, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_EffectiveDate)
VALUES (NEWID(), '{myDirectReport}', '{myIndirectReport}', 'XXX', GETUTCDATE(), GETUTCDATE(), 'X', 'X', DATEADD(day, -1, GETUTCDATE()))

INSERT INTO dbo.HRJobApplicant (HA_PK, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser)
VALUES ('3247149A-281C-4931-B8B0-3AA77C1CD29D', 'E549FEB8-547B-4FA4-8402-0644D07CA133', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.HRHiringRequest 
(HRR_PK, HRR_GS_NKReportingManager, HRR_JobTitle, HRR_SystemCreateTimeUtc, HRR_SystemLastEditTimeUtc, HRR_SystemCreateUser, HRR_SystemLastEditUser, HRR_HA_JobApplicant, HRR_ProbationDurationOverride, HRR_StartDate, HRR_WorkingDays)
VALUES
(newid(), 'BOS', 'Bos', GETUTCDATE(), GETUTCDATE(), 'X', 'X', '3247149A-281C-4931-B8B0-3AA77C1CD29D', NULL, GETDATE(), 0)

INSERT INTO dbo.HRHiringRequest 
(HRR_PK, HRR_GS_NKReportingManager, HRR_JobTitle, HRR_SystemCreateTimeUtc, HRR_SystemLastEditTimeUtc, HRR_SystemCreateUser, HRR_SystemLastEditUser, HRR_HA_JobApplicant, HRR_ProbationDurationOverride, HRR_StartDate, HRR_WorkingDays)
VALUES
(newid(), 'MID', 'Mid', GETUTCDATE(), GETUTCDATE(), 'X', 'X', '3247149A-281C-4931-B8B0-3AA77C1CD29D', GETDATE(), NULL, 0)

INSERT INTO dbo.HRHiringRequest 
(HRR_PK, HRR_GS_NKReportingManager, HRR_JobTitle, HRR_SystemCreateTimeUtc, HRR_SystemLastEditTimeUtc, HRR_SystemCreateUser, HRR_SystemLastEditUser, HRR_HA_JobApplicant, HRR_ProbationDurationOverride, HRR_StartDate, HRR_WorkingDays)
VALUES
(newid(), 'NEW', 'New', GETUTCDATE(), GETUTCDATE(), 'X', 'X', '3247149A-281C-4931-B8B0-3AA77C1CD29D', GETDATE(), GETDATE(), 0)

INSERT INTO dbo.HRHiringRequest 
(HRR_PK, HRR_GS_NKReportingManager, HRR_JobTitle, HRR_SystemCreateTimeUtc, HRR_SystemLastEditTimeUtc, HRR_SystemCreateUser, HRR_SystemLastEditUser, HRR_HA_JobApplicant, HRR_ProbationDurationOverride, HRR_StartDate, HRR_WorkingDays)
VALUES
(newid(), 'ZZ', 'ZZ', GETUTCDATE(), GETUTCDATE(), 'X', 'X', '3247149A-281C-4931-B8B0-3AA77C1CD29D', GETDATE(), GETDATE(), 0)

INSERT INTO dbo.HRHiringRequest 
(HRR_PK, HRR_GS_NKReportingManager, HRR_JobTitle, HRR_SystemCreateTimeUtc, HRR_SystemLastEditTimeUtc, HRR_SystemCreateUser, HRR_SystemLastEditUser, HRR_HA_JobApplicant, HRR_ProbationDurationOverride, HRR_StartDate, HRR_WorkingDays)
VALUES
('{requestWithTask1}', 'APP', 'App', GETUTCDATE(), GETUTCDATE(), 'X', 'X', '3247149A-281C-4931-B8B0-3AA77C1CD29D', GETDATE(), GETDATE(), 0)

INSERT INTO dbo.ProcessTasks
(P9_PK, P9_ParentID, P9_ParentTableCode, P9_GS_NKAssignedStaffMember)
VALUES
(newid(), '{requestWithTask1}', 'HRR', 'new')

INSERT INTO dbo.ProcessTasks
(P9_PK, P9_ParentID, P9_ParentTableCode, P9_GS_NKAssignedStaffMember)
VALUES
(newid(), '{requestWithTask1}', 'HRR', 'mid')

INSERT INTO dbo.HRHiringRequest 
(HRR_PK, HRR_GS_NKReportingManager, HRR_JobTitle, HRR_SystemCreateTimeUtc, HRR_SystemLastEditTimeUtc, HRR_SystemCreateUser, HRR_SystemLastEditUser, HRR_HA_JobApplicant, HRR_ProbationDurationOverride, HRR_StartDate, HRR_WorkingDays)
VALUES
('{requestWithTask2}', 'Foo', 'Foo', GETUTCDATE(), GETUTCDATE(), 'X', 'X', '3247149A-281C-4931-B8B0-3AA77C1CD29D', GETDATE(), GETDATE(), 0)

INSERT INTO dbo.ProcessTasks
(P9_PK, P9_ParentID, P9_ParentTableCode, P9_GS_NKAssignedStaffMember)
VALUES
(newid(), '{requestWithTask2}', 'HRR', 'Bos')");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff);
			var actual = results
				.Select()
				.Select(x => x[HRHiringRequestSchema.HRR_JobTitle.Name]);
			AssertContainsExactElementsInAnyOrder(new[] { "Bos", "Mid", "New", "App", "Foo" }, actual);
		}

		public void TestReturnsHRHiringRequestColumns()
		{
			var fromTable = GetColumNames("SELECT * FROM dbo.HRHiringRequest");
			var fromTvf = GetColumNames("SELECT * FROM GetRelevantHiringRequests(NEWID(), 'XXX', GETUTCDATE())");

			var missingColumns = fromTable.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";
			Assert(errorMessage, !missingColumns.Any());
		}

		protected override void SetUp()
		{
			base.SetUp();
			ClearTable(HRHiringRequestSchema.Constants.TableName);
			ClearTable(GlbStaffManagerSchema.Constants.TableName);
			ClearTable(GlbStaffSchema.Constants.TableName);
		}

		static void ClearTable(string tableName)
		{
			var query = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0}", tableName);
			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}
		}

		DataTable Execute(Guid loggedInStaff)
		{
			using (var command = TestConnection.Command("SELECT * FROM GetRelevantHiringRequests(@loggedInStaff, 'XXX', GETUTCDATE())"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		IEnumerable<string> GetColumNames(string commandText)
		{
			using (var command = TestConnection.Command(commandText))
			using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
			{
				return reader.GetSchemaTable()
					.Select()
					.Select(x => (string)x["ColumnName"]);
			}
		}
	}
}
