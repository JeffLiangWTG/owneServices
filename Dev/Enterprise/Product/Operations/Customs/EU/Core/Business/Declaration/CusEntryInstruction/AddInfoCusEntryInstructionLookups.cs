using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoCusEntryInstructionLookups : EUAddInfoLookups
	{
		public AddInfoCusEntryInstructionLookups(AddInfoCusEntryInstruction parent)
			: base(parent)
		{
		}

		protected new AddInfoCusEntryInstruction Parent => (AddInfoCusEntryInstruction)base.Parent;

		public CodeDescriptionPairList IdentificationOfGoodsCodeList => Factory.GetCachedValue<IdentificationOfGoodsCodeList>();

		public virtual CodeDescriptionPairList ProcessingProcedureCode => new CodeDescriptionPairList();

		public CodeDescriptionPairList YesNoList => Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>();
	}
}
