using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.GUI;

public sealed class EntryInstructionDetailsLayoutBuilder : Customs.GUI.EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>
{
	protected override int MaxColumns => 3;
}
