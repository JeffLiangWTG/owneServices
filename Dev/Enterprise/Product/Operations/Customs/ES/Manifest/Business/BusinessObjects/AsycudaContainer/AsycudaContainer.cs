using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
	}
}
