using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerBillOrPackageLink))]
	sealed class AsycudaPackageContainerLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContainer()
		{
			var packageContainerLink = (AsycudaContainerBillOrPackageLink)GetNewBusinessObject();
			AssertType<AsycudaContainer>(packageContainerLink.Container);
		}

		public void TestPack()
		{
			var packageContainerLink = (AsycudaContainerBillOrPackageLink)GetNewBusinessObject();
			AssertType<AsycudaPack>(packageContainerLink.Pack);
		}

		protected override BusinessObject GetNewBusinessObject() => CreateBusinessObject(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var pivot = CreateBusinessObject(factory);
			manifestHeader.SuspendCheckBusinessObjectType();
			return pivot;
		}

		AsycudaContainerBillOrPackageLink CreateBusinessObject(BusinessObjectFactory factory)
		{
			manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = manifestHeader.Containers.AddNew();
			pack.ContainerPK = container.PK;
			return pack.Pivot;
		}

		AsycudaManifestHeader manifestHeader;
	}
}
