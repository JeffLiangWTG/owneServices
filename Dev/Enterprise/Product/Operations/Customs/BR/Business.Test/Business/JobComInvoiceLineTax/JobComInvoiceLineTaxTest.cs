using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineTax))]
	class JobComInvoiceLineTaxTest : Customs.Business.Testing.JobComInvoiceLineTaxTest
	{
		public void TestDeleteIfNoOverriddenSiscomex()
		{
			var tax = GetNewBusinessObjectForDeleteTest(Factory) as JobComInvoiceLineTax;
			tax.JLT_Type = Constants.RateCodes.ImportDuty;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			Factory.Save();
			Assert("Duty JobComInvoiceLineTax should not be deleted when JLT_MethodOfCalculation is not empty", !tax.IsDeleted);

			tax.JLT_MethodOfCalculation = ZString.Empty;
			Factory.Save();
			Assert("Duty JobComInvoiceLineTax should be deleted when JLT_MethodOfCalculation is empty", tax.IsDeleted);
		}

		public void TestNoDeleteIfImportOnly()
		{
			var tax = GetNewBusinessObjectForDeleteTest(Factory) as JobComInvoiceLineTax;
			tax.JLT_Type = Constants.RateCodes.ImportDuty;

			tax.InvoiceLine.InvoiceHeader.JobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			Factory.Save();
			Assert("Duty JobComInvoiceLineTax should not be deleted when JE_MessageType is Import", !tax.IsDeleted);
		}

		public void TestShouldBeIncludedOnSpecialCases()
		{
			var tax = GetNewBusinessObjectForDeleteTest(Factory) as JobComInvoiceLineTax;
			tax.JLT_Type = Constants.RateCodes.ImportDuty;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.Reduction;
			Assert("ShouldBeIncludedOnSpecialCases should be false", !tax.ShouldBeIncludedOnSpecialCases);

			tax.JLT_Type = Constants.RateCodes.IPI;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			Assert("ShouldBeIncludedOnSpecialCases should be false", !tax.ShouldBeIncludedOnSpecialCases);

			tax.JLT_Type = Constants.RateCodes.PIS;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			Assert("ShouldBeIncludedOnSpecialCases should be false", !tax.ShouldBeIncludedOnSpecialCases);

			tax.JLT_Type = Constants.RateCodes.Cofins;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			Assert("ShouldBeIncludedOnSpecialCases should be false", !tax.ShouldBeIncludedOnSpecialCases);

			tax.JLT_Type = Constants.RateCodes.Antidumping;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			Assert("ShouldBeIncludedOnSpecialCases should be true", tax.ShouldBeIncludedOnSpecialCases);

			tax.JLT_Type = Constants.RateCodes.IPI;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.Reduced;
			Assert("ShouldBeIncludedOnSpecialCases should be true", tax.ShouldBeIncludedOnSpecialCases);

			tax.JLT_Type = Constants.RateCodes.PIS;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.Reduced;
			Assert("ShouldBeIncludedOnSpecialCases should be true", tax.ShouldBeIncludedOnSpecialCases);

			tax.JLT_Type = Constants.RateCodes.Cofins;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.Reduced;
			Assert("ShouldBeIncludedOnSpecialCases should be true", tax.ShouldBeIncludedOnSpecialCases);

			tax.InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			tax.JLT_Type = Constants.RateCodes.ImportDuty;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.Reduction;
			Assert("ShouldBeIncludedOnSpecialCases should be true", tax.ShouldBeIncludedOnSpecialCases);

			tax.JLT_Type = Constants.RateCodes.IPI;
			tax.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			Assert("ShouldBeIncludedOnSpecialCases should be true", tax.ShouldBeIncludedOnSpecialCases);

			tax.JLT_Type = Constants.RateCodes.IPI;
			tax.JLT_MethodOfCalculation = "000";
			Assert("ShouldBeIncludedOnSpecialCases should be false", !tax.ShouldBeIncludedOnSpecialCases);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var tax = declaration.Invoices.AddNew().InvoiceLines.AddNew().Taxes.AddNew(Constants.RateCodes.Antidumping);
			return tax;
		}
	}
}
