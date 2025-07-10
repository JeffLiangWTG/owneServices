using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CusReconEntryLookups : Customs.Business.CusReconEntryLookups
	{
		public CusReconEntryLookups(CusReconEntry parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList EntryTypeList => ImportDeclarationTypeList.GetSimplifiedDeclarationTypeList(Factory);
	}
}
