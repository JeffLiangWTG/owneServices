using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class ETLineSecurityBlockWrapper : IETLineSecurityBlock
{
	public ETLineSecurityBlockWrapper(CusEntryLine entryLine)
	{
		EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}
	protected CusEntryLine EntryLine { get; }

	public ZString UNDangerousGoodsCode
	{
		get
		{
			var relatedOrderedInvoiceLines = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.InvoiceHeader.JZ_InvoiceNumber).ThenBy(x => x.JI_LineNo);
			var firstUndg = relatedOrderedInvoiceLines.SelectMany(x => x.UNDGs).Cast<UNDGDataItem>().FirstOrDefault(x => !x.DI_DG.IsEmpty);
			return firstUndg?.Substance?.DG_UNNO ?? ZString.Empty;
		}
	}

	public ZString TransportChargesMethodOfPayment => ZString.Empty;

	public ZString CommercialReferenceNumber => ZString.Empty;

	public ITrader Consignor => new SADEmptyTraderWrapper();

	public ITrader Consignee => new SADEmptyTraderWrapper();
}
