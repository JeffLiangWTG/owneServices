using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APCreditNoteController : CreditNoteInvoiceController, INavigationControllerIDProvider
	{
		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			ReportInvalidDataSource(businessEntity);

			IZForm form = null;

			if (Reversing != null)
			{
				APInvoice aPInvoice = businessEntity as APInvoice;
				form = GetNewInvoiceForm(aPInvoice);
			}
			else
			{
				APCreditNote aPCreditNote = businessEntity as APCreditNote;
				aPCreditNote.SubmittedFromInvoicingForm = true;
				form = GetNewCreditNoteForm(aPCreditNote);
			}

			return form;
		}

		protected override bool CheckControllerIDMismatch(ControllerID controllerID) => !(Reversing != null && ControllerIDs.APInvoice.Equals(controllerID)) &&
				!GetValidControllerIDCollection().Contains(controllerID) && !IsMultipleReversing;

		protected virtual IEnumerable<ControllerID> GetValidControllerIDCollection()
		{
			return new List<ControllerID> { ID };
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			IZForm form = null;
			if (businessEntity is APCreditNote creditNote && AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(creditNote.AH_Ledger, creditNote.AH_GC))
			{
				Globals.Message.Show(AccountingMasterFilesUtils.APCreditNoteDisallowedMessage);
			}
			else
			{
				form = base.ShowFormForNewEntityCore(businessEntity);
			}
			return form;
		}

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return Res.GetString("e0f1e0ce-6373-42a3-bdf9-7b114be180ac", "The AP credit note cannot be displayed because another user has changed or deleted the record. Closing and re-opening this window will refresh your data.");
			}
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APCreditNote; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APCreditNote); }
		}

		protected override SecurityCheckpoint CheckPointForDelete => SecuritySettings.Delete;

		protected override SecurityCheckpoint CheckPointForNew => SecuritySettings.New;

		public override SecurityCheckpoint GetCheckPointForNew(BusinessObject bizObject)
		{
			APCreditNote apCreditNote = bizObject as APCreditNote;
			if (apCreditNote != null && apCreditNote.IsApprovingInvoice)
			{
				return Env.Security.None;
			}
			else
			{
				return base.GetCheckPointForNew(bizObject);
			}
		}

		protected override SecurityCheckpoint CheckPointForView => SecuritySettings.View;

		protected override SecurityCheckpoint CheckPointForEdit => SecuritySettings.Edit;

		BasicSecuritySettings SecuritySettings { get; } = ObjectFactory
			.Get<IInvoiceSecurityChecker>()
			.GetBasicSecuritySettings(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote);

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.APTransaction;
			}
		}

		#region INavigationControllerIDProvider

		ControllerID INavigationControllerIDProvider.GetValidControllerID(object dataSource)
		{
			var result = ID;
			var invoice = dataSource as AccTransactionHeader;

			if (invoice == null)
			{
				return result;
			}

			if (invoice.AH_Ledger == LedgerTypes.IncompleteTransactions || invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.TransactionsPendingAllocation)
			{
				result = new AccountingControllerIdDecider().GetControllerID(invoice.AH_TransactionType, invoice.AH_Ledger, GetValidControllerIdHelper.IsToSkipModuleId(ID) ? null : ModuleID);
			}

			return result;
		}

		public bool ShouldLoadBusinessObject { get => true; }

		#endregion
	}
}
