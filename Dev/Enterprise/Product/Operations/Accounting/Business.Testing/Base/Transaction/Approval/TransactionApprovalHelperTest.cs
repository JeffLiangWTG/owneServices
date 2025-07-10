using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	abstract class TransactionApprovalHelperTest<RequestType, DetailsType> : TestCaseWithFactory
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public void TestIsThereApprovedRequestForThisPostingDetails()
		{
			SetupDefaultPostingDetails(TestApproval);
			TestApproval.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingDetails);

			Factory.Save();
			AssertEquals(true, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingDetails);

			TestApproval.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingDetails);
			Factory.Save();

			var approvalReqestInDB = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupDefaultPostingAction(approvalReqestInDB);
			SetupAnotherPostingDetails(approvalReqestInDB);
			approvalReqestInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalReqestInDB.Factory.Save();
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingDetails);

			ResetTransactionApprovalHelper();
			SetupDefaultPostingDetails(approvalReqestInDB);
			approvalReqestInDB.XP_ParentID = ZGuid.NewZGuid();
			approvalReqestInDB.Factory.Save();
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingDetails);

			approvalReqestInDB.XP_ParentID = TestApproval.XP_ParentID;
			approvalReqestInDB.Factory.Save();
			AssertEquals(true, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingDetails);

			approvalReqestInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			approvalReqestInDB.Factory.Save();
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingDetails);
		}

		public void TestIsThereApprovedRequestForThisPostingActionButAnotherPostingDetails()
		{
			SetupDefaultPostingDetails(TestApproval);
			TestApproval.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails);
			Factory.Save();

			var approvalReqestInDB = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupAnotherPostingAction(approvalReqestInDB);
			SetupAnotherPostingDetails(approvalReqestInDB);
			approvalReqestInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalReqestInDB.Factory.Save();
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails);

			approvalReqestInDB.ResetPostingDetails_ForTestOnly();
			SetupDefaultPostingAction(approvalReqestInDB);
			approvalReqestInDB.Factory.Save();
			AssertEquals(true, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails);

			approvalReqestInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			approvalReqestInDB.Factory.Save();
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails);

			approvalReqestInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalReqestInDB.Factory.Save();
			AssertEquals(true, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails);

			ResetTransactionApprovalHelper();
			SetupDefaultPostingDetails(approvalReqestInDB);
			approvalReqestInDB.Factory.Save();
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails);
		}

		public void TestCancelApprovedRequestForThisPostingActionButAnotherPostingDetails()
		{
			SetupDefaultPostingDetails(TestApproval);
			TestApproval.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var anotherApprovalReqestInDB = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupAnotherPostingAction(anotherApprovalReqestInDB);
			SetupAnotherPostingDetails(anotherApprovalReqestInDB);
			anotherApprovalReqestInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var anotherApprovalReqestInDB2 = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupDefaultPostingAction(anotherApprovalReqestInDB2);
			SetupAnotherPostingDetails(anotherApprovalReqestInDB2);
			anotherApprovalReqestInDB2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var anotherApprovalReqestInDB3 = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupDefaultPostingAction(anotherApprovalReqestInDB3);
			SetupAnotherPostingDetails(anotherApprovalReqestInDB3);
			anotherApprovalReqestInDB3.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;

			TransactionApprovalHelperWithExposedMembers.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails();
			AssertEquals("TestApproval.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, TestApproval.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, anotherApprovalReqestInDB.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, anotherApprovalReqestInDB2.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB3.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, anotherApprovalReqestInDB3.XP_ApprovalStatus);

			BusinessObjectFactory.SaveTogether(Factory, anotherApprovalReqestInDB.Factory, anotherApprovalReqestInDB2.Factory, anotherApprovalReqestInDB3.Factory);

			AssertEquals("Recondition: IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails", true, TransactionApprovalHelperWithExposedMembers.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails);
			TransactionApprovalHelperWithExposedMembers.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails();
			Factory.Save();
			AssertEquals("TestApproval.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, TestApproval.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, anotherApprovalReqestInDB.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, anotherApprovalReqestInDB2.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB3.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, anotherApprovalReqestInDB3.XP_ApprovalStatus);
		}

		public void TestPostApprovedRequestForThisPostingDetails()
		{
			SetupDefaultPostingDetails(TestApproval);
			TestApproval.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;

			TransactionApprovalHelperWithExposedMembers.PostApprovedRequestForThisPostingDetails();
			AssertEquals("TestApproval.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, TestApproval.XP_ApprovalStatus);

			Factory.Save();
			TransactionApprovalHelperWithExposedMembers.PostApprovedRequestForThisPostingDetails();
			AssertEquals("TestApproval.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Posted, TestApproval.XP_ApprovalStatus);

			TestApproval.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			var anotherApprovalReqestInDB = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupAnotherPostingAction(anotherApprovalReqestInDB);
			SetupAnotherPostingDetails(anotherApprovalReqestInDB);
			anotherApprovalReqestInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var anotherApprovalReqestInDB2 = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupDefaultPostingAction(anotherApprovalReqestInDB2);
			SetupAnotherPostingDetails(anotherApprovalReqestInDB2);
			anotherApprovalReqestInDB2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var anotherApprovalReqestInDB3 = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupDefaultPostingAction(anotherApprovalReqestInDB3);
			SetupAnotherPostingDetails(anotherApprovalReqestInDB3);
			anotherApprovalReqestInDB3.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			BusinessObjectFactory.SaveTogether(anotherApprovalReqestInDB.Factory, anotherApprovalReqestInDB2.Factory, anotherApprovalReqestInDB3.Factory);

			TransactionApprovalHelperWithExposedMembers.PostApprovedRequestForThisPostingDetails();
			Factory.Save();
			AssertEquals("TestApproval.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Rejected, TestApproval.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, anotherApprovalReqestInDB.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Posted, anotherApprovalReqestInDB2.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB3.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, anotherApprovalReqestInDB3.XP_ApprovalStatus);
		}

		[TestDate(2014, 10, 10)]
		public void TestApproveAndPostRequestWithSecurityOverride()
		{
			SetupDefaultPostingDetails(TestApproval);
			AssertEquals("Precondition: ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, TestApproval.XP_ApprovalStatus);

			SecurityTestObject.CreateTestUser(true, Env.Security.None.Code, "tst", "newuser", "password");
			var testProvider = new NonInteractiveSecurityOverrideProvider();
			testProvider.OverrideLogin = "newuser";
			testProvider.OverridePassword = "password";
			((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(Env.Security.GLJournalApproval);
			AssertNotNull("Precondition: UserSecurityOverride", testProvider.UserSecurityOverride);
			TransactionApprovalHelperWithExposedMembers.ApproveAndPostRequest(testProvider);
			AssertEquals("ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Posted, TestApproval.XP_ApprovalStatus);
			AssertEquals("ApprovalStatus", "tst", TestApproval.XP_GS_NKApprovingUser1);
			AssertEquals("ApprovalStatus", ZDateTime.Now, TestApproval.XP_ApprovalDate);
		}

		[TestDate(2014, 10, 20)]
		public void TestApproveAndPostRequestWithoutSecurityOverride()
		{
			SetupDefaultPostingDetails(TestApproval);
			AssertEquals("Precondition: ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, TestApproval.XP_ApprovalStatus);

			var testProvider = new NonInteractiveSecurityOverrideProvider();
			TransactionApprovalHelperWithExposedMembers.ApproveAndPostRequest(testProvider);
			AssertEquals("ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Posted, TestApproval.XP_ApprovalStatus);
			AssertEquals("ApprovalStatus", Env.CurrentUser.Initials, TestApproval.XP_GS_NKApprovingUser1);
			AssertEquals("ApprovalStatus", ZDateTime.Now, TestApproval.XP_ApprovalDate);
		}

		public void TestAreThereAnyRequestedApprovalsForThisPostingAction()
		{
			TestApproval.XP_ParentID = ZGuid.NewZGuid();
			SetupDefaultPostingDetails(TestApproval);
			TestApproval.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Factory.Save();
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus);

			var anotherApprovalReqestInDB = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupAnotherPostingAction(anotherApprovalReqestInDB);
			SetupAnotherPostingDetails(anotherApprovalReqestInDB);
			anotherApprovalReqestInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			anotherApprovalReqestInDB.Factory.Save();
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus);

			anotherApprovalReqestInDB.ResetPostingDetails_ForTestOnly();
			SetupDefaultPostingAction(anotherApprovalReqestInDB);
			anotherApprovalReqestInDB.XP_ParentID = ZGuid.NewZGuid();
			anotherApprovalReqestInDB.Factory.Save();
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus);

			anotherApprovalReqestInDB.XP_ParentID = TestApproval.XP_ParentID;
			anotherApprovalReqestInDB.Factory.Save();
			AssertEquals(true, TransactionApprovalHelperWithExposedMembers.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus);

			anotherApprovalReqestInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Error;
			anotherApprovalReqestInDB.Factory.Save();
			AssertEquals(false, TransactionApprovalHelperWithExposedMembers.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus);
		}

		public void TestCancelRequestedOrErrorApprovalsForThisPostingAction()
		{
			SetupDefaultPostingDetails(TestApproval);
			TestApproval.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			var anotherApprovalReqestInDB = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupDefaultPostingAction(anotherApprovalReqestInDB);
			SetupAnotherPostingDetails(anotherApprovalReqestInDB);
			anotherApprovalReqestInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			BusinessObjectFactory.SaveTogether(Factory, anotherApprovalReqestInDB.Factory);
			AssertEquals("Precondition: AreThereAnyRequestedApprovalsForThisPostingAction", false, TransactionApprovalHelperWithExposedMembers.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus);
			TransactionApprovalHelperWithExposedMembers.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus();
			Factory.Save();
			AssertEquals("TestApproval.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, TestApproval.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, anotherApprovalReqestInDB.XP_ApprovalStatus);

			var anotherApprovalReqestInDB2 = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupDefaultPostingAction(anotherApprovalReqestInDB2);
			SetupAnotherPostingDetails(anotherApprovalReqestInDB2);
			anotherApprovalReqestInDB2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Error;
			anotherApprovalReqestInDB2.Factory.Save();
			TransactionApprovalHelperWithExposedMembers.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus();
			Factory.Save();
			AssertEquals("TestApproval.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, TestApproval.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, anotherApprovalReqestInDB.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, anotherApprovalReqestInDB2.XP_ApprovalStatus);

			var anotherApprovalReqestInDB3 = GetNewTestApprovalWithoutSetup(new BusinessObjectFactory());
			SetupDefaultPostingAction(anotherApprovalReqestInDB3);
			SetupAnotherPostingDetails(anotherApprovalReqestInDB3);
			anotherApprovalReqestInDB3.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			anotherApprovalReqestInDB3.Factory.Save();
			AssertEquals("Precondition: AreThereAnyRequestedApprovalsForThisPostingAction", true, TransactionApprovalHelperWithExposedMembers.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus);
			TransactionApprovalHelperWithExposedMembers.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus();
			Factory.Save();
			AssertEquals("TestApproval.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, TestApproval.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, anotherApprovalReqestInDB.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, anotherApprovalReqestInDB2.XP_ApprovalStatus);
			AssertEquals("anotherApprovalReqestInDB3.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, anotherApprovalReqestInDB3.XP_ApprovalStatus);
		}

		protected RequestType TestApproval
		{
			get
			{
				if (testApproval == null)
				{
					testApproval = GetNewTestApprovalWithoutSetup(Factory);
					SetupDefaultPostingAction(testApproval);
				}

				return testApproval;
			}
		}
		RequestType testApproval;

		protected TransactionApprovalHelper<RequestType, DetailsType> TransactionApprovalHelper
		{
			get { return transactionApprovalHelper ?? (transactionApprovalHelper = GetNewTransactionApprovalHelper(TestApproval)); }
		}
		TransactionApprovalHelper<RequestType, DetailsType> transactionApprovalHelper;

		void ResetTransactionApprovalHelper()
		{
			transactionApprovalHelper = null;
			ReleaseFactory();
			testApproval = Factory.Load<RequestType>(TestApproval.PK);
		}

		protected ITransactionApprovalHelper TransactionApprovalHelperWithExposedMembers
		{
			get { return TransactionApprovalHelper; }
		}

		protected abstract RequestType GetNewTestApprovalWithoutSetup(BusinessObjectFactory factory);
		protected abstract TransactionApprovalHelper<RequestType, DetailsType> GetNewTransactionApprovalHelper(RequestType approval);
		protected abstract void SetupDefaultPostingAction(RequestType approval);
		protected abstract void SetupAnotherPostingAction(RequestType approval);

		protected virtual void SetupDefaultPostingDetails(RequestType approval)
		{
			approval.PostingDetails.MaxAmountToApprove = 10M;
		}

		protected virtual void SetupAnotherPostingDetails(RequestType approval)
		{
			approval.PostingDetails.MaxAmountToApprove = 20M;
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}

	class ARCreditNoteApprovalHelperTest : TransactionApprovalHelperTest<ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		protected override ARCreditNoteApprovalRequest GetNewTestApprovalWithoutSetup(BusinessObjectFactory factory)
		{
			var request = factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			request.XP_ParentID = new ZGuid("2FB582B9-D12F-43B8-94EF-5AACBF1EC4C1"); //any the same GUID for all requests should be here

			return request;
		}

		protected override TransactionApprovalHelper<ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails> GetNewTransactionApprovalHelper(ARCreditNoteApprovalRequest approval)
		{
			return new ARCreditNoteApprovalHelper(approval);
		}

		protected override void SetupDefaultPostingAction(ARCreditNoteApprovalRequest approval)
		{
			approval.PostingDetails.PostingOption = "All";
		}

		protected override void SetupAnotherPostingAction(ARCreditNoteApprovalRequest approval)
		{
			approval.PostingDetails.PostingOption = "Revenue";
		}
	}

	class TransactionRelatedAPInvoiceChargesApprovalHelperTest : TransactionApprovalHelperTest<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		protected override APInvoiceChargesApprovalRequest GetNewTestApprovalWithoutSetup(BusinessObjectFactory factory)
		{
			var request = factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.XP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;

			return request;
		}

		protected override TransactionApprovalHelper<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails> GetNewTransactionApprovalHelper(APInvoiceChargesApprovalRequest approval)
		{
			return new APInvoiceChargesApprovalHelper(approval);
		}

		protected override void SetupDefaultPostingDetails(APInvoiceChargesApprovalRequest approval)
		{
			approval.PostingDetails.TransactionNumber = "1";
		}

		protected override void SetupAnotherPostingDetails(APInvoiceChargesApprovalRequest approval)
		{
			approval.PostingDetails.TransactionNumber = "2";
		}

		protected override void SetupDefaultPostingAction(APInvoiceChargesApprovalRequest approval)
		{
			approval.XP_ParentID = DefaultParentID;
		}

		protected override void SetupAnotherPostingAction(APInvoiceChargesApprovalRequest approval)
		{
			approval.XP_ParentID = AnotherParentID; //only one request is allowed for the parent ID for AP Invoice requests. So only new ParentID can make a request with different details.
		}

		ZGuid DefaultParentID
		{
			get
			{
				if (!defaultParentID.HasValue)
				{
					defaultParentID = ZGuid.NewZGuid();
				}
				return defaultParentID.Value;
			}
		}
		ZGuid? defaultParentID;

		ZGuid AnotherParentID
		{
			get
			{
				if (!anotherParentID.HasValue)
				{
					anotherParentID = ZGuid.NewZGuid();
				}
				return anotherParentID.Value;
			}
		}
		ZGuid? anotherParentID;
	}

	class JobRelatedAPInvoiceChargesApprovalHelperTest : TransactionApprovalHelperTest<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		public void TestGetCanceledRequestedAndApprovedRequestsNotInThisPostingAction()
		{
			var initialStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			var request1 = GetNewTestApprovalWithoutSetup(Factory);
			request1.PostingDetails.Creditor = "Creditor1";
			request1.XP_ApprovalStatus = initialStatus;
			var request2 = GetNewTestApprovalWithoutSetup(Factory);
			request2.PostingDetails.TransactionNumber = "INV1";
			request2.XP_ApprovalStatus = initialStatus;

			var requestsForTest = new List<APInvoiceChargesApprovalRequest>();
			Action<ZString, ZString> setupRequests = (creditor, transactionNumber) =>
				{
					foreach (CodeDescriptionPair status in GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList)
					{
						var request = GetNewTestApprovalWithoutSetup(Factory);
						request.PostingDetails.Creditor = creditor;
						request.PostingDetails.TransactionNumber = transactionNumber;
						request.XP_ApprovalStatus = status.Code;
						requestsForTest.Add(request);
					}
				};
			setupRequests("Creditor1", "");
			setupRequests("", "INV1");
			setupRequests("Creditor1", "INV1");

			var cancelledRequests = ((APInvoiceChargesApprovalHelper)TransactionApprovalHelper).GetCanceledRequestedAndApprovedRequestsNotInThisPostingAction(new[] { request1, request2 });

			AssertEquals("initial requests status should not be changed.", initialStatus, request1.XP_ApprovalStatus);
			AssertEquals("initial requests status should not be changed.", initialStatus, request2.XP_ApprovalStatus);
			Assert("All cancelledRequests Cancelled", cancelledRequests.All(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Cancelled));
			Assert("All cancelledRequests from requestsForTest", !cancelledRequests.Except(requestsForTest).Any());
			var notCancelledRequestIdAndStatuses =
				(
					from notCancelledRequest in requestsForTest.Except(cancelledRequests)
					select new
					{
						creditor = notCancelledRequest.PostingDetails.Creditor,
						transactionNumber = notCancelledRequest.PostingDetails.TransactionNumber,
						status = notCancelledRequest.XP_ApprovalStatus.ToString()
					}
				).ToArray();
			var expectedStatusesToNotCancel = new[] { Constants.GenApprovalRequestApprovalStatus.Cancelled, Constants.GenApprovalRequestApprovalStatus.Rejected, Constants.GenApprovalRequestApprovalStatus.Posted };
			Action<ZString, ZString> assertNotCancelledRequestStatuses = (creditor, transactionNumber) =>
				{
					var notCancelledRequestStatuses = from idAndStatus in notCancelledRequestIdAndStatuses
													  where idAndStatus.creditor == creditor && idAndStatus.transactionNumber == transactionNumber
													  select idAndStatus.status;
					AssertContainsExactElementsInAnyOrder("Correct elements remain not Cancelled.", expectedStatusesToNotCancel, notCancelledRequestStatuses);
				};
			assertNotCancelledRequestStatuses("Creditor1", "INV1");

			expectedStatusesToNotCancel = GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList.Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();
			assertNotCancelledRequestStatuses("Creditor1", "");
			assertNotCancelledRequestStatuses("", "INV1");
		}

		protected override APInvoiceChargesApprovalRequest GetNewTestApprovalWithoutSetup(BusinessObjectFactory factory)
		{
			var request = factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.XP_ParentID = new ZGuid("C1DBC0BB-9DF3-4899-BC3F-64F5E2E87C94"); //any the same GUID for all requests should be here

			return request;
		}

		protected override TransactionApprovalHelper<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails> GetNewTransactionApprovalHelper(APInvoiceChargesApprovalRequest approval)
		{
			return new APInvoiceChargesApprovalHelper(approval);
		}

		protected override void SetupDefaultPostingAction(APInvoiceChargesApprovalRequest approval)
		{
			approval.PostingDetails.Creditor = "Creditor1";
		}

		protected override void SetupAnotherPostingAction(APInvoiceChargesApprovalRequest approval)
		{
			approval.PostingDetails.Creditor = "Creditor2";
		}
	}

	class GLJournalApprovalHelperTest : TransactionApprovalHelperTest<GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		public void TestAreThereAnyRequestedApprovalsForThisPostingAction_WithNewJournal()
		{
			var journal = CreatorForOtherFactory.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			CreatorForOtherFactory.CreateGLJournalLine(journal, 10, DebitCredit.CR, CreatorForOtherFactory.ExchangeGainLossControlAccount.PK);
			CreatorForOtherFactory.CreateGLJournalLine(journal, 10, DebitCredit.DR, CreatorForOtherFactory.ExchangeGainLossAdjustmentAccount.PK);

			var request = GetNewTestApprovalWithoutSetup(Factory);
			request.Initialize(journal);

			var helper = (ITransactionApprovalHelper)GetNewTransactionApprovalHelper(request);
			Assert("Request should not be counted until it's saved.", !helper.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus);

			Factory.Save();
			request = GetNewTestApprovalWithoutSetup(Factory);
			request.Initialize(journal);
			helper = GetNewTransactionApprovalHelper(request);
			Assert("When we create next request, previous saved request should be found.", helper.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus);
		}

		public void TestIsThereApprovedRequestForThisPostingDetails_WithNewJournal()
		{
			var journal = CreatorForOtherFactory.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			CreatorForOtherFactory.CreateGLJournalLine(journal, 10, DebitCredit.CR, CreatorForOtherFactory.ExchangeGainLossControlAccount.PK);
			CreatorForOtherFactory.CreateGLJournalLine(journal, 10, DebitCredit.DR, CreatorForOtherFactory.ExchangeGainLossAdjustmentAccount.PK);

			var request = GetNewTestApprovalWithoutSetup(Factory);
			request.Initialize(journal);
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;

			var helper = (ITransactionApprovalHelper)GetNewTransactionApprovalHelper(request);
			Assert("Request should not be counted until it's saved.", !helper.IsThereApprovedRequestForThisPostingDetails);

			Factory.Save();
			request = GetNewTestApprovalWithoutSetup(Factory);
			request.Initialize(journal);
			helper = GetNewTransactionApprovalHelper(request);
			Assert("When we create next request, previous saved request should be found.", helper.IsThereApprovedRequestForThisPostingDetails);
		}

		public void TestPostApprovedRequestForThisPostingDetails_NewJournal()
		{
			var journal = CreatorForOtherFactory.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			CreatorForOtherFactory.CreateGLJournalLine(journal, 10, DebitCredit.CR, CreatorForOtherFactory.ExchangeGainLossControlAccount.PK);
			CreatorForOtherFactory.CreateGLJournalLine(journal, 10, DebitCredit.DR, CreatorForOtherFactory.ExchangeGainLossAdjustmentAccount.PK);

			var request = GetNewTestApprovalWithoutSetup(Factory);
			request.Initialize(journal);
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			var journalToPost = request.GetLinkedJournal().journal;
			var nextRequest = GetNewTestApprovalWithoutSetup(Factory);
			nextRequest.Initialize(journalToPost);
			var helper = (ITransactionApprovalHelper)GetNewTransactionApprovalHelper(nextRequest);

			AssertEquals("Precondition: request ParentID", ZGuid.Empty, request.XP_ParentID);
			helper.PostApprovedRequestForThisPostingDetails();

			AssertEquals("request ParentID", journalToPost.PK, request.XP_ParentID);
			AssertEquals("request Status", Constants.GenApprovalRequestApprovalStatus.Posted, request.XP_ApprovalStatus);
		}

		protected override GLJournalApprovalRequest GetNewTestApprovalWithoutSetup(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<GLJournalApprovalRequest>();
		}

		protected override TransactionApprovalHelper<GLJournalApprovalRequest, GLJournalApprovalRequestDetails> GetNewTransactionApprovalHelper(GLJournalApprovalRequest approval)
		{
			return new GLJournalApprovalHelper(approval, "");
		}

		protected override void SetupDefaultPostingAction(GLJournalApprovalRequest approval)
		{
			approval.Initialize(DefaultJournal);
		}

		protected override void SetupAnotherPostingAction(GLJournalApprovalRequest approval)
		{
			approval.Initialize(AnotherJournal);
		}

		protected override void SetupDefaultPostingDetails(GLJournalApprovalRequest approval)
		{
			approval.PostingDetails.Journal.AH_Desc = "Default Description";
			approval.PostingDetails.HasChanges = true; //to force serialization
		}

		protected override void SetupAnotherPostingDetails(GLJournalApprovalRequest approval)
		{
			approval.PostingDetails.Journal.AH_Desc = "Another Description";
			approval.PostingDetails.HasChanges = true; //to force serialization
		}

		GLJournal DefaultJournal
		{
			get
			{
				if (defaultJournal == null)
				{
					defaultJournal = CreatorForOtherFactory.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
					CreatorForOtherFactory.CreateGLJournalLine(defaultJournal, 10, DebitCredit.CR, CreatorForOtherFactory.ExchangeGainLossControlAccount.PK);
					CreatorForOtherFactory.CreateGLJournalLine(defaultJournal, 10, DebitCredit.DR, CreatorForOtherFactory.ExchangeGainLossAdjustmentAccount.PK);
					defaultJournal.Factory.Save();
				}
				return defaultJournal;
			}
		}
		GLJournal defaultJournal;

		GLJournal AnotherJournal
		{
			get
			{
				if (anotherJournal == null)
				{
					anotherJournal = CreatorForOtherFactory.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
					CreatorForOtherFactory.CreateGLJournalLine(anotherJournal, 10, DebitCredit.CR, CreatorForOtherFactory.ExchangeGainLossControlAccount.PK);
					CreatorForOtherFactory.CreateGLJournalLine(anotherJournal, 10, DebitCredit.DR, CreatorForOtherFactory.ExchangeGainLossAdjustmentAccount.PK);
					anotherJournal.Factory.Save();
				}
				return anotherJournal;
			}
		}
		GLJournal anotherJournal;

		TestObjectCreator CreatorForOtherFactory
		{
			get { return creatorForOtherFactory ?? (creatorForOtherFactory = new TestObjectCreator(new BusinessObjectFactory())); }
		}
		TestObjectCreator creatorForOtherFactory;
	}
}
