using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.IT.Business;

sealed class OutwardInvoiceLineProcedureCodeResolver
{
	public OutwardInvoiceLineProcedureCodeResolver(CusEntryInstruction entryInstruction, IWhsBondedWarehouseAttribute bondedWarehouseAttribute)
	{
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
		this.bondedWarehouseAttribute = Argument.NotNull(bondedWarehouseAttribute, nameof(bondedWarehouseAttribute));
	}

	public ZString GetProcedureCode()
	{
		var inwardProcedure = bondedWarehouseAttribute.WB_InwardProcedure.Left(2);
		if (!Is2CharsLength(inwardProcedure))
		{
			return ZString.Empty;
		}

		var instructionProcedure = entryInstruction.CEI_Procedure;
		if (!Is2CharsLength(instructionProcedure))
		{
			return ZString.Empty;
		}

		return FormattableString.Invariant($"{instructionProcedure}{inwardProcedure}");
	}

	readonly CusEntryInstruction entryInstruction;
	readonly IWhsBondedWarehouseAttribute bondedWarehouseAttribute;

	bool Is2CharsLength(ZString stringToCheck) => stringToCheck.Length == 2;
}
