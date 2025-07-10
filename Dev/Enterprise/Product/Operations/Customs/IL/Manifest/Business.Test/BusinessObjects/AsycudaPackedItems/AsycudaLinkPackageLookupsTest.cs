using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaLinkPackageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainers()
		{
			AssertSame("Containers should be same to the pack's Containers.", pack.Bill.Header.Containers, lookups.Containers);
			AssertEquals("HLBU9822559", lookups.Containers.Single().ACN_ContainerNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var linkPackage = packedItem.AsycudaLinkPackages.AddNew();
			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 10;
			pack.APA_MarksAndNumbers = "mark";
			linkPackage.Package = pack;
			var asycudaContainer = pack.Bill.Header.Containers.AddNew();
			asycudaContainer.ACN_ContainerNumber = "HLBU9822559";
			pack.ContainerPK = asycudaContainer.PK;
			lookups = linkPackage.Lookups;
		}

		AsycudaLinkPackageLookups lookups;
		AsycudaPack pack;
	}
}
