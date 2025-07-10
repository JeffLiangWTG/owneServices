using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.H7.Module
{
	public class EUH7BillController : ASYCUDA.Module.ASYCUDAManifestBillController
	{
		public override ControllerID ID => ControllerIDs.Customs.EU.EUH7Bill;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.EUH7Bill;

		protected override Security.SecurityCheckpoint CheckPointForView => Env.Security.EuH7;

		protected override Security.SecurityCheckpoint CheckPointForEdit => Env.Security.EuH7;

		protected override Security.SecurityCheckpoint CheckPointForDelete => Env.Security.EuH7;
	}
}
