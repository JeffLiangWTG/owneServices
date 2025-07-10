using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class InvoicingLineBaseExtensions
	{
		public static void LogLineDescriptionChangedOnInvoice(this InvoicingLineBase line, string jobNumber, string chargeCode)
		{
			var invoice = line.Factory.Load<InvoicingBase>(line.AL_AH);
			if (invoice != null)
			{
				string reference = Invariant($"Line Description edited for line with: Job: {jobNumber}, Charge: {chargeCode}, Local Amount: {line.AL_LocalExTaxAmount.ToString(line.TransactionCurrency.Decimals)}.");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				invoice.Logs.AddNew(Events.EditedARecord, reference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		public static ExchangeRateValidLedgerEnum GetExRateLedger(this InvoicingLineBase line)
		{
			if (line.InvoiceBase != null)
			{
				return line.InvoiceBase.GetExRateLedger();
			}

			return line.IsAP()
						? ExchangeRateValidLedgerEnum.AP
						: line.IsAR()
							? ExchangeRateValidLedgerEnum.AR
							: ExchangeRateValidLedgerEnum.None;
		}

		public static bool IsAP(this InvoicingLineBase line)
		{
			var lineType = line.AL_LineType;

			switch (lineType)
			{
				case TransactionLineTypes.Cost:
				case TransactionLineTypes.UnapprovedCost:
					return true;
				default:
					return false;
			}
		}

		public static bool IsAR(this InvoicingLineBase line)
		{
			var lineType = line.AL_LineType;

			switch (lineType)
			{
				case TransactionLineTypes.Revenue:
					return true;
				default:
					return false;
			}
		}
	}
}
