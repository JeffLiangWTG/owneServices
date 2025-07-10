using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CNOrgImpAddInfoLookups : AutoCNOrgImpAddInfoLookups
	{
		public CNOrgImpAddInfoLookups(AutoCNOrgImpAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList MessageSubTypeList => DecTypeList.GetDecTypeList(Factory);

		public CodeDescriptionPairList IntelligentDeclarationTypeList => Factory.GetCachedValue<IntelligentDeclarationTypeList>();
	}
}
