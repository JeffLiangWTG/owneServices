using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Business.Testing;

[TestedType(typeof(JobComInvoiceHeader))]
sealed class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
{
	public void TestJobComInvoiceLines()
	{
		var invoiceHeader = this.invoiceHeader;
		CombineAssertions(() =>
		{
			AssertType<JobComInvoiceLineViewCollection>("Type", invoiceHeader.JobComInvoiceLines);
			AssertSame("InvoiceLine same", invoiceHeader.JobComInvoiceLines, invoiceHeader.InvoiceLines);
		});
	}

	public void TestLookups_Import()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobComInvoiceHeaderLookups>(invoiceHeader.Lookups);
	}

	public void TestLookups_Export()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobComInvoiceHeaderLookups>(invoiceHeader.Lookups);
	}

	public void TestLookups_MiscellaneousCustoms()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobComInvoiceHeaderLookups>(invoiceHeader.Lookups);
	}

	public void TestValidation_Import()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobComInvoiceHeaderValidation>(invoiceHeader.Validation);
	}

	public void TestValidation_Export()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobComInvoiceHeaderValidation>(invoiceHeader.Validation);
	}

	public void TestValidation_MiscellaneousCustoms()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobComInvoiceHeaderValidation>(invoiceHeader.Validation);
	}

	public override void TestLocalCurrencyCodeCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.Finland, Factory.New<JobComInvoiceHeader>().LocalCurrencyCode);
	}

	public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Finland;

	protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
}
