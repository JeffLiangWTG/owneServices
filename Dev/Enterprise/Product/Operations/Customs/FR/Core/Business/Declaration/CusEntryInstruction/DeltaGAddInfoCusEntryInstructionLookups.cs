using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGAddInfoCusEntryInstructionLookups : AddInfoCusEntryInstructionLookups
	{
		public DeltaGAddInfoCusEntryInstructionLookups(AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList GetTransNatureList() => Factory.GetTranNatureList(Parent.Parent.JobDeclaration?.GetDefaultDataGroupingCode() ?? Core.Constants.CountryCodes.France);
	}
}
