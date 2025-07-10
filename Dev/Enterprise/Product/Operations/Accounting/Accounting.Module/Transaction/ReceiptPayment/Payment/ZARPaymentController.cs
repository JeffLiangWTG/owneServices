using System;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ZARPaymentController : ZPaymentController
	{
		public ZARPaymentController()
		{
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARPayment); }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ZARPayment; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewReceivablesPayment; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesPayment; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ReceivablesTransactions; }
		}

		protected override bool ShowPaymentApprovalBusinessObjectForNew
		{
			get { return true; }
		}

		protected override PaymentApprovalBase GetNewPaymentApproval()
		{
			return Factory.New<ARPaymentApprovalWithoutAuthorisation>();
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ARTransaction; }
		}
	}
}
