using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business
{
	public class EPaymentQuoteSummaryCollection : NonPersistentBusinessObjectCollection<EPaymentQuoteSummary>
	{
		public EPaymentQuoteSummaryCollection(APPaymentBatchPoster batchPoster) : base(batchPoster.Factory)
		{
			Argument.NotNull(batchPoster, nameof(batchPoster));
			QuoteCollection = new EPaymentQuoteCollection(batchPoster);
			ReloadSummaries();
		}

		readonly EPaymentQuoteCollection QuoteCollection;

		protected override BusinessObject CreateNonPersistentBusinessObject() => null;

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		public void ReloadSummaries(bool shouldReloadQuoteCollection = true)
		{
			RemoveAll();

			if (shouldReloadQuoteCollection)
			{
				QuoteCollection.Reload(true);
			}

			var summaryGroups = QuoteCollection.Cast<EPaymentQuote>()
				.Where(x => x.QU_Status != QuoteStatusCodes.Discarded)
				.GroupBy(x => (x.QU_ProviderCode, x.QU_RX_NKToCurrency, x.QU_Status, x.QU_ErrorDescription));

			foreach (var grouping in summaryGroups)
			{
				var (providerCode, paymentCurrency, status, error) = grouping.Key;
				Add(new EPaymentQuoteSummary(grouping, providerCode, paymentCurrency, status, error));
			}
		}
	}
}
