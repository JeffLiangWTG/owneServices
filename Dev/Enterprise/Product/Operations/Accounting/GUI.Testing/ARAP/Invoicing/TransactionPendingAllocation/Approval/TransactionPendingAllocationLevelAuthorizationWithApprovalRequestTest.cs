using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Utility.Testing.TestBase;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocationLevelAuthorizationWithApprovalRequest))]
	class TransactionPendingAllocationLevelAuthorizationWithApprovalRequestTest : LevelAuthorizationWithApprovalRequestTestBase<TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails, IPostingTransactionApprovalGUIProvider>
	{
		public void TestCorrectContextSetWhenCancellingApprovalRequestDueToUpdatingTransaction()
		{
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = true;
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("1", TestObjectCreator.Creditor1, 100);
			var approvalRequest = Factory.New<TransactionPendingAllocationApprovalRequest>();
			approvalRequest.Initialize(transaction);
			Factory.Save();
			AssertEquals(approvalRequest.XP_ApprovalStatus, Core.Constants.GenApprovalRequestApprovalStatus.Requested);

			transaction.AH_Desc = "Modified";
			Factory.Save();
			var approvalGUIProvider = new TransactionPendingAllocationFormApprovalGUIProvider();
			var levelAuthorisationProvider = new TransactionPendingAllocationLevelAuthorizationWithApprovalRequest(approvalGUIProvider, transaction, false);
			levelAuthorisationProvider.PerformLevelAuthorization();

			var approvalRequestInApprovalFactory = approvalGUIProvider.FactoryForApprovalRequests.Load<TransactionPendingAllocationApprovalRequest>(approvalRequest.PK);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequestInApprovalFactory.XP_ApprovalStatus);
			Assert("Request should have CancelApprovalRequestDueToUpdatingLinkedTransaction context", approvalRequestInApprovalFactory.HasContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction));

			approvalGUIProvider.FactoryForApprovalRequests.Save();

			Assert("Request should not have CancelApprovalRequestDueToUpdatingLinkedTransaction context anymore since request is saved", !approvalRequestInApprovalFactory.HasContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction));

			transaction.TransactionApprovalRequest.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			transaction.AH_Desc = "Modified after request approved";
			Factory.Save();

			levelAuthorisationProvider = new TransactionPendingAllocationLevelAuthorizationWithApprovalRequest(approvalGUIProvider, transaction, false);
			levelAuthorisationProvider.PerformLevelAuthorization();

			approvalRequestInApprovalFactory = approvalGUIProvider.FactoryForApprovalRequests.Load<TransactionPendingAllocationApprovalRequest>(approvalRequest.PK);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequestInApprovalFactory.XP_ApprovalStatus);
			Assert("Request should have CancelApprovalRequestDueToUpdatingLinkedTransaction context", approvalRequestInApprovalFactory.HasContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction));

			approvalGUIProvider.FactoryForApprovalRequests.Save();

			Assert("Request should not have CancelApprovalRequestDueToUpdatingLinkedTransaction context anymore since request is saved", !approvalRequestInApprovalFactory.HasContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction));
		}

		public override void TestIsSecondApproverApplicable()
		{
			var levelAuthorization = new TransactionPendingAllocationLevelAuthorizationWithApprovalRequest(null, null, false);
			AssertEquals(false, levelAuthorization.IsMultipleApproverApplicable);
		}

		protected override bool PerformTransactionLevelAuthorization(IPostingTransactionApprovalGUIProvider postingGUIProvider, TransactionPendingAllocation[] transactions,
			Tuple<
					Func<TransactionPendingAllocation, bool>,
					Func<TransactionPendingAllocation, bool>,
					Action<TransactionPendingAllocationApprovalRequest,
					TransactionPendingAllocation[]>,
					Func<TransactionPendingAllocationApprovalRequest, ITransactionApprovalHelper>
				> testOnlyOverrides)
		{
			var helper = new TransactionPendingAllocationLevelAuthorizationWithApprovalRequest(postingGUIProvider, transactions.Last(), alwaysCreateApprovalRequest);
			helper.IsLevelAuthorizationRequired_ForTestOnly = testOnlyOverrides.Item1;
			helper.CheckLevelSecurityRights_ForTestOnly = testOnlyOverrides.Item2;
			helper.InitializeApprovalRequest_ForTestOnly = testOnlyOverrides.Item3;
			helper.GetNewHelper_ForTestOnly = testOnlyOverrides.Item4;

			return helper.PerformLevelAuthorization();
		}

		protected override TransactionPendingAllocation CreateTransactionHeader(ZString transactionNumber, IPostingTransactionApprovalGUIProvider guiWrapper)
		{
			return TestObjectCreator.CreateTransactionPendingAllocation(transactionNumber, TestObjectCreator.Creditor1, 100);
		}

		protected override void CreateTransactionLine(TransactionPendingAllocation transaction, Job job, decimal localAmount)
		{
			//lines are not supported by TransactionPendingAllocation
		}

		protected override string GetExpectedMessageCaption()
		{
			return "Unallocated Transaction Approval Request";
		}

		protected override bool IsSingleTransactionApprovalTest
		{
			get { return true; }
		}

		readonly bool alwaysCreateApprovalRequest;
	}
}
