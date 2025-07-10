namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaContainerBillOrPackageLinkLookups : ManifestBase.AsycudaContainerBillOrPackageLinkLookups
	{
		public AsycudaContainerBillOrPackageLinkLookups(AsycudaContainerBillOrPackageLink parent) : base(parent)
		{
		}

		public new AsycudaContainerBillOrPackageLink Parent => (AsycudaContainerBillOrPackageLink)base.Parent;
	}
}
