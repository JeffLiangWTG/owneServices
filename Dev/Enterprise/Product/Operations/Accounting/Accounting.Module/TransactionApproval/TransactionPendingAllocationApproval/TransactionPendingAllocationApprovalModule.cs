using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class TransactionPendingAllocationApprovalModule : TransactionApprovalModule<TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		public TransactionPendingAllocationApprovalModule()
		{
			EInvoicingRequestCountryCompliance = TransactionPendingAllocationApprovalHelper.GetITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider(Factory, Env.CurrentCompany.Country.Code);
		}

		readonly ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider EInvoicingRequestCountryCompliance;

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.TransactionsPendingAllocationApproval; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.TransactionsPendingAllocationApproval);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new TransactionPendingAllocationApprovalFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new TransactionPendingAllocationApprovalRequestCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new TransactionPendingAllocationApprovalFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.TransactionsPendingAllocationApproval; }
		}

		protected override TransactionApprovalBulk<TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails> GetNewApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, params TransactionPendingAllocationApprovalRequest[] approvalRequests)
		{
			return new TransactionPendingAllocationApprovalBulk(factory, interactiveSecurityOverrideProvider, approvalRequests);
		}

		protected override ZForm GetNewApprovalBulkForm(TransactionApprovalBulk<TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails> bizo, TransactionApprovalFormModes actionMode)
		{
			return new TransactionPendingAllocationApprovalBulkForm((TransactionPendingAllocationApprovalBulk)bizo, actionMode);
		}

		protected override bool IsErrorStatusSupported
		{
			get { return true; }
		}
		protected override bool IsRelatedTransactionEditSupported
		{
			get { return true; }
		}

		protected override bool PostedTransactionCanBeRejected => false;

		protected override void ShowEditFormForRelatedTransaction(TransactionPendingAllocationApprovalRequest request)
		{
			var transaction = request.LinkedTransaction;
			if (transaction == null)
			{
				Globals.Message.ShowError(Res.GetString("f1047399-83db-4a8b-9590-0323fbfb7cdd", "Transaction can't be found for request {0}", request.ReferenceID));
			}
			else
			{
				var controller = ZControllerFactory.Create(ControllerIDs.TransactionsPendingAllocation);
#if DEBUG
				SetLastControllerForTest(controller);
#endif
				controller.ShowEditForm(transaction);
			}
		}

		#region EInvoicing Approval Request

		protected override bool IsTransactionEligibleToCreateRejectionRequest(TransactionPendingAllocationApprovalRequest approvalRequest) => approvalRequest.IsTransactionEligibleToCreateRejectionRequest(EInvoicingRequestCountryCompliance);

		protected override bool DoRejectAction(TransactionApprovalBulk<TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails> approvalBulk)
			=> EInvoicingRequestCountryCompliance != null
			? approvalBulk.RequestRejection()
			: base.DoRejectAction(approvalBulk);

		#endregion

#if DEBUG
		public ZController LastController_ForTestOnly { get; private set; }

		public void ResetLastController_ForTestOnly()
		{
			LastController_ForTestOnly = null;
		}

		void SetLastControllerForTest(ZController controller)
		{
			if (Globals.IsTest)
			{
				LastController_ForTestOnly = controller;
			}
		}
#endif
	}
}
