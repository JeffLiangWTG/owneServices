using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Accounting.Business.Billing
{
	public class AccBillingSummary
	{
		public AccBillingSummary(IEnumerable<AccBillingHeader> billingHeaders)
		{
			Argument.NotNull(billingHeaders, nameof(billingHeaders));
			BillingHeaders = billingHeaders;
		}

		public IEnumerable<AccBillingHeader> BillingHeaders { get; }

		/// <summary>
		/// Value will be calculated from BillingHeaders property.
		/// </summary>
		public int BilledItemCount => BillingHeaders?.Sum(h => h.ABH_BillingCounter) ?? 0;

		public string BilliedShipmentNumbersAsCSV => (BillingHeaders?.Any() ?? false)
															? string.Join(", ", BillingHeaders.OrderByDescending(h => h.ABH_EventTimeUtc)
																							.First()
																							.BillingItems.OfType<AccBillingItem>()
																										.OrderBy(i => i.ABI_ParentReferenceNumber)
																										.Select(i => i.ABI_ParentReferenceNumber).ToArray())
															: string.Empty;
	}
}
