using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.GUI.CashBook;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class OpeningPaymentController : AccountingTransactionController
	{
		public OpeningPaymentController()
		{
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			if (Reversing == null)
			{
				((OpeningPayment)businessEntity).SubmittedFromForm = true;
			}
			return new OpeningPaymentForm(businessEntity as OpeningPayment);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookOpeningPayment; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewCashBookOpeningPayment; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewCashBookOpeningPayment; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewCashBookOpeningPayment; }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.OpeningPayment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OpeningPayment); }
		}
	}
}
