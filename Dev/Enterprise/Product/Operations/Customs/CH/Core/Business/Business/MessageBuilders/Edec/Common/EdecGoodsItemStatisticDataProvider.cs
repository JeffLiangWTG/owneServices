using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class EdecGoodsItemStatisticDataProvider : IEdecGoodsItemStatistic
{
	public static EdecGoodsItemStatisticDataProvider New(CusEntryLine entryLine) => entryLine == null ? null : new EdecGoodsItemStatisticDataProvider(entryLine);

	EdecGoodsItemStatisticDataProvider(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		invoiceLine = this.entryLine.RandomLine;
	}
	readonly CusEntryLine entryLine;
	readonly JobComInvoiceLine invoiceLine;

	public string CustomsClearanceType => invoiceLine.JI_Procedure.IsEmpty ? (ZString)"0" : invoiceLine.JI_Procedure;

	public string CommercialGood => invoiceLine.JI_NonTradingGoods ? "2" : "1";

	public decimal StatisticalValue => entryLine.CL_StatisticalValue.Truncate();

	public bool StatisticalValueConfirmation => invoiceLine.JI_StatisticalValueConfirmation;

	public bool Repair => invoiceLine.InAndOutwardProcessingRepair;
}
