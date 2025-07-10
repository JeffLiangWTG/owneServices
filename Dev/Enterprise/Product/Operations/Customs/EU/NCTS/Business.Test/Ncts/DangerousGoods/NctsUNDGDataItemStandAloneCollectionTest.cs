using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsUNDGDataItemCollection.NctsUNDGDataItemStandAloneCollection))]
	sealed class NctsUNDGDataItemStandAloneCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsUNDGDataItemCollection.NctsUNDGDataItemStandAloneCollection>
	{
		protected override NctsUNDGDataItemCollection.NctsUNDGDataItemStandAloneCollection GetCollectionToTest()
		{
			return new NctsUNDGDataItemCollection.NctsUNDGDataItemStandAloneCollection(Factory, typeof(NctsUNDGDataItem));
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
