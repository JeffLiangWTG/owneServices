using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

partial class JobDeclarationTest
{
	public void TestLookups()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<JobDeclarationLookups>(dec.Lookups);
	}

	public void TestPreviousDocuments()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<PreviousDocumentCollection>(dec.PreviousDocuments);
	}

	public void TestCustomsEntryInstructions()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<CusEntryInstructionCollection>(dec.CustomsEntryInstructions);
	}

	public new void TestGetCustomsEntryInstructionProviderCore()
	{
		var dec = Factory.New<EU.Business.Declaration.JobDeclaration>();
		AssertType(typeof(EntryInstructionProvider), dec.CustomsEntryInstructionProvider);
	}

	public void TestJobComInvoiceGroupHeaders()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>>(dec.JobComInvoiceGroupHeaders);
	}

	public void TestFilteredInvoiceLines()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<InvoiceLineViewCollection>(dec.FilteredInvoiceLines);
	}

	public void TestInvoices()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<InvoiceHeaderActiveCollection>(dec.Invoices);
	}

	public void TestInvoiceLines()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<InvoiceLineCompleteCollection>(dec.InvoiceLines);
	}

	public void TestSupportingDocuments()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<SupportingDocumentCollection>(dec.SupportingDocuments);
	}

	protected override Type ExpectedMergeManagerType => typeof(MergeManager);

	protected override ZString DefaultBorderTransportModeForSea => "11";
}
