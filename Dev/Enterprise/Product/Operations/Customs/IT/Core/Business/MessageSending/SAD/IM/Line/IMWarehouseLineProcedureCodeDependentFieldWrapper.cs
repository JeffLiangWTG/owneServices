using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class IMWarehouseLineProcedureCodeDependentFieldWrapper : IMLineWrapper
{
	public IMWarehouseLineProcedureCodeDependentFieldWrapper(CusEntryLine entryLine) : base(entryLine)
	{
	}
	protected override ZString PreferencesCore => ZString.Empty;

	protected override IEnumerable<ZString> QuotasCore => null;

	protected override ZDecimal? ItemPriceEuroCore => null;

	protected override ZDecimal? AdjustmentInEuroCore => null;
}
