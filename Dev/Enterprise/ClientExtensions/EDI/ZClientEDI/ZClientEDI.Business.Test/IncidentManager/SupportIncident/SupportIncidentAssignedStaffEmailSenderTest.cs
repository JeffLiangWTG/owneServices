using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class SupportIncidentAssignedStaffEmailSenderTest : TestCaseWithFactory
	{
		#region Send Email for Support Stage

		[TestSemaphoreProvider()]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestSendEmailToAssignedStaff_Support()
		{
			var initialUserContext = Env.CurrentUserContext;

			var staff = NewStaff("ADL", "andrew@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			try
			{
				Assert(Env.LoginController.LoginLocation(Env.LoginController.LoginUser(staff.GS_LoginName, ""), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
				new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);
				Env.LoginController.Logout();
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertEmail("Email should be sent to assigned customer service incident staff", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.");
		}

		public void TestSendEmailToAssignedStaff_SupportNotLoggedIn()
		{
			using (EDIDataRegistry.Instance.IncidentStaffUnavailableSupportNotificationEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var staff = NewStaff("ADL", "andrew@cargowise.com");
				staff.GS_FullName = "Andrew";
				var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);
				incident.IM_GS_NKCustServiceContact = staff.GS_Code;

				var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
				new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

				AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);

				AssertEmail("Email should be sent to assigned customer service incident staff", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
	"RE Customer Service Incident Raised - CS00219870",
	@"This is the body of an Incident email.");

				AssertEmail("Email should be sent to support mailbox", 1, "Bob", "bob@test.com", "support@wisetechglobal.com",
	"User Andrew is unavailable for Customer Service",
	@"This is the body of an Incident email.

----
You are receiving this Email because:
 - Staff Member Andrew is not logged in.
 - You are the Fallback Recipient for incidents where the staff is unavailable.
");
			}
		}

		public void TestSendEmailToAssignedStaff_SupportNotLoggedIn_NotificationDisabled()
		{
			using (EDIDataRegistry.Instance.IncidentStaffUnavailableSupportNotificationEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var staff = NewStaff("ADL", "andrew@cargowise.com");
				staff.GS_FullName = "Andrew";
				var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);
				incident.IM_GS_NKCustServiceContact = staff.GS_Code;

				var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
				new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

				AssertEmail("Email should be sent to assigned customer service incident staff", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
	"RE Customer Service Incident Raised - CS00219870",
	@"This is the body of an Incident email.");
			}
		}

		public void TestSendEmailToAssignedStaff_SupportNotLoggedIn_NotificationEnabled_GroupSet()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupStaff = group.Staff.AddNew();
			groupStaff.GS_FullName = "aaa";
			groupStaff.GS_Code = "aaa";
			groupStaff.GS_EmailAddress = "aaa@email.com";
			Factory.Save();

			using (EDIDataRegistry.Instance.IncidentStaffUnavailableSupportNotificationEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (EDIDataRegistry.Instance.IncidentStaffUnavailableSupportNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
				{
					var staff = NewStaff("ADL", "andrew@cargowise.com");
					staff.GS_FullName = "Andrew";
					var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);
					incident.IM_GS_NKCustServiceContact = staff.GS_Code;

					var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
					new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

					AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);

					AssertEmail("Email should be sent to assigned customer service incident staff", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
		"RE Customer Service Incident Raised - CS00219870",
		@"This is the body of an Incident email.");

					AssertEmail("Email should be sent to support mailbox", 1, "Bob", "bob@test.com", groupStaff.GS_EmailAddress,
	"User Andrew is unavailable for Customer Service",
	@"This is the body of an Incident email.

----
You are receiving this Email because:
 - Staff Member Andrew is not logged in.
 - You are the Fallback Recipient for incidents where the staff is unavailable.
");
				}
			}
		}

		public void TestSendEmailToAssignedStaff_SupportNotLoggedInCritical()
		{
			var staff = NewStaff("ADL", "andrew@cargowise.com");
			staff.GS_FullName = "Andrew";
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.IM_Priority = "TST";
			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertEmail("Email should be sent to assigned customer service incident staff", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.");

			AssertEmail("Email should be sent to support mailbox", 1, "Bob", "bob@test.com", "support@wisetechglobal.com",
"User Andrew is unavailable for Customer Service (TST)",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - Staff Member Andrew is not logged in.
 - You are the Fallback Recipient for incidents where the staff is unavailable.
");
		}

		[TestSemaphoreProvider()]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestSendEmailToAssignedStaff_SupportNoActivity()
		{
			var initialUserContext = Env.CurrentUserContext;

			var staff = NewStaff("ADL", "andrew@cargowise.com");
			staff.GS_FullName = "Andrew";
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			try
			{
				Assert(Env.LoginController.LoginLocation(Env.LoginController.LoginUser(staff.GS_LoginName, ""), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				ZQuery query = new ZQuery(StmActivityLogSchema.S7_GS_NKUser, staff.GS_Code);
				query.OrderBy = StmActivityLogSchema.Constants.S7_OpenDateTimeUtc + OrderByClause.Descending;
				var log = Factory.LoadTop1<StmActivityLog>(query);
				log.S7_OpenDateTimeUtc = log.S7_OpenDateTimeUtc.AddMinutes(-70);
				Factory.Save();

				var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
				new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);
				Env.LoginController.Logout();
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertEmail("Email should be sent to assigned customer service incident staff", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.");

			AssertEmail("Email should be sent to support mailbox", 1, "Bob", "bob@test.com", "support@wisetechglobal.com",
"User Andrew is unavailable for Customer Service",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - Staff Member Andrew has no activity in the last 60 minutes.
 - You are the Fallback Recipient for incidents where the staff is unavailable.
");
		}

		public void TestSendEmailToAssignedStaff_Support_LastAssignedStaff()
		{
			var staff1 = NewStaff("ADL", "andrew@cargowise.com");
			var staff2 = NewStaff("SCW", "samuel@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);
			var task1 = SupportIncidentTestHelper.AddWorkflowTask(incident, "Confirm As Defect", "INV", ProcessTaskStatusCodeList.Codes.Closed, staff1.GS_Code, "", ZDateTimeOffset.Empty);
			var task2 = SupportIncidentTestHelper.AddWorkflowTask(incident, "New Open Unassigned Task", "INV", ProcessTaskStatusCodeList.Codes.Open);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to last closed task assignee", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - You are the Staff Assigned to Last Closed Task.
");
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			incident.IM_CloseTimeUtc = ZDateTime.Now.AddDays(-29);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to last closed task assignee", 0, "Bob", "bob@test.com", "samuel@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - You are the Staff Assigned to Last Closed Task.
");
		}

		public void TestSendEmailToAssignedStaff_Support_NoLastAssignedStaff()
		{
			AddProductAreaStaffAssignment("ARC", "Architecture", "PRD", "product.staff@cargowise.com");

			var staff1 = NewStaff("ADL", "andrew@cargowise.com");
			var staff2 = NewStaff("SCW", "samuel@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);
			incident.ProductArea = "ARC";
			var task1 = SupportIncidentTestHelper.AddWorkflowTask(incident, "Confirm As Defect", "INV", ProcessTaskStatusCodeList.Codes.Cancelled, staff1.GS_Code, "", ZDateTimeOffset.Empty);
			var task2 = SupportIncidentTestHelper.AddWorkflowTask(incident, "New Open Unassigned Task", "INV", ProcessTaskStatusCodeList.Codes.Open);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to product area assignee", 0, "Bob", "bob@test.com", "product.staff@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - There is no Staff Assigned to Last Closed Task.
 - You are the Incident Product Area Assignee.
");
		}

		public void TestSendEmailToAssignedStaff_Support_ClosedNoWorkflow()
		{
			AddProductAreaStaffAssignment("ARC", "Architecture", "PRD", "product.staff@cargowise.com");

			var staff = NewStaff("SCW", "samuel@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);
			incident.ProductArea = "ARC";
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_CloseTimeUtc = ZDateTime.Now.AddDays(-45);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to product area assignee", 0, "Bob", "bob@test.com", "product.staff@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Assigned Customer Service Staff.
 - You are the Incident Product Area Assignee.
");
		}

		public void TestSendEmailToAssignedStaff_Support_NoCustServiceContact_NoLastAssignedStaff_NoProductAreaAssignment()
		{
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to support mailbox", 0, "Bob", "bob@test.com", "support@wisetechglobal.com",
"Incident Customer Service Staff is Unavailable",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Assigned Customer Service Staff.
 - There is no Staff Assigned to Last Closed Task.
 - There is no Incident Product Area Assignee.
 - You are the Fallback Recipient for incidents where the staff is unavailable.
");
		}

		[TestDate(2025, 4, 10, 9, 0, 0)]
		public void TestSendEmailToAssignedStaff_ClosedIncidentLastAssigneeNotificationPeriod()
		{
			var initialUserContext = Env.CurrentUserContext;

			var staff = NewStaff("ADL", "andrew@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Support);
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_CloseTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			using (EDIDataRegistry.Instance.ClosedIncidentLastAssigneeNotificationPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				try
				{
					Assert(Env.LoginController.LoginLocation(Env.LoginController.LoginUser(staff.GS_LoginName, ""), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
					var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
					new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);
					Env.LoginController.Logout();
				}
				finally
				{
					Env.SetUserContext(initialUserContext);
				}

				AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("andrew@cargowise.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			}

			using (EDIDataRegistry.Instance.ClosedIncidentLastAssigneeNotificationPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -1))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();

				try
				{
					Assert(Env.LoginController.LoginLocation(Env.LoginController.LoginUser(staff.GS_LoginName, ""), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
					var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
					new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);
					Env.LoginController.Logout();
				}
				finally
				{
					Env.SetUserContext(initialUserContext);
				}

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertNotEquals("andrew@cargowise.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			}
		}

		#endregion

		#region Send Email For Defect Stage

		public void TestSendEmailToAssignedStaff_Defect()
		{
			AddProductAreaStaffAssignment("ARC", "Architecture", "PRD", "product.staff@cargowise.com");

			var staff = NewStaff("ADL", "andrew@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Defect);
			incident.ProductArea = "ARC";
			SupportIncidentTestHelper.AddWorkflowTask(incident, "Confirm As Defect", "INV", ProcessTaskStatusCodeList.Codes.Working, staff.GS_Code, "", ZDateTimeOffset.Empty);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to currently assigned Incident", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.");
		}

		public void TestSendEmailToAssignedStaff_Defect_NoAssignedToCurrent()
		{
			AddProductAreaStaffAssignment("ARC", "Architecture", "PRD", "product.staff@cargowise.com");

			var staff1 = NewStaff("ADL", "andrew@cargowise.com");
			var staff2 = NewStaff("SCW", "samuel@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Defect);
			incident.ProductArea = "ARC";
			var task1 = SupportIncidentTestHelper.AddWorkflowTask(incident, "Confirm As Defect", "INV", ProcessTaskStatusCodeList.Codes.Closed, staff1.GS_Code, "", ZDateTimeOffset.Empty);
			var task2 = SupportIncidentTestHelper.AddWorkflowTask(incident, "New Open Unassigned Task", "INV", ProcessTaskStatusCodeList.Codes.Open);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to last closed task", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - You are the Staff Assigned to Last Closed Task.
");

			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to last closed task", 0, "Bob", "bob@test.com", "samuel@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - You are the Staff Assigned to Last Closed Task.
");

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			incident.IM_CloseTimeUtc = ZDateTime.Now.AddDays(-29);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to last closed task", 0, "Bob", "bob@test.com", "samuel@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - You are the Staff Assigned to Last Closed Task.
");
		}

		public void TestSendEmailToAssignedStaff_Defect_NoAssignedToCurrent_NoLastAssignedStaff()
		{
			AddProductAreaStaffAssignment("ARC", "Architecture", "PRD", "product.staff@cargowise.com");

			var staff = NewStaff("ADL", "andrew@cargowise.com");
			var staff2 = NewStaff("SCW", "samuel@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Defect);
			incident.ProductArea = "ARC";
			var task1 = SupportIncidentTestHelper.AddWorkflowTask(incident, "Confirm As Defect", "INV", ProcessTaskStatusCodeList.Codes.Cancelled, staff.GS_Code, "", ZDateTimeOffset.Empty);
			var task2 = SupportIncidentTestHelper.AddWorkflowTask(incident, "New Open Unassigned Task", "INV", ProcessTaskStatusCodeList.Codes.Open);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to Product Area staff assignee", 0, "Bob", "bob@test.com", "product.staff@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - There is no Staff Assigned to Last Closed Task.
 - You are the Incident Product Area Assignee.
");

			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Completed;
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to last closed task", 1, "Bob", "bob@test.com", "product.staff@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - You are the Incident Product Area Assignee.
");
		}

		public void TestSendEmailToAssignedStaff_Defect_NoAssignedToCurrent_NoLastAssignedStaff_NoProductAreaAssignment()
		{
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.Defect);
			incident.ProductArea = "ARC";

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to support mailbox", 0, "Bob", "bob@test.com", "support@wisetechglobal.com",
"No Available Contact Assigned to Incident",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - There is no Staff Assigned to Last Closed Task.
 - There is no Incident Product Area Assignee.
 - You are the Fallback Recipient for incidents where the staff is unavailable.
");
		}

		#endregion

		#region Send Email For Feature Stage

		public void TestSendEmailToAssignedStaff_Feature()
		{
			AddProductAreaStaffAssignment("ARC", "Architecture", "PRD", "product.staff@cargowise.com");

			var staff = NewStaff("ADL", "andrew@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.FeatureRequest);
			incident.ProductArea = "ARC";
			SupportIncidentTestHelper.AddWorkflowTask(incident, "Provide Development Estimate", "EST", SupportIncidentLookups.Status.Working, staff.GS_Code, "", ZDateTimeOffset.Empty);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to currently assigned Incident", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.");
		}

		public void TestSendEmailToAssignedStaff_Feature_NoAssignedToCurrent()
		{
			AddProductAreaStaffAssignment("ARC", "Architecture", "PRD", "product.staff@cargowise.com");

			var staff1 = NewStaff("ADL", "andrew@cargowise.com");
			var staff2 = NewStaff("SCW", "samuel@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.FeatureRequest);
			incident.ProductArea = "ARC";
			var task1 = SupportIncidentTestHelper.AddWorkflowTask(incident, "Confirm As Defect", "INV", ProcessTaskStatusCodeList.Codes.Closed, staff1.GS_Code, "", ZDateTimeOffset.Empty);
			var task2 = SupportIncidentTestHelper.AddWorkflowTask(incident, "New Open Unassigned Task", "INV", ProcessTaskStatusCodeList.Codes.Open);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to last closed task", 0, "Bob", "bob@test.com", "andrew@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - You are the Staff Assigned to Last Closed Task.
");

			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to last closed task", 0, "Bob", "bob@test.com", "samuel@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - You are the Staff Assigned to Last Closed Task.
");

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			incident.IM_CloseTimeUtc = ZDateTime.Now.AddDays(-29);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to last closed task", 0, "Bob", "bob@test.com", "samuel@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - You are the Staff Assigned to Last Closed Task.
");
		}

		public void TestSendEmailToAssignedStaff_Feature_NoAssignedToCurrent_NoLastAssignedStaff()
		{
			AddProductAreaStaffAssignment("ARC", "Architecture", "PRD", "product.staff@cargowise.com");

			var staff = NewStaff("ADL", "andrew@cargowise.com");
			var staff2 = NewStaff("SCW", "samuel@cargowise.com");
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.FeatureRequest);
			incident.ProductArea = "ARC";
			var task1 = SupportIncidentTestHelper.AddWorkflowTask(incident, "Confirm As Defect", "INV", ProcessTaskStatusCodeList.Codes.Cancelled, staff.GS_Code, "", ZDateTimeOffset.Empty);
			var task2 = SupportIncidentTestHelper.AddWorkflowTask(incident, "New Open Unassigned Task", "INV", ProcessTaskStatusCodeList.Codes.Open);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to Product Area staff assignee", 0, "Bob", "bob@test.com", "product.staff@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - There is no Staff Assigned to Last Closed Task.
 - You are the Incident Product Area Assignee.
");

			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Completed;
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to staff assigned to last closed task", 1, "Bob", "bob@test.com", "product.staff@cargowise.com",
"RE Customer Service Incident Raised - CS00219870",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - You are the Incident Product Area Assignee.
");
		}

		public void TestSendEmailToAssignedStaff_Feature_NoAssignedToCurrent_NoLastAssignedStaff_NoProductAreaAssignment()
		{
			var incident = NewIncidentAtStage(SupportIncidentCategoriesList.Codes.FeatureRequest);

			var email = CreateEmail("Bob", "bob@test.com", "RE Customer Service Incident Raised - CS00219870", "This is the body of an Incident email.");
			new SupportIncidentAssignedStaffEmailSender(incident).SendEmailToAssignedStaff(email);

			AssertEmail("Email should be sent to support mailbox", 0, "Bob", "bob@test.com", "support@wisetechglobal.com",
"No Available Contact Assigned to Incident",
@"This is the body of an Incident email.

----
You are receiving this Email because:
 - There is no Staff Assigned to Current Task.
 - There is no Staff Assigned to Last Closed Task.
 - There is no Incident Product Area Assignee.
 - You are the Fallback Recipient for incidents where the staff is unavailable.
");
		}

		#endregion

		#region Implementation

		GlbStaff NewStaff(string code, string email)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_EmailAddress = email;
			staff.GS_LoginName = code;

			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";
			var security2 = Factory.NewWithValidTestData<GlbSecurity>();
			security2.GU_GS = staff.PK;
			security2.GU_SecurityItemIsAllowed = true;
			security2.GU_SecurityRight = "SpecializedRights";
			var security3 = Factory.NewWithValidTestData<GlbSecurity>();
			security3.GU_GS = staff.PK;
			security3.GU_SecurityItemIsAllowed = true;
			security3.GU_SecurityRight = "DocumentsReports";
			var security4 = Factory.NewWithValidTestData<GlbSecurity>();
			security4.GU_GS = staff.PK;
			security4.GU_SecurityItemIsAllowed = true;
			security4.GU_SecurityRight = "Notes";

			Factory.Save();

			return staff;
		}

		SupportIncident NewIncidentAtStage(string stage)
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Category = stage;
			return incident;
		}

		EmailDef CreateEmail(string fromName, string fromAddress, string subject, string body)
		{
			var result = new EmailDef();
			result.FromDisplayName = fromName;
			result.FromAddress = fromAddress;
			result.Subject = subject;
			result.Body = body;
			result.Headers[SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress] = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
			return result;
		}

		void AddProductAreaStaffAssignment(string productAreaCode, string productAreaDescription, string staffCode, string staffEmailAddress)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_EmailAddress = staffEmailAddress;
			Factory.Save();

			AddProductAreaStaffAssignment(productAreaCode, productAreaDescription, staff);
		}

		void AddProductAreaStaffAssignment(string productAreaCode, string productAreaDescription, GlbStaff staff)
		{
			var productAreas = new CodeDescriptionPairList(EDIDataRegistry.Instance.ProductAreas.Value);
			productAreas.AddPair(productAreaCode, productAreaDescription);
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);

			var productAreaAssignmentCollection = EDIDataRegistry.Instance.ProductAreaAssignments.Value;
			productAreaAssignmentCollection.AddNew(productAreaCode, staff.GS_Code);
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreaAssignmentCollection);
		}

		void AssertEmail(string message, int emailNumber, string fromName, string fromAddress, string recipient, string subject, string body, params string[] attachmentNames)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("FromName", fromName, Env.OutgoingMailManager.EmailsCreated[emailNumber].FromDisplayName);
				AssertEquals("FromAddress", fromAddress, Env.OutgoingMailManager.EmailsCreated[emailNumber].FromAddress);
				AssertEquals("Recipient", 1, Env.OutgoingMailManager.EmailsCreated[emailNumber].Recipients.Count);
				AssertEquals("Recipient", recipient, Env.OutgoingMailManager.EmailsCreated[emailNumber].Recipients[0].Email);
				AssertEquals("Subject", subject, Env.OutgoingMailManager.EmailsCreated[emailNumber].Subject);
				AssertEquals("Body", body, Env.OutgoingMailManager.EmailsCreated[emailNumber].Body);
				AssertEquals("Attachments", attachmentNames.Length, Env.OutgoingMailManager.EmailsCreated[emailNumber].Attachments.Count);
				for (int i = 0; i < Env.OutgoingMailManager.EmailsCreated[emailNumber].Attachments.Count; i++)
				{
					AssertEquals("AttachmentNames", attachmentNames[i], Env.OutgoingMailManager.EmailsCreated[emailNumber].Attachments[i].DisplayName);
				}

				if (Env.OutgoingMailManager.EmailsCreated[emailNumber].Headers.TryGetValue(SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress, out var header)) 
				{
					AssertEquals(SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll, header);
				}
			});
		}

		#endregion
	}
}
