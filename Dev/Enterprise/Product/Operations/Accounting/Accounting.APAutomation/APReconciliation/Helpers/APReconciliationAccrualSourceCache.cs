using System;
using Enterprise.Accounting.Business.APReconciliation;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.APAutomation.APReconciliation.Helpers
{
	internal static class APReconciliationAccrualSourceCache
	{
		internal static T Get<T>(AccDraftInvoiceHeader draftInvoice) where T : IAPReconciliationAccrualSource =>
			draftInvoice.Factory.GetCachedValue
			(
				CacheKey(draftInvoice),
				() => (T)(IAPReconciliationAccrualSource)new DraftInvoiceHeaderBasedAPReconciliationAccrualSource(draftInvoice)
			);

		internal static void Clear<T>(AccDraftInvoiceHeader draftInvoice) where T : IAPReconciliationAccrualSource =>
				draftInvoice.Factory.ClearCachedValue<T>(CacheKey(draftInvoice));

		static string CacheKey(AccDraftInvoiceHeader draftInvoice) => FormattableString.Invariant($"accrualcache_{draftInvoice.PK}");
	}
}
