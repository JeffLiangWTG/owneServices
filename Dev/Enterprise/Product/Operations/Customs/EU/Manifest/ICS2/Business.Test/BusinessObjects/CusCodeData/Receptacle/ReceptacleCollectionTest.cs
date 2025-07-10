using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ReceptacleCollection))]
	public class ReceptacleCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<Receptacle>
	{
		public void TestCollectionOrder()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var entry1 = header.Receptacles.AddNew();
			var entry2 = header.Receptacles.AddNew();
			var entry3 = header.Receptacles.AddNew();

			AssertEquals((short)1, entry1.CY_Order);
			AssertEquals((short)2, entry2.CY_Order);
			AssertEquals((short)3, entry3.CY_Order);

			header.Receptacles.RemoveAndDelete(entry2);

			AssertEquals((short)1, entry1.CY_Order);
			AssertEquals((short)2, entry3.CY_Order);
		}

		public void TestAllowNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var cmdDataCollection = new ReceptacleCollection(header);
			AssertEquals("Max count is 999", 999, cmdDataCollection.MaxCount);

			var maxCountValidator = ((ISupportMaxCountValidation)cmdDataCollection).MaxCountValidator;
			var notification = maxCountValidator.Notification;
			AssertEquals("Notification Message", "A maximum of 999 records is allowed.", notification.Message);
		}

		public void TestAsString()
		{
			var collection = (ReceptacleCollection)GetCollectionToTest();
			CombineAssertions(() =>
			{
				AssertEquals("Empty", 0, collection.Count);
				collection.AsString = "159753,753951,951753";
				AssertContainsExactElementsInAnyOrder(new ZString[] { "159753", "753951", "951753" }, collection.Select(x => x.CY_Data));
				collection.AddNew("","357159");
				AssertEquals("Add new", "159753,753951,951753,357159", collection.AsString);
			});
		}

		protected override CusCodeDataCollection<Receptacle> GetCusCodeDataCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new ReceptacleCollection(header);
		}
	}
}
