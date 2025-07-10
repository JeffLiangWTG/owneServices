using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementLineGroupCollection))]
	sealed class CusStatementLineGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFindOrCreateLineGroup()
		{
			var collection = (CusStatementLineGroupCollection)GetCollectionToTest();

			AssertNull("Should not create any data when the importer business number is empty.", collection.FindOrCreate(ZString.Empty));
			AssertEquals(0, collection.Count);

			var lineGroup1 = collection.FindOrCreate("BRM0000180101");
			AssertNotNull("Should create data when the importer business number is not empty.", lineGroup1);
			AssertEquals(1, collection.Count);

			AssertEquals(lineGroup1, collection.FindOrCreate("BRM0000180101"));
			AssertEquals(1, collection.Count);

			var lineGroup3 = collection.FindOrCreate("BRM0000180102");
			AssertNotEquals(lineGroup1, lineGroup3);
			AssertEquals(2, collection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<CusStatementHeader>();
			return header.LineGroupCollection;
		}
	}
}
