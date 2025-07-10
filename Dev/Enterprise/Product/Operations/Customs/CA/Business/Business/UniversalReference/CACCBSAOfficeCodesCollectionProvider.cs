using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	public class CACCBSAOfficeCodesCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.CA.ICACCBSAOfficeCodesCollectionProvider
	{
		public CACCBSAOfficeCodesCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(BusinessObjectFactory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		public override int MaxLength => UniversalReferenceConstants.CBSAOfficeCodeMaxLength;
	}
}
