using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDAManifest.Business
{
	public class AsycudaArrivalHeader : ASYCUDA.Business.AsycudaArrivalHeader, Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaArrivalHeader
	{
		public AsycudaArrivalHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
	}
}
