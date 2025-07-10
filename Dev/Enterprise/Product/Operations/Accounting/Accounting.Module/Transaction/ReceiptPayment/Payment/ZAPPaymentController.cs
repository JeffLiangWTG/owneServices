using System;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class ZAPPaymentController : ZPaymentController
	{
		public ZAPPaymentController()
		{
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APPayment); }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ZAPPayment; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesPayment; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReversePayablesPayment; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PayablesTransactions; }
		}

		protected override bool ShowPaymentApprovalBusinessObjectForNew
		{
			get { return true; }
		}

		protected override PaymentApprovalBase GetNewPaymentApproval()
		{
			return Factory.New<APPaymentApprovalWithoutAuthorisation>();
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APTransaction; }
		}
	}
}
