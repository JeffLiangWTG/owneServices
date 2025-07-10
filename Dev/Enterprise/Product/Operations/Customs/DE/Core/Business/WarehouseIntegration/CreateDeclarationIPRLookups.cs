using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CreateDeclarationIPRLookups : CreateDeclarationBizObjLookups
	{
		public CreateDeclarationIPRLookups(CreateDeclarationIPR parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList DeclarationTypeList => new CodeDescriptionPairList(base.DeclarationTypeList)
		{
			new CodeDescriptionPair(ImportDeclarationTypeList.Codes.AVABR, ImportDeclarationTypeList.Descriptions.AVABR)
		};
	}
}
