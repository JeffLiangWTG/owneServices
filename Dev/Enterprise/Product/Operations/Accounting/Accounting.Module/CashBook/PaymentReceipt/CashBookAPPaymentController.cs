using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CashBookAPPaymentController : ZAPPaymentController
	{
		public CashBookAPPaymentController()
		{
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.CashBookAPPayment; }
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
