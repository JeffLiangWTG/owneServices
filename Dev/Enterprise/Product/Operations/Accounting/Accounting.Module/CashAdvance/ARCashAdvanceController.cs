using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARCashAdvanceController : CashAdvanceController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.ARCashAdvance; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ARCashAdvance; }
		}
	}
}
