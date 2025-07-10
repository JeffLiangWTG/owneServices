using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(G4PreviousDocumentCollection))]
	class G4PreviousDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMaxCount()
		{
			var collection = GetCollectionToTest();
			AssertEquals(99, collection.MaxCount);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			return nctsHeader.ArrivalMovementHeader.G4PreviousDocuments;
		}
	}
}
