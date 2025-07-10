using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobComInvoiceLineSupportingDocumentsWithHarbourRateProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When invoiceLine is null",
			() => new JobComInvoiceLineSupportingDocumentsWithHarbourRateProvider(invoiceLine: null));
	}

	public void TestSupportingDocumentsMaster()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var provider = (ISupportingDocumentsWithHarbourRateProvider)new JobComInvoiceLineSupportingDocumentsWithHarbourRateProvider(invoiceLine);
		AssertSame($"{nameof(provider.SupportingDocumentsMaster)} Cached", invoiceLine, provider.SupportingDocumentsMaster);
	}

	public void TestHarbourRateProvider()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var provider = (ISupportingDocumentsWithHarbourRateProvider)new JobComInvoiceLineSupportingDocumentsWithHarbourRateProvider(invoiceLine);
		var harbourRateProvider = provider.HarbourRateProvider;
		AssertType<JobComInvoiceLineHarbourRateProvider>($"{nameof(provider.HarbourRateProvider)} Type", harbourRateProvider);
		AssertSame($"{nameof(provider.HarbourRateProvider)} Cached", harbourRateProvider, provider.HarbourRateProvider);
	}
}
