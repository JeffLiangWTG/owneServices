using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationNFeInvoiceProvider : IDeclarationNFeInvoice
	{
		public DeclarationNFeInvoiceProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly JobComInvoiceLine invoiceLine;

		public int NFEItemSequence => 0;
		public string NFEKey => invoiceLine?.JI_NFeNumber ?? string.Empty;
		public short NFEItemNumber => ZShort.ParseSafe(invoiceLine.JI_NFeItemNumber, ZShort.Zero);
		public decimal CustomsQuantityRelated => decimal.Zero;
		public string Type => string.Empty;
	}
}
