using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class GoodsMeasureDataProvider : IGoodsMeasure
{
	public static GoodsMeasureDataProvider New(CusEntryLine entryLine) => entryLine == null ? null : new GoodsMeasureDataProvider(entryLine);

	public GoodsMeasureDataProvider(CusEntryLine entryLine)
	{
		this.entryLine = entryLine;
	}
	readonly CusEntryLine entryLine;

	public decimal? GrossMass => entryLine.CalcGrossWeight;

	public decimal? NetMass => entryLine.Header.EntryInstruction.IsSimplified ? null : entryLine.CalcNetWeight.ReturnNullIfEmpty();

	public decimal? SupplementaryUnits => entryLine.RandomLine.EntryInstruction.IsSimplified ? null : entryLine.CalcAdditionalQty;
}
