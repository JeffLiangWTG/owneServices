using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer,
		Integration.Customs.ASYCUDA.EUManifest.IAsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
	}
}
