using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Transaction
{
	public class TransactionsPendingAllocationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.TransactionsPendingAllocation; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Environment.Env.Licence.Core; }
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Environment.Env.Security.TransactionsPendingAllocation; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.TransactionsPendingAllocation);
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new TransactionsPendingAllocationFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new TransactionsPendingAllocationFilterControl(GridCollection, (TransactionsPendingAllocationFilterBusinessObject)FilterBusinessObject);
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.APInvoiceCode; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());

			var isTransactionsPendingAllocationAsReceivableEnabled = CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) is IEnableTransactionsPendingAllocationAllocateAsReceivable;

			MenuItem allocateMenuItem = new ZMenuItem(isTransactionsPendingAllocationAsReceivableEnabled
				? ResString.GetMultilingualString("Accounting.TransactionsPendingAllocation.AllocateAsPayable", "Allocate as Payable")
				: ResString.GetMultilingualString("Accounting.TransactionsPendingAllocation.AllocateTransactions", "Allocate Transactions"), new EventHandler(HandleAllocateTransactionAP));
			menuItems.Add(allocateMenuItem);

			if (isTransactionsPendingAllocationAsReceivableEnabled)
			{
				MenuItem allocateAsReceivableMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.TransactionsPendingAllocation.AllocateAsReceivable", "Allocate as Receivable"), new EventHandler(HandleAllocateTransactionAR));
				menuItems.Add(allocateAsReceivableMenuItem);
			}

			return menuItems.ToArray();
		}
		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Add(new ZMenuItem("-"));
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.TransactionsPendingAllocation.BulkUnallocatedTransactions", "New Bulk Unallocated Transactions"), new EventHandler(HandleBulk)));
			return menuItems.ToArray();
		}
		void HandleBulk(object sender, EventArgs args)
		{
			TransactionsPendingAllocation bizO = new TransactionsPendingAllocation(Factory);
			TransactionsPendingAllocationForm form = new TransactionsPendingAllocationForm(bizO);
			form.DisableNewAction();
			ZFormModaliser.Show(form, Grid.FindForm());
		}

#if DEBUG
		protected
#endif
		void HandleAllocateTransactionAP(object sender, EventArgs args)
		{
			HandleShowingFormSafely(() => HandleAllocateTransactionCore(sender, args, AllocateTransactionAP));
		}

		void HandleAllocateTransactionAR(object sender, EventArgs args)
		{
			HandleShowingFormSafely(() => HandleAllocateTransactionCore(sender, args, AllocateTransactionAR));
		}

		void HandleAllocateTransactionCore(object sender, EventArgs args, Func<ZController, BusinessObject, IZForm> allocator)
		{
			BusinessObject[] selectedObjects = Grid.SelectedElements;
			if (selectedObjects != null && selectedObjects.Length > 0)
			{
				var selectedElement = (TransactionPendingAllocation)selectedObjects[0];
				if (selectedElement != null)
				{
					TryShowFormSafelyOnMainThread(selectedElement, allocator, null, ShouldAllocateTransaction, CreateTransactionsPendingAllocationController);
				}
			}
		}

		bool ShouldAllocateTransaction(BusinessObject businessObject)
		{
			var transactionPendingAllocation = (TransactionPendingAllocation)businessObject;

			if (transactionPendingAllocation.IsCancelled)
			{
				Globals.Message.ShowError(Res.GetString("079a2f3e-8a18-4168-a71c-62209208127c", "Cannot Allocate transaction that has been canceled."));
				return false;
			}
			else if (transactionPendingAllocation.TransactionApprovalRequest != null && transactionPendingAllocation.TransactionApprovalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Error)
			{
				Globals.Message.ShowError(Res.GetString("5F88F653-E012-4324-ACBC-3E8A2CB26BF0",
					"This transaction cannot be allocated because it has validation errors. The only way to allocate this transaction is to edit and save it. Editing and saving will cancel the current ‘Transaction Pending Allocation Request’ and will create a new item that can be approved for allocation."));
				return false;
			}
			else if (transactionPendingAllocation.AH_TransactionType == TransactionTypes.CreditNotePendingAllocation && AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(transactionPendingAllocation.AH_Ledger, transactionPendingAllocation.AH_GC))
			{
				Globals.Message.ShowError(Res.GetString("A5708EEB-B093-4A23-80B0-CE65A60CD8DE", "This transaction cannot be allocated because {0}", AccountingMasterFilesUtils.APCreditNoteDisallowedMessage));
				return false;
			}
			else if (transactionPendingAllocation.TransactionApprovalRequest != null
					&& (
						transactionPendingAllocation.TransactionApprovalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.ApprovalRequested
						|| transactionPendingAllocation.TransactionApprovalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.RejectionRequested
					 ))
			{
				Globals.Message.ShowError(Res.GetString("2EC47FF0-0CCD-4D04-AAD1-ABE09DB743B3", "Transaction has started approving/rejection process. You cannot allocate it."));
				return false;
			}
			else
			{
				return true;
			}
		}

		ZController CreateTransactionsPendingAllocationController()
		{
			return ZControllerFactory.Create(ControllerIDs.TransactionsPendingAllocation);
		}

		IZForm AllocateTransactionAP(ZController controller, BusinessObject businessObject)
			=> AllocateTransactionCore(controller, businessObject, TransactionAllocationConverter.ConvertUnallocatedToAP);

		IZForm AllocateTransactionAR(ZController controller, BusinessObject businessObject)
			=> AllocateTransactionCore(controller, businessObject, TransactionAllocationConverter.ConvertUnallocatedToAR);

		IZForm AllocateTransactionCore(ZController controller, BusinessObject businessObject, Func<TransactionPendingAllocation, (InvoicingBase Invoice, string ErrorMessage)> converter)
		{
			var allocationController = (TransactionsPendingAllocationController)controller;
			var transactionPendingAllocation = (TransactionPendingAllocation)businessObject;

			if (controller.IsFormShownFor(transactionPendingAllocation))
			{
				controller.SwitchToFormFor(transactionPendingAllocation);
				Globals.Message.ShowError(Res.GetString("BAD525FE-2016-488E-8A9A-B9406EC79F12",
					"This transaction cannot be allocated because it is open in another form. Please close that form before continuing."));
				return null;
			}
			else
			{
				(InvoicingBase Invoice, string ErrorMessage) convertResult;

				try
				{
					convertResult = converter(transactionPendingAllocation);
				}
				catch (JobCreationException ex)
				{
					Globals.Message.Show(ex.Message);
					return null;
				}

				if (convertResult.Invoice == null)
				{
					Globals.Message.ShowError(convertResult.ErrorMessage);
					return null;
				}
				else
				{
					var transaction = convertResult.Invoice;
					var form = allocationController.TrySwitchToExistingAllocationForm(transactionPendingAllocation, convertResult.Invoice.AH_Ledger);

					if (form == null)
					{
						bool isAllocationAllowed = true;
						var provider = transactionPendingAllocation.SecurityOverrideProvider;
						var guiProvider = new TransactionPendingAllocationFormApprovalGUIProvider();
						try
						{
							guiProvider.InitializeNewPosting();
							if (AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.Value)
							{
								isAllocationAllowed = new TransactionPendingAllocationLevelAuthorizationWithApprovalRequest(guiProvider, transactionPendingAllocation, false).PerformLevelAuthorization();
							}
						}
						finally
						{
							transactionPendingAllocation.SecurityOverrideProvider = provider;
						}

						if (isAllocationAllowed)
						{
							if (guiProvider.IsPostingCanceled)
							{
								guiProvider.FactoryForApprovalRequests.Save();
							}
							else
							{
								transaction.Factory.SetContext(BusinessContext.AllocatingTransaction);
								transaction.Factory.ChildFactories.Add(guiProvider.FactoryForApprovalRequests);
								form = allocationController.GetOperationController(transactionPendingAllocation, convertResult.Invoice.AH_Ledger).ShowFormForNewEntity(transaction);
							}
						}
						else
						{
							Globals.Message.ShowInformation(Res.GetString("4474842a-d0d2-4b54-b273-a989b2eb7278", "Allocation is not allowed."));
						}
					}

#if DEBUG
					SetFormSwitchToForTest(form);
#endif
					return form;
				}
			}
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new TransactionPendingAllocationModuleCollection(Factory, GridCollectionFilter);
		}

		ZQuery GridCollectionFilter
		{
			get
			{
				var filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.TransactionsPendingAllocation);
				var transactionTypeFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.InvoicePendingAllocation);
				transactionTypeFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNotePendingAllocation);
				filter.AddToFilter(transactionTypeFilter);
				return filter;
			}
		}

#if DEBUG
		public IZForm LastFormSwitchTo_ForTestOnly;

		void SetFormSwitchToForTest(IZForm controller)
		{
			if (Globals.IsTest)
			{
				LastFormSwitchTo_ForTestOnly = controller;
			}
		}
#endif

		protected override bool HasTypeErrorForSelectedBusinessObjects(BusinessObject selectedBusinessObject)
		{
			if (selectedBusinessObject != null)
			{
				var factory = new BusinessObjectFactory();
				var obj = factory.Load<TransactionPendingAllocation>(selectedBusinessObject.PK);
				return !(obj?.MatchesFilter(GridCollectionFilter) ?? false);
			}

			return false;
		}

		protected override ZString IncorrectTypeErrorMessage
		{
			get { return Res.GetString("c240db02-e41b-4e0f-a526-fe8f1ebd745c", "The selected transaction is no longer valid. Please refresh the grid and try again."); }
		}

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			var factory = new BusinessObjectFactory();
			var obj = factory.Load<TransactionPendingAllocation>(selectedBusinessObject.PK);
			return base.ShowDeleteForm(obj);
		}
	}
}
