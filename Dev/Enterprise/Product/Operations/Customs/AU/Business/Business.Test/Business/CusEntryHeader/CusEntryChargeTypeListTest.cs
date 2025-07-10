using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryChargeTypeList))]
	public class CusEntryChargeTypeListTest : Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
	{
		public void TestList()
		{
			AssertListElementIsCorrect(ChargeTypeList[0], CusEntryChargeTypeList.Codes.DutyAmount, CusEntryChargeTypeList.Descriptions.DutyAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[1], CusEntryChargeTypeList.Codes.EntryFee, CusEntryChargeTypeList.Descriptions.EntryFee, true, "");
			AssertListElementIsCorrect(ChargeTypeList[2], CusEntryChargeTypeList.Codes.GSTAmount, CusEntryChargeTypeList.Descriptions.GSTAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[3], CusEntryChargeTypeList.Codes.GSTDeferred, CusEntryChargeTypeList.Descriptions.GSTDeferred, true, "");
			AssertListElementIsCorrect(ChargeTypeList[4], CusEntryChargeTypeList.Codes.DutyDeferredAmount, CusEntryChargeTypeList.Descriptions.DutyDeferredAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[5], CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty, CusEntryChargeTypeList.Descriptions.InterimAntiDumpingDuty, true, "");
			AssertListElementIsCorrect(ChargeTypeList[6], CusEntryChargeTypeList.Codes.InterimCountervailingDuty, CusEntryChargeTypeList.Descriptions.InterimCountervailingDuty, true, "");
			AssertListElementIsCorrect(ChargeTypeList[7], CusEntryChargeTypeList.Codes.LCTAmount, CusEntryChargeTypeList.Descriptions.LCTAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[8], CusEntryChargeTypeList.Codes.MessageFee, CusEntryChargeTypeList.Descriptions.MessageFee, true, "");
			AssertListElementIsCorrect(ChargeTypeList[9], CusEntryChargeTypeList.Codes.ScreenFree, CusEntryChargeTypeList.Descriptions.ScreenFree, true, "");
			AssertListElementIsCorrect(ChargeTypeList[10], CusEntryChargeTypeList.Codes.TradegateGST, CusEntryChargeTypeList.Descriptions.TradegateGST, true, "");
			AssertListElementIsCorrect(ChargeTypeList[11], CusEntryChargeTypeList.Codes.WetAmount, CusEntryChargeTypeList.Descriptions.WetAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[12], CusEntryChargeTypeList.Codes.Woodlevy, CusEntryChargeTypeList.Descriptions.Woodlevy, true, "");
			AssertListElementIsCorrect(ChargeTypeList[13], CusEntryChargeTypeList.Codes.OtherCharges, CusEntryChargeTypeList.Descriptions.OtherCharges, true, "");
			AssertListElementIsCorrect(ChargeTypeList[14], CusEntryChargeTypeList.Codes.FlatDutyPortion, CusEntryChargeTypeList.Descriptions.FlatDutyPortion, false, "");
			AssertListElementIsCorrect(ChargeTypeList[15], CusEntryChargeTypeList.Codes.CountervailingSecurityAmount, CusEntryChargeTypeList.Descriptions.CountervailingSecurityAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[16], CusEntryChargeTypeList.Codes.DumpingSecurityAmount, CusEntryChargeTypeList.Descriptions.DumpingSecurityAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[17], CusEntryChargeTypeList.Codes.CountervailingDuty, CusEntryChargeTypeList.Descriptions.CountervailingDuty, true, "");
			AssertListElementIsCorrect(ChargeTypeList[18], CusEntryChargeTypeList.Codes.DumpingDuty, CusEntryChargeTypeList.Descriptions.DumpingDuty, true, "");
			AssertListElementIsCorrect(ChargeTypeList[19], CusEntryChargeTypeList.Codes.DutyOverride, CusEntryChargeTypeList.Descriptions.DutyOverride, true, "");
			AssertListElementIsCorrect(ChargeTypeList[20], CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, CusEntryChargeTypeList.Descriptions.AQISServicePaymentAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[21], CusEntryChargeTypeList.Codes.AQISProcessingCharge, CusEntryChargeTypeList.Descriptions.AQISProcessingCharge, true, "");
			AssertListElementIsCorrect(ChargeTypeList[22], CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, CusEntryChargeTypeList.Descriptions.DeclarationProcessingCharge, true, "");
			AssertListElementIsCorrect(ChargeTypeList[23], CusEntryChargeTypeList.Codes.TotalPayableAdmin, CusEntryChargeTypeList.Descriptions.TotalPayableAdmin, true, "");
			AssertListElementIsCorrect(ChargeTypeList[24], CusEntryChargeTypeList.Codes.AQISContainerCharges, CusEntryChargeTypeList.Descriptions.AQISContainerCharges, true, "");
			AssertListElementIsCorrect(ChargeTypeList[25], CusEntryChargeTypeList.Codes.StandardDutyOverriden, CusEntryChargeTypeList.Descriptions.StandardDutyOverriden, true, "");
			AssertListElementIsCorrect(ChargeTypeList[26], CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, CusEntryChargeTypeList.Descriptions.TotalDutyTaxForLine, false, "");
			AssertListElementIsCorrect(ChargeTypeList[27], CusEntryChargeTypeList.Codes.InterimDumpingDuty, CusEntryChargeTypeList.Descriptions.InterimDumpingDuty, true, "");
			AssertListElementIsCorrect(ChargeTypeList[28], CusEntryChargeTypeList.Codes.SecurityConcession, CusEntryChargeTypeList.Descriptions.SecurityConcession, false, "");
			AssertListElementIsCorrect(ChargeTypeList[29], CusEntryChargeTypeList.Codes.SecurityLiability, CusEntryChargeTypeList.Descriptions.SecurityLiability, false, "");
		}

		protected override Registry.Business.Customs.EntryChargeTypeList GetNewEntryChargeTypeList()
		{
			return new CusEntryChargeTypeList();
		}

		protected override CargoWise.Types.ZString CountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}
	}
}
