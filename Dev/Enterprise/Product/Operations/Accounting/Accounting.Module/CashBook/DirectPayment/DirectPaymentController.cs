using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.GUI.CashBook;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.MasterFiles.Business.AccTransactionHeader;

namespace Enterprise.Accounting.Module
{
	public class DirectPaymentController : AccountingTransactionController
	{
		public DirectPaymentController()
		{
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new DirectPaymentForm(businessEntity as DirectPayment);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookDirectPayment; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewCashBookDirectPayment; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewCashBookDirectPayment; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewCashBookDirectPayment; }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.DirectPayment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DirectPayment); }
		}

		protected override ZString CantReverseMessageBoxCaption
		{
			get { return Res.GetString("faabdf51-71ea-4164-ba8b-c2e119aca355", "Direct Payment"); }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.CashbookTransaction;
			}
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			if (inMemorySourceEntity is BankTransferCharge financeCharge)
			{
				var rowFromTransactionCount = financeCharge.AH_TransactionCount == BankTransferCharge.TransactionCount ? TransactionCountConstants.BankTransferFromRow : TransactionCountConstants.BankTransferFromRowWhenReversing;
				var rowFromFilter = AccountingUtils.GetTransactionFilter(financeCharge.AH_TransactionBelongsToGroup, financeCharge.AH_GC, rowFromTransactionCount);
				var bankTransferParent = Factory.LoadTop1<BankTransferToRow>(rowFromFilter);
				Globals.Message.ShowError(Res.GetString("10068B0C-9E49-441E-A092-36A83AACAF79", "The selected Direct Payment cannot be copy as it is linked to Bank Transfer {0}.", bankTransferParent.AH_TransactionNum));
				return null;
			}

			return base.ShowTemplateCopyFormFromBase(inMemorySourceEntity);
		}
	}
}
