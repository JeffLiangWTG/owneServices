using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.COManifest.IAsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new AsycudaContainerValidation Validation => (AsycudaContainerValidation)base.Validation;

		protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);
	}
}
