using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(ActiveABLEntryNumCollection))]
	sealed class ActiveABCEntryNumCollectionTest : ActiveBusinessObjectCollectionTestCase<ActiveABLEntryNumCollection>
	{
		public override void TestAdd()
		{
			Assert("Add is not allowed", true);
		}

		public override void TestDelete()
		{
			Assert("It's a db only query so no delete", true);
		}

		public override void TestTypedget_Item()
		{
			var item = Bill.CustomsEntryNumbers.AddNew();
			var collection = new ActiveABLEntryNumCollection(ManifestHeader, Core.Constants.CountryCodes.Vanuatu);
			collection.Load();
			AssertEquals("collection.Count", 1, collection.Count);
			AssertEquals(item, collection[0]);
		}

		protected override ActiveABLEntryNumCollection GetCollectionToTest() => new ActiveABLEntryNumCollection(ManifestHeader, Core.Constants.CountryCodes.Vanuatu);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Bill.CustomsEntryNumbers.AddNew();

		AsycudaManifestHeader manifestHeader;
		AsycudaManifestHeader ManifestHeader
		{
			get
			{
				if (manifestHeader == null)
				{
					manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
					manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Vanuatu;
				}
				return manifestHeader;
			}
		}

		AsycudaBill bill;
		AsycudaBill Bill => bill ?? (bill = ManifestHeader.Bills.AddNew());
	}
}
