using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	[TestedType(typeof(Phase5NctsDepartureCargoDescWrapperCollection))]
	sealed class Phase5NctsDepartureCargoDescWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<Phase5NctsDepartureCargoDescWrapperCollection>
	{
		public void TestCount()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			bill.GoodsItems.AddNew();
			var wrapper = Phase5NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			AssertEquals("1 bill with 1 item", 1, wrapper.Lines.Count);

			bill.GoodsItems.AddNew();
			wrapper = Phase5NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			AssertEquals("1 bill with 2 items", 2, wrapper.Lines.Count);

			var bill2 = nctsHeader.Bills.AddNew();
			bill2.GoodsItems.AddNew();
			wrapper = Phase5NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			AssertEquals("2 bills; 1 with 2 items and 1 with 1 item", 3, wrapper.Lines.Count);
		}

		protected override Phase5NctsDepartureCargoDescWrapperCollection GetCollectionToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			bill.GoodsItems.AddNew();
			return (Phase5NctsDepartureCargoDescWrapperCollection)Phase5NctsHeaderDocumentWrapper.New(nctsHeader, Factory).Lines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => NctsDepartureCargoDescWrapper.New(Factory.New<NctsDepartureCargoDesc>());
	}
}
