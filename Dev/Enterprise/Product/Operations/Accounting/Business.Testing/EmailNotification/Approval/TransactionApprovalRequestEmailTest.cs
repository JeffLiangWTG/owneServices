using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	public abstract class TransactionApprovalRequestEmailTest<RequestType, DetailsType> : TestCaseWithFactory
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		#region TestGetSubject

		public void TestGetSubject()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			Job job = TestObjectCreator.CreateJob(shipment);

			var approvalRequest = CreateRequest();
			approvalRequest.XP_ParentID = job.PK;
			approvalRequest.XP_ParentTableCode = job.TablePrefix;
			Factory.Save();

			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			var emailDef = GetNewEmail(approvalRequest).CreateEmailDef();
			AssertEquals("Email Subject", GetExpectedEmailSubjectForTest(false, "S001"), emailDef.Subject);

			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			emailDef = GetNewEmail(approvalRequest).CreateEmailDef();
			AssertEquals("Email Subject", GetExpectedEmailSubjectForTest(true, "S001"), emailDef.Subject);
		}
		#endregion

		#region TestAddRecepients

		public void TestAddRecepients()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			Job job = TestObjectCreator.CreateJob(shipment);

			var approvalRequest = CreateRequest();
			approvalRequest.XP_ParentID = job.PK;
			approvalRequest.XP_ParentTableCode = job.TablePrefix;
			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Factory.Save();
			AssertRecepients(1);

			approvalRequest.XP_SystemCreateUser = ZString.Empty;
			Factory.Save();
			AssertRecepients(0);

			approvalRequest.XP_SystemCreateUser = currentUserInCurrentFactory.GS_Code;
			Factory.Save();
			AssertRecepients(1);

			currentUserInCurrentFactory.GS_EmailAddress = ZString.Empty;
			Factory.Save();
			AssertRecepients(0);

			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Factory.Save();
			AssertRecepients(1);

			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			var notificationGroup = GetNotificationGroupForRequestedApproval();
			if (notificationGroup != null)
			{
				AccountingConfigurationRegistry.Instance.GLJournalsApprovalNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());
				AssertRecepients(1);
			}
			else
			{
				AssertRecepients(0);
			}

			void AssertRecepients(int recepientsCount)
			{
				var emailDef = GetNewEmail(approvalRequest).CreateEmailDef();
				AssertEquals("Email Recipients count", recepientsCount, emailDef.Recipients.Count);
				if (recepientsCount > 0)
				{
					AssertEquals("Email Recipients", "A@B.COM", emailDef.Recipients[0].Email);
				}
			}
		}
		#endregion

		#region TestGetBody

		public void TestGetBody()
		{
			GlbStaff user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_FullName = "New test user";

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			Job job = TestObjectCreator.CreateJob(shipment);

			var approvalRequest = CreateRequest();
			approvalRequest.XP_ParentID = job.PK;
			approvalRequest.XP_ParentTableCode = job.TablePrefix;
			approvalRequest.XP_ParentID = job.PK;
			approvalRequest.XP_ParentTableCode = job.TablePrefix;
			approvalRequest.XP_ReasonDescription = "Test approving";

			approvalRequest.XP_GS_NKApprovingUser1 = user.GS_Code;

			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Factory.Save();

			var approvalStatusCodes = new GenApprovalRequestLookups(CreateRequest()).ApprovalStatusList.ToArray().Select(x => x.Code);

			foreach (var statusCode in approvalStatusCodes)
			{
				approvalRequest.XP_ApprovalStatus = statusCode;
				var emailDef = GetNewEmail(approvalRequest).CreateEmailDef();
				AssertContainsInOrder("Email body", emailDef.Body, GetExpectedBody(approvalRequest));
			}
		}
		#endregion

		#region TestSendEmail

		public void TestSendEmail()
		{
			GlbStaff user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_FullName = "New test user";
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			Job job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();
			var approvalRequest = CreateRequest();
			AssertEquals("Email send result", EmailSendResult.Unsuccessful, GetNewEmail(approvalRequest).Send());

			Factory.Save();
			AssertEquals("Email send result", EmailSendResult.Unsuccessful, GetNewEmail(approvalRequest).Send());

			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Factory.Save();

			AssertEquals("Email send result", EmailSendResult.Unsuccessful, GetNewEmail(approvalRequest).Send());

			approvalRequest.XP_ParentID = job.PK;
			approvalRequest.XP_ParentTableCode = job.TablePrefix;
			approvalRequest.XP_ReasonDescription = "Test approving";
			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			approvalRequest.XP_GS_NKApprovingUser1 = user.GS_Code;

			AssertEquals("Email send result", EmailSendResult.Successful, GetNewEmail(approvalRequest).Send());
		}

		protected abstract string GetExpectedEmailSubjectForTest(bool approved = false, string jobNumber = null, bool firstCheck = false);

		#endregion

		protected abstract TransactionApprovalRequestEmail GetNewEmail(RequestType approvalRequest);
		protected abstract string GetApprovalBizoName();

		protected virtual RequestType CreateRequest() => Factory.NewWithValidTestData<RequestType>();

		protected virtual string[] GetExpectedBody(RequestType approvalRequest, string link = null, string approvingUsers = "New test user")
		{
			var approvalStatusDesc = new Dictionary<string, string>() {
				{ "REQ", "Requested" },
				{ "APP", "Approved" },
				{ "REJ", "Rejected" },
				{ "CAN", "Canceled" },
				{ "PST", "Posted" },
			};

			return new string[] { $"<p>{GetApprovalBizoName()} approval request with description 'Test approving' was {approvalStatusDesc[approvalRequest.XP_ApprovalStatus]} by user '{approvingUsers}'.</p>" };
		}

		protected virtual GlbGroup GetNotificationGroupForRequestedApproval() => null;

		protected virtual bool ShouldSendEmailForRequestedApproval => false;

		protected TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}
}
