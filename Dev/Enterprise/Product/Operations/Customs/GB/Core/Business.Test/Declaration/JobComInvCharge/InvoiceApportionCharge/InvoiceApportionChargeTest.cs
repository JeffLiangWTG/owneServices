using Enterprise.Customs.GB.CDS;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceApportionCharge))]
	public class InvoiceApportionChargeTest : EU.Business.Declaration.Testing.InvoiceApportionChargeTest
	{
		public void TestCDSChargeCode()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var invoice = dec.Invoices.AddNew();
			var charge = invoice.GroupCharges.AddNew();
			InvChargeTestHelper.TestCDSChargeCode(charge, x => ((InvoiceApportionCharge)x).CDSChargeCode);
			dec.JE_ApplicationCode = "CHF";
			InvChargeTestHelper.TestCDSChargeCode(charge, x => ((InvoiceApportionCharge)x).CDSChargeCode);
		}

		protected override (Customs.Business.BaseJobDeclaration, string) GetDeclarationAndChargeCodeForTest()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			// The ParentTypes of CHF are all GroupInvoice, cannot add Invoice Charge for now
			return (dec, CDSCustomsChargeTypeList.Codes.ContainersAndPackingCharge);
		}

		public void TestReApportionWhenInvoiceChargeAdded(string applicationCode)
		{
			var decAndCode = GetDeclarationAndChargeCodeForTest();
			var testDec = decAndCode.Item1;
			var chargeCode = decAndCode.Item2;
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			PrepareCharge(groupHeader.Charges.AddNew(chargeCode, 100, testDec.LocalCurrencyCode));
			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			testDec.JE_ApplicationCode = applicationCode;

			testDec.ResumeApportionment();

			AssertEquals("PreCondition: one apportioned charge", 1, invoice.GroupCharges.Count);
			AssertEquals("Apportioned Charge", 100m, invoice.GroupCharges[0].J7_Amount);
		}

		public void TestApportionmentDoesNotAddBackTheMissingPenny()
		{
			var decAndCode = GetDeclarationAndChargeCodeForTest();
			var testDec = decAndCode.Item1;
			var chargeCode = decAndCode.Item2;
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			PrepareCharge(groupHeader.Charges.AddNew(chargeCode, 1000, testDec.LocalCurrencyCode));
			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceAmount = 100;
			invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceAmount = 200;  // ratio 2:1, split the 1000 charge in ratio  666.666666:333.33333333
			invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			testDec.ResumeApportionment();

			AssertEquals("1 apportioned charge", 1, invoice1.GroupCharges.Count);
			AssertEquals("1 apportioned charge", 1, invoice2.GroupCharges.Count);
			AssertEquals(333.33m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("The apportioned amount is 66.66, not 666.67, because the rounding error of 0.01 is NOT added back",
							666.66m, invoice2.GroupCharges[0].J7_Amount);
		}

		public void TestValidation()
		{
			var parent = Factory.New<InvoiceApportionCharge>();
			AssertType<InvoiceApportionChargeValidation>(parent.Validation);
		}

		public void TestLookups()
		{
			var parent = Factory.New<InvoiceApportionCharge>();
			AssertType<InvoiceApportionChargeLookups>(parent.Lookups);
		}
	}
}
