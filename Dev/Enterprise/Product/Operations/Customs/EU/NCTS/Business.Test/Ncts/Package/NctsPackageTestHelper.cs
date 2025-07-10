using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
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

		public static ZString SetupUnpackCusCodeWithLanguage(this BusinessObjectFactory factory)
		{
			const string packageType = "NE";
			const string packageDesc = "Unpacked";
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UnitedNationsPackageTypes");
			helper.CreateOrGetLanguage("EN", "English");
			var codeList = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				packageType,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codeList.PK, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "1");
			helper.CreateCusCodeListLanguage(codeList, TranslationHelper.GetLanguageCode(Enterprise.Core.SharedConstants.Languages.EnglishBritish), packageDesc);
			factory.Save();
			return packageDesc;
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

		public static NctsPackageValidationDeciderTestContext<TNctsPackageValidationDecider>
			CreateValidationTestContext<TNctsPackageValidationDecider>(this NctsPackage package, params Type[] ruleDeciderInterfaceTypes)
			where TNctsPackageValidationDecider : class, INctsPackageValidationDecider
		{
			var result = new NctsPackageValidationDeciderTestContext<TNctsPackageValidationDecider>(package.Factory, ruleDeciderInterfaceTypes);
			result.AddAutoCacheResetObject(package);
			return result;
		}

		public static NctsPackageValidationDeciderTestContext<INctsPackageDeparturePhase5ValidationDecider>
			CreateDeparturePhase5ValidationTestContext(this NctsPackage package, params Type[] ruleDeciderInterfaceTypes)
			=> package.CreateValidationTestContext<INctsPackageDeparturePhase5ValidationDecider>(ruleDeciderInterfaceTypes);

		public static NctsPackageValidationDeciderTestContext<INctsPackageArrivalPhase5ValidationDecider>
			CreateArrivalPhase5ValidationTestContext(this NctsPackage package, params Type[] ruleDeciderInterfaceTypes)
			=> package.CreateValidationTestContext<INctsPackageArrivalPhase5ValidationDecider>(ruleDeciderInterfaceTypes);

		public static NctsPackageValidationDeciderTestContext<INctsPackagePhase5ValidationDecider>
			CreatePhase5ValidationTestContext(this NctsPackage package, params Type[] ruleDeciderInterfaceTypes)
			=> package.CreateValidationTestContext<INctsPackagePhase5ValidationDecider>(ruleDeciderInterfaceTypes);
	}
}
