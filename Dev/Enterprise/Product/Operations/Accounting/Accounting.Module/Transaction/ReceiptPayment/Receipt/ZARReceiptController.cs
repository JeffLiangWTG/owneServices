using System;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ZARReceiptController : ZReceiptController
	{
		public ZARReceiptController()
		{
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARReceipt); }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ZARReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewReceivablesReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ReceivablesTransactions; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ARTransaction; }
		}
	}
}
