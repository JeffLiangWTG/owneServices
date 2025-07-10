using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaContainerBillOrPackageLink))]
	class AsycudaContainerBillOrPackageLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClusterKey_UsedForBill()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = Factory.NewWithValidTestData<AsycudaContainer>();
			container.ACN_AMA_Manifest = manifestHeader.PK;
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_AMA = manifestHeader.PK;
			var link = Factory.New<AsycudaContainerBillOrPackageLink>();
			link.APC_ABL_Bill = bill.PK;
			link.APC_ACN_Container = container.PK;
			AssertEquals(0, link.APC_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated onSaving.", 1, link.APC_ClusterKey);
		}

		public void TestClusterKey_UsedForPack()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = Factory.NewWithValidTestData<AsycudaContainer>();
			container.ACN_AMA_Manifest = manifestHeader.PK;
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_AMA = manifestHeader.PK;
			var pack = Factory.New<AsycudaPack>();
			pack.APA_ABL_Bill = bill.PK;
			var link = Factory.New<AsycudaContainerBillOrPackageLink>();
			link.APC_APA_Pack = pack.PK;
			link.APC_ACN_Container = container.PK;
			AssertEquals(0, link.APC_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated onSaving.", 1, link.APC_ClusterKey);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "KSD";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Afghanistan;
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			return pack.Pivot;
		}
	}
}
