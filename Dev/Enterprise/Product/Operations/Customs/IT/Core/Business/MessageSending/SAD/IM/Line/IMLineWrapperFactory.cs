using CargoWise.Common;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public static class IMLineWrapperFactory
{
	public static IMLineWrapper GetIMLineWrapper(CusEntryLine entryLine)
	{
		Argument.NotNull(entryLine?.Header?.EntryInstruction, nameof(entryLine.Header.EntryInstruction));
		return entryLine.Header.EntryInstruction.HasIntoWarehouseProcedure ? new IMWarehouseLineProcedureCodeDependentFieldWrapper(entryLine) : new IMNonWarehouseLineProcedureCodeDependentFieldWrapper(entryLine);
	}
}
