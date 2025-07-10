using CargoWise.Common;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public static class IMHeaderWrapperFactory
{
	public static IMHeaderWrapper GetIMHeaderWrapper(CusEntryHeader entryHeader)
	{
		Argument.NotNull(entryHeader?.EntryInstruction, nameof(entryHeader.EntryInstruction));
		return entryHeader.EntryInstruction.HasIntoWarehouseProcedure ? new IMWarehouseHeaderProcedureWrapper(entryHeader) : new IMNonWarehouseHeaderProcedureWrapper(entryHeader);
	}
}
