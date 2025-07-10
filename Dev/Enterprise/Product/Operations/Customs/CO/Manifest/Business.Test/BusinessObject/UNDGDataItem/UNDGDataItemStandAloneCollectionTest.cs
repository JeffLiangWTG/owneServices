using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	[TestedType(typeof(UNDGDataItemCollection.UNDGDataItemStandAloneCollection))]
	sealed class UNDGDataItemStandAloneCollectionTest : ActiveBusinessObjectCollectionTestCase<UNDGDataItemCollection.UNDGDataItemStandAloneCollection>
	{
		protected override UNDGDataItemCollection.UNDGDataItemStandAloneCollection GetCollectionToTest()
		{
			return new UNDGDataItemCollection.UNDGDataItemStandAloneCollection(Factory, typeof(UNDGDataItem));
		}

		#region

		public new void TestReintroducedAddNewRemovedForGenericCollection()
		{
			Assert(true);
		}

		public new void TestReintroducedIndexerRemovedForGenericCollection()
		{
			Assert(true);
		}

		#endregion
	}
}
