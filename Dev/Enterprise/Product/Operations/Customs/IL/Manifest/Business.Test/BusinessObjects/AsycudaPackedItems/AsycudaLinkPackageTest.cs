using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaLinkPackage))]
	public class AsycudaLinkPackageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPackage()
		{
			var linkPackage = (AsycudaLinkPackage)GetNewBusinessObject();
			AssertEquals("Package should equal Pack entered via the setter.", pack, linkPackage.Package);
		}

		public void TestPackageNumber()
		{
			var linkPackage = (AsycudaLinkPackage)GetNewBusinessObject();
			AssertEquals("PackageNumber should be equal pack APA_MarksAndNumbers.", "mark", linkPackage.PackageNumber);
		}

		public void TestPackQty()
		{
			var linkPackage = (AsycudaLinkPackage)GetNewBusinessObject();
			AssertEquals("PackQty should be equal pack APA_PackQty.", 10, linkPackage.PackQty);
		}

		public void TestIsLinked()
		{
			var linkPackage = (AsycudaLinkPackage)GetNewBusinessObject();
			AssertEquals("Prerequisite:", 0, packedItem.PackagesPivot.Count);
			Assert(!linkPackage.IsLinked);
			linkPackage.IsLinked = true;
			AssertEquals("A pivot should have been created.", 1, packedItem.PackagesPivot.Count);
			Assert(linkPackage.IsLinked);
		}

		public void TestContainerPK()
		{
			var linkPackage = (AsycudaLinkPackage)GetNewBusinessObject();
			var asycudaContainer = pack.Bill.Header.Containers.AddNew();
			asycudaContainer.ACN_ContainerNumber = "HLBU9822559";
			pack.ContainerPK = asycudaContainer.PK;
			AssertEquals("ContainerPK should be equal to the pack's ContainerPK.", asycudaContainer.PK, linkPackage.ContainerPK);
		}

		public void TestLookups()
		{
			var linkPackage = (AsycudaLinkPackage)GetNewBusinessObject();
			AssertType<AsycudaLinkPackageLookups>(linkPackage.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();
			var linkPackage = packedItem.AsycudaLinkPackages.AddNew();
			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 10;
			pack.APA_MarksAndNumbers = "mark";
			linkPackage.Package = pack;

			return linkPackage;
		}

		AsycudaPack pack;
		AsycudaPackedItem packedItem;
	}
}
