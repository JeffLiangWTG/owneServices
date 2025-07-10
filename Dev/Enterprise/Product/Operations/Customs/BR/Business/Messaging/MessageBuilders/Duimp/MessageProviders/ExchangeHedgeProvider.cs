using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class ExchangeHedgeProvider : IExchangeHedge
	{
		ExchangeHedgeProvider(JobComInvoiceHeader invoiceHeader)
		{
			this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
		}
		readonly JobComInvoiceHeader invoiceHeader;

		public static ExchangeHedgeProvider New(JobComInvoiceHeader invoiceHeader) => invoiceHeader == null ? null : new ExchangeHedgeProvider(invoiceHeader);

		public string Type => ExchangeHedgeList.MapToCustomsCode(invoiceHeader.ExchangeHedgeType);

		public string ROFNumber => invoiceHeader.ExchangeHedgeROFBACENNumber;

		public double Value => (double)invoiceHeader.ExchangeHedgeValue;

		bool IsUpTo180DaysOrEmpty => invoiceHeader.ExchangeHedgeType.IsEmpty || invoiceHeader.ExchangeHedgeType == ExchangeHedgeList.Codes._1;

		public int? FinancialInstitution => IsUpTo180DaysOrEmpty ? null : ZInt.ParseSafe(invoiceHeader.ExchangeHedgeFinancialInstitution, 0);

		public int? ReasonCode => IsUpTo180DaysOrEmpty ? null : ZInt.ParseSafe(invoiceHeader.ExchangeHedgeReason, 0);
	}
}
