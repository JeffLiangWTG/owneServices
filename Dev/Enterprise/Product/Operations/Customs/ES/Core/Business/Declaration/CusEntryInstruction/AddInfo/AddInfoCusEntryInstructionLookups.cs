using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration;

public class AddInfoCusEntryInstructionLookups : EU.Business.Declaration.AddInfoCusEntryInstructionLookups
{
	public AddInfoCusEntryInstructionLookups(AddInfoCusEntryInstruction parent) : base(parent)
	{
	}

	public CodeDescriptionPairList RequestTypeList => Factory.GetCachedValue<RequestTypeList>();

	public CodeDescriptionPairList IndirectTypeList => Factory.GetCachedValue<IndirectTypeList>();
}
