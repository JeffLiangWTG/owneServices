using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobComInvoiceLineDeepCloneStrategy))]
sealed class JobComInvoiceLineDeepCloneStrategyTest : TestCaseWithFactory
{
	public void TestCloneForAdditionalTaxes()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var additionalTax = invoiceLine.AdditionalTaxes.AddNew();
		additionalTax.BZ_TaxType = "280";
		additionalTax.BZ_Tariff = "280-001";
		Factory.Save();

		var clonedInvoiceLine = (JobComInvoiceLine)new JobComInvoiceLineDeepCloneStrategy(invoiceLine, CloneType.TemplateCopy, invoice, null).Clone();
		CombineAssertions(() =>
		{
			var clonedAdditionalTax = clonedInvoiceLine.AdditionalTaxes.ToList<CusLineTariffDetail>().First();
			AssertEquals("Additional Taxes cloned", 1, clonedInvoiceLine.AdditionalTaxes.Count);
			AssertEquals("BZ_TaxType cloned", additionalTax.BZ_TaxType, clonedAdditionalTax.BZ_TaxType);
			AssertEquals("BZ_Tariff cloned", additionalTax.BZ_Tariff, clonedAdditionalTax.BZ_Tariff);
		});
	}

	public void TestCloneForAdditionalFees()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var additionalFee = invoiceLine.AdditionalFees.AddNew();
		additionalFee.BZ_TaxType = "280";
		additionalFee.BZ_Tariff = "280-001";
		Factory.Save();

		var clonedInvoiceLine = (JobComInvoiceLine)new JobComInvoiceLineDeepCloneStrategy(invoiceLine, CloneType.TemplateCopy, invoice, null).Clone();
		CombineAssertions(() =>
		{
			var clonedAdditionalFee = clonedInvoiceLine.AdditionalFees.ToList<CusLineTariffDetail>().First();
			AssertEquals("Additional Fees cloned", 1, clonedInvoiceLine.AdditionalFees.Count);
			AssertEquals("BZ_TaxType cloned", additionalFee.BZ_TaxType, clonedAdditionalFee.BZ_TaxType);
			AssertEquals("BZ_Tariff cloned", additionalFee.BZ_Tariff, clonedAdditionalFee.BZ_Tariff);
		});
	}
}
