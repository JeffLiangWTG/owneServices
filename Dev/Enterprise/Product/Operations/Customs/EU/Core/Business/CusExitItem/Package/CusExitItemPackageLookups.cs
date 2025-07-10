using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitItemPackageLookups : CusInvPackLookups
	{
		public CusExitItemPackageLookups(CusExitItemPackage parent) : base(parent)
		{
		}
		public override CodeDescriptionPairList UnitTypeList => GetPackageUnitTypeList(Factory);

		public static CodeDescriptionPairList GetPackageUnitTypeList(BusinessObjectFactory factory)
		{
			return Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				UNPackTypeStartDate);
		}

		public CodeDescriptionPairList BulkPackageUnitTypeList
		{
			get
			{
				return Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					UNPackTypeStartDate,
					false,
					Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk);
			}
		}

		public CodeDescriptionPairList UnpackedPackageUnitTypeList
		{
			get
			{
				return Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					UNPackTypeStartDate,
					false,
					Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk);
			}
		}
	}
}
