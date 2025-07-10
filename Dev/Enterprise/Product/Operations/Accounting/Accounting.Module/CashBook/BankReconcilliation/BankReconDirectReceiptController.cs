using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.GUI.CashBook.BankReconciliation;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class BankReconDirectReceiptController : MiscellaneousTransactionController
	{
		public override ModuleIdentifier ModuleID => null;

		public override Type TypeOfTopLevelBusinessObject => typeof(BankReconDirectReceipt);

		protected override ControllerID IDCore => ControllerIDs.BankReconDirectReceipt;

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new BankReconDirectReceiptForm((BankReconDirectReceipt)businessEntity);
		}
	}
}
