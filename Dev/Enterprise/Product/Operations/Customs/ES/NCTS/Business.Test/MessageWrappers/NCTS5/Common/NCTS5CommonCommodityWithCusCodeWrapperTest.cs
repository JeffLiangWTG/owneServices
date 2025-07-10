using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonCommodityWithCusCodeWrapperTest : WrapperHelperTest<NCTS5CommonCommodityWithCusCodeWrapper>
	{
		public void TestCusCode()
		{
			CombineAssertions(() =>
			{
				goodsItemDeparture.BY_CusC4Number = "0010111-1";
				AssertEquals("Expected filled CusCode when item is departure", "0010111-1", wrapperDeparture.CusCode);

				goodsItemArrival.BY_CusC4Number = "0020222-2";
				AssertEquals("Expected filled CusCode when item is arrival", "0020222-2", wrapperArrival.CusCode);

				goodsItemUnloaded.BY_CusC4Number = "0030333-3";
				AssertEquals("Expected filled CusCode when item is unloaded", "0030333-3", wrapperUnloaded.CusCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			goodsItemDeparture = Factory.New<NctsDepartureCargoDesc>();
			wrapperDeparture = GetWrapperDeparture(goodsItemDeparture);

			goodsItemArrival = Factory.New<NctsArrivalCargoDesc>();
			wrapperArrival = GetWrapperArrival(goodsItemArrival);

			goodsItemUnloaded = Factory.New<NctsUnloadedCargoDesc>();
			wrapperUnloaded = GetWrapperUnloaded(goodsItemUnloaded);
		}

		NctsDepartureCargoDesc goodsItemDeparture;
		NCTS5CommonCommodityWithCusCodeWrapper wrapperDeparture;
		NctsArrivalCargoDesc goodsItemArrival;
		NCTS5CommonCommodityWithCusCodeWrapper wrapperArrival;
		NctsUnloadedCargoDesc goodsItemUnloaded;
		NCTS5CommonCommodityWithCusCodeWrapper wrapperUnloaded;

		NCTS5CommonCommodityWithCusCodeWrapper GetWrapperDeparture(NctsDepartureCargoDesc item) => new NCTS5CommonCommodityWithCusCodeWrapper(item);
		NCTS5CommonCommodityWithCusCodeWrapper GetWrapperArrival(NctsArrivalCargoDesc item) => new NCTS5CommonCommodityWithCusCodeWrapper(item);
		NCTS5CommonCommodityWithCusCodeWrapper GetWrapperUnloaded(NctsUnloadedCargoDesc item) => new NCTS5CommonCommodityWithCusCodeWrapper(item);

		protected override NCTS5CommonCommodityWithCusCodeWrapper GetProvider() => wrapperDeparture;
	}
}
