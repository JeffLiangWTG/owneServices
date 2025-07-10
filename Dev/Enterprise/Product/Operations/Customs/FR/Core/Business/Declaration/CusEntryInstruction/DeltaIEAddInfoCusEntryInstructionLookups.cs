using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEAddInfoCusEntryInstructionLookups : AddInfoCusEntryInstructionLookups
	{
		public DeltaIEAddInfoCusEntryInstructionLookups(AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList GetTransNatureList() => Factory.GetCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, Parent.Parent?.JobDeclaration?.JE_MessageType ?? ZString.Empty);
	}
}
