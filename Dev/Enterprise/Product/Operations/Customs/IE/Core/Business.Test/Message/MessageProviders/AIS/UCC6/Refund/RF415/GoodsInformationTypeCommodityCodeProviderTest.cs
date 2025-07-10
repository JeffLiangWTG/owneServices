using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class GoodsInformationTypeCommodityCodeProviderTest : DataProviderTestCase<GoodsInformationTypeCommodityCodeProvider>
	{
		public void TestCombinedNomenclatureCode()
		{
			EntryLineWrapper.RandomInvoiceLine.JI_Tariff = "8703231100";
			AssertEquals("87032311", Provider.CombinedNomenclatureCode);
		}

		public void TestTaricCode()
		{
			EntryLineWrapper.RandomInvoiceLine.JI_Tariff = "8703231100";
			AssertEquals("00", Provider.TaricCode);
		}

		public void TestTaricAdditionalCode()
		{
			AssertNull(Provider.TaricAdditionalCode);
		}

		public void TestNationalAdditionalCommodityCode()
		{
			AssertNull(Provider.NationalAdditionalCommodityCode);
		}

		public void TestHarmonizedSystemSubheadingCode()
		{
			AssertNull(Provider.HarmonizedSystemSubheadingCode);
		}

		public void TestNationalAdditionalCode()
		{
			AssertEquals(Array.Empty<string>(), Provider.NationalAdditionalCode);
		}

		public void TestTypeGoods()
		{
			AssertNull(Provider.TypeGoods);
		}

		protected override GoodsInformationTypeCommodityCodeProvider GetProvider() => new GoodsInformationTypeCommodityCodeProvider(EntryLineWrapper);

		EntryLineWrapper EntryLineWrapper => entryLineWrapper ??= MessageProviderTestHelper.SetupBasicTestBizObjs(Factory).entryLineWrapper;
		EntryLineWrapper entryLineWrapper;
	}
}
