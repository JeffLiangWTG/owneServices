using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043CCommodityCodeProvider))]
	sealed class CC043CCommodityCodeProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("commodityCode missing", () => new CC043CCommodityCodeProvider(null));
			});
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			AssertEquals("Harmonized System Sub Heading Code", "ABC999", provider.HarmonizedSystemSubHeadingCode);
		}
		public void TestCombinedNomenclatureCode()
		{
			AssertEquals("Combined Nomenclature Code", "TEST123", provider.CombinedNomenclatureCode);
		}

		protected override void SetUp()
		{
			provider = new CC043CCommodityCodeProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.CommodityCodeType05()
			{
				CombinedNomenclatureCode = "TEST123",
				HarmonizedSystemSubHeadingCode = "ABC999",
			});
		}

		CC043CCommodityCodeProvider provider;
	}
}
