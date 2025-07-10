using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public abstract class AddInfoCusEntryInstructionLookups : EU.Business.Declaration.AddInfoCusEntryInstructionLookups
	{
		public AddInfoCusEntryInstructionLookups(AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TransNatureList => GetTransNatureList();

		protected abstract CodeDescriptionPairList GetTransNatureList();

		public CodeDescriptionPairList ValuationBypassCodeList => Factory.GetCachedValue<ValuationBypassCodeList>();
	}
}
