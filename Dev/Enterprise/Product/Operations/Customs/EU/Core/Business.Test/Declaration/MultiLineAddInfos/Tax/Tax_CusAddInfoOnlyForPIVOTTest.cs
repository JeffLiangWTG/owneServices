using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.MultiLineAddInfos.Testing;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(Tax_CusAddInfoOnlyForPIVOT))]
	class TaxTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCountryCode()
		{
			AssertEquals(Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode, tax.Data.CountryCode);
		}

		public void TestKeyToDeterimeUniqueness()
		{
			var tax = (Tax_CusAddInfoOnlyForPIVOT)GetNewBusinessObject();
			tax.G4_Type = "A00";
			AssertEquals("A00", tax.KeyToDeterimeUniqueness);
		}

		public void TestRateDutyRemovesMopForZeroAndExemptVat()
		{
			tax.Data.G4_MethodOfPayment = "F";
			tax.Data.G4_Type = "A00";
			AssertEquals("F", tax.Data.G4_MethodOfPayment);
			tax.Data.G4_RateDuty = "Z";
			AssertEquals("Setting zero rate for duty does nothing to MOP", "F", tax.Data.G4_MethodOfPayment);

			tax.Data.G4_RateDuty = "";
			tax.Data.G4_Type = "B00";
			tax.Data.G4_MethodOfPayment = "F";
			tax.Data.G4_RateDuty = "Z";
			AssertEquals("Setting zero rate for VAT removes MOP", "", tax.Data.G4_MethodOfPayment);

			tax.Data.G4_MethodOfPayment = "F";
			tax.Data.G4_RateDuty = "E";
			AssertEquals("Setting exempt rate for VAT removes MOP", "", tax.Data.G4_MethodOfPayment);

			tax.Data.G4_RateDuty = "Z";
			tax.Data.G4_Type = "";
			tax.Data.G4_MethodOfPayment = "F";
			tax.Data.G4_Type = "B00";
			AssertEquals("Setting type VAT for rate Z removes MOP", "", tax.Data.G4_MethodOfPayment);

			tax.Data.G4_RateDuty = "E";
			tax.Data.G4_Type = "";
			tax.Data.G4_MethodOfPayment = "F";
			tax.Data.G4_Type = "B00";
			AssertEquals("Setting type VAT for rate E removes MOP", "", tax.Data.G4_MethodOfPayment);
		}

		public void TestRateDutyMaySetOverride()
		{
			tax.Data.G4_RateDuty = "X";
			AssertEquals("", tax.Data.G4_RateOverride);
		}

		public void TestTax_Properties()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			var tax = invoiceLine.Taxes.AddNew().Data;

			tax.G4_Amount = "";
			AssertEquals("0.00", tax.G4_Amount);

			tax.G4_Amount = "0";
			AssertEquals("0.00", tax.G4_Amount);
			tax.G4_Amount = "12.349";
			AssertEquals("12.35", tax.G4_Amount);

			tax.G4_MethodOfPayment = "A";
			AssertEquals("A", tax.G4_MethodOfPayment);

			tax.G4_RateSuspension = "B";
			AssertEquals("B", tax.G4_RateSuspension);

			tax.G4_RateOverride = "ADD";
			AssertEquals("ADD", tax.G4_RateOverride);
		}

		public void TestCurrencyConversion()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			var tax = invLine.Taxes.AddNew();
			tax.JLT_Amount = 139m;
			AssertEquals("Pre-req, rates GBP vs USD is 1.39", 1.39m, invHeader.CurrencyConverter.GetExchangeRate(invHeader.Invoice_Currency));
			AssertEquals("No conversion 0 what we put in is assumed to be in the declaration currency, so it should come out verbatim", 139m, tax.CalcAmountInDeclarationCurrency);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return tax.Data;
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			var product = Factory.New<OrgSupplierPart>();
			pivot = product.PivotsForBinding.AddNew();
			tax = pivot.Taxes.AddNew();
		}
		CusClassPartPivot pivot;
		CusAddInfo<Tax_CusAddInfoOnlyForPIVOT> tax;
		#endregion
	}

	[TestedType(typeof(CusAddInfo<Tax_CusAddInfoOnlyForPIVOT>))]
	class CusAddInfo_Tax_CusAddInfoOnlyForPIVOTTest : CusAddInfoTest<CusAddInfo<Tax_CusAddInfoOnlyForPIVOT>>
	{
		protected override IEnumerable<CusAddInfo<Tax_CusAddInfoOnlyForPIVOT>> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var product = factory.NewWithValidTestData<Customs.Business.OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "11111111";
			yield return pivot.Taxes.AddNew();
		}
	}
}
