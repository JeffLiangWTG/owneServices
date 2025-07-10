using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	public class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		public void TestShouldResetDataOnMergingCore()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			CusEntryLineFee lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = CusEntryChargeTypeList.Codes.TotalDutyTaxForLine;
			AssertEquals("ShouldResetToZero for TotalDutyTaxForLine", false, lineFee.ShouldResetDataOnMerging);

			lineFee.CF_ChargeType = CusEntryChargeTypeList.Codes.DutyAmount;
			AssertEquals("ShouldResetToZero for DutyAmount", true, lineFee.ShouldResetDataOnMerging);

			lineFee.CF_ChargeType = CusEntryChargeTypeList.Codes.DumpingDuty;
			AssertEquals("ShouldResetToZero for DumpingDuty", false, lineFee.ShouldResetDataOnMerging);

			lineFee.CF_ChargeType = CusEntryChargeTypeList.Codes.GSTDeferred;
			AssertEquals("ShouldResetToZero for GSTDeferred", true, lineFee.ShouldResetDataOnMerging);

			lineFee.CF_ChargeType = CusEntryChargeTypeList.Codes.CountervailingDuty;
			AssertEquals("ShouldResetToZero for CountervailingDuty", false, lineFee.ShouldResetDataOnMerging);

			lineFee.CF_ChargeType = CusEntryChargeTypeList.Codes.SecurityConcession;
			AssertEquals("ShouldResetToZero for SecurityConcession", false, lineFee.ShouldResetDataOnMerging);
		}

		public void TestIsPayableToCustoms()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryLineFee charge = entryLine.Fees.AddNew();

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.DutyAmount;
			AssertEquals("IsPayableToCustomsForLine", true, charge.IsPayableToCustomsForLine);

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.CountervailingDuty;
			AssertEquals("IsPayableToCustomsForLine", true, charge.IsPayableToCustomsForLine);

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.DumpingDuty;
			AssertEquals("IsPayableToCustomsForLine", true, charge.IsPayableToCustomsForLine);

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.WetAmount;
			AssertEquals("IsPayableToCustomsForLine", true, charge.IsPayableToCustomsForLine);

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.GSTAmount;
			AssertEquals("IsPayableToCustomsForLine", true, charge.IsPayableToCustomsForLine);

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.LCTAmount;
			AssertEquals("IsPayableToCustomsForLine", true, charge.IsPayableToCustomsForLine);

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.GSTDeferred;
			AssertEquals("IsPayableToCustomsForLine", false, charge.IsPayableToCustomsForLine);

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.StandardDutyOverriden;
			AssertEquals("IsPayableToCustomsForLine", true, charge.IsPayableToCustomsForLine);
		}

		public void TestIsFeeTypeDeferrable()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryLineFee charge = entryLine.Fees.AddNew();

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.CountervailingDuty;
			AssertEquals("CountervailingDuty IsDeferrable", true, charge.IsDeferrable);
			AssertEquals("CountervailingDuty", true, CusEntryLineFee.IsFeeTypeDeferrable(CusEntryChargeTypeList.Codes.CountervailingDuty));

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.DumpingDuty;
			AssertEquals("DumpingDuty IsDeferrable", true, charge.IsDeferrable);
			AssertEquals("DumpingDuty", true, CusEntryLineFee.IsFeeTypeDeferrable(CusEntryChargeTypeList.Codes.DumpingDuty));

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.DutyAmount;
			AssertEquals("DutyAmount IsDeferrable", true, charge.IsDeferrable);
			AssertEquals("DutyAmount", true, CusEntryLineFee.IsFeeTypeDeferrable(CusEntryChargeTypeList.Codes.DutyAmount));

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.LCTAmount;
			AssertEquals("LCTAmount IsDeferrable", true, charge.IsDeferrable);
			AssertEquals("LCTAmount", true, CusEntryLineFee.IsFeeTypeDeferrable(CusEntryChargeTypeList.Codes.LCTAmount));

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.WetAmount;
			AssertEquals("WetAmount IsDeferrable", true, charge.IsDeferrable);
			AssertEquals("WetAmount", true, CusEntryLineFee.IsFeeTypeDeferrable(CusEntryChargeTypeList.Codes.WetAmount));

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.GSTAmount;
			AssertEquals("GSTAmount IsDeferrable", false, charge.IsDeferrable);
			AssertEquals("GSTAmount", false, CusEntryLineFee.IsFeeTypeDeferrable(CusEntryChargeTypeList.Codes.GSTAmount));

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.GSTDeferred;
			AssertEquals("GSTDeferred IsDeferrable", false, charge.IsDeferrable);
			AssertEquals("GSTDeferred", false, CusEntryLineFee.IsFeeTypeDeferrable(CusEntryChargeTypeList.Codes.GSTDeferred));

			charge.CF_ChargeType = CusEntryChargeTypeList.Codes.StandardDutyOverriden;
			AssertEquals("StandardDutyOverriden IsDeferrable", false, charge.IsDeferrable);
			AssertEquals("StandardDutyOverriden", false, CusEntryLineFee.IsFeeTypeDeferrable(CusEntryChargeTypeList.Codes.StandardDutyOverriden));
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryLineFee to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLineFee)).GetType() == typeof(CusEntryLineFee));
		}
	}
}
