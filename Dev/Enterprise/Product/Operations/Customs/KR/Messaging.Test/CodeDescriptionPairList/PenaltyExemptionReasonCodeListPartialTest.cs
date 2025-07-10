using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	public class PenaltyExemptionReasonCodeListPartialTest : TestCaseWithFactory
	{
		public void TestLegalReasonCode()
		{
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.A1));
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.A2));
			Assert(PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.A3));
			Assert(PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.A4));
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.A5));
			Assert(PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.A6));
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.B1));
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.B2));
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.B3));
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.B4));
			Assert(PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.B5));
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.B6));
			Assert(PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.B7));
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.B8));
			Assert(PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.B9));
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.C1));
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCodeList.Codes.C2));
		}
	}
}
