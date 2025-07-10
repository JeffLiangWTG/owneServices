using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>))]
	class NctsArrivalAndUnloadingCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>>
	{
		public void TestSetArrivalGoodsItemsReadOnly()
		{
			header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			header.ArrivalMovementHeader.GoodsItems.AddNew();
			var cargoDescCollection = header.ArrivalMovementHeader.GoodsItems;

			CombineAssertions(() =>
			{
				AssertOriginalFields(cargoDescCollection, true, "is readonly when ReceiveIE043UnloadingPermissionDetailsMessage is true and headerType is DepartureAndArrival");

				header.SetMovementType(NctsMovementType.Codes.Arrival);
				AssertOriginalFields(cargoDescCollection, true, "is readonly when ReceiveIE043UnloadingPermissionDetailsMessage is true and headerType is only Arrival");
			});
		}

		public void TestOnAdded_IsUnloadingMovementHeader()
		{
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var goodsItem = header.UnloadingMovementHeader.GoodsItems.AddNew();
			AssertEquals(true, goodsItem.IsNew);
		}

		protected override INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc> GetCollectionToTest()
		{
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			return new NctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>(header.ArrivalMovementHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		}
		NctsHeader header;

		static void AssertOriginalFields(INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc> commonCargoDescCollection, ZBool readOnly, ZString assertMessage)
		{
			AssertEquals("Collection " + assertMessage, readOnly, commonCargoDescCollection.ReadOnly);
			AssertEquals("LineNo " + assertMessage, readOnly, commonCargoDescCollection[0].BY_LineNoInfo.ReadOnly);
			AssertEquals("Commodity " + assertMessage, readOnly, commonCargoDescCollection[0].BY_HarmonisedTariffInfo.ReadOnly);
			AssertEquals("Description " + assertMessage, readOnly, commonCargoDescCollection[0].BY_DescriptionInfo.ReadOnly);
			AssertEquals("GrossWeight " + assertMessage, readOnly, commonCargoDescCollection[0].BY_GrossWeightInfo.ReadOnly);
			AssertEquals("GrossWeightUnit " + assertMessage, readOnly, commonCargoDescCollection[0].BY_GrossWeightUnitInfo.ReadOnly);
			AssertEquals("NetWeight " + assertMessage, readOnly, commonCargoDescCollection[0].BY_NetWeightInfo.ReadOnly);
			AssertEquals("NetWeightUnit " + assertMessage, readOnly, commonCargoDescCollection[0].BY_NetWeightUnitInfo.ReadOnly);
		}
	}
}
