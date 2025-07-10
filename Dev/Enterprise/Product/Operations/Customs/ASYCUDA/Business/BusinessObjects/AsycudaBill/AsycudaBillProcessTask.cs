using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillProcessTask : ProcessTask
	{
		public AsycudaBillProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		protected override System.Type ParentType => typeof(AsycudaBill);

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override ControllerID ParentControllerID
		{
			get => ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill;
		}
	}
}
