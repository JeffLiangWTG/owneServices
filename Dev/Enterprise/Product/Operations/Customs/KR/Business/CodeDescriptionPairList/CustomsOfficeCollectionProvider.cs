using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Business
{
	public class CustomsOfficeCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.KR.IKRCustomsOfficeCollectionProvider
	{
		public CustomsOfficeCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(BusinessObjectFactory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.CustomsOffice, ZDateTime.Today);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		public override int MaxLength => 3;
	}
}
