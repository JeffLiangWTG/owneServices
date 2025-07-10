using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureHeaderContainersActiveCollection))]
	sealed class NctsDepartureHeaderContainersActiveCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsDepartureHeaderContainersActiveCollection>
	{
		public void TestAddingRecordWillAddToDepartureHeaderContainers()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var collectionForSynching = header.ContainersAsICusInBondContainerCollectionForSynching;
			var mainCollection = header.DepartureHeaderContainers;
			AssertEquals("mainCollection.Count", 0, mainCollection.Count);
			var container1 = collectionForSynching.AddNew();
			AssertEquals("mainCollection.Count", 1, mainCollection.Count);
			AssertSame(container1, mainCollection[0]);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			header = newFactory.Load<NctsHeader>(header.PK);
			mainCollection = header.DepartureHeaderContainers;
			AssertEquals("mainCollection.Count", 1, mainCollection.Count);
			AssertEquals(container1.PK, mainCollection[0].PK);
		}

		public void TestIndexer()
		{
			var collection = (ICusInBondContainerCollection)GetCollectionToTest();
			var container = collection.AddNew();
			AssertSame(container, collection[0]);
		}

		protected override Type GetExpectedCollectionType() => typeof(NctsDepartureHeaderContainersActiveCollection);

		protected override NctsDepartureHeaderContainersActiveCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return (NctsDepartureHeaderContainersActiveCollection)header.ContainersAsICusInBondContainerCollectionForSynching;
		}
	}
}
