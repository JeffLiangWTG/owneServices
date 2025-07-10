using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using NctsGuarantee = Enterprise.Customs.GB.Business.CodeDescriptionPairLists.NctsGuarantee;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GBGuaranteeLookups : EU.Business.Declaration.GuaranteeForDeclarationLookups
	{
		public GBGuaranteeLookups(GBGuarantee guarantee)
		: base(guarantee)
		{
		}

		public new CusEntryInstructionCollection EntryInstructions => (CusEntryInstructionCollection)base.EntryInstructions;

		public CodeDescriptionPairList AuthorisationTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (Parent.IsGuarantee)
				{
					result = Factory.GetCachedValue<NctsGuarantee>();
				}
				return result;
			}
		}

		protected override CodeDescriptionPairList BondTypeListCore => Factory.GetCachedValue<GuaranteeTypeList>();

		protected new GBGuarantee Parent => (GBGuarantee)base.Parent;
	}
}
