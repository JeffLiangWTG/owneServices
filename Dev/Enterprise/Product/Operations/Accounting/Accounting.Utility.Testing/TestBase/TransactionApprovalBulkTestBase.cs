using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Security;

namespace Enterprise.Accounting.Utility.Testing.TestBase
{
	public abstract class TransactionApprovalBulkTestBase<ApprovalBulkType, TransactionType, RequestType, DetailsType, RequestParentType> : NonPersistentBusinessObjectTestCase
		where ApprovalBulkType : TransactionApprovalBulk<TransactionType, RequestType, DetailsType>
		where TransactionType : TransactionHeader
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
		where RequestParentType : BusinessObject
	{
		protected virtual RequestType SetupRequestForFirstApprovalLevel()
		{
			var approvalRequest1 = GetNewApprovalRequest();
			approvalRequest1.PostingDetails.MaxAmountToApprove = 100M;
			return approvalRequest1;
		}

		protected virtual RequestType SetupRequestForSecondApprovalLevel(bool usingNonCurrentBranchDepartment = false)
		{
			var approvalRequest2 = GetNewApprovalRequest();
			approvalRequest2.PostingDetails.MaxAmountToApprove = 300M;
			return approvalRequest2;
		}

		protected virtual RequestType GetNewApprovalRequest()
		{
			return Factory.NewWithValidTestData<RequestType>();
		}

		protected abstract ApprovalBulkType GetNewApprovalBulk(ISecurityOverrideProvider interactiveSecurityOverrideProvider, params RequestType[] approvalRequests);

		protected abstract void SetupSecurity(bool disallowSecondLevel = false, bool disallowTwoLevels = false, Guid? branchPK = null, Guid? departmentPK = null);

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
