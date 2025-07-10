using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CashBookARPaymentController : ZARPaymentController
	{
		public CashBookARPaymentController()
		{
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.CashBookARPayment; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookPayment; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewCashBookPayment; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewCashBookPayment; }
		}

		protected override bool ShowPaymentApprovalBusinessObjectForNew
		{
			get { return false; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
