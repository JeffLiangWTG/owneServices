using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Module;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Module
{
	public class CusReconDeclarationFilterLookups : CommonFilterLookups
	{
		public CusReconDeclarationFilterLookups(CusReconDeclarationFilterStripBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}
		public ZZRefCusCodeListCombinedCollection CustomsOffices => new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now);
		public CodeDescriptionPairList messageStatusList => CustomsMessageStatusTypeList.GetStatusListForRefundDeclaration(Factory);
		public CodeDescriptionPairList entryStatusList => CustomsEntryStatusTypeList.GetStatusListForRefundDeclaration(Factory);
	}
}
