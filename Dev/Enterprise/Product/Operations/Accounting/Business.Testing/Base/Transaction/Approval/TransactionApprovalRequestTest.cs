using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	public abstract class TransactionApprovalRequestTest<RequestType, DetailsType> : GenApprovalRequestTest<RequestType>
			where RequestType : TransactionApprovalRequest<DetailsType>
			where DetailsType : ApprovalRequestDetails
	{
		public abstract void TestIsPostingActionTheSame();

		public virtual void TestArePostingDetailsTheSame()
		{
			var approvalForTest1 = (RequestType)GetNewBusinessObject();
			var approvalForTest2 = (RequestType)GetNewBusinessObject();

			Assert(approvalForTest1.ArePostingDetailsTheSame(approvalForTest2));

			approvalForTest1.PostingDetails.MaxAmountToApprove = 1;
			Assert(!approvalForTest1.ArePostingDetailsTheSame(approvalForTest2));

			approvalForTest2.PostingDetails.MaxAmountToApprove = 1;
			Assert(approvalForTest1.ArePostingDetailsTheSame(approvalForTest2));
		}

		public virtual void TestIsAllowedToApproveOrCancel()
		{
			var approvalForTest = (RequestType)GetNewBusinessObject();
			Assert(approvalForTest.IsAllowedToApproveRequest);
			Assert(approvalForTest.IsAllowedToCancelRequest);
		}

		public virtual void TestIsAllowedToChangeStatus()
		{
			var approvalForTest = (RequestType)GetNewBusinessObject();
			Assert(approvalForTest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Requested));
			Assert(approvalForTest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled));
			Assert(approvalForTest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Rejected));
			Assert(approvalForTest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved));
			Assert(approvalForTest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Posted));
			Assert(approvalForTest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Error));
		}

		#region TestXP_ReasonCodeDescription
		public void TestXP_ReasonCodeDescription()
		{
			var approvalForTest1 = (RequestType)GetNewBusinessObject();
			approvalForTest1.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectOrganisationBilled;
			AssertContains("Reason Code Description should equal to IncorrectOrganisationBilled", Constants.GenApprovalRequestReasonCode.Description.IncorrectOrganisationBilled, approvalForTest1.XP_ReasonCodeDescription);
			approvalForTest1.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.DamagedGoods;
			AssertContains("Reason Code Description should equal to DamagedGoods", Constants.GenApprovalRequestReasonCode.Description.DamagedGoods, approvalForTest1.XP_ReasonCodeDescription);
			approvalForTest1.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.Discount;
			AssertContains("Reason Code Description should equal to Discount", Constants.GenApprovalRequestReasonCode.Description.Discount, approvalForTest1.XP_ReasonCodeDescription);
			approvalForTest1.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectCharges;
			AssertContains("Reason Code Description should equal to IncorrectCharges", Constants.GenApprovalRequestReasonCode.Description.IncorrectCharges, approvalForTest1.XP_ReasonCodeDescription);
			approvalForTest1.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectDate;
			AssertContains("Reason Code Description should equal to IncorrectDate", Constants.GenApprovalRequestReasonCode.Description.IncorrectDate, approvalForTest1.XP_ReasonCodeDescription);
			approvalForTest1.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
			AssertContains("Reason Code Description should equal to IncorrectJobDetails", Constants.GenApprovalRequestReasonCode.Description.IncorrectJobDetails, approvalForTest1.XP_ReasonCodeDescription);
			approvalForTest1.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectRating;
			AssertContains("Reason Code Description should equal to IncorrectRating", Constants.GenApprovalRequestReasonCode.Description.IncorrectRating, approvalForTest1.XP_ReasonCodeDescription);
			approvalForTest1.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.LateDelivery;
			AssertContains("Reason Code Description should equal to LateDelivery", Constants.GenApprovalRequestReasonCode.Description.LateDelivery, approvalForTest1.XP_ReasonCodeDescription);
		}
		#endregion

		#region TestXP_RequestID

		public void TestXP_RequestID()
		{
			TestApprovalRequest.XP_ReasonDescription = "set this just to initialize request";
			if (ShouldSetRequestIDFromNumberFountain)
			{
				Factory.Save();
				Assert("Postcondition: IsInDatabase", TestApprovalRequest.IsInDatabase);
				var expectedValue = ExpectedApprovalRequestID();
				AssertEquals("Should be set to current fountain value on next successful saving: XP_RequestID", expectedValue, TestApprovalRequest.XP_RequestID);
				Factory.Save();
				AssertEquals("Should not be changed on next savings: XP_RequestID", expectedValue, TestApprovalRequest.XP_RequestID);
			}
			else
			{
				Factory.Save();
				Assert("Postcondition: IsInDatabase", TestApprovalRequest.IsInDatabase);
				AssertEquals("XP_RequestID", "", TestApprovalRequest.XP_RequestID);
			}
		}

		protected virtual ZString ExpectedApprovalRequestID()
		{
			return "00000001";
		}

		protected virtual bool ShouldSetRequestIDFromNumberFountain
		{
			get { return false; }
		}

		#endregion

		public virtual void TestUpdateApprovalUserAndStatus()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			var job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestApprovalRequest.XP_ParentID = job.PK;
			TestApprovalRequest.XP_ParentTableCode = job.TablePrefix;

			TestApprovalRequest.XP_GS_NKApprovingUser1 = ZString.Empty;
			TestApprovalRequest.XP_ApprovalStatus = ZString.Empty;
			TestApprovalRequest.UpdateApprovalUserAndStatus("ad1", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", TestApprovalRequest.XP_GS_NKApprovingUser1);
			AssertEquals("", TestApprovalRequest.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, TestApprovalRequest.XP_ApprovalStatus);
			AssertNotNullOrEmpty(TestApprovalRequest.XP_ApprovalDate.ToString());
		}

		public virtual void TestReferenceType()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			var job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestApprovalRequest.XP_ParentID = job.PK;
			TestApprovalRequest.XP_ParentTableCode = job.TablePrefix;
			AssertEquals("Job", TestApprovalRequest.ReferenceType);

			TestApprovalRequest.XP_ParentID = consol.PK;
			TestApprovalRequest.XP_ParentTableCode = consol.TablePrefix;
			AssertEquals("Consol", TestApprovalRequest.ReferenceType);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1");
			TestApprovalRequest.XP_ParentID = invoice.PK;
			TestApprovalRequest.XP_ParentTableCode = invoice.TablePrefix;
			AssertEquals("Transaction", TestApprovalRequest.ReferenceType);

			var creditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "CRD1");
			TestApprovalRequest.XP_ParentID = creditNote.PK;
			TestApprovalRequest.XP_ParentTableCode = creditNote.TablePrefix;
			AssertEquals("Transaction", TestApprovalRequest.ReferenceType);
		}

		public void TestReadonly()
		{
			AssertEquals("PostingRequest.ReadOnly", true, TestApprovalRequest.PostingDetails.ReadOnly);
			AssertEquals("XP_GB_RequestingBranchInfo.ReadOnly", true, TestApprovalRequest.XP_GB_RequestingBranchInfo.ReadOnly);
			AssertEquals("XP_ParentIDInfo.ReadOnly", true, TestApprovalRequest.XP_ParentIDInfo.ReadOnly);
			AssertEquals("XP_ParentTableCodeInfo.ReadOnly", true, TestApprovalRequest.XP_ParentTableCodeInfo.ReadOnly);
			AssertEquals("XP_ApprovalTypeInfo.ReadOnly", true, TestApprovalRequest.XP_ApprovalTypeInfo.ReadOnly);
			AssertEquals("XP_SubSystemInfo.ReadOnly", true, TestApprovalRequest.XP_SubSystemInfo.ReadOnly);
			AssertEquals("XP_ApprovalDateInfo.ReadOnly", true, TestApprovalRequest.XP_ApprovalDateInfo.ReadOnly);
			AssertEquals("XP_ApprovalStatusInfo.ReadOnly", true, TestApprovalRequest.XP_ApprovalStatusInfo.ReadOnly);
			AssertEquals("XP_GS_NKApprovingUser1Info.ReadOnly", true, TestApprovalRequest.XP_GS_NKApprovingUser1Info.ReadOnly);
			AssertEquals("XP_GS_NKApprovingUser2Info.ReadOnly", true, TestApprovalRequest.XP_GS_NKApprovingUser2Info.ReadOnly);
			AssertEquals("XP_GS_NKApprovingUser3Info.ReadOnly", true, TestApprovalRequest.XP_GS_NKApprovingUser3Info.ReadOnly);
			AssertEquals("XP_GS_NKApprovingUser4Info.ReadOnly", true, TestApprovalRequest.XP_GS_NKApprovingUser4Info.ReadOnly);
			AssertEquals("XP_GS_NKApprovingUser5Info.ReadOnly", true, TestApprovalRequest.XP_GS_NKApprovingUser5Info.ReadOnly);
			AssertEquals("XP_GS_NKApprovingUser6Info.ReadOnly", true, TestApprovalRequest.XP_GS_NKApprovingUser6Info.ReadOnly);
			AssertEquals("XP_SystemCreateTimeUtcInfo.ReadOnly", true, TestApprovalRequest.XP_SystemCreateTimeUtcInfo.ReadOnly);
			AssertEquals("XP_SystemCreateUserInfo.ReadOnly", true, TestApprovalRequest.XP_SystemCreateUserInfo.ReadOnly);
			AssertEquals("XP_GB_JobBranchInfo.ReadOnly", true, TestApprovalRequest.XP_GB_JobBranchInfo.ReadOnly);
			AssertEquals("XP_GE_JobDepartmentInfo.ReadOnly", true, TestApprovalRequest.XP_GE_JobDepartmentInfo.ReadOnly);
		}

		public virtual void TestSetDefaultValues()
		{
			var approvalForTest = (RequestType)GetNewBusinessObject();
			AssertEquals("XP_GB_RequestingBranch", GlbBranch.CurrentBranch.PK, approvalForTest.XP_GB_RequestingBranch);
			AssertEquals("XP_SubSystem", Constants.GenApprovalRequestSubSystem.Accounting, approvalForTest.XP_SubSystem);
			AssertEquals("XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalForTest.XP_ApprovalStatus);
		}

		public virtual void TestValidationType()
		{
			AssertType(typeof(TransactionApprovalRequestValidation), TestApprovalRequest.Validation);
		}

		public virtual void TestXP_ApprovalStatus_ConcurrencyPolicy()
		{
			var approvalForTest = (RequestType)GetNewBusinessObject();
			AssertEquals(ConcurrencyPolicy.Strict, approvalForTest.XP_ApprovalStatusInfo.ConcurrencyPolicy);
		}

		public void TestApprovalRequestConcurrency()
		{
			var approvalRequest = (RequestType)GetNewBusinessObject();
			Factory.RefreshEnabled = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			var loadedApprovalRequest = newFactory.Load<RequestType>(approvalRequest.PK);

			approvalRequest.XP_ApprovalStatus = "REJ";
			loadedApprovalRequest.XP_ApprovalStatus = "REJ";

			try
			{
				Factory.Save();
				newFactory.Save();
			}
			catch (Exception ex)
			{
				AssertContains("CONCURRENCY Error Saving Record", ex.Message);
				AssertContains("XP_ApprovalStatus", ex.Message);
				AssertContains("Strict - Notify", ex.Message);
			}
		}

		#region TestSendEmail

		#region TestSentOnSaving

		public virtual void TestSentOnSaving()
		{
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Job job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestApprovalRequest.XP_ParentID = job.PK;
			TestApprovalRequest.XP_ParentTableCode = job.TablePrefix;
			SetupRequestForTestSendEmail(TestApprovalRequest);
			int previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			Factory.Save();
			if (ShouldSendEmailOnSaving)
			{
				AssertEquals("Should be 1 email sent", previousEmailCount + 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[Env.OutgoingMailManager.EmailsCreated.Count - 1];
				AssertNotNull("email", email);
				AssertEquals("Email Subject", GetExpectedEmailSubjectForTestSendEmail(TestApprovalRequest) + " was Requested", email.Subject);
				AssertEquals("Email Recipient count", 1, email.Recipients.Count);
				AssertEquals("Email Recipient", "A@B.COM", email.Recipients[0].Email);
			}
			else
			{
				AssertEquals("Should be no email sent", previousEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		protected virtual bool ShouldSendEmailOnSaving
		{
			get { return false; }
		}

		#endregion

		public void TestSendEmailOnApproving()
		{
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Job job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestApprovalRequest.XP_ParentID = job.PK;
			TestApprovalRequest.XP_ParentTableCode = job.TablePrefix;
			SetupRequestForTestSendEmail(TestApprovalRequest);
			Factory.Save();

			int previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			if (ShouldSendEmailOnApproving)
			{
				AssertEquals("Should be 1 email sent", previousEmailCount + 1, Env.OutgoingMailManager.EmailsCreated.Count);
				previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;

				EmailDef email = Env.OutgoingMailManager.EmailsCreated[Env.OutgoingMailManager.EmailsCreated.Count - 1];
				AssertNotNull("email", email);
				AssertEquals("Email Subject", GetExpectedEmailSubjectForTestSendEmail(TestApprovalRequest) + " was Approved", email.Subject);
				AssertEquals("Email Recipient count", 1, email.Recipients.Count);
				AssertEquals("Email Recipient", "A@B.COM", email.Recipients[0].Email);
			}
			else
			{
				AssertEquals("Should be no email sent", previousEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			Factory.Save();

			AssertEquals("Should be no email sent", previousEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendEmailOnRejecting()
		{
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Job job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestApprovalRequest.XP_ParentID = job.PK;
			TestApprovalRequest.XP_ParentTableCode = job.TablePrefix;
			SetupRequestForTestSendEmail(TestApprovalRequest);
			Factory.Save();

			int previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			Factory.Save();

			if (ShouldSendEmailOnRejecting)
			{
				AssertEquals("Should be 1 email sent", previousEmailCount + 1, Env.OutgoingMailManager.EmailsCreated.Count);
				previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;

				EmailDef email = Env.OutgoingMailManager.EmailsCreated[Env.OutgoingMailManager.EmailsCreated.Count - 1];
				AssertNotNull("email", email);
				AssertEquals("Email Subject", GetExpectedEmailSubjectForTestSendEmail(TestApprovalRequest) + " was Rejected", email.Subject);
				AssertEquals("Email Recipient count", 1, email.Recipients.Count);
				AssertEquals("Email Recipient", "A@B.COM", email.Recipients[0].Email);
			}
			else
			{
				AssertEquals("Should be no email sent", previousEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			Factory.Save();

			AssertEquals("Should be no email sent", previousEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendNoEmailsOnCancelling()
		{
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Job job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestApprovalRequest.XP_ParentID = job.PK;
			TestApprovalRequest.XP_ParentTableCode = job.TablePrefix;
			Factory.Save();

			int previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			Factory.Save();

			AssertEquals("Should be no email sent", previousEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		protected abstract string GetExpectedEmailSubjectForTestSendEmail(RequestType request);

		protected virtual void SetupRequestForTestSendEmail(RequestType request)
		{
		}

		protected virtual bool ShouldSendEmailOnApproving
		{
			get { return true; }
		}

		protected virtual bool ShouldSendEmailOnRejecting
		{
			get { return true; }
		}

		#endregion

		protected virtual RequestType TestApprovalRequest
		{
			get { return testTransactionApprovalRequest ?? (testTransactionApprovalRequest = (RequestType)GetNewBusinessObject()); }
		}
		RequestType testTransactionApprovalRequest;

		protected TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}
}
