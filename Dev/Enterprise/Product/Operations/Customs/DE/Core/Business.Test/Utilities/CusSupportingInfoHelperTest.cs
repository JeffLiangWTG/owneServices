using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusSupportingInfoHelperTest : TestCaseWithFactory
	{
		public void TestMissesAttribute_NullParameter() => Assert(CusSupportingInfoHelper.MissesAttribute(null, ZString.Empty));

		public void TestMissesAttribute()
		{
			var attributeName = "TestAttribute";
			PrepareRefCusCodesDataGrouping();
			helper.CreateCusCodeType(supportingDocumentOfExportDirectionCode, "Document Type (EU Box 44 Exports)");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, attributeName, supportingDocumentOfExportDirectionCode, germanCountryCode);

			var refCusCodeListWithAttribute = helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "EXP1", "Has attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeListWithAttribute.PK, attributeName, RefCusCodeListAttributes.Value.Yes);
			helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "EXP2", "Without attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var refCusCodeListCombinedWithAttribute = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "EXP1", germanCountryCode, supportingDocumentOfExportDirectionCode, ZDateTime.Today);
				var refCusCodeListCombinedWithoutAttribute = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "EXP2", germanCountryCode, supportingDocumentOfExportDirectionCode, ZDateTime.Today);
				AssertEquals("Has Attribute", false, CusSupportingInfoHelper.MissesAttribute(refCusCodeListCombinedWithAttribute, attributeName));
				AssertEquals("Attribute Missing", true, CusSupportingInfoHelper.MissesAttribute(refCusCodeListCombinedWithoutAttribute, attributeName));
			});
		}

		public void TestHasMandatoryProperty_NullParameter() => AssertEquals(false, CusSupportingInfoHelper.HasAttributeForMandatoryValidation(null, ZString.Empty));

		public void TestHasMandatoryProperty()
		{
			var attributeName = "TestAttribute";
			PrepareRefCusCodesDataGrouping();
			helper.CreateCusCodeType(supportingDocumentOfExportDirectionCode, "Document Type (EU Box 44 Exports)");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, attributeName, supportingDocumentOfExportDirectionCode, germanCountryCode);

			var refCusCodeListWithAttribute_y = helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "EXP1", "Has attribute - y", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeListWithAttribute_y.PK, attributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			var refCusCodeListWithAttribute_n = helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "EXP2", "Has attribute - n", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeListWithAttribute_n.PK, attributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No);
			helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "EXP3", "Without attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var refCusCodeListCombinedWithAttribute_y = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "EXP1", germanCountryCode, supportingDocumentOfExportDirectionCode, ZDateTime.Today);
				var refCusCodeListCombinedWithAttribute_n = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "EXP2", germanCountryCode, supportingDocumentOfExportDirectionCode, ZDateTime.Today);
				var refCusCodeListCombinedWithoutAttribute = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "EXP3", germanCountryCode, supportingDocumentOfExportDirectionCode, ZDateTime.Today);
				AssertEquals("Has Attribute - y", true, CusSupportingInfoHelper.HasAttributeForMandatoryValidation(refCusCodeListCombinedWithAttribute_y, attributeName));
				AssertEquals("Has Attribute - n", false, CusSupportingInfoHelper.HasAttributeForMandatoryValidation(refCusCodeListCombinedWithAttribute_n, attributeName));
				AssertEquals("Attribute Missing", false, CusSupportingInfoHelper.HasAttributeForMandatoryValidation(refCusCodeListCombinedWithoutAttribute, attributeName));
			});
		}

		public void TestGetDocumentCodesFilteredByLevelAttributes()
		{
			PrepareRefCusCodesDataGrouping();
			helper.CreateCusCodeType(supportingDocumentOfExportDirectionCode, "Document Type (EU Box 44 Exports)");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(levelAttributeName, levelAttributeName, supportingDocumentOfExportDirectionCode, germanCountryCode);
			helper.CreateCusCodeType(supportingDocumentOfImportDirectionCode, "Document Type (EU Box 44 Imports)");

			var codeWithHeaderAttribute = helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "CodeH", "Has Level-attribute 'Header'", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithHeaderAttribute.PK, levelAttributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Header);
			var codeWithItemAttribute = helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "CodeI", "Has Level-attribute 'Item'", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithItemAttribute.PK, levelAttributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Item);

			helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "Code", "Has no attributes", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfImportDirectionCode, "Code", "Has no attributes", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var result = CusSupportingInfoHelper.GetDocumentCodesFilteredByLevelAttributes(Factory, new ZString[] { supportingDocumentOfExportDirectionCode }, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Header);
				result.Load();
				AssertEquals("Parameters 'DC44E' 'Header': Count", 1, result.Count);
				AssertEquals("Parameters 'DC44E' 'Header': Code", "CodeH", result[0].ZZD_Code);

				result = CusSupportingInfoHelper.GetDocumentCodesFilteredByLevelAttributes(Factory, new ZString[] { supportingDocumentOfExportDirectionCode }, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Item);
				result.Load();
				AssertEquals("Parameters 'DC44E' 'Item': Count", 1, result.Count);
				AssertEquals("Parameters 'DC44E' 'Item': Code", "CodeI", result[0].ZZD_Code);

				result = CusSupportingInfoHelper.GetDocumentCodesFilteredByLevelAttributes(Factory, new ZString[] { supportingDocumentOfImportDirectionCode }, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Header);
				result.Load();
				AssertEquals("Parameters 'DC44I' 'Header': Count", 0, result.Count);

				result = CusSupportingInfoHelper.GetDocumentCodesFilteredByLevelAttributes(Factory, new ZString[] { supportingDocumentOfImportDirectionCode }, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Item);
				result.Load();
				AssertEquals("Parameters 'DC44I' 'Item': Count", 0, result.Count);
			});
		}

		public void TestGetDocumentCodesFilteredByLevelAttributesWithAdditionalFilter()
		{
			PrepareRefCusCodesDataGrouping();
			helper.CreateCusCodeType(supportingDocumentOfExportDirectionCode, "Document Type (EU Box 44 Exports)");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(levelAttributeName, levelAttributeName, supportingDocumentOfExportDirectionCode, germanCountryCode);

			var codeWithHeaderAttribute = helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "CodeH", "Has Level-attribute 'Header'", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithHeaderAttribute.PK, levelAttributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Header);
			var codeWithItemAttribute = helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "CodeI", "Has Level-attribute 'Item'", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithItemAttribute.PK, levelAttributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Item);
			var codeWithHeaderAttributeToBeFiltered = helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "CodeHFiltered", "To be filtered (level-attribute Header)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithHeaderAttributeToBeFiltered.PK, levelAttributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Header);
			var codeWithLevelAttributeToBeFiltered = helper.CreateNewOrGetExistingCusCodeList(germanCountryCode, supportingDocumentOfExportDirectionCode, "CodeIFiltered", "To be filtered (level-attribute Item)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithLevelAttributeToBeFiltered.PK, levelAttributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Item);
			Factory.Save();

			var excludeCodesQuery = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.NotEqual, new[] { "CodeIFiltered", "CodeHFiltered" });

			CombineAssertions(() =>
			{
				var result = CusSupportingInfoHelper.GetDocumentCodesFilteredByLevelAttributes(Factory, excludeCodesQuery, new ZString[] { supportingDocumentOfExportDirectionCode }, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Header);
				result.Load();
				AssertContainsExactElementsInAnyOrder("Parameters 'DC44E' 'Header'", new[] { "CodeH" }, result.Select(x => x.ZZD_Code));

				result = CusSupportingInfoHelper.GetDocumentCodesFilteredByLevelAttributes(Factory, excludeCodesQuery, new ZString[] { supportingDocumentOfExportDirectionCode }, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Item);
				result.Load();
				AssertContainsExactElementsInAnyOrder("Parameters 'DC44E' 'Item'", new[] { "CodeI" }, result.Select(x => x.ZZD_Code));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
		}

		UniversalReferenceTestDataHelper helper;
		string germanCountryCode => Core.Constants.CountryCodes.Germany;
		string supportingDocumentOfExportDirectionCode => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		string supportingDocumentOfImportDirectionCode => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
		string levelAttributeName => UniversalReferenceConstants.RefCusCodeListAttributes.Name.Level;

		void PrepareRefCusCodesDataGrouping() => helper.CreateNewOrGetExistingDataGrouping(germanCountryCode, "Germany", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union"));
	}
}
