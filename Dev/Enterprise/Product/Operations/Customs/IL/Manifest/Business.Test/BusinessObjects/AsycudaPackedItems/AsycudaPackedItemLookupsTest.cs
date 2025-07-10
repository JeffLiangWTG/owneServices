using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	public class AsycudaPackedItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsItems()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var lookups = packedItem.Lookups;
			AssertNotNull(lookups.PackStatusList);
			AssertEquals(lookups.PackStatusList.GetType(), typeof(ILPackStatusList));
		}

		public void TestPackStatusList()
		{
			var packStatusList = Factory.GetCachedValue<ILPackStatusList>();

			AssertEquals("Should contain 2 items", 2, packStatusList.Count);

			var expectedPackStatusListList = new CodeDescriptionPairList();
			expectedPackStatusListList.AddPair("D", "Dangerous");
			expectedPackStatusListList.AddPair("N", "Not Dangerous");
			AssertEquals("Elements", expectedPackStatusListList.ElementsAsString, packStatusList.ElementsAsString);

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var lookups = packedItem.Lookups;

			AssertSame("PackStatusList is cached", packStatusList, lookups.PackStatusList);
		}
	}
}
