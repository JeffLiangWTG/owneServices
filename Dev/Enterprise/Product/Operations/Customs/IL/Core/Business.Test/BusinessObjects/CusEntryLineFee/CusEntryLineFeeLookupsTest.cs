using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var entryLineFee = entryLine.Fees.AddNew();
			var chargeTypeList = entryLineFee.Lookups.ChargeTypeList;

			AssertCodeDescriptionPairList(chargeTypeList,
				("ADD", "Other Additional Charges"),
				("COM", "Commission"),
				("DED", "Other Deduction Charges"),
				("DIS", "Discount"),
				("EXW", "Ex-Works Amount"),
				("FIF", "Foreign Inland Freight"),
				("LCH", "Landing Charges (Local Charges)"),
				("OFT", "International Freight"),
				("ONS", "International Insurance"),
				("OTH", "Other Charges"),
				("PAC", "Packing Costs"),
				("15", "VAT"));
			AssertSame(chargeTypeList, entryLineFee.Lookups.ChargeTypeList);
		}

		public void TestRateOverrideReasonList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var entryLineFee = entryLine.Fees.AddNew();
			var rateOverrideReasonList = entryLineFee.Lookups.RateOverrideReasonList;

			AssertCodeDescriptionPairList(rateOverrideReasonList, ("ADD", "Additional"), ("OVR", "Override"), ("PRE", "Pre-calculated"));
			AssertSame(rateOverrideReasonList, entryLineFee.Lookups.RateOverrideReasonList);
		}
	}
}
