using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class IMNonWarehouseLineProcedureCodeDependentFieldWrapper : IMLineWrapper
{
	public IMNonWarehouseLineProcedureCodeDependentFieldWrapper(CusEntryLine entryLine) : base(entryLine)
	{
	}
	protected override ZString PreferencesCore => entryLine.PreferenceCode;

	protected override IEnumerable<ZString> QuotasCore => !entryLine.QuotaOrderNumber.IsEmpty ? new ZString[] { entryLine.QuotaOrderNumber } : System.Array.Empty<ZString>();

	protected override ZDecimal? ItemPriceEuroCore => entryLine.TotalLinePriceInLocalCurrency;

	protected override ZDecimal? AdjustmentInEuroCore => entryLine.ZG_AdjustmentAmount;
}
