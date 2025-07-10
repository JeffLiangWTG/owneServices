using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaContainerBillOrPackageLink))]
	sealed class AsycudaPackageContainerLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContainerAndPack()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			AssertNull(pack.Pivot);
			pack.ContainerPK = cont.PK;
			AssertEquals(cont, pack.Pivot.Container);
			AssertEquals(pack, pack.Pivot.Pack);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			return pack.Pivot;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}
}
