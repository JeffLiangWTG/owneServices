using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CustomsChargesTest : CusEntryHeaderCustomsChargesTest
	{
		public override void AddCountrySpecificCharges(Customs.Business.CusEntryHeader entryHeader)
		{
			CusEntryHeader header = entryHeader as CusEntryHeader;
			CusEntryLine line = header.MergedLines.AddNew();

			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 101m);
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTAmount, 102m);
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 103m);
			line.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 104m);

			header.Charges.AddNew(CusEntryChargeTypeList.Codes.Woodlevy, 105m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.EntryFee, 106m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.MessageFee, 107m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.TradegateGST, 108m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.ScreenFree, 109m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.OtherCharges, 110m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.GSTDeferred, 111m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty, 112m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.InterimCountervailingDuty, 113m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.FlatDutyPortion, 114m);

			header.Charges.AddNew(CusEntryChargeTypeList.Codes.CountervailingSecurityAmount, 115m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.DumpingSecurityAmount, 116m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.CountervailingDuty, 117m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.DumpingDuty, 118m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.DutyOverride, 119m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, 120m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 121m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 122m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.TotalPayableAdmin, 123m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISContainerCharges, 124m);

			header.Charges.AddNew(CusEntryChargeTypeList.Codes.StandardDutyOverriden, 125m);
			header.Charges.AddNew(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 127m);
		}

		public override void AssertCustomsCharges(CustomsCharge[] charges)
		{
			AssertEquals("There should be 24 customs charges", 24, charges.Length);

			AssertCustomsCharge(charges[0], 101m, CusEntryChargeTypeList.Descriptions.DutyAmount);
			AssertCustomsCharge(charges[1], 102m, CusEntryChargeTypeList.Descriptions.GSTAmount);
			AssertCustomsCharge(charges[2], 103m, CusEntryChargeTypeList.Descriptions.LCTAmount);
			AssertCustomsCharge(charges[3], 104m, CusEntryChargeTypeList.Descriptions.WetAmount);

			AssertCustomsCharge(charges[4], 105m, CusEntryChargeTypeList.Descriptions.Woodlevy);
			AssertCustomsCharge(charges[5], 106m, CusEntryChargeTypeList.Descriptions.EntryFee);
			AssertCustomsCharge(charges[6], 107m, CusEntryChargeTypeList.Descriptions.MessageFee);
			AssertCustomsCharge(charges[7], 108m, CusEntryChargeTypeList.Descriptions.TradegateGST);
			AssertCustomsCharge(charges[8], 109m, CusEntryChargeTypeList.Descriptions.ScreenFree);
			AssertCustomsCharge(charges[9], 110m, CusEntryChargeTypeList.Descriptions.OtherCharges);
			AssertCustomsCharge(charges[10], 111m, CusEntryChargeTypeList.Descriptions.GSTDeferred);
			AssertCustomsCharge(charges[11], 112m, CusEntryChargeTypeList.Descriptions.InterimAntiDumpingDuty);
			AssertCustomsCharge(charges[12], 113m, CusEntryChargeTypeList.Descriptions.InterimCountervailingDuty);

			AssertCustomsCharge(charges[13], 115m, CusEntryChargeTypeList.Descriptions.CountervailingSecurityAmount);
			AssertCustomsCharge(charges[14], 116m, CusEntryChargeTypeList.Descriptions.DumpingSecurityAmount);
			AssertCustomsCharge(charges[15], 117m, CusEntryChargeTypeList.Descriptions.CountervailingDuty);
			AssertCustomsCharge(charges[16], 118m, CusEntryChargeTypeList.Descriptions.DumpingDuty);
			AssertCustomsCharge(charges[17], 119m, CusEntryChargeTypeList.Descriptions.DutyOverride);
			AssertCustomsCharge(charges[18], 120m, CusEntryChargeTypeList.Descriptions.AQISServicePaymentAmount);
			AssertCustomsCharge(charges[19], 121m, CusEntryChargeTypeList.Descriptions.AQISProcessingCharge);
			AssertCustomsCharge(charges[20], 122m, CusEntryChargeTypeList.Descriptions.DeclarationProcessingCharge);
			AssertCustomsCharge(charges[21], 123m, CusEntryChargeTypeList.Descriptions.TotalPayableAdmin);
			AssertCustomsCharge(charges[22], 124m, CusEntryChargeTypeList.Descriptions.AQISContainerCharges);
			AssertCustomsCharge(charges[23], 125m, CusEntryChargeTypeList.Descriptions.StandardDutyOverriden);
		}

		public override Customs.Business.CusEntryHeader GetEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var header = declaration.CustomsEntryHeaders.AddNew();
			return header;
		}
	}
}
