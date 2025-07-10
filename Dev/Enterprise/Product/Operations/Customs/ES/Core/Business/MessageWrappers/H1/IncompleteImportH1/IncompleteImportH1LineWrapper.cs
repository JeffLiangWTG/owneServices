using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class IncompleteImportH1LineWrapper : ImportH1CommonLineWrapper, IIncompleteImportH1GoodsShipmentItem
{
	public IncompleteImportH1LineWrapper(CusEntryLine entryLine) : base(entryLine)
	{
		Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
		randomLine = entryLine.RandomLine;
	}
	readonly JobComInvoiceLine randomLine;

	public ICommonH1Procedure Procedure => procedure ??= procedure = new ImportH1CommonProcedureWrapper(randomLine);
	ImportH1CommonProcedureWrapper procedure;

	public ZString CountryOfOrigin => randomLine.JI_CountryOfOrigin;

	public IIncompleteImportH1Commodity Commodity => commodity ??= commodity = new IncompleteImportH1CommodityWrapper(randomLine);
	IncompleteImportH1CommodityWrapper commodity;
}
