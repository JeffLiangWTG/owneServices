using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.GUI.CashBook;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class OpeningReceiptController : AccountingTransactionController
	{
		public OpeningReceiptController()
		{
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			if (Reversing == null)
			{
				((OpeningReceipt)businessEntity).SubmittedFromForm = true;
			}
			return new OpeningReceiptForm((OpeningReceipt)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookOpeningReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewCashBookOpeningReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewCashBookOpeningReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewCashBookOpeningReceipt; }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.OpeningReceipt; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OpeningReceipt); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
