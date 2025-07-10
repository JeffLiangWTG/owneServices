using Enterprise.Customs.EU.H7.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.H7.Module
{
	public class GBH7BillController : EUH7BillController
	{
		public override ControllerID ID => ControllerIDs.Customs.GB.H7Bill;
	}
}
