using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(GlbStaffChangeRequestWorkflowTasks))]
	class GlbStaffChangeRequestWorkflowTasksTest : DbCreateScriptTest
	{
		public void TestGivenNoTask_ThenIsEmpty()
		{
			var rows = ExecuteQuery();
			AssertEquals(0, rows.Length);
		}

		public void TestGivenUnassignedTask_ThenIsNotSelf()
		{
			var taskPK = AddChangeRequestWorkflowTask("OPN");

			var rows = ExecuteQuery();
			AssertEquals(1, rows.Length);
			AssertTaskRow(rows, taskPK, isSelf: false, status: "OPN");
		}

		public void TestGivenTaskAssignedToOtherUser_ThenIsNotSelf()
		{
			TestDataCreator.CreateGlbStaff("PEP", "Pepito");
			var taskPK = AddChangeRequestWorkflowTask("ASN", "PEP");

			var rows = ExecuteQuery();
			AssertEquals(1, rows.Length);
			AssertTaskRow(rows, taskPK, isSelf: false, status: "ASN", assignedStaffMember: "PEP");
		}

		public void TestGivenTask_AssignedToCurrentUser_ThenIsSelf()
		{
			var taskPK = AddChangeRequestWorkflowTask("ASN", staffCode: LoggedInUserCode);

			var rows = ExecuteQuery();
			AssertEquals(1, rows.Length);
			AssertTaskRow(rows, taskPK, isSelf: true, status: "ASN", assignedStaffMember: LoggedInUserCode);
		}

		public void TestGivenTaskWithCapability_WhenUserNotInCapability_ThenIsNotSelf()
		{
			var otherUserPK = TestDataCreator.CreateGlbStaff("PEP", "Pepito");
			var capabilityPK = AddCapabilityWithStaffUsers(new[] { otherUserPK }, "CAP");
			var taskPK = AddChangeRequestWorkflowTask("ASN", requiredCapabilityPK: capabilityPK);

			var rows = ExecuteQuery();
			AssertEquals(1, rows.Length);
			AssertTaskRow(rows, taskPK, isSelf: false, status: "ASN", requiredCapability: capabilityPK);
		}

		public void TestGivenTaskWithCapability_WhenUserInCapability_ThenIsSelf()
		{
			var capabilityPK = AddCapabilityWithStaffUsers(new[] { LoggedInUserPK }, "CAP");
			var taskPK = AddChangeRequestWorkflowTask("ASN", requiredCapabilityPK: capabilityPK);

			var rows = ExecuteQuery();
			AssertEquals(1, rows.Length);
			AssertTaskRow(rows, taskPK, isSelf: true, status: "ASN", requiredCapability: capabilityPK);
		}

		public void TestGivenTaskWithCapability_AssignedToOtherUser_WhenUserInCapability_ThenIsNotSelf()
		{
			var otherUserPK = TestDataCreator.CreateGlbStaff("PEP", "Pepito");
			var capabilityPK = AddCapabilityWithStaffUsers(new[] { LoggedInUserPK, otherUserPK }, "CAP");
			var taskPK = AddChangeRequestWorkflowTask("ASN", staffCode: "PEP", requiredCapabilityPK: capabilityPK);

			var rows = ExecuteQuery();
			AssertEquals(1, rows.Length);
			AssertTaskRow(rows, taskPK, isSelf: false, status: "ASN", assignedStaffMember: "PEP", requiredCapability: capabilityPK);
		}

		public void TestGivenMultipleTasks_ShouldSetIsSelfCorrectly()
		{
			var otherUserPK = TestDataCreator.CreateGlbStaff("PEP", "Pepito");
			var capability1PK = AddCapabilityWithStaffUsers(new[] { otherUserPK }, "CA1");
			var capability2PK = AddCapabilityWithStaffUsers(new[] { LoggedInUserPK }, "CA2");

			var task1PK = AddChangeRequestWorkflowTask("ASN", staffCode: "PEP");
			var task2PK = AddChangeRequestWorkflowTask("ASN", staffCode: LoggedInUserCode);
			var task3PK = AddChangeRequestWorkflowTask("ASN", requiredCapabilityPK: capability1PK);
			var task4PK = AddChangeRequestWorkflowTask("ASN", requiredCapabilityPK: capability2PK);

			var rows = ExecuteQuery();
			AssertEquals(4, rows.Length);
			AssertTaskRow(rows, task1PK, isSelf: false, status: "ASN", assignedStaffMember: "PEP");
			AssertTaskRow(rows, task2PK, isSelf: true, status: "ASN", assignedStaffMember: "007");
			AssertTaskRow(rows, task3PK, isSelf: false, status: "ASN", requiredCapability: capability1PK);
			AssertTaskRow(rows, task4PK, isSelf: true, status: "ASN", requiredCapability: capability2PK);
		}

		void AddChangeRequestWithTemplate(string templateName)
		{
			if (ChangeRequestPK != Guid.Empty)
			{
				return;
			}
			ChangeRequestPK = Guid.NewGuid();

			var templatePK = AddChangeRequestTemplate(templateName);
			var requestValues = AddAuditValues("GCR", new Dictionary<string, string>()
			{
				{ "GCR_PK", "@requestPK" },
				{ "GCR_Status", "'REQ'" },
				{ "GCR_GSG_Template", "@templatePK" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbStaffChangeRequest({0}) VALUES ({1})",
				string.Join(",", requestValues.Keys),
				string.Join(",", requestValues.Values)), (p) =>
				{
					p.AddParameter("@requestPK", SqlDbType.UniqueIdentifier, ChangeRequestPK);
					p.AddParameter("@templatePK", SqlDbType.UniqueIdentifier, templatePK);
					p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
				});
		}

		Guid AddChangeRequestTemplate(string templateName)
		{
			var templatePK = Guid.NewGuid();
			var templateValues = AddAuditValues("GSG", new Dictionary<string, string>()
			{
				{ "GSG_PK", "@templatePK" },
				{ "GSG_Code", "@templateCode" },
				{ "GSG_TemplateName", "@templateName" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbStaffChangeRequestTemplate({0}) VALUES ({1})",
				string.Join(",", templateValues.Keys),
				string.Join(",", templateValues.Values)), (p) =>
			{
				p.AddParameter("@templatePK", SqlDbType.UniqueIdentifier, templatePK);
				p.AddParameter("@templateCode", SqlDbType.VarChar, templatePK.ToString().Substring(0,5));
				p.AddParameter("@templateName", SqlDbType.VarChar, templateName);
				p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
			});

			return templatePK;
		}

		Guid AddCapabilityWithStaffUsers(Guid[] staffPKs, string capabilityCode)
		{
			var capabilityPK = Guid.NewGuid();
			var capabilityRecord = AddAuditValues("G4", new Dictionary<string, string>()
			{
				{ "G4_PK", "@capabilityPK" },
				{ "G4_Code", "@capabilityCode" },
				{ "G4_Description", "@capabilityCode" },
				{ "G4_IsActive", "1" },
				{ "G4_CapacityScope", "'GLB'" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbCapability({0}) VALUES ({1})",
				string.Join(",", capabilityRecord.Keys),
				string.Join(",", capabilityRecord.Values)), (p) =>
				{
					p.AddParameter("@capabilityPK", SqlDbType.UniqueIdentifier, capabilityPK);
					p.AddParameter("@capabilityCode", SqlDbType.VarChar, capabilityCode);
					p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
				});

			foreach (var staffPK in staffPKs)
			{
				var capabilityPivotPK = Guid.NewGuid();
				var pivotRecord = AddAuditValues("G5", new Dictionary<string, string>()
				{
					{ "G5_PK", "@pivotPK" },
					{ "G5_G4_Capability", "@capabilityPK" },
					{ "G5_GS_Resource", "@staffPK" },
					{ "G5_SkillLevel", "1" },
					{ "G5_DateExperienceGained", "@testDateUtc" },
				});
				TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbResourceCapabilityPivot({0}) VALUES ({1})",
					string.Join(",", pivotRecord.Keys),
					string.Join(",", pivotRecord.Values)), (p) =>
					{
						p.AddParameter("@pivotPK", SqlDbType.UniqueIdentifier, capabilityPivotPK);
						p.AddParameter("@capabilityPK", SqlDbType.UniqueIdentifier, capabilityPK);
						p.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK);
						p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
					});
			}
			return capabilityPK;
		}

		Guid AddChangeRequestWorkflowTask(string status, string staffCode = "", Guid? requiredCapabilityPK = null)
		{
			AddChangeRequestWithTemplate("My template");
			var taskPK = Guid.NewGuid();
			var requestValues = AddAuditValues("P9", new Dictionary<string, string>()
			{
				{ "P9_PK", "@taskPK" },
				{ "P9_Description", "'Test Task'" },
				{ "P9_G4_RequiredCapability", requiredCapabilityPK != null ? "@requiredCapabilityPK" : "NULL" },
				{ "P9_GS_NKAssignedStaffMember", "@staffCode" },
				{ "P9_ParentID", "@changeRequestPK" },
				{ "P9_ParentTableCode", "'GCR'" },
				{ "P9_Status", "@status" },
				{ "P9_Type", "'UDF'" },
				{ "P9_IsValid", "1" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.WorkflowTask({0}) VALUES ({1})",
				string.Join(",", requestValues.Keys),
				string.Join(",", requestValues.Values)), (p) =>
			{
				p.AddParameter("@taskPK", SqlDbType.UniqueIdentifier, taskPK);
				p.AddParameter("@requiredCapabilityPK", SqlDbType.UniqueIdentifier, requiredCapabilityPK ?? Guid.Empty);
				p.AddParameter("@staffCode", SqlDbType.VarChar, staffCode);
				p.AddParameter("@changeRequestPK", SqlDbType.UniqueIdentifier, ChangeRequestPK);
				p.AddParameter("@status", SqlDbType.VarChar, status);
				p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
			});

			return taskPK;
		}

		Dictionary<string, string> AddAuditValues(string prefix, Dictionary<string, string> recordValues)
		{
			recordValues.Add($"{prefix}_SystemCreateTimeUtc", "@testDateUtc");
			recordValues.Add($"{prefix}_SystemCreateUser", "'~BP'");
			recordValues.Add($"{prefix}_SystemLastEditTimeUtc", "@testDateUtc");
			recordValues.Add($"{prefix}_SystemLastEditUser", "'~BP'");
			return recordValues;
		}

		DataRow[] ExecuteQuery()
		{
			using (var command = TestConnection.Command($"SELECT * FROM GlbStaffChangeRequestWorkflowTasks(@loggedInStaff)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, LoggedInUserPK);
				return DataUtils.GetDataTableFromCommand(command).Select();
			}
		}

		void AssertTaskRow(DataRow[] rows, Guid taskPK, bool isSelf, string status, string assignedStaffMember = "", Guid? requiredCapability = null)
		{
			var row = rows.SingleOrDefault(r => (Guid)r["P9_PK"] == taskPK);
			AssertNotNull($"Expected task row with PK={taskPK} to be included in result.", row);

			AssertEquals("Test Task", (string)row["P9_Description"]);
			AssertEquals(assignedStaffMember, (string)row["P9_GS_NKAssignedStaffMember"]);
			AssertEquals(status, (string)row["P9_Status"]);
			AssertEquals(ChangeRequestPK, (Guid)row["P9_ParentID"]);
			AssertEquals("GCR", (string)row["P9_ParentTableCode"]);
			AssertEquals("UDF", (string)row["P9_Type"]);
			AssertEquals(true, (bool)row["P9_IsPublished"]);
			AssertEquals(true, (bool)row["P9_IsValid"]);
			AssertEquals((object)requiredCapability ?? DBNull.Value, row["P9_G4_RequiredCapability"]);

			AssertEquals(isSelf, (bool)row["IsSelf"]);
			AssertEquals(false, (bool)row["IsManaged1"]);
			AssertEquals(false, (bool)row["IsManaged2"]);
			AssertEquals(false, (bool)row["IsManaged3"]);

			AssertEquals("~BP", (string)row["P9_SystemCreateUser"]);
			AssertEquals("~BP", (string)row["P9_SystemLastEditUser"]);
			AssertEquals(TestDateUtc, (DateTime)row["P9_SystemCreateTimeUtc"]);
			AssertEquals(TestDateUtc, (DateTime)row["P9_SystemLastEditTimeUtc"]);

			AssertEquals(DBNull.Value, row["P9_ActualDate"]);
			AssertEquals(DBNull.Value, row["P9_ActualDateUtc"]);
			AssertEquals(DBNull.Value, row["P9_ActualDuration"]);
			AssertEquals(DBNull.Value, row["P9_CompletedTimeUtc"]);
			AssertEquals(DBNull.Value, row["P9_EstDuration"]);
			AssertEquals(DBNull.Value, row["P9_FH_ProcessHeader"]);
			AssertEquals(DBNull.Value, row["P9_Notes"]);
			AssertEquals(string.Empty, (string)row["P9_Outcome"]);
			AssertEquals(string.Empty, (string)row["P9_SE_NKTaskCompletionEvent"]);
		}

		readonly DateTime TestDateUtc = new DateTime(2023, 1, 1, 8, 0, 0);
		readonly string LoggedInUserCode = "007";

		Guid ChangeRequestPK = Guid.Empty;

		Guid LoggedInUserPK => loggedInUserPK == Guid.Empty ? (loggedInUserPK = TestDataCreator.CreateGlbStaff(LoggedInUserCode, "Bond")) : loggedInUserPK;
		Guid loggedInUserPK = Guid.Empty;
	}
}
