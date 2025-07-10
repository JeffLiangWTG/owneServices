using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaLinkPackageCollection<>))]
	public class AsycudaLinkPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AsycudaLinkPackageCollection<AsycudaLinkPackage>>
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestLoad()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			var asycudaLinkPackageCollection = new AsycudaLinkPackageCollection<AsycudaLinkPackage>(packedItem);
			asycudaLinkPackageCollection.Load();

			AssertEquals("There should be no link package as there is no pack", 0, asycudaLinkPackageCollection.Count);

			var pack = bill.Packs.AddNew();
			asycudaLinkPackageCollection.Load();
			AssertEquals("There should be 1 link package as there is 1 pack", 1, asycudaLinkPackageCollection.Count);
			Assert("There should be 1 link package link to pack", asycudaLinkPackageCollection.Cast<AsycudaLinkPackage>().Any(x => x.Package.PK == pack.PK));

			var pack2 = bill.Packs.AddNew();
			AssertEquals("Sync should be done thus there should be 2 link packages as there are 2 packs", 2, asycudaLinkPackageCollection.Count);
			Assert("There should be 1 link package link to pack2", asycudaLinkPackageCollection.Cast<AsycudaLinkPackage>().Any(x => x.Package.PK == pack2.PK));

			bill.Packs.RemoveAndDeleteAll();
			AssertEquals("There should be 0 link package as there is no more pack", 0, asycudaLinkPackageCollection.Count);
		}

		protected override AsycudaLinkPackageCollection<AsycudaLinkPackage> GetCollectionToTest()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			return new AsycudaLinkPackageCollection<AsycudaLinkPackage>(packedItem);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			return new AsycudaLinkPackage(packedItem);
		}
	}
}
