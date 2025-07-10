using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class TaxBreakdownProvider : IXlsxProvider
	{
		public TaxBreakdownProvider(TaxBreakdown taxBreakdown)
		{
			this.taxBreakdown = Argument.NotNull(taxBreakdown, nameof(taxBreakdown));
		}

		readonly TaxBreakdown taxBreakdown;

		[XlsxField(1, "Tax Type")]
		public ZString TaxType => taxBreakdown.TaxType43;

		[XlsxField(2, "Payable Amount")]
		public ZDecimal PayableAmount => taxBreakdown.PayableAmount46;
	}
}
