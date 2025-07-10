using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class JobComInvoiceLineTaxLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRateSuspensionList()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var tax = invLine.Taxes.AddNew().Data;

			foreach (ICodeDescription pair in new CodeDescriptionPairLists.TaxRateCustomsSuspensionListImport())
			{
				Assert(tax.Lookups.RateSuspensionList.Contains(pair));
			}
		}

		public void TestRateOverrideList()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var tax = invLine.Taxes.AddNew().Data;

			foreach (ICodeDescription pair in new CodeDescriptionPairLists.TaxRateAntiDumpingOverrideListImport())
			{
				Assert(tax.Lookups.RateOverrideList.Contains(pair));
			}

			foreach (ICodeDescription pair in new CodeDescriptionPairLists.TaxRateCAPOverrideListImport())
			{
				Assert(tax.Lookups.RateOverrideList.Contains(pair));
			}

			foreach (ICodeDescription pair in new CodeDescriptionPairLists.TaxRateCustomsOverrideListImport())
			{
				Assert(tax.Lookups.RateOverrideList.Contains(pair));
			}

			foreach (ICodeDescription pair in new CodeDescriptionPairLists.TaxRateExciseOverrideListImport())
			{
				Assert(tax.Lookups.RateOverrideList.Contains(pair));
			}

			foreach (ICodeDescription pair in new CodeDescriptionPairLists.TaxRateVATOverrideListImport())
			{
				Assert(tax.Lookups.RateOverrideList.Contains(pair));
			}
		}

		public void TestTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var tax = invLine.Taxes.AddNew().Data;

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Contains B00 - VAT", tax.Lookups.TypeList.ContainsCode(Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat));

			tax = Factory.New<JobComInvoiceLineTax>();
			AssertNull((tax as IEuTax).ImportExportParent);
			AssertEquals(0, tax.Lookups.TypeList.Count);
		}
	}
}
