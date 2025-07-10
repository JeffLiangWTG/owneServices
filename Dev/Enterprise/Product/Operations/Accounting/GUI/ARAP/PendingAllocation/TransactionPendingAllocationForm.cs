using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class TransactionPendingAllocationForm : ZForm
	{
		public TransactionPendingAllocationForm()
		{
		}

		public TransactionPendingAllocationForm(TransactionPendingAllocation bizO)
			: base(bizO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			var dataExportBatchSource = BusinessEntity as IDataExportBatchSource;
			if (dataExportBatchSource != null && dataExportBatchSource.IsDataExportBatchSupported)
			{
				PlugIns.Add(ControllerIDs.DataExportBatchPlugin);
			}
			workflowTabPage.Initialize(bizO);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				var transaction = BusinessEntity as TransactionPendingAllocation;
				if (transaction != null)
				{
					if (!transaction.AH_NumberOfSupportingDocumentsVisible_ReadOnly)
					{
						AH_NumberOfSupportingDocumentsCalcEdit.Visible = false;
						sourceXmlTabPage.RunWhenTabInitialized((sender, args) => importedInvoiceXMLControl.HideNumberOfDocuments());
					}

					PlaceOfSupplyDropEdit.Visible = transaction.NeedPlaceOfSupplyAtHeaderLevel;
					sourceXmlTabPage.TabVisible = transaction.IsImportedFromUniversalXML;

					if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
					{
						TaxBranchFindBox.Visible = false;
					}
				}
				if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.Value)
				{
					AH_GovernmentAllocatedIDTextBox.Visible = false;
				}
			}
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			var factoryList = new List<ITransactionParticipant>(new[] { ApprovalGUIProvider.FactoryForApprovalRequests });
			factoryList.AddRange(factories);

			base.Save(factoryList.ToArray());
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = ContinueWithSave.Yes;
			ApprovalGUIProvider.InitializeNewPosting();

			if (result == ContinueWithSave.Yes && AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.Value)
			{
				var transactionToApprove = BusinessEntity as TransactionPendingAllocation;
				if (transactionToApprove != null && transactionToApprove.IsInDatabase && transactionToApprove.HasApprovalRequest)
				{
					var provider = transactionToApprove.SecurityOverrideProvider;
					try
					{
						var alwaysCreateApprovalRequest = IsTransactionWithApprovalRequestEditing;
						ApprovalGUIProvider.AlwaysCreateApprovalRequest = alwaysCreateApprovalRequest;

						var continueProcessing = new TransactionPendingAllocationLevelAuthorizationWithApprovalRequest(ApprovalGUIProvider, transactionToApprove, alwaysCreateApprovalRequest).PerformLevelAuthorization();
						result = continueProcessing ? ContinueWithSave.Yes : ContinueWithSave.No;
					}
					finally
					{
						transactionToApprove.SecurityOverrideProvider = provider;
					}
				}
			}

			return result;
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			var transaction = BusinessEntity as TransactionPendingAllocation;
			if (transaction != null && !transaction.CanDelete)
			{
				Globals.Message.ShowError(transaction.ReasonForNotAbleToDelete);
				return ContinueWithDelete.No;
			}
			return base.ShowPreDeleteDialogs();
		}

		TransactionPendingAllocationFormApprovalGUIProvider ApprovalGUIProvider
		{
			get { return approvalGUIProvider ?? (approvalGUIProvider = new TransactionPendingAllocationFormApprovalGUIProvider()); }
		}
		TransactionPendingAllocationFormApprovalGUIProvider approvalGUIProvider;

		bool IsTransactionWithApprovalRequestEditing
		{
			get
			{
				var transaction = BusinessEntity as TransactionPendingAllocation;
				return transaction != null && transaction.HasApprovalRequest;
			}
		}
	}
}
