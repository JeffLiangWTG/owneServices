using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using ITNctsDepartureCargoDesc = Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc;
using ITNctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(ITNctsDepartureCargoDescWrapperCollection))]
sealed class ITNctsDepartureCargoDescWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ITNctsDepartureCargoDescWrapperCollection>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("when nctsHeader is null", () => new ITNctsDepartureCargoDescWrapperCollection(null, Factory));

		var nctsHeader = Factory.New<ITNctsHeader>();
		AssertExceptionThrown<ArgumentNullException>("when Factory is null", () => new ITNctsDepartureCargoDescWrapperCollection(nctsHeader, null));

		nctsHeader.SetMovementType(Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes.Departure);
		AssertNoExceptionThrown("when nctsHeader and movementHeader are valid", () => new ITNctsDepartureCargoDescWrapperCollection(nctsHeader, Factory));
	}

	protected override ITNctsDepartureCargoDescWrapperCollection GetCollectionToTest()
	{
		var nctsHeader = Factory.NewWithValidTestData<ITNctsHeader>();
		nctsHeader.SetMovementType(Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes.Departure);

		return new ITNctsDepartureCargoDescWrapperCollection(nctsHeader, Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var line = Factory.NewWithValidTestData<ITNctsDepartureCargoDesc>();
		return ITNctsDepartureCargoDescWrapper.New(line, Factory);
	}

	public void TestCollectionItems()
	{
		var header = Factory.New<ITNctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var bill1 = header.MovementHeader.Header.Bills.AddNew();
		bill1.GoodsItems.AddNew();
		bill1.GoodsItems.AddNew();

		var bill2 = header.MovementHeader.Header.Bills.AddNew();
		bill2.GoodsItems.AddNew();
		bill2.GoodsItems.AddNew();
		bill2.GoodsItems.AddNew();

		var collectionWrapper = new ITNctsDepartureCargoDescWrapperCollection(header, Factory);

		AssertEquals("Wrapped Goods Items number", 5, collectionWrapper.Count);

		var bill3 = header.MovementHeader.Header.Bills.AddNew();
		bill3.GoodsItems.AddNew().BY_Status = "DEL";

		collectionWrapper = new ITNctsDepartureCargoDescWrapperCollection(header, Factory);

		AssertEquals("Wrapped Goods Items number", 5, collectionWrapper.Count);

		var bill4 = header.MovementHeader.Header.Bills.AddNew();
		bill4.GoodsItems.AddNew().BY_Status = ZString.Empty;
		bill4.GoodsItems.AddNew().BY_Status = "DLR";

		collectionWrapper = new ITNctsDepartureCargoDescWrapperCollection(header, Factory);

		AssertEquals("Wrapped Goods Items number", 7, collectionWrapper.Count);
	}
}
