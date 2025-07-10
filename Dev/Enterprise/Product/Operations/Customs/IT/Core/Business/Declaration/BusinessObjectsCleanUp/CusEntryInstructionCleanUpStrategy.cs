using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class CusEntryInstructionCleanUpStrategy : ICleanUpStrategy
{
	public CusEntryInstructionCleanUpStrategy(CusEntryInstruction entryInstruction)
	{
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
		declaration = Argument.NotNull(entryInstruction.JobDeclaration, nameof(entryInstruction.JobDeclaration));
	}

	readonly CusEntryInstruction entryInstruction;
	readonly JobDeclaration declaration;

	void ICleanUpStrategy.CleanUp()
	{
		if (!declaration.IsImport)
		{
			entryInstruction.FiscalReferences.RemoveAndDeleteAll();
			entryInstruction.ClearanceByEntryLine = false;
			entryInstruction.PreviousDocuments.RemoveAndDeleteAll();
		}

		if (!declaration.IsUCC6AndIsExport)
		{
			entryInstruction.ZG_PresentationStartDate = ZDateTime.Empty;
			entryInstruction.AdditionalInfos.RemoveAndDeleteAll();
			entryInstruction.SupportingDocuments.RemoveAndDeleteAll();
		}

		if (!declaration.IsUCC6)
		{
			CleanupWarehouseTypeAndIds();
		}
		else
		{
			entryInstruction.ZG_SealsCount = 0;
			entryInstruction.Seals.RemoveAndDeleteAll();
		}
	}

	void CleanupWarehouseTypeAndIds()
	{
		entryInstruction.ZG_FromWarehouseID = ZString.Empty;
		entryInstruction.ZG_FromWarehouseType = ZString.Empty;
		entryInstruction.ZG_ToWarehouseID = ZString.Empty;
		entryInstruction.ZG_ToWarehouseType = ZString.Empty;
	}
}
