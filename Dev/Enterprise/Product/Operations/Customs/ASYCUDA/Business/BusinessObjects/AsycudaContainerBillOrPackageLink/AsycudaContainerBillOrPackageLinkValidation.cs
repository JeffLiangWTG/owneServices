namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackageContainerLinkValidation : ManifestBase.AsycudaContainerBillOrPackageLinkValidation
	{
		public AsycudaPackageContainerLinkValidation(AsycudaContainerBillOrPackageLink parent) : base(parent)
		{
		}

		public new AsycudaContainerBillOrPackageLink Parent => (AsycudaContainerBillOrPackageLink)base.Parent;
	}
}
