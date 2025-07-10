using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public static class InvChargeTestHelper
	{
		public static void TestCDSChargeCode(BaseJobComInvHeaderCharge charge, Func<BaseJobComInvHeaderCharge, ZString> cdsChargeCodeGetter)
		{
			Assertion.CombineAssertions(() =>
			{
				AssertCDSChargeCode("AB", charge, "CBR", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AB", charge, "CBR", false, 1.1m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AD", charge, "CPA", false, 1.1m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AE", charge, "MCP", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AF", charge, "TDM", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AF", charge, "TDM", false, 1.1m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AG", charge, "MAC", false, 1.1m, string.Empty, cdsChargeCodeGetter);

				AssertCDSChargeCode("AH", charge, "EDA", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AH", charge, "EDA", false, 1.1m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AI", charge, "RLF", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AI", charge, "RLF", false, 1.1m, string.Empty, cdsChargeCodeGetter);

				AssertCDSChargeCode("AJ", charge, "PSR", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AK", charge, "ONS", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AL", charge, "IPO", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AN", charge, "AA7", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AN", charge, "AA7", false, 1.1m, string.Empty, cdsChargeCodeGetter);

				AssertCDSChargeCode("AP", charge, "OFT", true, 0m, "VAL", cdsChargeCodeGetter);
				AssertCDSChargeCode("AQ", charge, "OFT", true, 0m, "WGT", cdsChargeCodeGetter);
				AssertCDSChargeCode("AP", charge, "OFT", false, 0m, "VAL", cdsChargeCodeGetter);
				AssertCDSChargeCode("AQ", charge, "OFT", false, 0m, "WGT", cdsChargeCodeGetter);

				AssertCDSChargeCode("AT", charge, "ADD", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("AV", charge, "ADV", true, 0m, "VAL", cdsChargeCodeGetter);
				AssertCDSChargeCode("AW", charge, "ADV", true, 0m, "WGT", cdsChargeCodeGetter);

				AssertCDSChargeCode("AR", charge, "AFT", true, 0m, "VAL", cdsChargeCodeGetter);
				AssertCDSChargeCode("AS", charge, "AFT", true, 0m, "WGT", cdsChargeCodeGetter);
				AssertCDSChargeCode("AR", charge, "AFT", false, 0m, "VAL", cdsChargeCodeGetter);
				AssertCDSChargeCode("AS", charge, "AFT", false, 0m, "WGT", cdsChargeCodeGetter);

				AssertCDSChargeCode("BB", charge, "CEA", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("BD", charge, "INT", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("BD", charge, "INT", false, 1.1m, string.Empty, cdsChargeCodeGetter);

				AssertCDSChargeCode("BE", charge, "CRR", false, 0m, string.Empty, cdsChargeCodeGetter);

				AssertCDSChargeCode("BM", charge, "BCM", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("BM", charge, "BCM", false, 1.1m, string.Empty, cdsChargeCodeGetter);

				AssertCDSChargeCode("BG", charge, "DE7", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("BG", charge, "DE7", false, 1.1m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("BH", charge, "DIS", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("BH", charge, "DIS", false, 1.1m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode("BT", charge, "DED", false, 0m, string.Empty, cdsChargeCodeGetter);

				AssertCDSChargeCode(string.Empty, charge, "&12", false, 0m, string.Empty, cdsChargeCodeGetter);
				AssertCDSChargeCode(string.Empty, charge, string.Empty, false, 0m, string.Empty, cdsChargeCodeGetter);
			});
		}

		public static void SetUp(BaseJobComInvHeaderCharge charge, ZString chargeType, bool isDutiable, ZDecimal percentage, ZString distributeBy)
		{
			charge.J7_ChargeType = chargeType;
			charge.J7_IsDutiable = isDutiable;
			charge.J7_Percentage = percentage;
			charge.J7_DistributeBy = distributeBy;
		}

		static void AssertCDSChargeCode(ZString expectedCDSChargeCode
			, BaseJobComInvHeaderCharge charge
			, ZString chargeType, bool isDutiable, ZDecimal percentage, ZString distributeBy
			, Func<BaseJobComInvHeaderCharge, ZString> cdsChargeCodeGetter)
		{
			SetUp(charge, chargeType, isDutiable, percentage, distributeBy);
			if (charge.Parent.JobDeclaration.JE_ApplicationCode == "CHF")
			{
				Assertion.AssertEquals($"CHF.{chargeType}.{isDutiable}.{percentage}.{distributeBy}", ZString.Empty, cdsChargeCodeGetter.Invoke(charge));
			}
			else
			{
				Assertion.AssertEquals($"CDS.{chargeType}.{isDutiable}.{percentage}.{distributeBy}", expectedCDSChargeCode, cdsChargeCodeGetter.Invoke(charge));
			}
		}

		public static void SetUpCharge(BaseJobComInvHeaderCharge charge, ZString chargeType, bool isDutiable, ZDecimal percentage,
			ZString distributeBy, bool isIncludeInvoiceLine, ZDecimal amount)
		{
			InvChargeTestHelper.SetUp(charge, chargeType, isDutiable, percentage, distributeBy);
			charge.J7_IsIncludedInITOT = isIncludeInvoiceLine;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			charge.J7_Amount = amount;
		}
	}
}
