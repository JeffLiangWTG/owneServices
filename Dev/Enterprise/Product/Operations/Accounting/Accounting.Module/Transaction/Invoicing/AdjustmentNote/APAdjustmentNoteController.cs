using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APAdjustmentNoteController : InvoicingBaseController, INavigationControllerIDProvider
	{
		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			ReportInvalidDataSource(businessEntity);

			return new AdjustmentNoteForm((APAdjustmentNote)businessEntity);
		}

		protected override bool CheckControllerIDMismatch(ControllerID controllerID) => !ID.Equals(controllerID) && !IsMultipleReversing;

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return Res.GetString("8ae9c34d-4676-4b18-b400-542835889e83", "The AP adjustment note cannot be displayed because another user has changed or deleted the record. Closing and re-opening this window will refresh your data.");
			}
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APAdjustmentNote; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APAdjustmentNote); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReversePayablesAjdustmentNote; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesAjdustmentNote; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewPayablesTransaction; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewPayablesTransaction; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APTransaction; }
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

			if (invoice.AH_Ledger == LedgerTypes.IncompleteTransactions || invoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				result = new AccountingControllerIdDecider().GetControllerID(invoice.AH_TransactionType, invoice.AH_Ledger, GetValidControllerIdHelper.IsToSkipModuleId(ID) ? null : ModuleID);
			}

			return result;
		}

		public bool ShouldLoadBusinessObject { get => true; }

		#endregion
	}
}
