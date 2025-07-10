using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ExportCusEntryInstructionLookups : CusEntryInstructionLookups
{
	public ExportCusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction)
	{
	}

	protected override CodeDescriptionPairList DeclarationTypeListCore => Factory.GetCachedValue<ExportCusEntryInstructionsDeclarationTypeList>();
}
