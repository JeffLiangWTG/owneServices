using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(TaxStruct))]
	class TaxStructTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAggregatingNumeric()
		{
			var dec = Factory.New<JobDeclaration>();
			var invLine = dec.InvoiceLines.AddNew();
			var tax = invLine.Taxes.AddNew().Data;
			tax.G4_Amount = "6.2";

			var taxStruct = new TaxStruct();
			taxStruct.AddTax(tax);  // now 6.20

			tax.G4_Amount = "1.23";
			taxStruct.AddTax(tax);
			AssertEquals(7.43m, decimal.Parse(taxStruct.G4_Amount));
		}

		public void TestCurrencyConversion()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			var tax = invLine.Taxes.AddNew().Data;

			// These two represent the declaration currency
			tax.G4_Amount = "139";
			tax.G4_BaseAmount = 1390m;

			AssertEquals("Pre-req, rates GBP vs USD is 1.39", 1.39m, invHeader.CurrencyConverter.GetExchangeRate(invHeader.Invoice_Currency));

			var taxStruct = new TaxStruct();
			taxStruct.AddTax(tax);
			// No conversion should occur, what we get out is what we put in:
			AssertEquals(139m, decimal.Parse(taxStruct.G4_Amount_InDeclarationCurrency));
			AssertEquals(1390m, taxStruct.G4_BaseAmount_InDeclarationCurrency);
		}

		public void TestAggregatingBlank()
		{
			var dec = Factory.New<JobDeclaration>();
			var invLine = dec.InvoiceLines.AddNew();
			var tax = invLine.Taxes.AddNew().Data;
			tax.G4_Amount = "";

			var taxStruct = new TaxStruct();
			taxStruct.AddTax(tax);
			taxStruct.AddTax(tax);

			AssertEquals(taxStruct.G4_Amount + " - should be zero", "0.00", taxStruct.G4_Amount);
		}

		public void TestAgregatingNonZeroAndZero()
		{
			var dec = Factory.New<JobDeclaration>();
			var invLine = dec.InvoiceLines.AddNew();
			var tax = invLine.Taxes.AddNew().Data;
			tax.G4_Amount = "6.2";

			var taxStruct = new TaxStruct();
			taxStruct.AddTax(tax);  // now 6.20

			tax.G4_Amount = "0";
			taxStruct.AddTax(tax);
			AssertEquals(6.2m, decimal.Parse(taxStruct.G4_Amount));
		}

		public void TestIDocSADHLineTaxBoxSupporterMembers()
		{
			var tax = Factory.New<JobComInvoiceLineTax>();
			tax.G4_Type = "A00";
			tax.G4_BaseAmount = 122.12m;
			tax.G4_RateDuty = "RDY";
			tax.G4_RateOverride = "OVR";
			tax.G4_Amount = "999.99";
			tax.G4_MethodOfPayment = "A";

			CombineAssertions(() =>
			{
				var taxStruct = new TaxStruct();
				taxStruct.AddTax(tax);
				var taxBoxSupporter = (IDocSADHLineTaxBoxSupporter)taxStruct;
				AssertEquals("AmountInDeclarationCurrency", "999.99", taxBoxSupporter.AmountInDeclarationCurrency);
				AssertEquals("MethodOfPayment", "A", taxBoxSupporter.MethodOfPayment);
				AssertEquals("Rate", "RDY", taxBoxSupporter.Rate);
				AssertEquals("RateDuty", "RDY", taxBoxSupporter.RateDuty);
				AssertEquals("RateOverride", "OVR", taxBoxSupporter.RateOverride);
				AssertEquals("TaxBase", "122.12", taxBoxSupporter.TaxBase);
				AssertEquals("Type", "A00", taxBoxSupporter.Type);
			});
		}
	}
}
