using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Customs.IT.Business.Declaration;

class Ucc6ExportActiveBorderGroupWithEntryInstruction : Ucc6ExportActiveBorderGroup
{
	public Ucc6ExportActiveBorderGroupWithEntryInstruction(JobDeclaration declaration, CusEntryInstruction entryInstruction) : base(declaration)
	{
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
	}

	protected override IEnumerable<CusEntryInstruction> GetEntryInstructions()
	{
		yield return entryInstruction;
	}

	readonly CusEntryInstruction entryInstruction;
}
