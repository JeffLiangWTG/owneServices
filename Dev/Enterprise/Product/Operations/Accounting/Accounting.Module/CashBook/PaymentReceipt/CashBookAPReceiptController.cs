using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CashBookAPReceiptController : ZAPReceiptController
	{
		public CashBookAPReceiptController()
		{
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.CashBookAPReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewCashBookReceipt; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewCashBookReceipt; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
