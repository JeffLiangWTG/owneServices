using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.MXManifest.IAsycudaContainer
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
