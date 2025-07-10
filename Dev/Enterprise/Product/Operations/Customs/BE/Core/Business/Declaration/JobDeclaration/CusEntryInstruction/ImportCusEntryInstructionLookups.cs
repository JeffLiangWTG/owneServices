using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ImportCusEntryInstructionLookups : CusEntryInstructionLookups
{
	public ImportCusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction)
	{
	}

	protected override CodeDescriptionPairList DeclarationTypeListCore => Factory.GetCachedValue<ImportCusEntryInstructionsDeclarationTypeList>();
}
