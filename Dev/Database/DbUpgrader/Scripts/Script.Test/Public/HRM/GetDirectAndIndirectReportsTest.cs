using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(GetDirectAndIndirectReports))]
	class GetDirectAndIndirectReportsTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			var loggedInStaff = Guid.NewGuid();
			var myDirectReport = Guid.NewGuid();
			var anotherDirectReport = Guid.NewGuid();
			var myIndirectReport = Guid.NewGuid();
			var noLongerReportsToMe = Guid.NewGuid();
			var notYetReportingToMe = Guid.NewGuid();
			var differentReportingLine = Guid.NewGuid();

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

INSERT INTO dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES('{noLongerReportsToMe}', 'old', 'old', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaffManager (GSM_PK, GSM_GS_Manager, GSM_GS_Staff, GSM_ManagerType, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_EffectiveDate, GSM_EndDate)
VALUES (NEWID(), '{loggedInStaff}', '{noLongerReportsToMe}', 'XXX', GETUTCDATE(), GETUTCDATE(), 'X', 'X', DATEADD(day, -7, GETUTCDATE()), DATEADD(day, -1, GETUTCDATE()))

INSERT INTO dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES('{notYetReportingToMe}', 'fut', 'future', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaffManager (GSM_PK, GSM_GS_Manager, GSM_GS_Staff, GSM_ManagerType, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_EffectiveDate)
VALUES (NEWID(), '{loggedInStaff}', '{notYetReportingToMe}', 'XXX', GETUTCDATE(), GETUTCDATE(), 'X', 'X', DATEADD(day, 7, GETUTCDATE()))

-- Someone else
INSERT INTO dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES(newid(), 'foo', 'foo', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{anotherDirectReport}', 'mi2', 'mid 2', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaffManager (GSM_PK, GSM_GS_Manager, GSM_GS_Staff, GSM_ManagerType, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_EffectiveDate)
VALUES (NEWID(), '{loggedInStaff}', '{anotherDirectReport}', 'XXX', GETUTCDATE(), GETUTCDATE(), 'X', 'X', DATEADD(day, -7, GETUTCDATE()))

INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{differentReportingLine}', 'mi3', 'mid 3', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaffManager (GSM_PK, GSM_GS_Manager, GSM_GS_Staff, GSM_ManagerType, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_EffectiveDate)
VALUES (NEWID(), '{loggedInStaff}', '{differentReportingLine}', 'YYY', GETUTCDATE(), GETUTCDATE(), 'X', 'X', DATEADD(day, -7, GETUTCDATE()))
");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff);
			var actual = results
				.Select()
				.Select(x => x[GlbStaffManagerSchema.GSM_GS_Staff.Name]);
			AssertContainsExactElementsInAnyOrder(new[] { myDirectReport, myIndirectReport, anotherDirectReport }, actual);
		}

		public void TestExecute_IsApproved_False_DirectReport()
		{
			var loggedInStaff = Guid.NewGuid();
			var myDirectReport = Guid.NewGuid();
			var myIndirectReport = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{loggedInStaff}', 'bos', 'bos', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{myDirectReport}', 'mid', 'mid', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaffManager (GSM_PK, GSM_GS_Manager, GSM_GS_Staff, GSM_ManagerType, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_EffectiveDate, GSM_IsApproved)
VALUES (NEWID(), '{loggedInStaff}', '{myDirectReport}', 'XXX', GETUTCDATE(), GETUTCDATE(), 'X', 'X', DATEADD(day, -1, GETUTCDATE()), 0)

INSERT INTO dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES('{myIndirectReport}', 'new', 'new', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaffManager (GSM_PK, GSM_GS_Manager, GSM_GS_Staff, GSM_ManagerType, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_EffectiveDate)
VALUES (NEWID(), '{myDirectReport}', '{myIndirectReport}', 'XXX', GETUTCDATE(), GETUTCDATE(), 'X', 'X', DATEADD(day, -1, GETUTCDATE()))
");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff);
			AssertEmpty(results);
		}

		public void TestExecute_IsApproved_False_IndirectReport()
		{
			var loggedInStaff = Guid.NewGuid();
			var myDirectReport = Guid.NewGuid();
			var myIndirectReport = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{loggedInStaff}', 'bos', 'bos', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES ('{myDirectReport}', 'mid', 'mid', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaffManager (GSM_PK, GSM_GS_Manager, GSM_GS_Staff, GSM_ManagerType, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_EffectiveDate)
VALUES (NEWID(), '{loggedInStaff}', '{myDirectReport}', 'XXX', GETUTCDATE(), GETUTCDATE(), 'X', 'X', DATEADD(day, -1, GETUTCDATE()))

INSERT INTO dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES('{myIndirectReport}', 'new', 'new', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaffManager (GSM_PK, GSM_GS_Manager, GSM_GS_Staff, GSM_ManagerType, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_EffectiveDate, GSM_IsApproved)
VALUES (NEWID(), '{myDirectReport}', '{myIndirectReport}', 'XXX', GETUTCDATE(), GETUTCDATE(), 'X', 'X', DATEADD(day, -1, GETUTCDATE()), 0)
");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff);
			var actual = results
				.Select()
				.Select(x => x[GlbStaffManagerSchema.GSM_GS_Staff.Name]);
			AssertContainsExactElementsInAnyOrder(new[] { myDirectReport }, actual);
		}

		protected override void SetUp()
		{
			base.SetUp();
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
			using (var command = TestConnection.Command("SELECT * FROM GetDirectAndIndirectReports(@loggedInStaff, 'XXX', GETUTCDATE())"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		void AssertEmpty(DataTable table)
		{
			var errorMsg = string.Join(Environment.NewLine, table
				.Select()
				.Select((row, index) => $"{index}: {string.Join(", ", row.ItemArray.Select(UsefulName))}"));

			Assert(errorMsg, string.IsNullOrEmpty(errorMsg));

			string UsefulName(object o)
			{
				if (o is null || o is DBNull)
				{
					return "NULL";
				}

				if (o is string s)
				{
					return $"'{s}'";
				}

				return o.ToString();
			}
		}
	}
}
