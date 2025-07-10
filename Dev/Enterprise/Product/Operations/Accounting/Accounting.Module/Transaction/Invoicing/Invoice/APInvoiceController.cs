using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APInvoiceController : CreditNoteInvoiceController, INavigationControllerIDProvider
	{
		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			ReportInvalidDataSource(businessEntity);

			IZForm form;

			if (Reversing != null)
			{
				APCreditNote aPCreditNote = businessEntity as APCreditNote;
				form = GetNewCreditNoteForm(aPCreditNote);
			}
			else
			{
				APInvoice aPInvoice = businessEntity as APInvoice;
				aPInvoice.SubmittedFromInvoicingForm = true;
				form = GetNewInvoiceForm(aPInvoice);
			}
			return form;
		}

		protected override bool CheckControllerIDMismatch(ControllerID controllerID) => !(Reversing != null && ControllerIDs.APCreditNote.Equals(controllerID)) &&
				!GetValidControllerIDCollection().Contains(controllerID) &&
				!IsMultipleReversing;

		protected virtual IEnumerable<ControllerID> GetValidControllerIDCollection()
		{
			return new List<ControllerID> { ID };
		}

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return Res.GetString("f3da6965-f660-4791-93b7-6b1feb4b8f44", "The AP invoice cannot be displayed because another user has changed or deleted the record. Closing and re-opening this window will refresh your data.");
			}
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var businessEntity = base.GetNewBusinessEntityInLocalFactory();

			if (Reversing == null)
			{
				((APInvoice)businessEntity).AllowDefaultChargeCodeLineToBeAdded = true;
			}

			return businessEntity;
		}

		protected new APInvoiceReversing Reversing
		{
			get { return base.Reversing as APInvoiceReversing; }
#if DEBUG
			set { base.Reversing = value; }
#endif
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APInvoice; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APInvoice); }
		}

		protected override SecurityCheckpoint CheckPointForDelete => SecuritySettings.Delete;

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			var result = Env.Security.None;
			var apInvoice = bizObject as APInvoice;
			if (apInvoice != null)
			{
				result = apInvoice.IsSelfBillingInvoice ? Env.Security.ReverseSelfBilledPayablesInvoice : Env.Security.ReversePayablesInvoice;
			}
			return result;
		}

		protected override SecurityCheckpoint CheckPointForNew => SecuritySettings.New;

		public override SecurityCheckpoint GetCheckPointForNew(BusinessObject bizObject)
		{
			APInvoice apInvoice = bizObject as APInvoice;
			if (apInvoice != null && apInvoice.IsApprovingInvoice)
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
			.GetBasicSecuritySettings(LedgerTypes.AccountsPayable, TransactionTypes.Invoice);

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APTransaction; }
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			return base.ShowTemplateCopyFormFromBase(inMemorySourceEntity);
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
