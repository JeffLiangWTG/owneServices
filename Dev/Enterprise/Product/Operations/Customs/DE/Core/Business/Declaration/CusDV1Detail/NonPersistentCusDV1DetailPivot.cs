namespace Enterprise.Customs.DE.Business.Declaration;

public class NonPersistentCusDV1DetailPivot : EU.Business.Declaration.NonPersistentCusDV1DetailPivot
{
	public NonPersistentCusDV1DetailPivot(CusEntryInstruction entryInstruction)
		: base(entryInstruction)
	{
	}

	protected override void AddIsForEntryInstructionPivot()
	{
		if (!HasPivot)
		{
			foreach (var pivot in ((CusEntryInstruction)EntryInstruction).DV1DetailsPivots)
			{
				pivot.DeleteIsForEntryInstructionPivot();
			}
		}
		base.AddIsForEntryInstructionPivot();
	}
}

