using System;
using CargoWise.Common;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class JobComInvoiceLineSupportingDocumentsWithHarbourRateProvider : ISupportingDocumentsWithHarbourRateProvider
{
	public JobComInvoiceLineSupportingDocumentsWithHarbourRateProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		lazyHarbourRateProvider = new Lazy<IHarbourRateProvider>(() => new JobComInvoiceLineHarbourRateProvider(invoiceLine));
	}

	ISupportingDocumentsProvider ISupportingDocumentsWithHarbourRateProvider.SupportingDocumentsMaster => invoiceLine;

	IHarbourRateProvider ISupportingDocumentsWithHarbourRateProvider.HarbourRateProvider => lazyHarbourRateProvider.Value;

	readonly JobComInvoiceLine invoiceLine;
	readonly Lazy<IHarbourRateProvider> lazyHarbourRateProvider;
}
