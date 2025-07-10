using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRBillsCollection))]
	class JPAFRBillsCollectionTest : ActiveBusinessObjectCollectionTestCase<JPAFRBillsCollection>
	{
		public void TestAddNewAndIndexer_BillNumber()
		{
			var header = Factory.New<JPAFRHeader>();
			var collection = header.Bills;
			var bill1 = collection.AddNew("ISS1B1");
			var bill2 = collection.AddNew("ISS2B1");
			var bill3 = collection.AddNew("ISS1B2");

			AssertNull(collection[""]);
			AssertNull(collection["B2"]);
			AssertEquals(bill3, collection["ISS1B2"]);
			AssertEquals(bill1, collection["ISS1B1"]);
			AssertEquals(bill2, collection["ISS2B1"]);
		}

		protected override JPAFRBillsCollection GetCollectionToTest() => new JPAFRBillsCollection(Factory.New<JPAFRHeader>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<JPAFRBills>();
	}
}
