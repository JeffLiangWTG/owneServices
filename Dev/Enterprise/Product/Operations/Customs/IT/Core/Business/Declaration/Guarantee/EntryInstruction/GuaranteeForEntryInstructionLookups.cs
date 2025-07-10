using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class GuaranteeForEntryInstructionLookups : EU.Business.Declaration.GuaranteeForEntryInstructionLookups
{
	public GuaranteeForEntryInstructionLookups(GuaranteeForEntryInstruction guarantee) : base(guarantee)
	{
	}

	protected override CodeDescriptionPairList BondTypeListCore => Factory.GetCachedValue<ImportGuaranteeSubTypeList>();
}
