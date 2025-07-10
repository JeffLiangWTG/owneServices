using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	sealed class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public override void TestIsIncludedInLinesReadOnly()
		{
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			BaseInvoiceCharge aDD = invoice.Charges.AddNew();
			aDD.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;

			BaseInvoiceCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;

			BaseInvoiceCharge oTH = invoice.Charges.AddNew();
			oTH.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;

			AssertEquals("ADD Included in ITOT should not be readonly", false, aDD.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("OFT Included in ITOT should be readonly", true, oFT.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("OTH Included in ITOT should not be readonly", false, oTH.J7_IsIncludedInITOTInfo.ReadOnly);
		}
	}
}
