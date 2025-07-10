using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class SpecialCaseTaxValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTaxGroup()
		{
			ReferenceTestDataHelper.CreateRefCusRateCodeAndType(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			ValidationTestHelper.AssertErrorIfNotEntered(specialCaseTax.TaxGroupInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(specialCaseTax.TaxGroupInfo, Constants.RateCodes.ImportDuty, Constants.RateCodes.Antidumping);
			AssertNoErrorContaining("TaxGroupInfo", specialCaseTax.TaxGroupInfo, "Ad Valorem rate has been overridden. You cannot add Special Rates.");

			invoiceLine.IPIRateIsOverridden = true;
			specialCaseTax.TaxGroup = Constants.RateCodes.IPI;
			specialCaseTax.RateOrUnitValue = 5m;
			AssertHasErrorContaining("IPI", specialCaseTax.TaxGroupInfo, "Ad Valorem rate has been overridden. You cannot add Special Rates.");

			invoiceLine.PisRateIsOverridden = true;
			specialCaseTax.TaxGroup = Constants.RateCodes.PIS;
			specialCaseTax.RateOrUnitValue = 10m;
			AssertHasErrorContaining("PIS", specialCaseTax.TaxGroupInfo, "Ad Valorem rate has been overridden. You cannot add Special Rates.");

			invoiceLine.CofinsRateIsOverridden = true;
			specialCaseTax.TaxGroup = Constants.RateCodes.Cofins;
			specialCaseTax.RateOrUnitValue = 15m;
			AssertHasErrorContaining("Cofins", specialCaseTax.TaxGroupInfo, "Ad Valorem rate has been overridden. You cannot add Special Rates.");
		}

		public void TestCheckTaxGroup_Duplicate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			AssertNoError(specialCaseTax.TaxGroupInfo, "You have entered a duplicate Tax Group");

			specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			AssertHasError(specialCaseTax.TaxGroupInfo, "You have entered a duplicate Tax Group");

			specialCaseTax.TaxGroup = Constants.RateCodes.PIS;
			AssertNoError(specialCaseTax.TaxGroupInfo, "You have entered a duplicate Tax Group");
		}

		public void TestCheckTaxType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			ValidationTestHelper.AssertErrorIfNotEntered(specialCaseTax.TaxTypeInfo);

			specialCaseTax.TaxGroup = Constants.RateCodes.PIS;
			ValidationTestHelper.AssertErrorIfInvalidCode(specialCaseTax.TaxTypeInfo, "XXX", SpecialCaseTaxTypeList.Codes.QuantityPerUnit);

			specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			ValidationTestHelper.AssertErrorIfInvalidCode(specialCaseTax.TaxTypeInfo, "XXX", SpecialCaseTaxTypeList.Codes.AdValoremRate);

			specialCaseTax.TaxGroup = Constants.RateCodes.Cofins;
			ValidationTestHelper.AssertErrorIfInvalidCode(specialCaseTax.TaxTypeInfo, SpecialCaseTaxTypeList.Codes.AdValoremRate, SpecialCaseTaxTypeList.Codes.QuantityPerUnit);
		}

		public void TestCheckRateOrUnitValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();

			specialCaseTax.RateOrUnitValue = -1;
			ValidationTestHelper.AssertErrorIfValueIsNegative(specialCaseTax.RateOrUnitValueInfo);
		}
	}
}
