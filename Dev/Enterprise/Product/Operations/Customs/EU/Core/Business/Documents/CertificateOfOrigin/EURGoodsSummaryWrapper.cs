using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public class EURGoodsSummaryWrapper : IEURGoodsSummary
	{
		public EURGoodsSummaryWrapper(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			InvoiceLines = Argument.NotNull(invoiceLines, nameof(invoiceLines));
		}

		protected IEnumerable<JobComInvoiceLine> InvoiceLines { get; }

		ZString IGoodsSummary.Description => Builder.ItemsInfoBox8;

		ZString IGoodsSummary.WeightAndVolume => Builder.GrossMassVolumeBox9;

		ZString IEURGoodsSummary.InvoiceNumbers => Builder.InvoicesBox10;

		EUR1BoxItemsBuilder Builder => builder ?? (builder = InitializeBuilder());
		EUR1BoxItemsBuilder builder;

		EUR1BoxItemsBuilder InitializeBuilder()
		{
			var builder = GetNewBuilder();
			builder.Build();
			return builder;
		}

		protected virtual EUR1BoxItemsBuilder GetNewBuilder() => new EUR1BoxItemsBuilder(InvoiceLines);
	}
}
