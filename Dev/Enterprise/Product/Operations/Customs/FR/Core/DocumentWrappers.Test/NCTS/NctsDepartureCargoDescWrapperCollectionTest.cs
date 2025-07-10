using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.NCTS.Testing;

[TestedType(typeof(NctsDepartureCargoDescWrapperCollection))]
sealed class FRNctsDepartureCargoDescWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsDepartureCargoDescWrapperCollection>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("when nctsHeader is null", () => new NctsDepartureCargoDescWrapperCollection(null, Factory));

		var nctsHeader = Factory.New<NctsHeader>();
		AssertExceptionThrown<ArgumentNullException>("when Factory is null", () => new NctsDepartureCargoDescWrapperCollection(nctsHeader, null));

		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		AssertNoExceptionThrown("when nctsHeader and movementHeader are valid", () => new NctsDepartureCargoDescWrapperCollection(nctsHeader, Factory));
	}

	protected override NctsDepartureCargoDescWrapperCollection GetCollectionToTest()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		return new NctsDepartureCargoDescWrapperCollection(nctsHeader, Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var line = Factory.NewWithValidTestData<NctsDepartureCargoDesc>();
		return NctsDepartureCargoDescWrapper.New(line, Factory);
	}

	public void TestCollectionItems()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		header.MovementHeader.GoodsItems.AddNew();
		header.MovementHeader.GoodsItems.AddNew();
		header.MovementHeader.GoodsItems.AddNew();

		var collectionWrapper = new NctsDepartureCargoDescWrapperCollection(header, Factory);

		AssertEquals("wrapped Goods Items number", 3, collectionWrapper.Count);
	}
}
