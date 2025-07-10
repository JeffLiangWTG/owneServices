using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderDocManagerInfo : DocManagerInfo
	{
		public AsycudaManifestHeaderDocManagerInfo(BusinessObject parent)
			: base(parent, Core.Constants.DocManagerCodes.AsycudaManifest)
		{
		}
	}
}
