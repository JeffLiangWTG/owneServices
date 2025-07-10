using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaContainerFetchStrategy : AsycudaFetchStrategy
	{
		public AsycudaContainerFetchStrategy(AsycudaContainer container)
			: base(container)
		{
		}

		protected new AsycudaContainer BusinessObject => (AsycudaContainer)base.BusinessObject;

		protected override void AddFetchHintsForDeleteCore()
		{
			base.AddFetchHintsForDeleteCore();
			Factory.AddFetchHint(typeof(AsycudaContainerBillOrPackageLink), AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container, BusinessObject.PK);
		}
	}
}

