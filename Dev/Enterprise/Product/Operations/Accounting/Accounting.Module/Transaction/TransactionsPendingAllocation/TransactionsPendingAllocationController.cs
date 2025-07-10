using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Module.Transaction.Base;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Transaction
{
#if DEBUG
	internal
#endif
	class TransactionsPendingAllocationController : TransactionControllerWithReadOnlyBehaviourControlledBySource, INavigationControllerIDProvider
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.TransactionsPendingAllocation; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TransactionsPendingAllocationDelete; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TransactionsPendingAllocationNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TransactionsPendingAllocation; }
		}

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return Res.GetString("e2fd1616-b132-4b28-bab1-76b41040951f", "The transaction pending allocation cannot be displayed because another user has changed or deleted the record. Closing and re-opening this window will refresh your data.");
			}
		}

		protected override SecurityCheckpoint GetEditCheckPointForDirectEnteredTransaction(InvoicingBase invoice) => Env.Security.TransactionsPendingAllocationEdit_DirectEntered;

		protected override SecurityCheckpoint GetEditCheckPointForUniveralXMLImportedTransaction(InvoicingBase invoice) => Env.Security.TransactionsPendingAllocationEdit_ImportSourced;

		protected override SecurityCheckpoint GetEditHeaderCheckPointForDirectEnteredTransaction(InvoicingBase invoice) => Env.Security.TransactionsPendingAllocationEdit_DirectEntered_EditInvoiceHeader;

		protected override SecurityCheckpoint GetEditHeaderCheckPointForUniveralXMLImportedTransaction(InvoicingBase invoice) => Env.Security.TransactionsPendingAllocationEdit_ImportSourced_EditInvoiceHeader;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			ReportInvalidDataSource(businessEntity);

			return new TransactionPendingAllocationForm((TransactionPendingAllocation)businessEntity);
		}

		protected override bool CheckControllerIDMismatch(ControllerID controllerID) => !ID.Equals(controllerID);

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			var cancellableEntity = sourceEntity as ICancellable;
			var allocatable = sourceEntity as TransactionPendingAllocation;
			if (cancellableEntity != null && cancellableEntity.IsCancelled)
			{
				Globals.Message.ShowError(Res.GetString("e339b980-e991-4a6c-a78f-cf6807e1440a", "This transaction is canceled and cannot be modified."));
			}
			else if (allocatable.ExportedBatchSequence != null)
			{
				Globals.Message.ShowError(Res.GetString("a1255c13-4db7-47bc-9340-7f5bac12880a", "This transaction has been exported and cannot be edited"));
			}
			else if (!IsTransactionEditable(allocatable))
			{
				Globals.Message.ShowError(Res.GetString("17eac34b-1789-4f78-bd96-79f95a211b98", "This transaction is approved or awaiting approval and cannot be modified."));
			}
			else
			{
				result = TrySwitchToExistingAllocationForm(allocatable) ?? base.ShowEditForm(sourceEntity);
			}

			return result;
		}

		bool IsTransactionEditable(TransactionPendingAllocation transaction)
		{
			var functionalityProvider = Factory.GetCachedValue("IsEInvoicingRequestImplementedFor" + Env.CurrentCompany.Country.Code,
				   () => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Env.CurrentCompany.Country.Code) as IInstanceProvider<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider>)?.Get());

			return functionalityProvider?.IsTransactionEditable(transaction) ?? true;
		}

		public ZController GetOperationController(TransactionPendingAllocation dataSource, string ledger = LedgerTypes.AccountsPayable)
		{
			ZController controller = null;

			if (dataSource != null)
			{
				if (ledger == LedgerTypes.AccountsPayable)
				{
					if (dataSource.AH_TransactionType == TransactionTypes.InvoicePendingAllocation)
					{
						controller = ZControllerFactory.Create(ControllerIDs.APInvoice);
					}
					else if (dataSource.AH_TransactionType == TransactionTypes.CreditNotePendingAllocation)
					{
						controller = ZControllerFactory.Create(ControllerIDs.APCreditNote);
					}
				}
				else if (ledger == LedgerTypes.AccountsReceivable && dataSource.AH_TransactionType == TransactionTypes.InvoicePendingAllocation)
				{
					controller = ZControllerFactory.Create(ControllerIDs.ARCreditNote);
				}
			}

			return controller;
		}

		public IZForm TrySwitchToExistingAllocationForm(TransactionPendingAllocation dataSource, string ledger = LedgerTypes.AccountsPayable)
		{
			if (ledger == LedgerTypes.AccountsPayable)
			{
				var apController = ZControllerFactory.Create(ControllerIDs.APInvoice);
				var creditNoteController = ZControllerFactory.Create(ControllerIDs.APCreditNote);

				return SwitchToController(dataSource, apController) ?? SwitchToController(dataSource, creditNoteController);
			}
			else if (ledger == LedgerTypes.AccountsReceivable && dataSource.AH_TransactionType == TransactionTypes.InvoicePendingAllocation)
			{
				var creditNoteController = ZControllerFactory.Create(ControllerIDs.ARCreditNote);

				return SwitchToController(dataSource, creditNoteController);
			}
			return null;
		}

		IZForm SwitchToController(TransactionPendingAllocation dataSource, ZController controller)
		{
			if (dataSource != null && controller.IsFormShownFor(dataSource))
			{
				controller.SwitchToFormFor(dataSource);
				return controller.LastShownForm;
			}
			return null;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (!sourceEntity.CanDelete)
			{
				Globals.Message.ShowError(sourceEntity.ReasonForNotAbleToDelete);
				return null;
			}
			else
			{
				return base.ShowDeleteForm(sourceEntity);
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.TransactionsPendingAllocation; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(TransactionPendingAllocation); }
		}

		#region INavigationControllerIDProvider

		ControllerID INavigationControllerIDProvider.GetValidControllerID(object dataSource)
		{
			var result = ID;
			var invoice = dataSource as InvoicingBase;

			if (invoice == null)
			{
				return result;
			}

			if (invoice.AH_Ledger == LedgerTypes.TransactionsPendingAllocation || invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				result = new AccountingControllerIdDecider().GetControllerID(invoice.AH_TransactionType, invoice.AH_Ledger, GetValidControllerIdHelper.IsToSkipModuleId(ID) ? null : ModuleID);
			}

			return result;
		}

		public bool ShouldLoadBusinessObject { get => true; }

		#endregion
	}
}
