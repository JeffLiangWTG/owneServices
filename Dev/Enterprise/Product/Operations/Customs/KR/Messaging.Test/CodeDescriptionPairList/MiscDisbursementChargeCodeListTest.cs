namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class MiscDisbursementChargeCodeListTest : NUnit.Framework.TestCase
	{
		public void GetChargeTypeCode()
		{
			AssertEquals(MiscDisbursementChargeCodeList.Codes._1, MiscDisbursementChargeCodeList.GetChargeTypeCode(ChargeTypeList.Codes.DIF));
			AssertEquals(MiscDisbursementChargeCodeList.Codes._2, MiscDisbursementChargeCodeList.GetChargeTypeCode(ChargeTypeList.Codes.PAF));
			AssertEquals(MiscDisbursementChargeCodeList.Codes._3, MiscDisbursementChargeCodeList.GetChargeTypeCode(ChargeTypeList.Codes.TOF));
		}
	}
}
