using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSDocumentLookups : CusSupportingInfoLookups
	{
		public EMCSDocumentLookups(EMCSDocument parent)
			: base(parent)
		{
			declaration = parent.Declaration;
		}
		readonly EMCSJobDeclaration declaration;

		public ZZRefCusCodeListCombinedCollection DocumentTypesList
			=> ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, declaration.GetDefaultDataGroupingCode(), Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSDocumentTypes, ZDateTime.Now);
	}
}
