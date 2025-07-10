using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.AU.Testing
{
	[TestedType(typeof(EntryChargeTypeList))]
	public class EntryChargeTypeListTest : Enterprise.Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
	{
		public void TestList()
		{
			AssertListElementIsCorrect(ChargeTypeList[0], EntryChargeTypeList.Codes.DutyAmount, EntryChargeTypeList.Descriptions.DutyAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[1], EntryChargeTypeList.Codes.EntryFee, EntryChargeTypeList.Descriptions.EntryFee, true, "");
			AssertListElementIsCorrect(ChargeTypeList[2], EntryChargeTypeList.Codes.GSTAmount, EntryChargeTypeList.Descriptions.GSTAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[3], EntryChargeTypeList.Codes.GSTDeferred, EntryChargeTypeList.Descriptions.GSTDeferred, true, "");
			AssertListElementIsCorrect(ChargeTypeList[4], EntryChargeTypeList.Codes.DutyDeferredAmount, EntryChargeTypeList.Descriptions.DutyDeferredAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[5], EntryChargeTypeList.Codes.InterimAntiDumpingDuty, EntryChargeTypeList.Descriptions.InterimAntiDumpingDuty, true, "");
			AssertListElementIsCorrect(ChargeTypeList[6], EntryChargeTypeList.Codes.InterimCountervailingDuty, EntryChargeTypeList.Descriptions.InterimCountervailingDuty, true, "");
			AssertListElementIsCorrect(ChargeTypeList[7], EntryChargeTypeList.Codes.LCTAmount, EntryChargeTypeList.Descriptions.LCTAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[8], EntryChargeTypeList.Codes.MessageFee, EntryChargeTypeList.Descriptions.MessageFee, true, "");
			AssertListElementIsCorrect(ChargeTypeList[9], EntryChargeTypeList.Codes.ScreenFree, EntryChargeTypeList.Descriptions.ScreenFree, true, "");
			AssertListElementIsCorrect(ChargeTypeList[10], EntryChargeTypeList.Codes.TradegateGST, EntryChargeTypeList.Descriptions.TradegateGST, true, "");
			AssertListElementIsCorrect(ChargeTypeList[11], EntryChargeTypeList.Codes.WetAmount, EntryChargeTypeList.Descriptions.WetAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[12], EntryChargeTypeList.Codes.Woodlevy, EntryChargeTypeList.Descriptions.Woodlevy, true, "");
			AssertListElementIsCorrect(ChargeTypeList[13], EntryChargeTypeList.Codes.OtherCharges, EntryChargeTypeList.Descriptions.OtherCharges, true, "");
			AssertListElementIsCorrect(ChargeTypeList[14], EntryChargeTypeList.Codes.FlatDutyPortion, EntryChargeTypeList.Descriptions.FlatDutyPortion, false, "");

			AssertListElementIsCorrect(ChargeTypeList[15], EntryChargeTypeList.Codes.CountervailingSecurityAmount, EntryChargeTypeList.Descriptions.CountervailingSecurityAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[16], EntryChargeTypeList.Codes.DumpingSecurityAmount, EntryChargeTypeList.Descriptions.DumpingSecurityAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[17], EntryChargeTypeList.Codes.CountervailingDuty, EntryChargeTypeList.Descriptions.CountervailingDuty, true, "");
			AssertListElementIsCorrect(ChargeTypeList[18], EntryChargeTypeList.Codes.DumpingDuty, EntryChargeTypeList.Descriptions.DumpingDuty, true, "");
			AssertListElementIsCorrect(ChargeTypeList[19], EntryChargeTypeList.Codes.DutyOverride, EntryChargeTypeList.Descriptions.DutyOverride, true, "");
			AssertListElementIsCorrect(ChargeTypeList[20], EntryChargeTypeList.Codes.AQISServicePaymentAmount, EntryChargeTypeList.Descriptions.AQISServicePaymentAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[21], EntryChargeTypeList.Codes.AQISProcessingCharge, EntryChargeTypeList.Descriptions.AQISProcessingCharge, true, "");
			AssertListElementIsCorrect(ChargeTypeList[22], EntryChargeTypeList.Codes.DeclarationProcessingCharge, EntryChargeTypeList.Descriptions.DeclarationProcessingCharge, true, "");
			AssertListElementIsCorrect(ChargeTypeList[23], EntryChargeTypeList.Codes.TotalPayableAdmin, EntryChargeTypeList.Descriptions.TotalPayableAdmin, true, "");
			AssertListElementIsCorrect(ChargeTypeList[24], EntryChargeTypeList.Codes.AQISContainerCharges, EntryChargeTypeList.Descriptions.AQISContainerCharges, true, "");
			AssertListElementIsCorrect(ChargeTypeList[25], EntryChargeTypeList.Codes.StandardDutyOverriden, EntryChargeTypeList.Descriptions.StandardDutyOverriden, true, "");

			AssertListElementIsCorrect(ChargeTypeList[26], EntryChargeTypeList.Codes.TotalDutyTaxForLine, EntryChargeTypeList.Descriptions.TotalDutyTaxForLine, false, "");
			AssertListElementIsCorrect(ChargeTypeList[27], EntryChargeTypeList.Codes.InterimDumpingDuty, EntryChargeTypeList.Descriptions.InterimDumpingDuty, true, "");
			AssertListElementIsCorrect(ChargeTypeList[28], EntryChargeTypeList.Codes.SecurityConcession, EntryChargeTypeList.Descriptions.SecurityConcession, false, "");
			AssertListElementIsCorrect(ChargeTypeList[29], EntryChargeTypeList.Codes.SecurityLiability, EntryChargeTypeList.Descriptions.SecurityLiability, false, "");
		}

		public void TestRemoveAllAction()
		{
			var allCodes = new EntryChargeTypeList();
			AssertEquals("Code exists", EntryChargeTypeList.Descriptions.AQISServicePaymentAmount, allCodes.GetDescriptionFromCode(EntryChargeTypeList.Codes.AQISServicePaymentAmount));

			allCodes.RemoveWhere(c => c.Code == EntryChargeTypeList.Codes.AQISServicePaymentAmount);
			AssertEquals("Code was removed", null, allCodes.GetDescriptionFromCode(EntryChargeTypeList.Codes.AQISServicePaymentAmount));
		}

		protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetNewEntryChargeTypeList()
		{
			return new EntryChargeTypeList();
		}

		protected override ZString CountryCode => Core.Constants.CountryCodes.Australia;
	}
}
