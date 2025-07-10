namespace Enterprise.Customs.IN.Business;

class CusContainerOnEntryInstructionValidation : Customs.Business.CusContainerOnEntryInstructionValidation
{
	public CusContainerOnEntryInstructionValidation(CusContainerOnEntryInstruction parent) : base(parent)
	{
	}

	public new CusContainerOnEntryInstruction Parent => (CusContainerOnEntryInstruction)base.Parent;

	protected override void CheckIsForEntry()
	{
		base.CheckIsForEntry();

		var parent = Parent;
		if (parent.Pivot?.EntryInstruction is CusEntryInstruction instruction && instruction.IsExport && instruction.ContainersForInstructionForBindingOnly.AllLinkedContainers.Length > 99)
		{
			parent.IsForEntryInfo.AddMessageError(Res.GetString("D5E80FA1-A1A9-47DA-B395-155ABF71E697", "Only 99 containers are allowed per Entry Instructions."));
		}
	}
}
