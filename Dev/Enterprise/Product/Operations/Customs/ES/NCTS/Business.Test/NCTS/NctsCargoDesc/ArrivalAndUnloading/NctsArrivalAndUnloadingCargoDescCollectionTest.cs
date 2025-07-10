using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	sealed class NctsArrivalAndUnloadingCargoDescCollectionTest : TestCaseWithFactory
	{
		public void TestSetArrivalGoodsItemsReadOnly()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			header.ArrivalMovementHeader.GoodsItems.AddNew();
			var cargoDescCollection = header.ArrivalMovementHeader.GoodsItems;
			_ = (EU.NCTS.Business.NctsCommonMovementHeader)cargoDescCollection.Relationship.Master;

			CombineAssertions(() =>
			{
				AssertOriginalFields(cargoDescCollection, false, "is not readonly when ReceiveIE043UnloadingPermissionDetailsMessage is false and AutoPopulatedArrivalGoodsItems is false");

				var departureHeader = Factory.New<NctsHeader>();
				departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departureHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				departureHeader.MovementHeader.GoodsItems.AddNew();

				header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				header.ArrivalMovementHeader.PopulateArrivalGoodsItemsFromDeparture(departureHeader.MovementHeader);

				AssertOriginalFields(cargoDescCollection, true, "is readonly when ReceiveIE043UnloadingPermissionDetailsMessage is false and AutoPopulatedArrivalGoodsItems is set to true by PopulateArrivalGoodsItemsFromDeparture");
			});
		}

		void AssertOriginalFields(EU.NCTS.Business.INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc> departureCargoDescCollection, ZBool readOnly, ZString assertMessage)
		{
			AssertEquals("Collection " + assertMessage, readOnly, departureCargoDescCollection.ReadOnly);
			AssertEquals("LineNo " + assertMessage, readOnly, departureCargoDescCollection[0].BY_LineNoInfo.ReadOnly);
			AssertEquals("Commodity " + assertMessage, readOnly, departureCargoDescCollection[0].BY_HarmonisedTariffInfo.ReadOnly);
			AssertEquals("Description " + assertMessage, readOnly, departureCargoDescCollection[0].BY_DescriptionInfo.ReadOnly);
			AssertEquals("GrossWeight " + assertMessage, readOnly, departureCargoDescCollection[0].BY_GrossWeightInfo.ReadOnly);
			AssertEquals("GrossWeightUnit " + assertMessage, readOnly, departureCargoDescCollection[0].BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals("NetWeight " + assertMessage, readOnly, departureCargoDescCollection[0].BY_NetWeightInfo.ReadOnly);
			AssertEquals("NetWeightUnit " + assertMessage, readOnly, departureCargoDescCollection[0].BY_NetWeightUnitInfo.ReadOnly);
			AssertEquals("StatisticalValue " + assertMessage, readOnly, departureCargoDescCollection[0].BY_MonetaryValueInfo.ReadOnly);
		}
	}
}
