using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	public class InvoiceChargeTest : EU.Business.Declaration.Testing.InvoiceChargeTest
	{
		public void TestCDSChargeCode()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var invoice = dec.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();
			InvChargeTestHelper.TestCDSChargeCode(charge, x => ((InvoiceCharge)x).CDSChargeCode);
			dec.JE_ApplicationCode = "CHF";
			InvChargeTestHelper.TestCDSChargeCode(charge, x => ((InvoiceCharge)x).CDSChargeCode);
		}

		protected override void SetupAllTestObjects()
		{
			base.SetupAllTestObjects();
			testDec.JE_ApplicationCode = "CDS";
		}

		public override void TestIsIncludedInLinesReadOnly()
		{
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			var aft = invoice.Charges.AddNew();
			aft.J7_ChargeType = "AFT";

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = "OFT";

			var oTH = invoice.Charges.AddNew();
			oTH.J7_ChargeType = "OTH";

			AssertEquals("AFT Included in ITOT should be readonly", true, aft.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("OFT Included in ITOT should be readonly", true, oFT.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("OTH  Included in ITOT should not be readonly", false, oTH.J7_IsIncludedInITOTInfo.ReadOnly);
		}

		public void TestValidation()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertType<InvoiceChargeValidation>(parent.Validation);
		}

		public void TestLookups()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertType<InvoiceChargeLookups>(parent.Lookups);
		}
	}
}
