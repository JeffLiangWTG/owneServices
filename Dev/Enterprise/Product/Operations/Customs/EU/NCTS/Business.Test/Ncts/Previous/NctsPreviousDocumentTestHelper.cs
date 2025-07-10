using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentTestHelper
	{
		internal static void SetupCusCodes(BusinessObjectFactory factory, string correctCodeType, string incorrectCodeType)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);
			helper.CreateNewOrGetExistingCusCodeType(correctCodeType, correctCodeType);
			helper.CreateNewOrGetExistingCusCodeType(incorrectCodeType, incorrectCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", correctCodeType, Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", incorrectCodeType, Core.Constants.CountryCodes.Latvia);

			var valid1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, correctCodeType, $"{correctCodeType}_1", "Valid1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(valid1.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			var valid2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, correctCodeType, $"{correctCodeType}_2", "Valid2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(valid2.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			var valid3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, correctCodeType, $"{correctCodeType}_3", "Valid code from parent grouping", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(valid3.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);

			var invalid1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, correctCodeType, "Invalid1", "Invalid country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(invalid1.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			var invalid2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, correctCodeType, "Invalid2", "Invalid attribute Level", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(invalid2.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, correctCodeType, "Invalid3", "No attribute Level", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var invalid4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, incorrectCodeType, "Invalid4", "Invalid code type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(invalid4.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
		}

		internal static void SetupCusCodes_IncludeParentDataGrouping(BusinessObjectFactory factory, string correctCodeType, string incorrectCodeType)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			helper.CreateNewOrGetExistingCusCodeType(correctCodeType, correctCodeType);
			helper.CreateNewOrGetExistingCusCodeType(incorrectCodeType, incorrectCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", correctCodeType, Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", incorrectCodeType, Core.Constants.CountryCodes.Latvia);

			var valid1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, correctCodeType, $"{correctCodeType}_1", "Valid code from parent grouping", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(valid1.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);

			var invalid1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, correctCodeType, "Invalid1", "Invalid country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(invalid1.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			var invalid2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, correctCodeType, "Invalid2", "Invalid attribute Level", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(invalid2.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, correctCodeType, "Invalid3", "No attribute Level", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var invalid4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, incorrectCodeType, "Invalid4", "Invalid code type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(invalid4.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
		}
	}
}
