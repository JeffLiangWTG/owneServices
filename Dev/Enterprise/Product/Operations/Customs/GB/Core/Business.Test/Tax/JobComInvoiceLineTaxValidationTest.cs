using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class JobComInvoiceLineTaxValidationTest : TestCaseWithFactory
	{
		public void TestAllowModeOfPaymentCombinations()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var taxLine1 = invoiceLine.Taxes.AddNew().Data;
			taxLine1.G4_Type = "A00";
			taxLine1.G4_MethodOfPayment = "F";
			AssertNoMessageErrorContaining(taxLine1.G4_MethodOfPaymentInfo, "Invalid combination");
			var taxLine2 = invoiceLine.Taxes.AddNew().Data;
			taxLine2.G4_Type = "A00";
			taxLine2.G4_MethodOfPayment = "A";
			AssertNoMessageErrorContaining(taxLine2.G4_MethodOfPaymentInfo, "Invalid combination");
			var taxLine3 = invoiceLine.Taxes.AddNew().Data;
			taxLine3.G4_Type = "A00";
			taxLine3.G4_MethodOfPayment = "N";
			AssertNoMessageErrorContaining(taxLine3.G4_MethodOfPaymentInfo, "Invalid combination");
			taxLine3.G4_MethodOfPayment = "P";
			AssertHasMessageErrorContaining(taxLine3.G4_MethodOfPaymentInfo, "Invalid combination");

			taxLine1.G4_MethodOfPayment = "D";
			taxLine2.G4_MethodOfPayment = "F";
			taxLine3.G4_MethodOfPayment = "P";
			AssertNoMessageErrorContaining(taxLine3.G4_MethodOfPaymentInfo, "Invalid combination");
			taxLine3.G4_MethodOfPayment = "";
			AssertNoMessageErrorContaining(taxLine3.G4_MethodOfPaymentInfo, "Invalid combination");
			taxLine3.G4_MethodOfPayment = "N";
			AssertHasMessageErrorContaining(taxLine3.G4_MethodOfPaymentInfo, "Invalid combination");

			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			taxLine2.G4_MethodOfPayment = string.Empty;
			AssertNoMessageErrorContaining(taxLine2.G4_MethodOfPaymentInfo, "Invalid combination");
			taxLine2.G4_MethodOfPayment = "F";
			AssertHasMessageErrorContaining(taxLine2.G4_MethodOfPaymentInfo, "Invalid combination");
		}

		public void TestAllowModeOfPaymentCombinations_G()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var taxLine1 = invoiceLine.Taxes.AddNew().Data;
			taxLine1.G4_Type = "A00";
			taxLine1.G4_MethodOfPayment = "G";
			AssertNoMessageErrorContaining(taxLine1.G4_MethodOfPaymentInfo, "must be the only");
			var taxLine2 = invoiceLine.Taxes.AddNew().Data;
			taxLine2.G4_Type = "A00";
			taxLine2.G4_MethodOfPayment = "G";
			AssertHasMessageErrorContaining(taxLine2.G4_MethodOfPaymentInfo, "must be the only");

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var taxLine3 = invoiceLine2.Taxes.AddNew().Data;
			taxLine3.G4_Type = "A00";
			taxLine3.G4_MethodOfPayment = "F";
			AssertNoMessageErrorContaining(taxLine3.G4_MethodOfPaymentInfo, "must be the only");
			var taxLine4 = invoiceLine.Taxes.AddNew().Data;
			taxLine4.G4_Type = "A00";
			taxLine4.G4_MethodOfPayment = "G";
			AssertHasMessageErrorContaining(taxLine4.G4_MethodOfPaymentInfo, "must be the only");
		}

		public void TestCheckG4_RateSuspension()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var tax = invLine.Taxes.AddNew().Data;
			tax.G4_RateSuspension = "V";
			AssertHasMessageError(tax.G4_RateSuspensionInfo, "The code you have selected is not in the list.");
			tax.G4_RateSuspension = TaxRateCustomsSuspensionListImport.Codes.GoodsSubjectToSuspensionWithAnAirworthinessCertificate;
			AssertNoMessageError(tax.G4_RateSuspensionInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckG4_RateOverride()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var tax = invLine.Taxes.AddNew().Data;
			tax.G4_RateOverride = "VWG";
			AssertHasMessageError(tax.G4_RateOverrideInfo, "The code you have selected is not in the list.");
			tax.G4_RateOverride = TaxRateVATOverrideListImport.Codes.ToClaimExemptionFromPaymentOfVat;
			AssertNoMessageError(tax.G4_RateOverrideInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckG4_RateDuty_ListValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var inv = declaration.Invoices.AddNew();
			var invoiceLine = inv.InvoiceLines.AddNew();
			var taxLine = invoiceLine.Taxes.AddNew().Data;

			taxLine.G4_Type = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			taxLine.G4_RateDuty = TaxRateVATDutyListImport.Codes.VATTheGoodsAreLiableToVATAtTheStandardRate; // S for A00 is bad
			AssertHasMessageErrorContaining(taxLine.G4_RateDutyInfo, "list");
			taxLine.G4_RateDuty = TaxRateCustomsDutyListImport.Codes.DutyTheGoodsAreLiableToDutyAtTheFullRateThisIncludesGoodsBeingEnteredForATariffQuotaReliefToWhichNoneOfTheCodesBelowApply; // F for A00 is OK
			AssertNoMessageErrorContaining(taxLine.G4_RateDutyInfo, "list");

			taxLine.G4_Type = UniversalReferenceConstants.RefCusRateCodes.Vat;
			taxLine.G4_RateDuty = TaxRateVATDutyListImport.Codes.VATTheGoodsAreLiableToVATAtTheStandardRate; // S for B00 is OK
			AssertNoMessageErrorContaining(taxLine.G4_RateDutyInfo, "list");
			taxLine.G4_RateDuty = TaxRateCustomsDutyListImport.Codes.DutyTheGoodsAreLiableToDutyAtTheFullRateThisIncludesGoodsBeingEnteredForATariffQuotaReliefToWhichNoneOfTheCodesBelowApply; // F for B00 is bad
			AssertHasMessageErrorContaining(taxLine.G4_RateDutyInfo, "list");
			taxLine.G4_RateDuty = "~K$";
			AssertHasMessageErrorContaining(taxLine.G4_RateDutyInfo, "list");
		}
	}
}
