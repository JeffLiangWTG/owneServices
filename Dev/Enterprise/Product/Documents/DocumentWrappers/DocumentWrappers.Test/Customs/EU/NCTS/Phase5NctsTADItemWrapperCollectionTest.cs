using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	[TestedType(typeof(Phase5NctsTADItemWrapperCollection))]
	sealed class Phase5NctsTADItemWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<Phase5NctsTADItemWrapperCollection>
	{
		public void TestCount()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			bill.GoodsItems.AddNew();
			var wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("1 bill with 1 item", 1, wrapperCollection.Count);

			bill.GoodsItems.AddNew();
			wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("1 bill with 2 items", 2, wrapperCollection.Count);

			var bill2 = nctsHeader.Bills.AddNew();
			bill2.GoodsItems.AddNew();
			wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("2 bills; 1 with 2 items and 1 with 1 item", 3, wrapperCollection.Count);
		}

		protected override Phase5NctsTADItemWrapperCollection GetCollectionToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			bill.GoodsItems.AddNew();
			return new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Phase5NctsTADItemWrapper.New(Factory.New<NctsDepartureCargoDesc>(), Factory);
	}
}
