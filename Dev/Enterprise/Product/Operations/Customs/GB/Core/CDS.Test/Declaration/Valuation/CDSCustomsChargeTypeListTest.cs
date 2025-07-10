using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSCustomsChargeTypeListTest : TestCaseWithFactory
	{
		public void TestMapToCDSAdditionDeductionChargeType()
		{
			AssertEquals("AB", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("CBR", false, string.Empty, false));
			AssertEquals("AD", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("CPA", false, string.Empty, false));
			AssertEquals("AE", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("MCP", false, string.Empty, false));
			AssertEquals("AF", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("TDM", false, string.Empty, false));
			AssertEquals("AF", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("TDM", false, string.Empty, false));
			AssertEquals("AG", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("MAC", false, string.Empty, false));

			AssertEquals("AH", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("EDA", false, string.Empty, false));
			AssertEquals("AI", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("RLF", false, string.Empty, false));

			AssertEquals("AJ", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("PSR", false, string.Empty, false));
			AssertEquals("AK", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("ONS", false, string.Empty, false));
			AssertEquals("AL", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("IPO", false, string.Empty, false));
			AssertEquals("AN", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("AA7", false, string.Empty, false));

			AssertEquals("AT", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("ADD", false, string.Empty, false));
			AssertEquals("AV", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("ADV", true, "VAL", false));
			AssertEquals("AW", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("ADV", true, "VOL", false));

			AssertEquals("BB", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("CEA", false, string.Empty, false));
			AssertEquals("BD", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("INT", false, string.Empty, false));

			AssertEquals("BE", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("CRR", false, string.Empty, false));

			AssertEquals("BM", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("BCM", false, string.Empty, false));

			AssertEquals("BG", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("DE7", false, string.Empty, false));
			AssertEquals("BH", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("DIS", false, string.Empty, false));
			AssertEquals("BT", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("DED", false, string.Empty, false));

			AssertEquals(string.Empty, CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("&12", false, string.Empty, false));
			AssertEquals(string.Empty, CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType(string.Empty, false, string.Empty, false));
		}

		public void TestAirTransportCostsCharges()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AR", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("AFT", true, "VAL", false));
				AssertEquals("AS", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("AFT", true, "WGT", false));
				AssertEquals("AR", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("AFT", false, "VAL", false));
				AssertEquals("AS", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("AFT", false, "WGT", false));

				AssertEquals("BR", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("AFT", true, "VAL", true));
				AssertEquals("BS", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("AFT", true, "WGT", true));
				AssertEquals("BR", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("AFT", false, "VAL", true));
				AssertEquals("BS", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("AFT", false, "WGT", true));
			});
		}

		public void TestTransportCostsCharges()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AP", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("OFT", true, "VAL", false));
				AssertEquals("AQ", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("OFT", true, "WGT", false));
				AssertEquals("AP", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("OFT", false, "VAL", false));
				AssertEquals("AQ", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("OFT", false, "WGT", false));

				AssertEquals("BA", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("OFT", true, "VAL", true));
				AssertEquals("BU", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("OFT", true, "WGT", true));
				AssertEquals("BA", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("OFT", false, "VAL", true));
				AssertEquals("BU", CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType("OFT", false, "WGT", true));
			});
		}
	}
}
