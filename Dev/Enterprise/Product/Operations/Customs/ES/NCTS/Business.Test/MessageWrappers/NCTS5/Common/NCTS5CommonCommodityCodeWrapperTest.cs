using System;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonCommodityCodeWrapperTest : WrapperHelperTest<NCTS5CommonCommodityCodeWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if item is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "item"), () => new NCTS5CommonCommodityCodeWrapper(null));
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled HarmonizedSystemSubHeadingCode when item is departure", "220300", wrapperDeparture.HarmonizedSystemSubHeadingCode);

				AssertEquals("Expected filled HarmonizedSystemSubHeadingCode when item is arrival", "110201", wrapperArrival.HarmonizedSystemSubHeadingCode);

				AssertEquals("Expected filled HarmonizedSystemSubHeadingCode when item is unloaded", "330103", wrapperUnloaded.HarmonizedSystemSubHeadingCode);

				AssertEquals("Expected filled HarmonizedSystemSubHeadingCode when given as argument", "440504", wrapperWithCodes.HarmonizedSystemSubHeadingCode);
			});
		}

		public void TestCombinedNomenclatureCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled CombinedNomenclatureCode when item is departure", "10", wrapperDeparture.CombinedNomenclatureCode);

				AssertEquals("Expected filled CombinedNomenclatureCode when item is arrival", "20", wrapperArrival.CombinedNomenclatureCode);

				AssertEquals("Expected filled CombinedNomenclatureCode when item is unloaded", "30", wrapperUnloaded.CombinedNomenclatureCode);

				AssertEquals("Expected filled CombinedNomenclatureCode when given as argument", "40", wrapperWithCodes.CombinedNomenclatureCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			goodsItemDeparture = Factory.New<NctsDepartureCargoDesc>();
			goodsItemDeparture.BY_HarmonisedTariff = "2203001023";
			wrapperDeparture = new NCTS5CommonCommodityCodeWrapper(goodsItemDeparture);

			goodsItemArrival = Factory.New<NctsArrivalCargoDesc>();
			goodsItemArrival.BY_HarmonisedTariff = "1102012025";
			wrapperArrival = new NCTS5CommonCommodityCodeWrapper(goodsItemArrival);

			goodsItemUnloaded = Factory.New<NctsUnloadedCargoDesc>();
			goodsItemUnloaded.BY_HarmonisedTariff = "3301033026";
			wrapperUnloaded = new NCTS5CommonCommodityCodeWrapper(goodsItemUnloaded);

			wrapperWithCodes = new NCTS5CommonCommodityCodeWrapper("440504", "40");
		}

		NctsDepartureCargoDesc goodsItemDeparture;
		NCTS5CommonCommodityCodeWrapper wrapperDeparture;
		NctsArrivalCargoDesc goodsItemArrival;
		NCTS5CommonCommodityCodeWrapper wrapperArrival;
		NctsUnloadedCargoDesc goodsItemUnloaded;
		NCTS5CommonCommodityCodeWrapper wrapperUnloaded;
		NCTS5CommonCommodityCodeWrapper wrapperWithCodes;

		protected override NCTS5CommonCommodityCodeWrapper GetProvider() => wrapperDeparture;
	}
}
