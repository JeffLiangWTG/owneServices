using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business
{
	public static class PackageHelper
	{
		public static bool PackTypeIsBulk(ZString packageType, BusinessObjectFactory factory) => BulkPackageUnitTypeList(factory).ContainsCode(packageType);

		static CodeDescriptionPairList BulkPackageUnitTypeList(BusinessObjectFactory factory)
			=>  Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					UNPackTypeStartDate,
					false,
					Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk);
	}
}
