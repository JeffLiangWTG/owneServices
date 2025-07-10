using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class OfficeCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOfficeCodeListExportCustomOffice()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				var officeCode = declaration.CustomsOffices.AddNew();
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
				var officeCodeList = officeCode.Lookups.OfficeCodeList;
				officeCodeList.Load();
				CombineAssertions(() =>
				{
					AssertEquals("For PRE expected same Customs as EXP", 2, officeCodeList.Count);
					AssertContainsExactElementsInAnyOrder("For PRE expected both Customs", new[] { "IEDUB100", "ES009999" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

					officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
					officeCodeList = officeCode.Lookups.OfficeCodeList;
					officeCodeList.Load();
					AssertEquals("For EXT expected base behaviour", 2, officeCodeList.Count);
					AssertContainsExactElementsInAnyOrder("For EXT expected both Customs", new[] { "EINCUSTOM", "EXTCUSTOM" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

					officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExport;
					officeCodeList = officeCode.Lookups.OfficeCodeList;
					officeCodeList.Load();
					AssertEquals("For EXP expected base behaviour", 2, officeCodeList.Count);
					AssertContainsExactElementsInAnyOrder("For EXP expected both Customs", new[] { "IEDUB100", "ES009999" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
				});
			}
		}

		public void TestOfficeCodeListImportCustomOffice()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var officeCode = declaration.CustomsOffices.AddNew();

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent;
				var officeCodeList = officeCode.Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertEquals("For ENT expected base behaviour", 2, officeCodeList.Count);
				AssertContainsExactElementsInAnyOrder("For ENT expected", new[] { "ES001111", "ES002222" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

				officeCode.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
				officeCodeList = officeCode.Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertEquals("For SCO expected only SCO", 1, officeCodeList.Count);
				AssertContainsExactElementsInAnyOrder("For SCO expected", new[] { "ES003333" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

				CombineAssertions("When FilterBusinessObjectDefault for Customs Office code SCO", () =>
				{
					var listTypeProperty = $"{ZZRefCusCodeListFilters.ListType}:Property";
					AssertEquals(listTypeProperty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, officeCodeList.FilterBusinessObjectDefaults[listTypeProperty].Value);
					var attributeNameProperty = $"{ZZRefCusCodeListFilters.AttributeName}:Property";
					AssertEquals(attributeNameProperty, RefCusCodeListAttributeTypes.Codes.ROLE, officeCodeList.FilterBusinessObjectDefaults[attributeNameProperty].Value);
					var attributeValueComparisonOperator = $"{ZZRefCusCodeListFilters.AttributeValue}:ComparisonOperator";
					AssertEquals(attributeValueComparisonOperator, ModuleTextFilter.ComparisonConstants.Exact, officeCodeList.FilterBusinessObjectDefaults[attributeValueComparisonOperator].Value);
					var attributeValueProperty = $"{ZZRefCusCodeListFilters.AttributeValue}:Property";
					AssertEquals(attributeValueProperty, EuOfficeCodesTypes.Codes.AuthorityControlCode, officeCodeList.FilterBusinessObjectDefaults[attributeValueProperty].Value);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: eunZZZ);
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

			var irelandCustom = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var spanishCustom = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ES009999", "TEST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var einCustom = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "EINCUSTOM", "EXT CUSTOMS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var extCustom = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "EXTCUSTOM", "EIN CUSTOMS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(irelandCustom.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(spanishCustom.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(einCustom.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EIN");
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(extCustom.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");

			var spanishENT1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ES001111", "TEST1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var spanishENT2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ES002222", "TEST2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var spanishSCO = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ES003333", "TEST3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(spanishENT1.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent);
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(spanishENT2.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent);
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(spanishSCO.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.SupervisingCustomsOffice);
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
