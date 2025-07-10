using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderProcessTask : ProcessTask, Integration.Customs.ASYCUDA.IAsycudaManifestHeaderProcessTask
	{
		public AsycudaManifestHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		protected override System.Type ParentType
		{
			get { return typeof(AsycudaManifestHeader); }
		}

		public new AsycudaManifestHeader Parent
		{
			get { return (AsycudaManifestHeader)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest; }
		}
	}
}
