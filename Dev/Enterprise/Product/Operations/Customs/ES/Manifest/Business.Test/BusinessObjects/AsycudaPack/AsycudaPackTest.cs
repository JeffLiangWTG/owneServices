using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPivot()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaContainerBillOrPackageLink>(pack.Pivot);
		}

		public void TestBill()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaBill>(pack.Bill);
		}

		public void TestContainer()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaContainer>(pack.Container);
		}

		public void TestPackedItems()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaPackPackedItemPivotCollection>(pack.PackedItems);
		}

		public void TestLookups()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaPackLookups>(pack.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = manifestHeader.Containers.AddNew();
			pack.ContainerPK = container.PK;
			return pack;
		}
	}
}
