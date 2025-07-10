using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AddInfoCusEntryInstructionLookups : EU.Business.Declaration.AddInfoCusEntryInstructionLookups
{
	public AddInfoCusEntryInstructionLookups(AddInfoCusEntryInstruction parent) : base(parent)
	{
	}

	public CodeDescriptionPairList ParticipantTypeList => Factory.GetCachedValue<ParticipantTypeList>();

	public RefCurrencyCollection CurrencyList => new RefCurrencyCollection(Factory);
}
