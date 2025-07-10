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
	[TestedType(typeof(GlbStaffChangeRequest_WithSecurity))]
	class GlbStaffChangeRequest_WithSecurityTest : DbCreateScriptTest
	{
		public void TestGivenNoChangeRequests_ReturnsEmpty()
		{
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertEquals(0, rows.Length);
		}

		public void TestGivenChangeRequests_ReturnsCorrectRows()
		{
			var templatePK = AddChangeRequestTemplate("GEHTemplate", "code");

			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var changeRequest1 = AddChangeRequest(templatePK);
			AddEmploymentHistory(changeRequest1, loggedInStaffPK);

			var anotherStaffPK = TestDataCreator.CreateGlbStaff("MI6", "Not Bond");
			var changeRequest2 = AddChangeRequest(templatePK);
			AddEmploymentHistory(changeRequest2, anotherStaffPK);

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertEquals(2, rows.Length);
			AssertChangeRequestRow(rows, changeRequest1, templatePK, isSelf: true, hasAssignedTasks: false);
			AssertChangeRequestRow(rows, changeRequest2, templatePK, isSelf: false, hasAssignedTasks: false);
		}

		public void TestGivenChangeRequest_WithEmploymentHistory()
		{
			var templatePK = AddChangeRequestTemplate("SomeTemplate", "code");
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var changeRequest = AddChangeRequest(templatePK);
			AddEmploymentHistory(changeRequest, loggedInStaffPK);

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertEquals(1, rows.Length);
			AssertChangeRequestRow(rows, changeRequest, templatePK, isSelf: true, hasAssignedTasks: false);
		}

		public void TestGivenChangeRequest_WithEmploymentTeam()
		{
			var templatePK = AddChangeRequestTemplate("SomeTemplate", "code");
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var changeRequest = AddChangeRequest(templatePK);
			AddEmploymentTeam(changeRequest, loggedInStaffPK, "TEA");

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertEquals(1, rows.Length);
			AssertChangeRequestRow(rows, changeRequest, templatePK, isSelf: true, hasAssignedTasks: false);
		}

		public void TestGivenChangeRequest_WithStaffManager()
		{
			var templatePK = AddChangeRequestTemplate("SomeTemplate", "code");
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var changeRequest = AddChangeRequest(templatePK);
			AddStaffManager(changeRequest, loggedInStaffPK, "MGR");

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertEquals(1, rows.Length);
			AssertChangeRequestRow(rows, changeRequest, templatePK, isSelf: true, hasAssignedTasks: false);
		}

		public void TestGivenChangeRequest_WithWorkPattern()
		{
			var templatePK = AddChangeRequestTemplate("SomeTemplate", "code");
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var changeRequest = AddChangeRequest(templatePK);
			AddWorkPattern(changeRequest, loggedInStaffPK);

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertEquals(1, rows.Length);
			AssertChangeRequestRow(rows, changeRequest, templatePK, isSelf: true, hasAssignedTasks: false);
		}

		public void TestGivenChangeRequest_WithMultipleSimilarRecords_ShouldNotDuplicate()
		{
			var templatePK = AddChangeRequestTemplate("SomeTemplate", "code");
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var pepitoStaffPK = TestDataCreator.CreateGlbStaff("PEP", "Pepito");
			var changeRequest = AddChangeRequest(templatePK);

			AddEmploymentHistory(changeRequest, pepitoStaffPK);
			AddEmploymentHistory(changeRequest, loggedInStaffPK);

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertEquals(1, rows.Length);
			AssertChangeRequestRow(rows, changeRequest, templatePK, isSelf: true, hasAssignedTasks: false);
		}

		public void TestGivenChangeRequest_WithMultipleDistinctRecords_ShouldNotDuplicate()
		{
			var templatePK = AddChangeRequestTemplate("SomeTemplate", "code");
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var pepitoStaffPK = TestDataCreator.CreateGlbStaff("PEP", "Pepito");
			var changeRequestPK = AddChangeRequest(templatePK);

			AddEmploymentHistory(changeRequestPK, pepitoStaffPK);
			AddEmploymentTeam(changeRequestPK, pepitoStaffPK, "TEA");
			AddStaffManager(changeRequestPK, loggedInStaffPK, "GOD");
			AddWorkPattern(changeRequestPK, pepitoStaffPK);

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertEquals(1, rows.Length);
			AssertChangeRequestRow(rows, changeRequestPK, templatePK, isSelf: true, hasAssignedTasks: false);
		}

		public void TestGivenTasks_WhenAssignedToCurrentUser_ThenHasAssignedTasks()
		{
			var templatePK = AddChangeRequestTemplate("GEHTemplate", "code");
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var changeRequestPK = AddChangeRequest(templatePK);
			AddEmploymentHistory(changeRequestPK, loggedInStaffPK);
			AddWorkflowTask(changeRequestPK, "007");

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertChangeRequestRow(rows, changeRequestPK, templatePK, isSelf: true, hasAssignedTasks: true);
		}

		public void TestGivenTasks_WhenAssignedToOtherUser_ThenHasNoAssignedTasks()
		{
			var templatePK = AddChangeRequestTemplate("GEHTemplate", "code");
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var changeRequestPK = AddChangeRequest(templatePK);
			AddEmploymentHistory(changeRequestPK, loggedInStaffPK);

			TestDataCreator.CreateGlbStaff("MI6", "Another");
			AddWorkflowTask(changeRequestPK, "MI6");

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertChangeRequestRow(rows, changeRequestPK, templatePK, isSelf: true, hasAssignedTasks: false);
		}

		public void TestGivenTasks_WhenAssignedToCurrentUserCapability_ThenHasAssignedTasks()
		{
			var templatePK = AddChangeRequestTemplate("GEHTemplate", "code");
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var capabilityPK = AddCapabilityWithStaffUser(loggedInStaffPK, "CAP");
			var changeRequestPK = AddChangeRequest(templatePK);
			AddEmploymentHistory(changeRequestPK, loggedInStaffPK);
			AddWorkflowTask(changeRequestPK, "", capabilityPK);

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertChangeRequestRow(rows, changeRequestPK, templatePK, isSelf: true, hasAssignedTasks: true);
		}

		public void TestGivenTasks_WhenAssignedToOtherUserCapability_ThenHasNoAssignedTasks()
		{
			var templatePK = AddChangeRequestTemplate("GEHTemplate", "code");
			var loggedInStaffPK = TestDataCreator.CreateGlbStaff("007", "Bond");
			var anotherStaffPK = TestDataCreator.CreateGlbStaff("MI6", "Another");
			var capabilityPK = AddCapabilityWithStaffUser(anotherStaffPK, "CAP");
			var changeRequestPK = AddChangeRequest(templatePK);
			AddEmploymentHistory(changeRequestPK, loggedInStaffPK);
			AddWorkflowTask(changeRequestPK, "", capabilityPK);

			var rows = RunChangeRequestsQuery(loggedInStaffPK).Select();
			AssertChangeRequestRow(rows, changeRequestPK, templatePK, isSelf: true, hasAssignedTasks: false);
		}

		Guid AddChangeRequestTemplate(string templateName, string code)
		{
			var templatePK = Guid.NewGuid();
			var templateValues = AddAuditValues("GSG", new Dictionary<string, string>()
			{
				{ "GSG_PK", "@templatePK" },
				{ "GSG_TemplateName", "@templateName" },
				{ "GSG_Code", "@code" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbStaffChangeRequestTemplate({0}) VALUES ({1})",
				string.Join(",", templateValues.Keys),
				string.Join(",", templateValues.Values)), (p) =>
			{
				p.AddParameter("@templatePK", SqlDbType.UniqueIdentifier, templatePK);
				p.AddParameter("@templateName", SqlDbType.VarChar, templateName);
				p.AddParameter("@code", SqlDbType.VarChar, code);
				p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
			});

			return templatePK;
		}

		Guid AddEmploymentHistory(Guid changeRequestPK, Guid staffPK)
		{
			var employmentPK = Guid.NewGuid();
			var historyRecord = AddAuditValues("GEH", new Dictionary<string, string>()
			{
				{ "GEH_PK", "@historyPK" },
				{ "GEH_GS_Staff", "@staffPK" },
				{ "GEH_EffectiveDate", "@testDateUtc" },
				{ "GEH_GCR_ChangeRequest", "@changeRequestPK" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbEmploymentHistory({0}) VALUES ({1})",
				string.Join(",", historyRecord.Keys),
				string.Join(",", historyRecord.Values)), (p) =>
				{
					p.AddParameter("@historyPK", SqlDbType.UniqueIdentifier, employmentPK);
					p.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK);
					p.AddParameter("@changeRequestPK", SqlDbType.UniqueIdentifier, changeRequestPK);
					p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
				});

			return employmentPK;
		}

		Guid AddEmploymentTeam(Guid changeRequestPK, Guid staffPK, string teamCode)
		{
			var teamPK = Guid.NewGuid();
			var teamRecord = AddAuditValues("GST", new Dictionary<string, string>()
			{
				{ "GST_PK", "@teamPK" },
				{ "GST_Code", "@teamCode" },
				{ "GST_Name", "@teamCode" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbTeam({0}) VALUES ({1})",
				string.Join(",", teamRecord.Keys),
				string.Join(",", teamRecord.Values)), (p) =>
				{
					p.AddParameter("@teamPK", SqlDbType.UniqueIdentifier, teamPK);
					p.AddParameter("@teamCode", SqlDbType.VarChar, teamCode);
					p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
				});

			var employmentTeamPK = Guid.NewGuid();
			var employmentTeamRecord = AddAuditValues("GET", new Dictionary<string, string>()
			{
				{ "GET_PK", "@employmentTeamPK" },
				{ "GET_GS_Staff", "@staffPK" },
				{ "GET_GST_NKTeamCode", "@teamCode" },
				{ "GET_EffectiveDate", "@testDateUtc" },
				{ "GET_GCR_ChangeRequest", "@changeRequestPK" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbEmploymentTeam({0}) VALUES ({1})",
				string.Join(",", employmentTeamRecord.Keys),
				string.Join(",", employmentTeamRecord.Values)), (p) =>
				{
					p.AddParameter("@employmentTeamPK", SqlDbType.UniqueIdentifier, employmentTeamPK);
					p.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK);
					p.AddParameter("@teamCode", SqlDbType.VarChar, teamCode);
					p.AddParameter("@changeRequestPK", SqlDbType.UniqueIdentifier, changeRequestPK);
					p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
				});

			return employmentTeamPK;
		}

		Guid AddStaffManager(Guid changeRequestPK, Guid staffPK, string managerCode)
		{
			var managerPK = TestDataCreator.CreateGlbStaff(managerCode, managerCode);
			var staffManagerPK = Guid.NewGuid();
			var staffManagerRecord = AddAuditValues("GSM", new Dictionary<string, string>()
			{
				{ "GSM_PK", "@staffManagerPK" },
				{ "GSM_GS_Staff", "@staffPK" },
				{ "GSM_ManagerType", "'PPL'" },
				{ "GSM_GS_Manager", "@managerPK" },
				{ "GSM_EffectiveDate", "@testDateUtc" },
				{ "GSM_GCR_ChangeRequest", "@changeRequestPK" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbStaffManager({0}) VALUES ({1})",
				string.Join(",", staffManagerRecord.Keys),
				string.Join(",", staffManagerRecord.Values)), (p) =>
				{
					p.AddParameter("@staffManagerPK", SqlDbType.UniqueIdentifier, staffManagerPK);
					p.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK);
					p.AddParameter("@managerPK", SqlDbType.UniqueIdentifier, managerPK);
					p.AddParameter("@changeRequestPK", SqlDbType.UniqueIdentifier, changeRequestPK);
					p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
				});

			return staffManagerPK;
		}

		Guid AddWorkPattern(Guid changeRequestPK, Guid staffPK)
		{
			var workPatternPK = Guid.NewGuid();
			var workPatternRecord = AddAuditValues("GWP", new Dictionary<string, string>()
			{
				{ "GWP_PK", "@staffManagerPK" },
				{ "GWP_GS_Staff", "@staffPK" },
				{ "GWP_EffectiveDate", "@testDateUtc" },
				{ "GWP_StandardDuration", "'1901-01-01 00:00:00'" },
				{ "GWP_GCR_ChangeRequest", "@changeRequestPK" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbWorkPattern({0}) VALUES ({1})",
				string.Join(",", workPatternRecord.Keys),
				string.Join(",", workPatternRecord.Values)), (p) =>
				{
					p.AddParameter("@staffManagerPK", SqlDbType.UniqueIdentifier, workPatternPK);
					p.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK);
					p.AddParameter("@changeRequestPK", SqlDbType.UniqueIdentifier, changeRequestPK);
					p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
				});

			return workPatternPK;
		}

		Guid AddChangeRequest(Guid templatePK)
		{
			var changeRequestPK = Guid.NewGuid();
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
				p.AddParameter("@requestPK", SqlDbType.UniqueIdentifier, changeRequestPK);
				p.AddParameter("@templatePK", SqlDbType.UniqueIdentifier, templatePK);
				p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
			});

			return changeRequestPK;
		}

		Guid AddCapabilityWithStaffUser(Guid staffPK, string capabilityCode)
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
			return capabilityPK;
		}

		void AddWorkflowTask(Guid changeRequestPK, string staffCode, Guid? requiredCapabilityPK = null)
		{
			var taskPK = Guid.NewGuid();
			var requestValues = AddAuditValues("P9", new Dictionary<string, string>()
			{
				{ "P9_PK", "@taskPK" },
				{ "P9_Description", "'Test Task'" },
				{ "P9_G4_RequiredCapability", requiredCapabilityPK != null ? "@requiredCapabilityPK" : "NULL" },
				{ "P9_GS_NKAssignedStaffMember", "@staffCode" },
				{ "P9_ParentID", "@changeRequestPK" },
				{ "P9_ParentTableCode", "'GCR'" },
				{ "P9_Status", "'ASN'" },
				{ "P9_Type", "'UDF'" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.WorkflowTask({0}) VALUES ({1})",
				string.Join(",", requestValues.Keys),
				string.Join(",", requestValues.Values)), (p) =>
			{
				p.AddParameter("@taskPK", SqlDbType.UniqueIdentifier, taskPK);
				p.AddParameter("@requiredCapabilityPK", SqlDbType.UniqueIdentifier, requiredCapabilityPK ?? Guid.Empty);
				p.AddParameter("@staffCode", SqlDbType.VarChar, staffCode);
				p.AddParameter("@changeRequestPK", SqlDbType.UniqueIdentifier, changeRequestPK);
				p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
			});
		}

		Dictionary<string, string> AddAuditValues(string prefix, Dictionary<string, string> recordValues)
		{
			recordValues.Add($"{prefix}_SystemCreateTimeUtc", "@testDateUtc");
			recordValues.Add($"{prefix}_SystemCreateUser", "'E'");
			recordValues.Add($"{prefix}_SystemLastEditTimeUtc", "@testDateUtc");
			recordValues.Add($"{prefix}_SystemLastEditUser", "'E'");
			return recordValues;
		}

		DataTable RunChangeRequestsQuery(Guid loggedInStaff)
		{
			using (var command = TestConnection.Command($"SELECT * FROM GlbStaffChangeRequest_WithSecurity(@loggedInStaff)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);
				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		void AssertChangeRequestRow(DataRow[] rows, Guid requestPK, Guid templatePK, bool isSelf, bool hasAssignedTasks)
		{
			var row = rows.Single(r => (Guid)r["GCR_PK"] == requestPK);

			AssertEquals((Guid)row["GCR_GSG_Template"], templatePK);
			AssertEquals((string)row["GCR_Status"], "REQ");
			AssertEquals((DateTime)row["GCR_SystemCreateTimeUtc"], TestDateUtc);
			AssertEquals((string)row["GCR_SystemCreateUser"], "E");
			AssertEquals((DateTime)row["GCR_SystemLastEditTimeUtc"], TestDateUtc);
			AssertEquals((string)row["GCR_SystemLastEditUser"], "E");
			AssertEquals((bool)row["IsSelf"], isSelf);
			AssertEquals((bool)row["HasAssignedTasks"], hasAssignedTasks);
		}

		DateTime TestDateUtc => new DateTime(2023, 1, 1, 8, 0, 0);
	}
}
