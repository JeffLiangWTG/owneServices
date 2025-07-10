using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	public class ARCreditNoteApprovalRequestEmailTest : InvoicingBaseApprovalRequestEmailTest<ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public void TestSendChildRequestEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_FullName = "New test user";
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			var job = TestObjectCreator.CreateJob(shipment);
			var approvalRequest = CreateRequest();

			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Factory.Save();

			approvalRequest.XP_ParentID = job.PK;
			approvalRequest.XP_ReasonDescription = "Child send test";
			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			approvalRequest.XP_GS_NKApprovingUser1 = user.GS_Code;

			approvalRequest.XP_ParentTableCode = GenApprovalRequestSchema.Constants.Prefix;

			Assert("Request is child", !approvalRequest.IsParentRequest);

			var previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			Factory.Save();
			AssertEquals("Should be 0 emails sent", previousEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestCreateEmailDef_ApprovedRejectedBody()
		{
			var user1 = Factory.NewWithValidTestData<GlbStaff>();
			user1.GS_FullName = "New test user1";
			var user2 = Factory.NewWithValidTestData<GlbStaff>();
			user2.GS_FullName = "New test user2";

			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			var job = TestObjectCreator.CreateJob(shipment);
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";

			RunTestEmailCreate(true, true, "New test user1, New test user2");
			RunTestEmailCreate(true, false, "New test user1");
			RunTestEmailCreate(false, true, "New test user2");
			RunTestEmailCreate(false, false, "New test user1");

			void RunTestEmailCreate(bool approved, bool registrySettings, string approvingUsers)
			{
				AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySettings);
				AuthorizationModeAndSettings setting = new AuthorizationModeAndSettings { AuthorizationMode = registrySettings ? Constants.AuthorizationMode.Codes.TwoApprovers : Constants.AuthorizationMode.Codes.Default };
				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);

				var approvalRequest = CreateRequest();
				approvalRequest.XP_ParentID = job.PK;
				approvalRequest.XP_ParentTableCode = job.TablePrefix;
				approvalRequest.XP_ReasonDescription = (approved ? "Approved" : "Rejected") + " create test";
				approvalRequest.XP_ApprovalStatus = (approved ? Constants.GenApprovalRequestApprovalStatus.Approved : Constants.GenApprovalRequestApprovalStatus.Rejected);
				approvalRequest.XP_GS_NKApprovingUser1 = user1.GS_Code;
				approvalRequest.XP_GS_NKApprovingUser2 = registrySettings ? user2.GS_Code : ZString.Empty;
				Factory.Save();

				var mailingClassInstance = GetNewEmail(approvalRequest);
				var emailDef = mailingClassInstance.CreateEmailDef();

				AssertNotNull("email", emailDef);
				AssertEquals("Email Subject", GetExpectedEmailSubjectForTest(approved, approvalRequest.JobNumber), emailDef.Subject);
				AssertEquals("Email Recipient count", 1, emailDef.Recipients.Count);
				AssertEquals("Email Recipient", "A@B.COM", emailDef.Recipients[0].Email);

				var expectedBody = GetExpectedBody(approvalRequest, expectedApprovers: approvingUsers);
				AssertContainsInOrder("Email Body", emailDef.Body, expectedBody);
			}
		}

		public virtual void TestLinkForMoreDetails()
		{
			GlbStaff user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_FullName = "New test user";
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment);

			var consol = TestObjectCreator.CreateConsol();

			var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR101010101", GlbCompany.CurrentCompany.LocalCurrency, 1, 100, 0, 100, 0);

			var parentApproval = Factory.NewWithValidTestData<GenApprovalRequest>();
			parentApproval.XP_ParentID = job.PK;
			parentApproval.XP_ParentTableCode = job.TablePrefix;

			Factory.Save();

			var approvalRequest = CreateRequest();
			approvalRequest.XP_ReasonDescription = "Test approving";
			approvalRequest.XP_GS_NKApprovingUser1 = user.GS_Code;

			string expectedCollectionCallsURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.JobShipment, shipment.PK);
			var link = $@"<p>Refer to <a href=""{expectedCollectionCallsURL}"">S001</a></p>";

			var mailingClassInstance = GetNewEmail(approvalRequest);

			approvalRequest.XP_ParentID = job.PK;
			approvalRequest.XP_ParentTableCode = job.TablePrefix;
			AssertEmailBody();

			approvalRequest.XP_ParentID = consol.PK;
			approvalRequest.XP_ParentTableCode = consol.TablePrefix;
			AssertEmailBody();

			approvalRequest.XP_ParentID = transaction.PK;
			approvalRequest.XP_ParentTableCode = transaction.TablePrefix;
			AssertEmailBody();

			approvalRequest.XP_ParentID = parentApproval.PK;
			approvalRequest.XP_ParentTableCode = parentApproval.TablePrefix;
			AssertEmailBody();

			void AssertEmailBody()
			{
				var emailDef = mailingClassInstance.CreateEmailDef();
				AssertContainsInOrder("Email body", emailDef.Body, GetExpectedBody(approvalRequest, link));
			}
		}

		protected override TransactionApprovalRequestEmail GetNewEmail(ARCreditNoteApprovalRequest approvalRequest) => new ARCreditNoteApprovalRequestEmail(approvalRequest);

		protected override string GetApprovalBizoName() => "AR Credit Note";

		protected override string[] GetExpectedBody(ARCreditNoteApprovalRequest approvalRequest, string link = null, string expectedApprovers = "New test user")
		{
			switch (approvalRequest.XP_ApprovalStatus)
			{
				case Constants.GenApprovalRequestApprovalStatus.Approved:
				case Constants.GenApprovalRequestApprovalStatus.Rejected:
					var expectedBody = new string[]
					{
						$@"<p>The following AR Credit Note approval request was {approvalRequest.Lookups.ApprovalStatusList.GetDescriptionFromCode(approvalRequest.XP_ApprovalStatus).ToLower()} on {approvalRequest.XP_ApprovalDate}:</p>",
					};

					if (link != null)
					{
						expectedBody.Append(link);
					}

					expectedBody.Append($@"
		<p><span style=""font-weight:bold;"">Created on: </span>{approvalRequest.CreatedTimeLocal.ToBestReadableDateTimeString()}</p>
		<p><span style=""font-weight:bold;"">Reason code: </span>{approvalRequest.XP_ReasonCode}</p>
		<p><span style=""font-weight:bold;"">Description: </span>{approvalRequest.XP_ReasonDescription}</p>
		<p><span style=""font-weight:bold;"">{approvalRequest.Lookups.ApprovalStatusList.GetDescriptionFromCode(approvalRequest.XP_ApprovalStatus)} by: </span>{expectedApprovers}</p>");

					if (approvalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved)
					{
						expectedBody.Append($"<p>{(AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.Value ? @"No further action is required. The credit note will be automatically posted." : @"You can now post the credit note.")}</p>");
					}

					return expectedBody;

				default:
					return base.GetExpectedBody(approvalRequest, link, expectedApprovers);
			}
		}

		protected override string GetExpectedEmailSubjectForTest(bool approved = false, string jobNumber = null, bool firstCheck = false)
		{
			return firstCheck ? "AR Credit Note approval request for Job Number '' was Requested"
							  : $"AR Credit Note Approval request - {jobNumber} was {(approved ? "Approved" : "Rejected")}";
		}
	}
}
