using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaContainerBillOrPackageLink : ManifestBase.AsycudaContainerBillOrPackageLink
		, Integration.Customs.ASYCUDA.IAsycudaContainerBillOrPackageLink
	{
		public AsycudaContainerBillOrPackageLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly AsycudaContainerBillOrPackageLinkDecider TypeDecider = new AsycudaContainerBillOrPackageLinkDecider();

		public new AsycudaPackageContainerLinkValidation Validation => (AsycudaPackageContainerLinkValidation)base.Validation;

		protected override ManifestBase.AsycudaContainerBillOrPackageLinkValidation GetNewValidation() => new AsycudaPackageContainerLinkValidation(this);

		public new AsycudaContainerBillOrPackageLinkLookups Lookups => (AsycudaContainerBillOrPackageLinkLookups)base.Lookups;

		protected override ManifestBase.AsycudaContainerBillOrPackageLinkLookups GetNewLookups() => new AsycudaContainerBillOrPackageLinkLookups(this);

		public new AsycudaContainer Container => (AsycudaContainer)base.Container;

		public new AsycudaPack Pack => (AsycudaPack)base.Pack;
	}
}
