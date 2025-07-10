using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(RouteEntryCollection))]
	public class RouteEntryCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<RouteEntry>
	{
		public void TestCollectionOrder()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var entry1 = header.Itinerary.AddNew();
			var entry2 = header.Itinerary.AddNew();
			var entry3 = header.Itinerary.AddNew();

			AssertEquals((short)1, entry1.CY_Order);
			AssertEquals((short)2, entry2.CY_Order);
			AssertEquals((short)3, entry3.CY_Order);

			header.Itinerary.RemoveAndDelete(entry2);

			AssertEquals((short)1, entry1.CY_Order);
			AssertEquals((short)2, entry3.CY_Order);
		}

		protected override CusCodeDataCollection<RouteEntry> GetCusCodeDataCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new RouteEntryCollection(header);
		}

		public void TestAllowNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var cmdDataCollection = new RouteEntryCollection(header);
			AssertEquals("Max count is 99", 99, cmdDataCollection.MaxCount);
		}
	}
}
