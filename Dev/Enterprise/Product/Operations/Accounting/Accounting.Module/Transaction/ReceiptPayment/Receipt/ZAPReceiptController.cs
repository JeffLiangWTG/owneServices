using System;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ZAPReceiptController : ZReceiptController
	{
		public ZAPReceiptController()
		{
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APReceipt); }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ZAPReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReversePayablesReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PayablesTransactions; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APTransaction; }
		}
	}
}
