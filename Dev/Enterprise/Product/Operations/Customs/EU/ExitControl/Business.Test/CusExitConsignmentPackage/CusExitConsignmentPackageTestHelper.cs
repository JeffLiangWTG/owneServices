using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	public static class CusExitConsignmentPackageTestHelper
	{
		public static ZString SetupBulkCusCode(this BusinessObjectFactory factory)
		{
			const string packageType = "VG";
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				packageType, packageType, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			factory.Save();
			return packageType;
		}

		public static ZString SetupBreakBulkCusCode(this BusinessObjectFactory factory)
		{
			const string packageType = "NE";
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				packageType, packageType, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			factory.Save();
			return packageType;
		}
	}
}
