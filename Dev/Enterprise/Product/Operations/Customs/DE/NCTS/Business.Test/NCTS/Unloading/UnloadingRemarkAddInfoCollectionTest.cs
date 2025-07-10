using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(UnloadingRemarkAddInfoCollection))]
	public class UnloadingRemarkAddInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var collection = (UnloadingRemarkAddInfoCollection)GetCollectionToTest();
			var child = collection.AddNew();

			AssertEquals("Y", child.Data.G9_UnloadingCompletion);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			return new UnloadingRemarkAddInfoCollection(header);
		}

		public new void TestReintroducedAddNewRemovedForGenericCollection()
		{
			Assert(true);
		}

		public new void TestReintroducedIndexerRemovedForGenericCollection()
		{
			Assert(true);
		}
	}
}
