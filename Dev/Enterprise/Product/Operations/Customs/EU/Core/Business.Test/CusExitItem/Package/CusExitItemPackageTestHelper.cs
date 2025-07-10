using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	public static class NctsPackageTestHelper
	{
		public static ZString SetupBulkCusCode(this BusinessObjectFactory factory)
		{
			const string packageType = "VQ";
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UnitedNationsPackageTypes");
			var code = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				packageType,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code.PK, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "1");
			factory.Save();
			return packageType;
		}

		public static ZString SetupUnpackCusCode(this BusinessObjectFactory factory)
		{
			const string packageType = "NE";
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UnitedNationsPackageTypes");
			var code = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				packageType,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code.PK, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "1");
			factory.Save();
			return packageType;
		}

		public static ZString SetupStandardPackCusCode(this BusinessObjectFactory factory)
		{
			const string packageType = "AE";
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Packages");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				packageType,
				"AE Desc",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
			return packageType;
		}
	}
}

