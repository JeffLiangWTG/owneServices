using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationLevelAuthorizationWithApprovalRequest : LevelAuthorizationWithApprovalRequest<TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		public TransactionPendingAllocationLevelAuthorizationWithApprovalRequest(IPostingTransactionApprovalGUIProvider postingGUIProvider, TransactionPendingAllocation transaction, bool alwaysCreateApprovalRequest)
			: base(postingGUIProvider)
		{
			this.transaction = transaction;
			this.alwaysCreateApprovalRequest = alwaysCreateApprovalRequest;
			postingGUIProvider?.FactoryForApprovalRequests.ServiceContainer.AddService(new ParentFactoryService(transaction.Factory));
		}

		readonly TransactionPendingAllocation transaction;
		readonly bool alwaysCreateApprovalRequest;

		public bool PerformLevelAuthorization()
		{
			return PerformTransactionLevelAuthorization(new[] { transaction }, out bool canContinueReversing);
		}

		protected override bool IsLevelAuthorizationRequiredCore(TransactionPendingAllocation transaction)
		{
			var requiredCheckpoint = RequiredSecurityCheckPoint;
			return alwaysCreateApprovalRequest || !requiredCheckpoint.IsAllowed;
		}

		protected override bool CheckLevelSecurityRightsCore(TransactionPendingAllocation transaction)
		{
			return !alwaysCreateApprovalRequest && CheckLevelSecurityRights(transaction);
		}

		protected override void InitializeApprovalRequestCore(TransactionPendingAllocationApprovalRequest request, TransactionPendingAllocation[] transactions)
		{
			request.Initialize(transaction);
		}

		protected override ITransactionApprovalHelper GetNewHelperCore(TransactionPendingAllocationApprovalRequest request)
		{
			return new TransactionPendingAllocationApprovalHelper(request);
		}

		public static bool CheckLevelSecurityRights(TransactionPendingAllocation transaction)
		{
			var requiredCheckPoint = RequiredSecurityCheckPoint;
			return SecurityOverrideProviderSource.Get(transaction).Provider.SecurityCertificates[requiredCheckPoint].IsAllowed;
		}

		static SecurityCheckpoint RequiredSecurityCheckPoint
		{
			get { return Env.Security.TransactionsPendingAllocationAllocate; }
		}
	}
}
