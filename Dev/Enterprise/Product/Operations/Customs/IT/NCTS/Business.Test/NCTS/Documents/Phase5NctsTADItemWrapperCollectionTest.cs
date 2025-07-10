using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(Phase5NctsTADItemWrapperCollection))]
sealed class Phase5NctsTADItemWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<Phase5NctsTADItemWrapperCollection>
{
	public void TestCount_ShouldExcludeBillsWithStatusDELOrDLR()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		CombineAssertions(() =>
		{
			var bill = nctsHeader.Bills.AddNew();
			bill.GoodsItems.AddNew();
			var wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("Expected 1 item : 1 bill with 1 goods item", 1, wrapperCollection.Count);

			bill.GoodsItems.AddNew();
			wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("Expected 2 items : 1 bill with 2 goods items", 2, wrapperCollection.Count);

			var bill2 = nctsHeader.Bills.AddNew();
			bill2.GoodsItems.AddNew();
			wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("Expected 3 items : 1 bill with 2 goods items and 1 bill with 1 goods item", 3, wrapperCollection.Count);

			bill.B0_BillStatus = "DEL";
			wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("Expected 1 item : first bill is marked as deleted (DEL), only second bill remains", 1, wrapperCollection.Count);

			bill2.B0_BillStatus = "DLR";
			wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("Expected 0 items : first bill is marked as deleted (DEL), second bill is marked as deletion requested (DLR)", 0, wrapperCollection.Count);
		});
	}

	public void TestCount_ShouldExcludeGoodsItemsWithStatusDELOrDLR()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		CombineAssertions(() =>
		{
			var bill = nctsHeader.Bills.AddNew();
			bill.GoodsItems.AddNew();
			var wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("Expected 1 item : 1 bill with 1 goods item", 1, wrapperCollection.Count);

			var item2 = bill.GoodsItems.AddNew();
			wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("Expected 2 items : 1 bill with 2 goods items", 2, wrapperCollection.Count);

			var bill2 = nctsHeader.Bills.AddNew();
			var item3 = bill2.GoodsItems.AddNew();
			wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("Expected 3 items : 1 bill with 2 goods items and 1 bill with 1 goods item", 3, wrapperCollection.Count);

			item2.BY_Status = "DEL";
			wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("Expected 2 items : one goods item is marked as deleted (DEL) in the first bill", 2, wrapperCollection.Count);

			item3.BY_Status = "DLR";
			wrapperCollection = new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
			AssertEquals("Expected 1 item : one goods item in the first bill is marked as deleted (DEL), one goods item in the second bill is marked as deletion requested (DLR)", 1, wrapperCollection.Count);
		});
	}

	protected override Phase5NctsTADItemWrapperCollection GetCollectionToTest()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var bill = nctsHeader.Bills.AddNew();
		bill.GoodsItems.AddNew();
		return new Phase5NctsTADItemWrapperCollection(nctsHeader, Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return Phase5NctsTADItemWrapper.New(Factory.New<NctsDepartureCargoDesc>(), Factory);
	}
}
