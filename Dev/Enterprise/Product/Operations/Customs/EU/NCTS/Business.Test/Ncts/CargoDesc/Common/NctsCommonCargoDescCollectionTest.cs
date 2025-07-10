using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCommonCargoDescCollection<NctsCommonCargoDesc>))]
	class NctsCommonCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<INctsCommonCargoDescCollection<NctsCommonCargoDesc>>
	{
		public void TestBY_NetWeightUnitValueDefault()
		{
			CombineAssertions(() =>
			{
				var goodItem = header.ArrivalMovementHeader.GoodsItems.AddNew();
				AssertEquals("default BY_NetWeightUnit value is Kg", Core.Constants.Weight.Kilograms, goodItem.BY_NetWeightUnit);
				goodItem.BY_NetWeightUnit = Core.Constants.Weight.Grams;
				AssertEquals("value BY_NetWeightUnit can be change", Core.Constants.Weight.Grams, goodItem.BY_NetWeightUnit);
			});
		}

		public void TestBY_GrossWeightUnitValueDefault()
		{
			CombineAssertions(() =>
			{
				var goodItem = header.ArrivalMovementHeader.GoodsItems.AddNew();
				AssertEquals("default BY_GrossWeightUnit value is Kg", Core.Constants.Weight.Kilograms, goodItem.BY_GrossWeightUnit);
				goodItem.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
				AssertEquals("value BY_GrossWeightUnit can be change", Core.Constants.Weight.Grams, goodItem.BY_GrossWeightUnit);
			});
		}

		public void TestBY_RX_NKCurrencyValueDefault_MovementHeader()
		{
			var goodItem = header.ArrivalMovementHeader.GoodsItems.AddNew();
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, goodItem.BY_RX_NKCurrency);
		}

		public void TestBY_RX_NKCurrencyValueDefault_Bill()
		{
			var collection = GetCollectionToTest();

			var goodItem = collection.AddNew();
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, goodItem.BY_RX_NKCurrency);
		}

		public void TestSetArrivalGoodsItemsReadOnly()
		{
			void AssertOriginalFields(INctsCommonCargoDescCollection<NctsCommonCargoDesc> departureCargoDescCollection, ZBool readOnly, ZString assertMessage)
			{
				AssertEquals("Collection " + assertMessage, readOnly, departureCargoDescCollection.ReadOnly);
				AssertEquals("LineNo " + assertMessage, true, departureCargoDescCollection[0].BY_LineNoInfo.ReadOnly);
				AssertEquals("Commodity " + assertMessage, readOnly, departureCargoDescCollection[0].BY_HarmonisedTariffInfo.ReadOnly);
				AssertEquals("Description " + assertMessage, readOnly, departureCargoDescCollection[0].BY_DescriptionInfo.ReadOnly);
				AssertEquals("GrossWeight " + assertMessage, readOnly, departureCargoDescCollection[0].BY_GrossWeightInfo.ReadOnly);
				AssertEquals("GrossWeightUnit " + assertMessage, readOnly, departureCargoDescCollection[0].BY_GrossWeightUnitInfo.ReadOnly);
				AssertEquals("NetWeight " + assertMessage, readOnly, departureCargoDescCollection[0].BY_NetWeightInfo.ReadOnly);
				AssertEquals("NetWeightUnit " + assertMessage, readOnly, departureCargoDescCollection[0].BY_NetWeightUnitInfo.ReadOnly);
			}

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			header.ArrivalMovementHeader.GoodsItems.AddNew();
			var cargoDescCollection = header.ArrivalMovementHeader.GoodsItems;

			CombineAssertions(() =>
			{
				AssertOriginalFields(cargoDescCollection, true, "is readonly when ReceiveIE043UnloadingPermissionDetailsMessage is true and AutoPopulatedArrivalGoodsItems is false");

				var departureHeader = Factory.New<NctsHeader>();
				departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var movementHeader = departureHeader.MovementHeader;
				movementHeader.GoodsItems.AddNew();

				header.SetMovementType(NctsMovementType.Codes.Arrival);
				var arrivalMovementHeader = header.ArrivalMovementHeader;
				arrivalMovementHeader.PopulateArrivalGoodsItemsFromDeparture(movementHeader);
				cargoDescCollection = arrivalMovementHeader.GoodsItems;

				AssertOriginalFields(cargoDescCollection, true, "is readonly when ReceiveIE043UnloadingPermissionDetailsMessage is true and AutoPopulatedArrivalGoodsItems is set to true by PopulateArrivalGoodsItemsFromDeparture");
			});
		}

		protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> GetCollectionToTest() => new NctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>(header.ArrivalMovementHeader);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
		}
		NctsHeader header;
	}
}
