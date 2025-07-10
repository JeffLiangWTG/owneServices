using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Utility.Testing.TestBase;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	public abstract class TransactionApprovalBulkTest<ApprovalBulkType, TransactionType, RequestType, DetailsType, RequestParentType> : TransactionApprovalBulkTestBase<ApprovalBulkType, TransactionType, RequestType, DetailsType, RequestParentType>
		where ApprovalBulkType : TransactionApprovalBulk<TransactionType, RequestType, DetailsType>
		where TransactionType : TransactionHeader
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
		where RequestParentType : BusinessObject
	{
		[ExpectNoExceptions]
		public void TestApproveRequestWithBlankBranchAndDepartment()
		{
			SetUpTestPeriods();

			var approvalRequest1 = SetupRequestWithEmptyBranchAndDepartment();
			approvalRequest1.XP_SystemCreateUser = "NIL";
			Factory.Save();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1);
			AssertEquals(approvalRequestBulk.Approvals.Count, 1);
			approvalRequestBulk.Approve();
			AssertEquals("Approval request should be approved.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest1.XP_ApprovalStatus);
		}

		public virtual void TestPostApprovalsAndRemovePosted()
		{
			SetUpTestPeriods();

			var approvalRequest1 = GetNewApprovalRequest();
			approvalRequest1.XP_SystemCreateUser = "NIL";
			var approvalRequest2 = GetNewApprovalRequest();
			approvalRequest2.XP_SystemCreateUser = "NIL";
			Factory.Save();

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk1 = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1, approvalRequest2);

			var mockSecurityOverrideProviderWithApprovalRequestSupport = new Mock<ISecurityOverrideProviderWithApprovalRequest>();

			var mockGUIProvider = GetIPostingJobAndTransactionApprovalGUIProvider();
			SetupPostingJobAndTransactionApprovalGUIProviderMock(mockGUIProvider, approvalRequest1, approvalRequest2);

			AssertEquals(approvalRequestBulk1.Approvals.Count, 2);

			approvalRequestBulk1.Approve();
			Factory.Save();

			AssertEquals("All approvals should not be removed before successfully posted.", approvalRequestBulk1.Approvals.Count, 2);
			AssertEquals("Approval request should be approved.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest1.XP_ApprovalStatus);
			AssertEquals("Approval request should be approved.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest2.XP_ApprovalStatus);

			PostApprovalsAndRemovePosted(approvalRequestBulk1, mockGUIProvider.Object);
			mockGUIProvider.Verify();

			AssertEquals("All approvals should be removed after approving.", approvalRequestBulk1.Approvals.Count, 0);
			AssertEquals("Approval request should be posted.", Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequest1.XP_ApprovalStatus);
			AssertEquals("Approval request should be posted.", Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequest2.XP_ApprovalStatus);
		}

		public virtual void TestStatusAndParentNullInPostApprovalsAndRemovePosted()
		{
			SetUpTestPeriods();

			var approvalRequest = GetNewApprovalRequest();
			approvalRequest.XP_SystemCreateUser = "NIL";
			Factory.Save();

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);

			var mockGUIProvider = GetIPostingJobAndTransactionApprovalGUIProvider();

			AssertEquals(1, approvalRequestBulk.Approvals.Count);
			AssertEquals("Approval request should be requested at first.", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest.XP_ApprovalStatus);

			var expectedRowError = string.Format("This request can’t be posted. Request status is ‘{0}’ now. Only approved request can be posted.", Constants.GenApprovalRequestApprovalStatus.Requested);
			PostApprovalsAndRemovePosted(approvalRequestBulk, mockGUIProvider.Object);

			foreach (var error in approvalRequest.RowErrors)
			{
				AssertEquals(expectedRowError, error.Message);
			}
			approvalRequest.ClearAllNotifications();

			approvalRequestBulk.Approve();

			approvalRequest.XP_ParentID = ZGuid.NewZGuid();
			Factory.Save();

			AssertEquals("All approvals should not be removed when failed at posting.", 1, approvalRequestBulk.Approvals.Count);
			AssertEquals("Approval request should be approved when failed at posting.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest.XP_ApprovalStatus);

			PostApprovalsAndRemovePosted(approvalRequestBulk, mockGUIProvider.Object);

			AssertEquals("All approvals should not be removed when failed at posting.", 1, approvalRequestBulk.Approvals.Count);
			AssertEquals("Approval request should be approved when failed at posting.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest.XP_ApprovalStatus);

			expectedRowError = $"This request is approved, but related {ParentName} cannot be found.";
			foreach (var error in approvalRequest.RowErrors)
			{
				AssertEquals(expectedRowError, error.Message);
			}
		}

		[DisableZeroExchangeRateOverriding]
		public virtual void TestParentValidationErrorInPostApprovalsAndRemovePosted()
		{
			SetUpTestPeriods();

			var approvalRequest = GetNewApprovalRequest();
			approvalRequest.XP_SystemCreateUser = "NIL";

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);

			AssertEquals(1, approvalRequestBulk.Approvals.Count);
			AssertEquals("Approval request should be requested at first.", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest.XP_ApprovalStatus);

			approvalRequestBulk.Approve();
			Factory.Save();

			var parent = GetParentFromApprovalRequest(approvalRequest);

			ModifyParentToHaveValidationError(parent);

			parent.RunPreSaveValidation();
			Assert(parent.HasErrors);

			var mockGUIProvider = GetIPostingJobAndTransactionApprovalGUIProvider();
			PostApprovalsAndRemovePosted(approvalRequestBulk, mockGUIProvider.Object);

			AssertEquals("All approvals should not be removed when failed at posting.", 1, approvalRequestBulk.Approvals.Count);
			AssertEquals("Approval request should be approved when failed at posting.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest.XP_ApprovalStatus);

			var transactionRequestSpecificErroMessage = IsTransactionRelatedRequest ? $"This request is approved, but related {ParentName} has validation errors.\r\n" : "";

			var expectedRowError = $"{transactionRequestSpecificErroMessage}{GetErrorMessageForParentWithValidationError()}";
			foreach (var error in approvalRequest.RowErrors)
			{
				AssertEquals(expectedRowError, error.Message);
			}
		}

		public virtual void TestTwiceClickingPostApprovalsAndRemovePosted()
		{
			SetUpTestPeriods();

			var approvalRequest = GetNewApprovalRequest();
			approvalRequest.XP_SystemCreateUser = "NIL";

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);

			AssertEquals(1, approvalRequestBulk.Approvals.Count);
			AssertEquals("Approval request should be requested at first.", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest.XP_ApprovalStatus);

			approvalRequestBulk.Approve();
			Factory.Save();

			var parent = GetParentFromApprovalRequest(approvalRequest);

			ModifyParentToHaveValidationError(parent);

			parent.RunPreSaveValidation();
			Assert(parent.HasErrors);

			var mockGUIProvider = GetIPostingJobAndTransactionApprovalGUIProvider();
			PostApprovalsAndRemovePosted(approvalRequestBulk, mockGUIProvider.Object);

			AssertEquals("All approvals should not be removed when failed at posting.", 1, approvalRequestBulk.Approvals.Count);
			AssertEquals("Approval request should be approved when failed at posting.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest.XP_ApprovalStatus);

			var expectedRowError =
$@"This request is approved, but related {ParentName} has validation errors.
{GetErrorMessageForParentWithValidationError()}";
			foreach (var error in approvalRequest.RowErrors)
			{
				AssertEquals(expectedRowError, error.Message);
			}

			AssertNoExceptionThrown(() => PostApprovalsAndRemovePosted(approvalRequestBulk, mockGUIProvider.Object));

			AssertEquals("All approvals should not be removed when failed at posting.", 1, approvalRequestBulk.Approvals.Count);
			AssertEquals("Approval request should be approved when failed at posting.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest.XP_ApprovalStatus);

			foreach (var error in approvalRequest.RowErrors)
			{
				AssertEquals(expectedRowError, error.Message);
			}
		}

		public virtual void TestParentPostedAndRemoveFromApprovals()
		{
			SetUpTestPeriods();

			var approvalRequest1 = GetNewApprovalRequest();
			approvalRequest1.XP_SystemCreateUser = "NIL";

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk1 = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1);

			var mockSecurityOverrideProviderWithApprovalRequestSupport =
				new Mock<ISecurityOverrideProviderWithApprovalRequest> { CallBase = true };

			var mockGUIProvider = GetIPostingJobAndTransactionApprovalGUIProvider();
			SetupPostingJobAndTransactionApprovalGUIProviderMock(mockGUIProvider, approvalRequest1);

			AssertEquals(1, approvalRequestBulk1.Approvals.Count);

			approvalRequestBulk1.Approve();
			Factory.Save();

			var parent1 = GetParentFromApprovalRequest(approvalRequest1);
			AssertParentIsPosted("New parent shouldn't be in database before posted.", parent1, false);

			AssertEquals("All approvals should not be removed before successfully posted.", 1, approvalRequestBulk1.Approvals.Count);
			AssertEquals("Approval request should be approved.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest1.XP_ApprovalStatus);

			PostApprovalsAndRemovePosted(approvalRequestBulk1, mockGUIProvider.Object);
			mockGUIProvider.Verify();

			AssertEquals("All approvals should be removed after approving.", 0, approvalRequestBulk1.Approvals.Count);
			AssertEquals("Approval request should be posted.", Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequest1.XP_ApprovalStatus);

			var approvalRequestInDB = new BusinessObjectFactory().Load<RequestType>(approvalRequest1.PK);
			parent1 = GetParentFromApprovalRequest(approvalRequestInDB);
			AssertParentIsPosted("New parent should be in database after posted.", parent1, true);
		}

		public virtual void TestGUIProviderOfPostApprovalsAndRemovePosted()
		{
			SetUpTestPeriods();

			var approvalRequest = GetNewApprovalRequest();
			approvalRequest.XP_SystemCreateUser = "NIL";
			Factory.Save();

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);

			var mockGUIProvider = GetIPostingJobAndTransactionApprovalGUIProvider();

			AssertEquals(1, approvalRequestBulk.Approvals.Count);
			AssertEquals("Approval request should be requested at first.", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest.XP_ApprovalStatus);

			approvalRequestBulk.Approve();
			Factory.Save();

			var mockHelper = new Mock<ITransactionApprovalHelper> { CallBase = true };
			mockHelper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
			var helper = mockHelper.Object;
			approvalRequestBulk.GetNewHelperForLevelAuthorization_ForTestOnly = approval => helper;

			PostApprovalsAndRemovePosted(approvalRequestBulk, mockGUIProvider.Object);

			AssertEquals("All approvals should not be removed when failed at posting.", 1, approvalRequestBulk.Approvals.Count);
			AssertEquals("Approval request should be approved when failed at posting.", Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest.XP_ApprovalStatus);

			var expectedRowError = "Can't post this request because source details have been modified since then.";
			foreach (var error in approvalRequest.RowErrors)
			{
				AssertEquals(expectedRowError, error.Message);
			}
		}

		public void TestRemoveApproval()
		{
			var approvalRequest1 = SetupRequestForFirstApprovalLevel();
			var approvalRequest2 = SetupRequestForFirstApprovalLevel();
			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider>();
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1, approvalRequest2);

			AssertEquals(2, approvalRequestBulk.Approvals.Count);

			approvalRequestBulk.Approvals.RemoveFromRelationship(approvalRequest1);

			AssertEquals(1, approvalRequestBulk.Approvals.Count);
		}

		public void TestApprovals()
		{
			var approvalRequest1 = GetNewApprovalRequest();
			var approvalRequest2 = GetNewApprovalRequest();
			var approvalRequest3 = GetNewApprovalRequest();
			var approvalRequest4 = GetNewApprovalRequest();
			Factory.Save();

			ReleaseFactory();

			var approvalRequestBulk = GetNewApprovalBulk(new DefaultAccessSecurityProvider(), approvalRequest2, approvalRequest3);
			AssertEquals("Approvals.Count", 2, approvalRequestBulk.Approvals.Count);
			AssertEquals("Only elements passed to constructor should be in collection.", true, approvalRequestBulk.Approvals.Contains(approvalRequest2));
			AssertEquals("Only elements passed to constructor should be in collection.", true, approvalRequestBulk.Approvals.Contains(approvalRequest3));
		}

		[TestDate(2011, 03, 10)]
		public void TestApprove()
		{
			AssertStatusChange(x => x.Approve(), Constants.GenApprovalRequestApprovalStatus.Approved, loginUnderAnotherUser: false);
		}

		[TestDate(2011, 03, 10)]
		public void TestApprove_UnderAnotherUser()
		{
			AssertStatusChange(x => x.Approve(), Constants.GenApprovalRequestApprovalStatus.Approved, loginUnderAnotherUser: true);
		}

		[TestDate(2011, 03, 10)]
		public void TestReject()
		{
			AssertStatusChange(x => x.Reject(), Constants.GenApprovalRequestApprovalStatus.Rejected, loginUnderAnotherUser: false);
		}

		[TestDate(2011, 03, 10)]
		public void TestReject_UnderAnotherUser()
		{
			AssertStatusChange(x => x.Reject(), Constants.GenApprovalRequestApprovalStatus.Rejected, loginUnderAnotherUser: true);
		}

		protected void AssertStatusChange(Action<ApprovalBulkType> doAction, string expectedStatus, bool allowToApproveOwnRequest = true, bool loginUnderAnotherUser = false)
		{
			var approvalRequest1 = SetupRequestForFirstApprovalLevel();
			var approvalRequest2 = SetupRequestForSecondApprovalLevel();
			Factory.Save();

			var user = loginUnderAnotherUser ? Factory.NewWithValidTestData<GlbStaff>() : GlbStaff.CurrentUser;
			Factory.Save();
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SetupSecurity(disallowSecondLevel: true);
				var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
				var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1, approvalRequest2);
				bool approvalIsNotAllowedInAnyWay = allowToApproveOwnRequest || loginUnderAnotherUser;
				if (approvalIsNotAllowedInAnyWay)
				{
					mockSecurityProvider.Protected().Setup<SecurityCore>("RequestLoginCredentials", ItExpr.IsAny<SecurityCheckpoint>()).Returns((SecurityCore)null);
				}
				else
				{
					mockSecurityProvider.Protected().Verify("RequestLoginCredentials", Times.Never(), ItExpr.IsAny<SecurityCheckpoint>());
				}
				doAction(approvalRequestBulk);
				mockSecurityProvider.VerifyAll();
				AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest1.XP_ApprovalStatus);
				AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest2.XP_ApprovalStatus);
				AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Empty, approvalRequest1.XP_ApprovalDate);
				AssertEquals("approvalRequest1.XP_ApprovalDate", "", approvalRequest1.XP_GS_NKApprovingUser1);
				AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Empty, approvalRequest2.XP_ApprovalDate);
				AssertEquals("approvalRequest2.XP_ApprovalDate", "", approvalRequest2.XP_GS_NKApprovingUser1);

				if (approvalIsNotAllowedInAnyWay)
				{
					mockSecurityProvider.Protected().Setup<SecurityCore>("RequestLoginCredentials", ItExpr.IsAny<SecurityCheckpoint>()).Returns(new AlternativeCredentials("newuser", "password").UserSecurity);
				}
				else
				{
					mockSecurityProvider.Protected().Verify("RequestLoginCredentials", Times.Never(), ItExpr.IsAny<SecurityCheckpoint>());
				}
				doAction(approvalRequestBulk);
				mockSecurityProvider.VerifyAll();
				if (approvalIsNotAllowedInAnyWay)
				{
					AssertEquals("approvalRequest1.XP_ApprovalStatus", expectedStatus, approvalRequest1.XP_ApprovalStatus);
					AssertEquals("approvalRequest2.XP_ApprovalStatus", expectedStatus, approvalRequest2.XP_ApprovalStatus);
					AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Now, approvalRequest1.XP_ApprovalDate);
					AssertEquals("approvalRequest1.XP_ApprovalDate", "tst", approvalRequest1.XP_GS_NKApprovingUser1);
					AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Now, approvalRequest2.XP_ApprovalDate);
					AssertEquals("approvalRequest2.XP_ApprovalDate", "tst", approvalRequest2.XP_GS_NKApprovingUser1);
				}
				else
				{
					AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest1.XP_ApprovalStatus);
					AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest2.XP_ApprovalStatus);
					AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Empty, approvalRequest1.XP_ApprovalDate);
					AssertEquals("approvalRequest1.XP_ApprovalDate", "", approvalRequest1.XP_GS_NKApprovingUser1);
					AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Empty, approvalRequest2.XP_ApprovalDate);
					AssertEquals("approvalRequest2.XP_ApprovalDate", "", approvalRequest2.XP_GS_NKApprovingUser1);
				}

				approvalRequest1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
				approvalRequest2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
				approvalRequest1.XP_ApprovalDate = ZDateTime.Empty;
				approvalRequest2.XP_ApprovalDate = ZDateTime.Empty;
				approvalRequest1.XP_GS_NKApprovingUser1 = "";
				approvalRequest2.XP_GS_NKApprovingUser1 = "";

				SetupSecurity();
				mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
				approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1, approvalRequest2);
				doAction(approvalRequestBulk);
				if (approvalIsNotAllowedInAnyWay)
				{
					AssertEquals("approvalRequest1.XP_ApprovalStatus", expectedStatus, approvalRequest1.XP_ApprovalStatus);
					AssertEquals("approvalRequest2.XP_ApprovalStatus", expectedStatus, approvalRequest2.XP_ApprovalStatus);
					AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Now, approvalRequest1.XP_ApprovalDate);
					AssertEquals("approvalRequest1.XP_ApprovalDate", GlbStaff.CurrentUser.GS_Code, approvalRequest1.XP_GS_NKApprovingUser1);
					AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Now, approvalRequest2.XP_ApprovalDate);
					AssertEquals("approvalRequest2.XP_ApprovalDate", GlbStaff.CurrentUser.GS_Code, approvalRequest2.XP_GS_NKApprovingUser1);
				}
				else
				{
					AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest1.XP_ApprovalStatus);
					AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest2.XP_ApprovalStatus);
					AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Empty, approvalRequest1.XP_ApprovalDate);
					AssertEquals("approvalRequest1.XP_ApprovalDate", "", approvalRequest1.XP_GS_NKApprovingUser1);
					AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Empty, approvalRequest2.XP_ApprovalDate);
					AssertEquals("approvalRequest2.XP_ApprovalDate", "", approvalRequest2.XP_GS_NKApprovingUser1);
				}
			}
		}

		[TestDate(2011, 03, 10)]
		public void TestCancelWithNonCurrentUser()
		{
			var approvalRequest1 = SetupRequestForFirstApprovalLevel();
			var approvalRequest2 = SetupRequestForSecondApprovalLevel();
			Factory.Save();

			Enterprise.Security.Testing.SecurityTestObject.CreateTestUser(true, Env.Security.ReceivablesTransactions.Code, "XXX", "user.XXX", "password");
			approvalRequest1.XP_SystemCreateUser = "XXX";
			approvalRequest2.XP_SystemCreateUser = "XXX";

			SetupSecurity(disallowSecondLevel: true);
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1, approvalRequest2);
			mockSecurityProvider
				.Protected()
				.Setup<SecurityCore>("RequestLoginCredentials", ItExpr.IsAny<SecurityCheckpoint>())
				.Returns((SecurityCore)null);
			approvalRequestBulk.Cancel();
			mockSecurityProvider.VerifyAll();
			AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest1.XP_ApprovalStatus);
			AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest2.XP_ApprovalStatus);
			AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Empty, approvalRequest1.XP_ApprovalDate);
			AssertEquals("approvalRequest1.XP_ApprovalDate", "", approvalRequest1.XP_GS_NKApprovingUser1);
			AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Empty, approvalRequest2.XP_ApprovalDate);
			AssertEquals("approvalRequest2.XP_ApprovalDate", "", approvalRequest2.XP_GS_NKApprovingUser1);

			mockSecurityProvider
				.Protected()
				.Setup<SecurityCore>("RequestLoginCredentials", ItExpr.IsAny<SecurityCheckpoint>())
				.Returns(new AlternativeCredentials("newuser", "password").UserSecurity);
			approvalRequestBulk.Cancel();
			mockSecurityProvider.VerifyAll();
			AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest1.XP_ApprovalStatus);
			AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest2.XP_ApprovalStatus);
			AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Empty, approvalRequest1.XP_ApprovalDate);
			AssertEquals("approvalRequest1.XP_ApprovalDate", "", approvalRequest1.XP_GS_NKApprovingUser1);
			AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Empty, approvalRequest2.XP_ApprovalDate);
			AssertEquals("approvalRequest2.XP_ApprovalDate", "", approvalRequest2.XP_GS_NKApprovingUser1);

			approvalRequest1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequest2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequest1.XP_ApprovalDate = ZDateTime.Now.AddDays(-1);
			approvalRequest2.XP_ApprovalDate = ZDateTime.Now.AddDays(-2);
			approvalRequest1.XP_GS_NKApprovingUser1 = "AAA";
			approvalRequest2.XP_GS_NKApprovingUser1 = "BBB";

			SetupSecurity();
			mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1, approvalRequest2);
			approvalRequestBulk.Cancel();
			AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest1.XP_ApprovalStatus);
			AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest2.XP_ApprovalStatus);
			AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Now.AddDays(-1), approvalRequest1.XP_ApprovalDate);
			AssertEquals("approvalRequest1.XP_ApprovalDate", "AAA", approvalRequest1.XP_GS_NKApprovingUser1);
			AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Now.AddDays(-2), approvalRequest2.XP_ApprovalDate);
			AssertEquals("approvalRequest2.XP_ApprovalDate", "BBB", approvalRequest2.XP_GS_NKApprovingUser1);
		}

		[TestDate(2011, 03, 10)]
		public void TestCancelWithDiferentUsers()
		{
			var approvalRequest1 = SetupRequestForFirstApprovalLevel();
			var approvalRequest2 = SetupRequestForSecondApprovalLevel();
			Factory.Save();

			Enterprise.Security.Testing.SecurityTestObject.CreateTestUser(true, Env.Security.ReceivablesTransactions.Code, "XXX", "user.XXX", "password");
			approvalRequest2.XP_SystemCreateUser = "XXX";

			SetupSecurity(disallowSecondLevel: true);
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1, approvalRequest2);
			mockSecurityProvider.Protected().Setup<SecurityCore>("RequestLoginCredentials", ItExpr.IsAny<SecurityCheckpoint>()).Returns((SecurityCore)null);

			approvalRequestBulk.Cancel();
			mockSecurityProvider.VerifyAll();
			AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest1.XP_ApprovalStatus);
			AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest2.XP_ApprovalStatus);
			AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Empty, approvalRequest1.XP_ApprovalDate);
			AssertEquals("approvalRequest1.XP_ApprovalDate", "", approvalRequest1.XP_GS_NKApprovingUser1);
			AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Empty, approvalRequest2.XP_ApprovalDate);
			AssertEquals("approvalRequest2.XP_ApprovalDate", "", approvalRequest2.XP_GS_NKApprovingUser1);

			mockSecurityProvider.Protected().Setup<SecurityCore>("RequestLoginCredentials", ItExpr.IsAny<SecurityCheckpoint>()).Returns(new AlternativeCredentials("newuser", "password").UserSecurity);
			approvalRequestBulk.Cancel();
			mockSecurityProvider.VerifyAll();
			AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest1.XP_ApprovalStatus);
			AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest2.XP_ApprovalStatus);
			AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Empty, approvalRequest1.XP_ApprovalDate);
			AssertEquals("approvalRequest1.XP_ApprovalDate", "", approvalRequest1.XP_GS_NKApprovingUser1);
			AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Empty, approvalRequest2.XP_ApprovalDate);
			AssertEquals("approvalRequest2.XP_ApprovalDate", "", approvalRequest2.XP_GS_NKApprovingUser1);

			approvalRequest1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequest2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequest1.XP_ApprovalDate = ZDateTime.Now.AddDays(-1);
			approvalRequest2.XP_ApprovalDate = ZDateTime.Now.AddDays(-2);
			approvalRequest1.XP_GS_NKApprovingUser1 = "AAA";
			approvalRequest2.XP_GS_NKApprovingUser1 = "BBB";

			SetupSecurity();
			mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1, approvalRequest2);
			approvalRequestBulk.Cancel();
			AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest1.XP_ApprovalStatus);
			AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest2.XP_ApprovalStatus);
			AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Now.AddDays(-1), approvalRequest1.XP_ApprovalDate);
			AssertEquals("approvalRequest1.XP_ApprovalDate", "AAA", approvalRequest1.XP_GS_NKApprovingUser1);
			AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Now.AddDays(-2), approvalRequest2.XP_ApprovalDate);
			AssertEquals("approvalRequest2.XP_ApprovalDate", "BBB", approvalRequest2.XP_GS_NKApprovingUser1);
		}

		[TestDate(2011, 03, 10)]
		public void TestCancelWithCurrentUsers()
		{
			var approvalRequest1 = SetupRequestForFirstApprovalLevel();
			var approvalRequest2 = SetupRequestForSecondApprovalLevel();
			Factory.Save();

			SetupSecurity(disallowTwoLevels: true);
			var approvalRequestBulk = GetNewApprovalBulk(new DefaultAccessSecurityProvider(), approvalRequest1, approvalRequest2);
			approvalRequestBulk.Cancel();
			AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest1.XP_ApprovalStatus);
			AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest2.XP_ApprovalStatus);
			AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Empty, approvalRequest1.XP_ApprovalDate);
			AssertEquals("approvalRequest1.XP_ApprovalDate", "", approvalRequest1.XP_GS_NKApprovingUser1);
			AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Empty, approvalRequest2.XP_ApprovalDate);
			AssertEquals("approvalRequest2.XP_ApprovalDate", "", approvalRequest2.XP_GS_NKApprovingUser1);

			approvalRequest1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequest2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequest1.XP_ApprovalDate = ZDateTime.Now.AddDays(-1);
			approvalRequest2.XP_ApprovalDate = ZDateTime.Now.AddDays(-2);
			approvalRequest1.XP_GS_NKApprovingUser1 = "AAA";
			approvalRequest2.XP_GS_NKApprovingUser1 = "BBB";

			approvalRequestBulk.Cancel();
			AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest1.XP_ApprovalStatus);
			AssertEquals("approvalRequest2.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest2.XP_ApprovalStatus);
			AssertEquals("approvalRequest1.XP_ApprovalDate", ZDateTime.Now.AddDays(-1), approvalRequest1.XP_ApprovalDate);
			AssertEquals("approvalRequest1.XP_ApprovalDate", "AAA", approvalRequest1.XP_GS_NKApprovingUser1);
			AssertEquals("approvalRequest2.XP_ApprovalDate", ZDateTime.Now.AddDays(-2), approvalRequest2.XP_ApprovalDate);
			AssertEquals("approvalRequest2.XP_ApprovalDate", "BBB", approvalRequest2.XP_GS_NKApprovingUser1);
		}

		protected virtual ZString ParentName => ZString.Empty;

		protected virtual RequestParentType GetParentFromApprovalRequest(RequestType request)
		{
			throw new NotImplementedException();
		}

		protected virtual void ModifyParentToHaveValidationError(RequestParentType parent)
		{
			throw new NotImplementedException();
		}

		protected virtual string GetErrorMessageForParentWithValidationError()
		{
			throw new NotImplementedException();
		}

		protected virtual void AssertParentIsPosted(string message, RequestParentType parent, bool isPosted)
		{
			throw new NotImplementedException();
		}

		protected virtual void PostApprovalsAndRemovePosted(ApprovalBulkType approvalRequestBulk, IPostingTransactionApprovalGUIProvider guiProvider)
		{
			throw new NotImplementedException();
		}

		protected RequestType SetupRequestWithEmptyBranchAndDepartment()
		{
			var approvalRequest = SetupRequestForFirstApprovalLevel();
			approvalRequest.XP_GB_JobBranch = ZGuid.Empty;
			approvalRequest.XP_GE_JobDepartment = ZGuid.Empty;
			return approvalRequest;
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected void SetUpTestPeriods()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
			TestObjectCreator.Factory.Save();
		}

		protected virtual Mock<IPostingTransactionApprovalGUIProvider> GetIPostingTransactionApprovalGUIProvider()
		{
			var mock = new Mock<IPostingTransactionApprovalGUIProvider>();
			SetupPostingTransactionApprovalGUIProviderMock(mock);

			return mock;
		}

		protected virtual Mock<IPostingJobAndTransactionApprovalGUIProvider> GetIPostingJobAndTransactionApprovalGUIProvider()
		{
			var mock = new Mock<IPostingJobAndTransactionApprovalGUIProvider>();
			SetupPostingJobAndTransactionApprovalGUIProviderMock(mock);

			return mock;
		}

		protected virtual void SetupPostingTransactionApprovalGUIProviderMock(Mock<IPostingTransactionApprovalGUIProvider> mock, params RequestType[] requestsWhichWillBePosted)
		{
			var factoryForApprovalRequest = new BusinessObjectFactory();
			var mockSecurityOverrideProviderWithApprovalRequestSupport = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			mock.Setup(m => m.FactoryForApprovalRequests).Returns(factoryForApprovalRequest);
			mock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(mockSecurityOverrideProviderWithApprovalRequestSupport.Object);
			mock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			mock.Setup(m => m.SecurityItemForTest).Returns(string.Empty);
		}

		protected virtual void SetupPostingJobAndTransactionApprovalGUIProviderMock(Mock<IPostingJobAndTransactionApprovalGUIProvider> mock, params RequestType[] requestsWhichWillBePosted)
		{
			var factoryForApprovalRequest = new BusinessObjectFactory();
			var mockSecurityOverrideProviderWithApprovalRequestSupport = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			mock.Setup(m => m.FactoryForApprovalRequests).Returns(factoryForApprovalRequest);
			mock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(mockSecurityOverrideProviderWithApprovalRequestSupport.Object);
			mock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			mock.Setup(m => m.SecurityItemForTest).Returns(string.Empty);
		}

		static bool IsTransactionRelatedRequest => typeof(RequestParentType) == typeof(TransactionType);
	}
}
