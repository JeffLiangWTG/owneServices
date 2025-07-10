using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.GUI.CashBook.BankReconciliation;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class BankReconDirectPaymentController : MiscellaneousTransactionController
	{
		public override ModuleIdentifier ModuleID => null;

		public override Type TypeOfTopLevelBusinessObject => typeof(BankReconDirectPayment);

		protected override ControllerID IDCore => ControllerIDs.BankReconDirectPayment;

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new BankReconDirectPaymentForm((BankReconDirectPayment)businessEntity);
		}
	}
}
