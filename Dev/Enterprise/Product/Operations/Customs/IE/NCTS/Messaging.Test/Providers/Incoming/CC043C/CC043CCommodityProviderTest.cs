using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043CCommodityProvider))]
	sealed class CC043CCommodityProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("commodity missing", () => new CC043CCommodityProvider(null));
			});
		}

		public void TestDescriptionOfGoods()
		{
			AssertEquals("Goods Description:=", "Test item", provider.DescriptionOfGoods);
		}

		public void TestCusCode()
		{
			AssertEquals("Cua Code", "1234", provider.CusCode);
		}

		public void TestGoodsMeasure()
		{
			AssertType<CC043CGoodsMeasureProvider>(provider.GoodsMeasure);
		}

		public void TestGoodsMeasure_Null()
		{
			var goodsMeasureNullProvider = new CC043CCommodityProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.CommodityType08());
			AssertNull(goodsMeasureNullProvider.GoodsMeasure);
		}

		public void TestCommodityCode()
		{
			AssertType<CC043CCommodityCodeProvider>(provider.CommodityCode);
		}

		public void TestCommodityCode_Null()
		{
			var commodityCodeCodeNullProvider = new CC043CCommodityProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.CommodityType08());
			AssertNull(commodityCodeCodeNullProvider.CommodityCode);
		}

		protected override void SetUp()
		{
			provider = new CC043CCommodityProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.CommodityType08()
			{
				DescriptionOfGoods = "Test item",
				CusCode = "1234",
				CommodityCode = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.CommodityCodeType05()
				{
					CombinedNomenclatureCode = "123",
					HarmonizedSystemSubHeadingCode = "ABC",
				},
				GoodsMeasure = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.GoodsMeasureType03()
				{
					GrossMass = 12m,
					NetMass = 2.5m,
				}
			});
		}
		CC043CCommodityProvider provider;
	}
}
