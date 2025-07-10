using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APCashAdvanceController : CashAdvanceController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.APCashAdvance; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APCashAdvance; }
		}
	}
}
