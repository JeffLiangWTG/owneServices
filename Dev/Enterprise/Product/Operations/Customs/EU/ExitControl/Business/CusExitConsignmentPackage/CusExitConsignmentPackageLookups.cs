using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentPackageLookups : ExitControlBase.Business.CusExitConsignmentPackageLookups
	{
		public CusExitConsignmentPackageLookups(AutoCusExitConsignmentPackage parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList PackTypeList => RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, EU.Business.UniversalReferenceConstants.UNPackTypeStartDate);

		public CodeDescriptionPairList BulkPackageUnitTypeList
		{
			get
			{
				return Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					EU.Business.UniversalReferenceConstants.UNPackTypeStartDate,
					false,
					Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk);
			}
		}

		public CodeDescriptionPairList BreakBulkPackageUnitTypeList
		{
			get
			{
				return Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					EU.Business.UniversalReferenceConstants.UNPackTypeStartDate,
					false,
					Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk);
			}
		}

		public CodeDescriptionPairList StatusList => StatusListCore;
		protected virtual CodeDescriptionPairList StatusListCore => new CodeDescriptionPairList();

		protected new CusExitConsignmentPackage Parent => (CusExitConsignmentPackage)base.Parent;
	}
}
