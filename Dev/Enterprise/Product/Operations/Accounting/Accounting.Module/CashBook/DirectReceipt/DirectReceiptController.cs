using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.GUI.CashBook;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class DirectReceiptController : AccountingTransactionController
	{
		public DirectReceiptController()
		{
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new DirectReceiptForm(businessEntity as DirectReceipt);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookDirectReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewCashBookDirectReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewCashBookDirectReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewCashBookDirectReceipt; }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.DirectReceipt; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DirectReceipt); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CashbookTransaction; }
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			return base.ShowTemplateCopyFormFromBase(inMemorySourceEntity);
		}
	}
}
