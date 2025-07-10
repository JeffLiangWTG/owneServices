using System.Linq;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class SaudiArabiaEInvoicingPivotStatusProvider : IEInvoicingPivotStatusProvider
	{
		public bool CanCreateNotEligibleForEInvoicingPivot(AccTransactionHeader transaction) => false;

		public string GetInitialPivotStatus(ITransactionHeader header)
		{
			if (header != null)
			{
				var headerWithLines = header as ITransactionHeaderWithLines;
				if (headerWithLines != null)
				{
					var isNotReportable = headerWithLines.Lines.OfType<TransactionLine>().All(line => line.TaxRate?.IsNonReportable ?? true);
					if (isNotReportable)
					{
						return EInvoicingPivotState.Discarded;
					}
					return EInvoicingPivotState.Queued;
				}
			}
			return EInvoicingPivotState.Discarded;
		}
	}
}
