using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	public abstract class InvoicingBaseApprovalRequestEmailTest<RequestType, DetailsType> : TransactionApprovalRequestEmailTest<RequestType, DetailsType>
		where RequestType : InvoicingBaseApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public void TestCreateEmailDefWithLink()
		{
			int previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			GlbStaff user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_FullName = "New test user";
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			Job job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();
			var approvalRequest = CreateRequest();

			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Factory.Save();
			approvalRequest.XP_ParentID = job.PK;
			approvalRequest.XP_ParentTableCode = job.TablePrefix;
			approvalRequest.XP_ReasonDescription = "Test approving";
			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			approvalRequest.XP_GS_NKApprovingUser1 = user.GS_Code;

			var mail = GetNewEmail(approvalRequest);
			var emailDef = mail.CreateEmailDef();

			AssertNotNull("email", emailDef);
			AssertEquals("Email Subject", GetExpectedEmailSubjectForTest(false, "S001"), emailDef.Subject);
			AssertEquals("Email Recipient count", 1, emailDef.Recipients.Count);
			AssertEquals("Email Recipient", "A@B.COM", emailDef.Recipients[0].Email);

			string expectedCollectionCallsURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.JobShipment, shipment.PK);
			var link = $@"<p>Refer to <a href=""{expectedCollectionCallsURL}"">S001</a></p>";
			var expectedBody = GetExpectedBody(approvalRequest, link);
			AssertContainsInOrder("Email Body", emailDef.Body, expectedBody);

			approvalRequest.XP_ParentID = consol.PK;
			approvalRequest.XP_ParentTableCode = consol.TablePrefix;
			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;

			mail = GetNewEmail(approvalRequest);
			emailDef = mail.CreateEmailDef();

			AssertNotNull("email", emailDef);
			AssertEquals("Email Subject", GetExpectedEmailSubjectForTest(true, "C001"), emailDef.Subject);
			AssertEquals("Email Recipient count", 1, emailDef.Recipients.Count);
			AssertEquals("Email Recipient", "A@B.COM", emailDef.Recipients[0].Email);

			expectedCollectionCallsURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.JobConsol, consol.PK);
			link = $@"<p>Refer to <a href=""{expectedCollectionCallsURL}"">C001</a></p>";
			expectedBody = GetExpectedBody(approvalRequest, link);
			AssertContainsInOrder("Email Body", emailDef.Body, expectedBody);
		}

		public void TestCreateEmailDefWhenParentIDInvalid()
		{
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			GlbStaff user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_FullName = "New test user";
			Job job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var approvalRequest = CreateRequest();
			approvalRequest.XP_ParentID = job.PK;
			approvalRequest.XP_ParentTableCode = job.TablePrefix;
			approvalRequest.XP_ReasonDescription = "Test approving";
			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			approvalRequest.XP_GS_NKApprovingUser1 = user.GS_Code;
			Factory.Save();

			var mail = GetNewEmail(approvalRequest);
			var emailDef = mail.CreateEmailDef();

			AssertNotNull("email", emailDef);
			AssertEquals("Email Subject", GetExpectedEmailSubjectForTest(false, "S001"), emailDef.Subject);
			AssertEquals("Email Recipient count", 1, emailDef.Recipients.Count);
			AssertEquals("Email Recipient", "A@B.COM", emailDef.Recipients[0].Email);

			var expectedBody = GetExpectedBody(approvalRequest);
			AssertContainsInOrder("Email Body", emailDef.Body, expectedBody);
		}

		protected override string GetExpectedEmailSubjectForTest(bool approved = false, string jobNumber = null, bool firstCheck = false)
		{
			return GetApprovalBizoName() + @" approval request for Job Number '" + jobNumber + @"' was " + (approved ? "Approved" : "Rejected");
		}

		protected override string[] GetExpectedBody(RequestType approvalRequest, string link = null, string approvingUsers = "New test user")
		{
			var expectedBody = new string[] { $@"<p>{GetApprovalBizoName()} approval request with description 'Test approving' was {approvalRequest.Lookups.ApprovalStatusList.GetDescriptionFromCode(approvalRequest.XP_ApprovalStatus)} by user '{approvingUsers}'.</p>" };

			if (link != null)
			{
				expectedBody.Append(link);
			}

			return expectedBody;
		}
	}
}
