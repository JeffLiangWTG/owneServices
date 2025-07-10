using System;
using System.Linq;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	[TestedType(typeof(IncidentApprovalLogSubscriber))]
	public class IncidentApprovalRequestLogSubscriberTest : LogSubscriberTest<IncidentApprovalLogSubscriber>
	{
		public void TestProcessQueuedLogsWithERA()
		{
			var template = new NotificationEmailTemplate(typeof(DocIncidentRequest), "Subject - (*IncidentNumber*)", "Body - (*Summary*)");
			EDIDataRegistry.Instance.ERequestPendingApprovalNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, template);
			var webuser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.WebUserCode);
			using (Env.SetTemporaryUserContext(webuser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
				var reportingContact = lic.Company.Header.Contacts.AddNew();
				reportingContact.OC_ContactName = "Joe";
				reportingContact.OC_Email = "joe@test.org";
				var request1 = Factory.New<IncidentRequest>();
				request1.INC_OC_ReportedBy = reportingContact.PK;
				request1.INC_Criticality = "CR5";
				request1.INC_Details = "how stuff happen?";
				request1.INC_OC_ApprovedBy = reportingContact.PK;
				request1.INC_Summary = "stuff happened";
				request1.INC_Type = "ENT";
				request1.INC_Status = "APR";
				request1.GetLogs().AddNew(Events.IncidentApprovalRequested);
				//approvers
				var approver1 = reportingContact.Header.Contacts.AddNew() as EDIOrgContact;
				approver1.OC_ContactName = "admin1";
				approver1.OC_Email = "admin1@test.org";
				approver1.IsERequestApprover = true;
				var approver2 = reportingContact.Header.Contacts.AddNew() as EDIOrgContact;
				approver2.OC_ContactName = "admin2";
				approver2.OC_Email = "admin2@test.org";
				approver2.IsERequestApprover = true;
				var approver3 = reportingContact.Header.Contacts.AddNew() as EDIOrgContact;
				approver3.OC_ContactName = "admin3";
				approver3.OC_Email = "admin2@test.org"; // <-- same email address
				approver3.IsERequestApprover = true;
				var user1 = reportingContact.Header.Contacts.AddNew() as EDIOrgContact;
				user1.OC_ContactName = "user1";
				user1.OC_Email = "user1@test.com";
				user1.IsERequestApprover = false;
				//followers
				var follower1 = reportingContact.Header.Contacts.AddNew() as EDIOrgContact;
				follower1.OC_ContactName = "follower1";
				follower1.OC_Email = "follower1@test.com";
				follower1.IsERequestApprover = false;
				var follower2 = Factory.NewWithValidTestData<OrgHeader>();
				follower2.MainAddress.OA_Email = "OrgAsfollower2@test.com";
				var follower3 = Factory.NewWithValidTestData<OrgHeader>();
				follower3.MainAddress.OA_Email = "admin1@test.org"; // <-- same email address
				var conversation = Factory.New<JobConversation>();
				conversation.JCC_ParentID = request1.PK;
				conversation.JCC_ParentTableCode = "INC";
				conversation.Participants.AddNewParticipant(follower1);
				conversation.Participants.AddNewParticipant(follower2);
				Factory.Save();
				AssertEquals(false, Env.OutgoingMailManager.EmailsCreated.Any());
				RunLogWalkerCycleForTest();

				var email = Env.OutgoingMailManager.EmailsCreated.Single();
				AssertEquals($"Subject - {request1.INC_IncidentNumber}", email.Subject);
				AssertEquals($"Body - {request1.INC_Summary}", email.Body);
				AssertEquals("admin1@test.org|admin2@test.org", string.Join("|", email.Recipients.OfType<RecipientDef>().Select(x => x.Email).OrderBy(x => x)));
				AssertEquals("follower1@test.com|joe@test.org|OrgAsfollower2@test.com", string.Join("|", email.CCRecipients.OfType<RecipientDef>().Select(x => x.Email).OrderBy(x => x)));
			}
		}

		public void TestProcessQueuedLogsWithoutERA()
		{
			var template = new NotificationEmailTemplate(typeof(DocIncidentRequest), "Subject - (*IncidentNumber*)", "Body - (*Summary*)");
			EDIDataRegistry.Instance.ERequestPendingApprovalNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, template);
			var webuser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.WebUserCode);
			using (Env.SetTemporaryUserContext(webuser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
				var reportingContact = lic.Company.Header.Contacts.AddNew();
				reportingContact.OC_ContactName = "Joe";
				reportingContact.OC_Email = "joe@test.org";
				var request1 = Factory.New<IncidentRequest>();
				request1.INC_OC_ReportedBy = reportingContact.PK;
				request1.INC_Criticality = "CR5";
				request1.INC_Details = "how stuff happen?";
				request1.INC_OC_ApprovedBy = reportingContact.PK;
				request1.INC_Summary = "stuff happened";
				request1.INC_Type = "ENT";
				request1.INC_Status = "APR";
				request1.GetLogs().AddNew(Events.IncidentApprovalRequested);
				//followers
				var follower1 = reportingContact.Header.Contacts.AddNew() as EDIOrgContact;
				follower1.OC_ContactName = "follower1";
				follower1.OC_Email = "follower1@test.com";
				follower1.IsERequestApprover = false;
				var follower2 = Factory.NewWithValidTestData<OrgHeader>();
				follower2.MainAddress.OA_Email = "OrgAsfollower2@test.com";
				var follower3 = Factory.NewWithValidTestData<OrgHeader>();
				follower3.MainAddress.OA_Email = "OrgAsfollower2@test.com"; // <-- same email address
				var follower4 = Factory.NewWithValidTestData<OrgHeader>();
				follower4.MainAddress.OA_Email = "joe@test.org"; // <-- same email address
				var conversation = Factory.New<JobConversation>();
				conversation.JCC_ParentID = request1.PK;
				conversation.JCC_ParentTableCode = "INC";
				conversation.Participants.AddNewParticipant(follower1);
				conversation.Participants.AddNewParticipant(follower2);
				conversation.Participants.AddNewParticipant(follower3);
				conversation.Participants.AddNewParticipant(follower4);
				Factory.Save();
				AssertEquals(false, Env.OutgoingMailManager.EmailsCreated.Any());
				RunLogWalkerCycleForTest();

				var email = Env.OutgoingMailManager.EmailsCreated.Single();
				AssertEquals($"Subject - {request1.INC_IncidentNumber}", email.Subject);
				AssertEquals($"Body - {request1.INC_Summary}", email.Body);
				AssertEquals("joe@test.org", email.Recipients.OfType<RecipientDef>().Single().Email);
				AssertEquals("follower1@test.com|OrgAsfollower2@test.com", string.Join("|", email.CCRecipients.OfType<RecipientDef>().Select(x => x.Email).OrderBy(x => x)));
			}
		}
	}
}
