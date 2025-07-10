using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class EUUniversalLookupsHelperTest : TestCaseWithFactory
	{
		public void TestGetUCCAdditionalInformationList()
		{
			var config = new RefDataConfig(
				dataGroupings:
				[
					new(code: "EUN", description: "European Union", parent: ""),
					new(code: "LV", description: "Latvia", parent: "EUN"),
					new(code: "FR", description: "France", parent: "EUN")
				],
				cusCodeTypes:
				[
					new(typeCode: "TYPE1", description: "Test Type1 Desc.", attributeTypes: [new(name: "Level", dataGrouping: "EUN"), new(name: "Level", dataGrouping: "FR")],
						cusCodes:
						[
							new(code: "CODE1", dataGrouping: "FR", attributes: [new(name: "Level", value: "Header")]),
							new(code: "CODE2", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
							new(code: "CODE3", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
						]
					),
					new(typeCode: "TYPE2", description: "Test Type2 Desc.", attributeTypes: [new(name: "Level", dataGrouping: "EUN")],
						cusCodes:
						[
							new (code: "CODE4", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
						]
					)
				]
			);
			EUUniversalTestDataHelper.SetUpTestRefData(Factory, config);

			CombineAssertions("Include Parent DataGrouping", () =>
			{
				var list = EUUniversalLookupsHelper.GetUCCAdditionalInformationList(Factory, null, "FR", new ZString[] { "TYPE1" }, "Item", false, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly);
				AssertEquals("includeParentDataGrouping = ChildOnly: Load for FR only even empty", 0, list.Count);

				list = EUUniversalLookupsHelper.GetUCCAdditionalInformationList(Factory, null, "FR", new ZString[] { "TYPE1" }, "Item");
				AssertContainsExactElementsInAnyOrder("includeParentDataGrouping = ChildFirstThenParent: code for FR not exist", new ZString[] { "CODE3" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

				list = EUUniversalLookupsHelper.GetUCCAdditionalInformationList(Factory, null, "FR", new ZString[] { "TYPE1" }, "Header", false, RefCusCodeListTypes.IncludeParentDataGroupingOptions.Union);
				AssertContainsExactElementsInAnyOrder("includeParentDataGrouping = Union", new ZString[] { "CODE1", "CODE2", "CODE3" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			});

			CombineAssertions("Ignore Level", () =>
			{
				var list = EUUniversalLookupsHelper.GetUCCAdditionalInformationList(Factory, null, "EUN", new ZString[] { "TYPE1" }, "Item", ignoreLevel: true, RefCusCodeListTypes.IncludeParentDataGroupingOptions.Union);
				AssertContainsExactElementsInAnyOrder("IgnoreLevel = true", new ZString[] { "CODE2", "CODE3" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

				list = EUUniversalLookupsHelper.GetUCCAdditionalInformationList(Factory, null, "EUN", new ZString[] { "TYPE1" }, "Item", ignoreLevel: false, RefCusCodeListTypes.IncludeParentDataGroupingOptions.Union);
				AssertContainsExactElementsInAnyOrder("IgnoreLevel = false", new ZString[] { "CODE3" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			});

			CombineAssertions("Additional Filter", () =>
			{
				var excludeCodesQuery = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.NotEqual, "CODE1");
				var list = EUUniversalLookupsHelper.GetUCCAdditionalInformationList(Factory, excludeCodesQuery, "FR", new ZString[] { "TYPE1" }, "Header", ignoreLevel: false, RefCusCodeListTypes.IncludeParentDataGroupingOptions.Union);
				AssertContainsExactElementsInAnyOrder("includeParentDataGrouping = Union", new ZString[] { "CODE2", "CODE3" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			});

			CombineAssertions("Multiple codeTypes", () =>
			{
				var list = EUUniversalLookupsHelper.GetUCCAdditionalInformationList(Factory, null, "FR", new ZString[] { "TYPE1", "TYPE2" }, "Header", ignoreLevel: false, RefCusCodeListTypes.IncludeParentDataGroupingOptions.Union);
				AssertContainsExactElementsInAnyOrder("includeParentDataGrouping = Union", new ZString[] { "CODE1", "CODE2", "CODE3", "CODE4" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			});
		}

		public void TestGetSupportingDocumentList()
		{
			PrepareGetSupportingDocumentTestData();

			CombineAssertions(() =>
			{
				var cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Latvia, Import, Item);
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Import With Item", new[] { "SD01", "SD03", "SD05" }, cusCodeList.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));

				cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Latvia, Import, Header);
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Import With Header", new[] { "SD04" }, cusCodeList.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));

				cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Latvia, Export, Item);
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Export With Item", new[] { "SD03", "SD05" }, cusCodeList.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));

				cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Latvia, Export, Header);
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Export With Header", new[] { "SD02", "SD04" }, cusCodeList.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));

				cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Latvia, Import, "");
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Import With Item or Header", new[] { "SD01", "SD03", "SD04", "SD05" }, cusCodeList.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));

				cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Latvia, Export, "");
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Export With Item or Header", new[] { "SD02", "SD03", "SD04", "SD05" }, cusCodeList.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));

				cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Latvia, Both, "");
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Import or Export With Item or Header", new[] { "SD01", "SD02", "SD03", "SD03", "SD04", "SD04", "SD05", "SD05" }, cusCodeList.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
			});
		}

		public void TestGetSupportingDocumentList_ShouldLoadParentCodesTogether()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var eunCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("Y966", currentCountryCode), new TestSupportingDocumentCodeList("2800", eunCountryCode));
			Factory.Save();

			var list = Factory.GetSupportingDocumentList(currentCountryCode);
			list.Load();
			AssertContainsExactElementsInAnyOrder("It should load records from current country and parent(EU) together.", new[] { "Y966", "2800" }, list.Select(x => x.ZZD_Code));
		}

		public void TestGetSupportingDocumentList_ShouldLoadForIncludeParentGroupingMode()
		{
			PrepareGetSupportingDocumentTestData();
			CombineAssertions(() =>
			{
				var cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Latvia, Both, "", includeParentDataGroupings: RefCusCodeListTypes.IncludeParentDataGroupingOptions.Union);
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Return union of code for child and parent groupings", new[] { "SD01", "SD02", "SD03", "SD03", "SD04", "SD04", "SD05", "SD05" }, cusCodeList.Select(a => a.ZZD_Code));

				cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Latvia, Both, "", includeParentDataGroupings: RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly);
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Return only child grouping codes", new[] { "SD03", "SD03", "SD04", "SD04", "SD05", "SD05" }, cusCodeList.Select(a => a.ZZD_Code));

				cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Latvia, Both, "", includeParentDataGroupings: RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Return only child grouping codes", new[] { "SD03", "SD03", "SD04", "SD04", "SD05", "SD05" }, cusCodeList.Select(a => a.ZZD_Code));

				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland", euGrouping);
				Factory.Save();

				cusCodeList = Factory.GetSupportingDocumentList(Core.Constants.CountryCodes.Poland, Both, "", includeParentDataGroupings: RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
				cusCodeList.Load();
				AssertContainsExactElementsInAnyOrder("No codes for child grouping, should load from parent", new[] { "SD01", "SD02" }, cusCodeList.Select(a => a.ZZD_Code));
			});
		}

		public void TestGetSupportingDocumentList_ShouldNotLoadCodesFromOtherCountries()
		{
			var frCountryCode = Core.Constants.CountryCodes.France;
			var deCountryCode = Core.Constants.CountryCodes.Germany;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(deCountryCode))
			{
				Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("Y966", frCountryCode), new TestSupportingDocumentCodeList("3LLB81E", deCountryCode));
				Factory.Save();

				var list = Factory.GetSupportingDocumentList(deCountryCode);
				list.Load();
				AssertContainsExactElementsInAnyOrder("It should load code from current country(DE) only.", new[] { "3LLB81E" }, list.Select(x => x.ZZD_Code));
			}
		}

		public void TestGetSupportingDocumentList_ShouldLoadBothImportAndExportCodes()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2800", isImport: true), new TestSupportingDocumentCodeList("2800", isImport: false), new TestSupportingDocumentCodeList("3200", isImport: false));
			Factory.Save();

			var list = Factory.GetSupportingDocumentList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			list.Load();
			AssertEquals("There are two 2800 with different code types.", 3, list.Count);
			AssertContainsExactElementsInAnyOrder("It should load both Import and Export codes.", new[] { "2800", "2800", "3200" }, list.Select(x => x.ZZD_Code));
		}

		public void TestGetSupportingDocumentListWithPermitAttribute_ShouldOnlyLoadThoseHavePermitAttributeDefined()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2800", hasPermitAttribute: true), new TestSupportingDocumentCodeList("2801", isImport: false, hasPermitAttribute: true), new TestSupportingDocumentCodeList("2802", hasPermitAttribute: false));
			Factory.Save();

			var list = Factory.GetSupportingDocumentListWithPermitAttribute(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			list.Load();
			AssertContainsExactElementsInAnyOrder("It should only load those have permit attribute defined.", new[] { "2800", "2801" }, list.Select(x => x.ZZD_Code));
		}

		public void TestGetSupportingDocumentListWithPermitAttribute_ShouldOnlyLoadThoseHavePermitAttributeDefined_LoadByDifferentDirection()
		{
			Factory.CreateSupportingDocumentCodeLists(
				new TestSupportingDocumentCodeList("AAA", isImport: true, hasPermitAttribute: true),
				new TestSupportingDocumentCodeList("AAA", isImport: false, hasPermitAttribute: false),
				new TestSupportingDocumentCodeList("BBB", isImport: true, hasPermitAttribute: false),
				new TestSupportingDocumentCodeList("BBB", isImport: false, hasPermitAttribute: true));
			Factory.Save();

			var list = Factory.GetSupportingDocumentListWithPermitAttribute(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			list.Load();
			AssertContainsExactElementsInAnyOrder("Codes with permit attribute defined for import can be loaded.", new[] { "AAA" }, list.Select(x => x.ZZD_Code));

			list = Factory.GetSupportingDocumentListWithPermitAttribute(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			list.Load();
			AssertContainsExactElementsInAnyOrder("No codes should be loaded as no codes for export have permit attribute.", new[] { "BBB" }, list.Select(x => x.ZZD_Code));
		}

		public void TestGetAdditionalInformationList()
		{
			PrepareAdditionalInformationTestData();

			CombineAssertions(() =>
			{
				AssertEquals("Import With Item", "AD01, AD03, AD05", Factory.GetAdditionalInformationList(Core.Constants.CountryCodes.Latvia, Item, Import).CodesAsString);
				AssertEquals("Import With Header", "AD04, AD05", Factory.GetAdditionalInformationList(Core.Constants.CountryCodes.Latvia, Header, Import).CodesAsString);
				AssertEquals("Export With Item", "AD03, AD05", Factory.GetAdditionalInformationList(Core.Constants.CountryCodes.Latvia, Item, Export).CodesAsString);
				AssertEquals("Export With Header", "AD02, AD04, AD05", Factory.GetAdditionalInformationList(Core.Constants.CountryCodes.Latvia, Header, Export).CodesAsString);
				AssertEquals("Import With Item or Header", "AD01, AD03, AD04, AD05", Factory.GetAdditionalInformationList(Core.Constants.CountryCodes.Latvia, Both, Import).CodesAsString);
				AssertEquals("Export With Item or Header", "AD02, AD03, AD04, AD05", Factory.GetAdditionalInformationList(Core.Constants.CountryCodes.Latvia, Both, Export).CodesAsString);
				AssertEquals("Export With Item or Header", "AD01, AD02, AD03, AD04, AD05", Factory.GetAdditionalInformationList(Core.Constants.CountryCodes.Latvia, Both, Both).CodesAsString);
			});
		}

		public void TestGetAdditionalInformationCodeByDirectionAndSubType()
		{
			var dataGroupingEUN = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var dataGroupingLV = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", dataGroupingEUN);

			helper.CreateNewOrGetExistingCusCodeList(dataGroupingLV.ZZZ_DataGrouping, UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalInformation, "IMADD01", "IMADD01 INFO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingLV.ZZZ_DataGrouping, UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalReference, "IMREF02", "IMREF02 REF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingEUN.ZZZ_DataGrouping, UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "IMTR03", "IMTR03 TRANSP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeList(dataGroupingLV.ZZZ_DataGrouping, UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalInformation, "EXPADD01", "EXPADD01 INFO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingLV.ZZZ_DataGrouping, UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference, "EXPREF02", "EXPREF02 REF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingEUN.ZZZ_DataGrouping, UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, "EXPTR03", "EXPTR03 TRANSP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			CombineAssertions("Import direction", () =>
			{
				AssertEquals("Subtype: INF, IncludeParentDataGrouping: true", "IMADD01", Factory.GetAdditionalInformationCode(Core.Constants.CountryCodes.Latvia, "IMADD01", AdditionalInfoSubTypeList.Codes.AdditionalInformation, Import).ZZD_Code);
				AssertEquals("Subtype: REF, IncludeParentDataGrouping: true", "IMREF02", Factory.GetAdditionalInformationCode(Core.Constants.CountryCodes.Latvia, "IMREF02", AdditionalInfoSubTypeList.Codes.AdditionalReference, Import).ZZD_Code);
				AssertEquals("Subtype: TRA, IncludeParentDataGrouping: true", "IMTR03", Factory.GetAdditionalInformationCode(Core.Constants.CountryCodes.Latvia, "IMTR03", AdditionalInfoSubTypeList.Codes.TransportDocument, Import).ZZD_Code);
				AssertNull("Subtype: TRA, IncludeParentDataGrouping: false", Factory.GetAdditionalInformationCode(Core.Constants.CountryCodes.Latvia, "IMTR03", AdditionalInfoSubTypeList.Codes.TransportDocument, Import, includeParentDataGrouping: false));
			});

			CombineAssertions("Export direction", () =>
			{
				AssertEquals("Subtype: INF, IncludeParentDataGrouping: true", "EXPADD01", Factory.GetAdditionalInformationCode(Core.Constants.CountryCodes.Latvia, "EXPADD01", AdditionalInfoSubTypeList.Codes.AdditionalInformation, Export).ZZD_Code);
				AssertEquals("Subtype: REF, IncludeParentDataGrouping: true", "EXPREF02", Factory.GetAdditionalInformationCode(Core.Constants.CountryCodes.Latvia, "EXPREF02", AdditionalInfoSubTypeList.Codes.AdditionalReference, Export).ZZD_Code);
				AssertEquals("Subtype: TRA, IncludeParentDataGrouping: true", "EXPTR03", Factory.GetAdditionalInformationCode(Core.Constants.CountryCodes.Latvia, "EXPTR03", AdditionalInfoSubTypeList.Codes.TransportDocument, Export).ZZD_Code);
				AssertNull("Subtype: TRA, IncludeParentDataGrouping: false", Factory.GetAdditionalInformationCode(Core.Constants.CountryCodes.Latvia, "EXPTR03", AdditionalInfoSubTypeList.Codes.TransportDocument, Export, includeParentDataGrouping: false));
			});
		}

		public void TestGetTranNatureList()
		{
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Norway, "Norway", euGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "EU01", "EU01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "EU02", "EU02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Norway, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "NO03", "NO03", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Norway, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "NO04", "NO04", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("LV should contain the EU Tran Natures", "EU01, EU02", Factory.GetTranNatureList(Core.Constants.CountryCodes.Latvia).CodesAsString);
				AssertSame("List should be cached", Factory.GetTranNatureList(Core.Constants.CountryCodes.Latvia), Factory.GetTranNatureList(Core.Constants.CountryCodes.Latvia));
				AssertEquals("NO should contain the NO Tran Natures", "NO03, NO04", Factory.GetTranNatureList(Core.Constants.CountryCodes.Norway).CodesAsString);
			});
		}

		public void TestGetValuationMethodList()
		{
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ValuationMethod, "Valuation Method");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ValuationMethod, "3", "3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ValuationMethod, "6", "6", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ValuationMethod, "9", "9", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var valuationMethodList = Factory.GetValuationMethodList(Core.Constants.CountryCodes.Latvia);
			AssertEquals("9", valuationMethodList.CodesAsString);
		}

		public void TestGetAgreedPlaceCodeList()
		{
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: euGrouping);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "1", "EU IncoTerm 1", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "2", "EU IncoTerm 2", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "3", "EU IncoTerm 3", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "4", "DE IncoTerm 4", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "5", "DE IncoTerm 5", yesterday, tomorrow);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("LV should contain values only from EU (EUN)", "1, 2, 3", Factory.GetAgreedPlaceCodeList(Core.Constants.CountryCodes.Latvia).CodesAsString);
				AssertSame("Factory Cache around the list exists", Factory.GetCachedValue($"AgreedPlaceCodeList_{Core.Constants.CountryCodes.Latvia}", () => new CodeDescriptionPairList()), Factory.GetAgreedPlaceCodeList(Core.Constants.CountryCodes.Latvia));
				AssertEquals("DE should contain values only from DE and not EU (EUN)", "4, 5", Factory.GetAgreedPlaceCodeList(Core.Constants.CountryCodes.Germany).CodesAsString);
			});
		}

		public void TestGetCachedRatesByType()
		{
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Andorra, "Andorra", euGrouping);
			var euDtyRateType = helper.CreateCusRateType(euGrouping.ZZZ_DataGrouping, RefCusRateTypes.Dty);
			var euCvdRateType = helper.CreateCusRateType(euGrouping.ZZZ_DataGrouping, RefCusRateTypes.CountervailingDuty);
			var adDtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Andorra, RefCusRateTypes.Dty);
			var fjDtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Fiji, RefCusRateTypes.Dty);
			var fjVatRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Fiji, RefCusRateTypes.Vat);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, euDtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, euCvdRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent, adDtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnSugarContents, adDtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnFlourContents, fjDtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.Vat, fjVatRateType.PK);
			Factory.Save();

			CombineAssertions("DTY rates, AD data grouping (includes EUN rates)", () =>
			{
				var adDtyRates = Factory.GetCachedRatesByType(Core.Constants.CountryCodes.Andorra, RefCusRateTypes.Dty);
				AssertEquals("Count", 3, adDtyRates.Count());
				AssertEquals("Contains EA", true, adDtyRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent));
				AssertEquals("Contains A00", true, adDtyRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts));
				AssertEquals("Contains ADSZ", true, adDtyRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnSugarContents));
			});

			AssertEquals("VAT rates, AD data grouping (includes EUN rates)", 0, Factory.GetCachedRatesByType(Core.Constants.CountryCodes.Andorra, RefCusRateTypes.Vat).Count());

			CombineAssertions("DTY rates, FJ data grouping", () =>
			{
				var fjDtyRates = Factory.GetCachedRatesByType(Core.Constants.CountryCodes.Fiji, RefCusRateTypes.Dty);
				AssertEquals("Count", 1, fjDtyRates.Count());
				AssertEquals("Contains ADFM", true, fjDtyRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnFlourContents));
			});

			CombineAssertions("VAT rates, FJ data grouping", () =>
			{
				var fjVatRates = Factory.GetCachedRatesByType(Core.Constants.CountryCodes.Fiji, RefCusRateTypes.Vat);
				AssertEquals("Count", 1, fjVatRates.Count());
				AssertEquals("Contains B00", true, fjVatRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.Vat));
			});

			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts, euDtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat, fjVatRateType.PK);
			Factory.Save();

			AssertEquals("DTY rates, AD data grouping, cached count (after adding another rate)", 3, Factory.GetCachedRatesByType(Core.Constants.CountryCodes.Andorra, RefCusRateTypes.Dty).Count());
			AssertEquals("VAT rates, FJ data grouping, cached count (after adding another rate)", 1, Factory.GetCachedRatesByType(Core.Constants.CountryCodes.Fiji, RefCusRateTypes.Vat).Count());

			CombineAssertions("DTY+CVD rates, AD data grouping (includes EUN rates, new cache key DTY_CVD)", () =>
			{
				var adDtyCvdRates = Factory.GetCachedRatesByType(Core.Constants.CountryCodes.Andorra, RefCusRateTypes.Dty, RefCusRateTypes.CountervailingDuty);
				AssertEquals("Count", 5, adDtyCvdRates.Count());
				AssertEquals("Contains EA", true, adDtyCvdRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent));
				AssertEquals("Contains A00", true, adDtyCvdRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts));
				AssertEquals("Contains ADSZ", true, adDtyCvdRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnSugarContents));
				AssertEquals("Contains A10", true, adDtyCvdRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts));
				AssertEquals("Contains A40", true, adDtyCvdRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty));
			});

			CombineAssertions("All rates, FJ data grouping", () =>
			{
				var fjRates = Factory.GetCachedRatesByType(Core.Constants.CountryCodes.Fiji);
				AssertEquals("Count", 3, fjRates.Count());
				AssertEquals("Contains ADFM", true, fjRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnFlourContents));
				AssertEquals("Contains B00", true, fjRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.Vat));
				AssertEquals("Contains B10", true, fjRates.Any(r => r.ZY1_RateCode == UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat));
			});
		}

		public void TestGetWorstCaseAdditionalCodes()
		{
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "Additional Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, "Default Rate", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var lastMonth = ZDateTime.Today.AddMonths(-1);
			var today = ZDateTime.Today;
			var nextMonth = ZDateTime.Today.AddMonths(1);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "1", "EU ADDCD 1", ZDateTime.MinSmallDateTimeValue, lastMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "2", "EU ADDCD 2", lastMonth, nextMonth);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "3", "EU ADDCD 3", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "4", "EU ADDCD 4", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "5", "EU ADDCD 5", nextMonth, ZDateTime.MaxSmallDateTimeValue)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			Factory.Save();

			AssertEquals("Get from parent if the children list is empty.", "3, 4", new BusinessObjectFactory().GetWorstCaseAdditionalCodes(Core.Constants.CountryCodes.Latvia, today).CodesAsString);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, "Default Rate", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "A", "FR ADDCD A", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "B", "FR ADDCD B", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			Factory.Save();

			AssertEquals("Get from children list if there is any.", "A, B", new BusinessObjectFactory().GetWorstCaseAdditionalCodes(Core.Constants.CountryCodes.Latvia, today).CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		}
		UniversalReferenceTestDataHelper helper;
		RefDataGrouping euGrouping;

		void PrepareGetSupportingDocumentTestData()
		{
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);

			var eunCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var latviaCountryCode = Core.Constants.CountryCodes.Latvia;
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Level, "EU Level Import", importCodeType, eunCountryCode);
			attributeNameValuePairs.Add(Level, new[] { Item });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(eunCountryCode, new string[] { importCodeType }, "SD01", "SD01 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Level, "EU Level Export", exportCodeType, eunCountryCode);
			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add(Level, new[] { Header });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(eunCountryCode, new string[] { exportCodeType }, "SD02", "SD02 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add(Level, new[] { Item });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(latviaCountryCode, new string[] { importCodeType, exportCodeType }, "SD03", "SD03 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add(Level, new[] { Header });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(latviaCountryCode, new string[] { importCodeType, exportCodeType }, "SD04", "SD04 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add(Level, new[] { Item, Header });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(latviaCountryCode, new string[] { importCodeType, exportCodeType }, "SD05", "SD05 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		void PrepareAdditionalInformationTestData()
		{
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Direction, "EU Direction", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Level, "EU Level", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var additionalInformationcusCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "AD01", "AD01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			additionalInformationcusCode1.Attributes.AddNew(Direction, Import);
			additionalInformationcusCode1.Attributes.AddNew(Level, Item);

			var additionalInformationcusCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "AD02", "AD02 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			additionalInformationcusCode2.Attributes.AddNew(Direction, Export);
			additionalInformationcusCode2.Attributes.AddNew(Level, Header);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Direction, "LV Direction", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Level, "LV Level", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.CountryCodes.Latvia);
			var additionalInformationcusCode3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "AD03", "AD03 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			additionalInformationcusCode3.Attributes.AddNew(Direction, Import);
			additionalInformationcusCode3.Attributes.AddNew(Direction, Export);
			additionalInformationcusCode3.Attributes.AddNew(Level, Item);

			var additionalInformationcusCode4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "AD04", "AD04 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			additionalInformationcusCode4.Attributes.AddNew(Direction, Import);
			additionalInformationcusCode4.Attributes.AddNew(Direction, Export);
			additionalInformationcusCode4.Attributes.AddNew(Level, Header);

			var additionalInformationcusCode5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "AD05", "AD05 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			additionalInformationcusCode5.Attributes.AddNew(Direction, Import);
			additionalInformationcusCode5.Attributes.AddNew(Direction, Export);
			additionalInformationcusCode5.Attributes.AddNew(Level, Item);
			additionalInformationcusCode5.Attributes.AddNew(Level, Header);

			Factory.Save();
		}

		public static void CreateSupportingDocumentCodeLists(BusinessObjectFactory factory, params TestSupportingDocumentCodeList[] codeLists)
		{
			SetEuropeanUnionAsParentIfRequired(factory);

			var helper = new UniversalReferenceTestDataHelper(factory);
			foreach (var codeList in codeLists)
			{
				var codeType = codeList.IsImport ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				var attributes = new Dictionary<string, string[]>();
				if (codeList.HasPermitAttribute)
				{
					attributes.Add(RefCusCodeListAttributeTypes.Codes.Permit, new string[] { "" });
				}
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(codeList.Country, new[] { codeType }, codeList.Code, codeList.Code, attributes, ZDateTime.Today, ZDateTime.Today.AddDays(1));
			}
		}

		public void TestGetEXNATCountryList() => CombineAssertions(() =>
		{
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var eurpoeanUnionCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);
			helper.CreateNewOrGetExistingCusCodeType("EXNAT", "CountryList – Export Nationality");
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, "EXNAT", Core.Constants.CountryCodes.Australia, "Australien", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, "EXNAT", Core.Constants.CountryCodes.Germany, "Deutschland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, "EXNAT", Core.Constants.CountryCodes.France, "Frankreich", startDate, endDate);
			Factory.Save();

			var countries = Factory.GetEXNATCountryList();
			countries.Load();

			var expectedCodes = new ZString[] { "AU", "DE", "FR" };
			AssertContainsExactElementsInAnyOrder(expectedCodes, countries.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

			var listTypeFilter = countries.FilterBusinessObjectDefaults["List Type:Property"];
			AssertEquals("List Type - Value", "EXNAT", listTypeFilter.Value);
			AssertEquals("List Type - IsRemovable", false, listTypeFilter.IsRemovable);

			var dataGroupingFilter = countries.FilterBusinessObjectDefaults["Country/Region or Grouping:Property"];
			AssertEquals("Data Grouping - Value", "EUN", dataGroupingFilter.Value);
			AssertEquals("Data Grouping - IsRemovable", false, dataGroupingFilter.IsRemovable);

			AssertSame("Cached", countries, Factory.GetEXNATCountryList());
		});

		public void TestGetUCCAuthorizationCodeCustomsValue() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, "cw1", "customs", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();
			AssertEquals("not found", ZString.Empty, EUUniversalLookupsHelper.GetUCCAuthorizationCodeCustomsValue(Factory, "xxx"));
			AssertEquals("cw1", "customs", EUUniversalLookupsHelper.GetUCCAuthorizationCodeCustomsValue(Factory, "cw1"));
		});

		public void TestGetEuropeanUnionAuthorizationCustomsCodeMaps()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMapType("EUCTY", MapDirectionList.Codes.OUT, "EU ISO country code or UNLOCO to fake EU country code", true);
			helper.CreateCusMap(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, "ACE", "C522", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, "ACP", "C511", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap("EUCTY", "PR", "US", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Ireland);
			helper.CreateCusMap(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, "ACP", "CSAS", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, "OTE", "CES", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, "OTI", "CIT", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Core.Constants.CountryCodes.Italy);
			Factory.Save();

			CombineAssertions(() =>
			{
				var maps = EUUniversalLookupsHelper.GetEuropeanUnionAuthorizationCustomsCodeMaps(Factory, ZString.Empty);
				AssertEquals("Length when no currentCountryCode added", 2, maps.Length);
				AssertEquals("ZZM_CustomsValue values when no currentCountryCode added", "C522, C511", maps.Select(x => x.ZZM_CustomsValue).JoinAsString(", "));
				AssertSame("Cached when no currentCountryCode added", maps, EUUniversalLookupsHelper.GetEuropeanUnionAuthorizationCustomsCodeMaps(Factory, ZString.Empty));

				maps = EUUniversalLookupsHelper.GetEuropeanUnionAuthorizationCustomsCodeMaps(Factory, Core.Constants.CountryCodes.Spain);
				AssertEquals("Length when currentCountryCode is ES", 4, maps.Length);
				AssertEquals("ZZM_CustomsValue values when currentCountryCode is ES", "CSAS, CES, C522, C511", maps.Select(x => x.ZZM_CustomsValue).JoinAsString(", "));
				AssertSame("Cached when currentCountryCode is ES", maps, EUUniversalLookupsHelper.GetEuropeanUnionAuthorizationCustomsCodeMaps(Factory, Core.Constants.CountryCodes.Spain));

				maps = EUUniversalLookupsHelper.GetEuropeanUnionAuthorizationCustomsCodeMaps(Factory, Core.Constants.CountryCodes.Italy);
				AssertEquals("Length when currentCountryCode is IT", 3, maps.Length);
				AssertEquals("ZZM_CustomsValue values when currentCountryCode is IT", "CIT, C522, C511", maps.Select(x => x.ZZM_CustomsValue).JoinAsString(", "));
				AssertSame("Cached when currentCountryCode is IT", maps, EUUniversalLookupsHelper.GetEuropeanUnionAuthorizationCustomsCodeMaps(Factory, Core.Constants.CountryCodes.Italy));
			});
		}

		public void TestGetSupportingDocumentList_ShouldLoadCodesFromDataGroupings()
		{
			var ieCountryCode = "IE";
			var ie5GroupingCode = "IE5";
			var zzzGroupingCode = "ZZZ";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(ieCountryCode))
			{
				Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("Y966", ieCountryCode), new TestSupportingDocumentCodeList("3LLB81E", ie5GroupingCode), new TestSupportingDocumentCodeList("Z999", zzzGroupingCode));
				Factory.Save();

				CombineAssertions(() =>
				{
					var list = Factory.GetSupportingDocumentList(new ZString[] { ieCountryCode, ie5GroupingCode });
					list.Load();
					AssertContainsExactElementsInAnyOrder("It should load code from other grouping code.", new[] { "Y966", "3LLB81E" }, list.Select(x => x.ZZD_Code));

					list = Factory.GetSupportingDocumentList(new ZString[] { ieCountryCode, ie5GroupingCode, zzzGroupingCode });
					list.Load();
					AssertContainsExactElementsInAnyOrder("It should load code from other grouping codes.", new[] { "Y966", "3LLB81E", "Z999" }, list.Select(x => x.ZZD_Code));
				});
			}
		}

		[TestDate(2022, 07, 01)]

		public void TestGetCountryAddressPostcodeOnlyList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType("CL148", "CL148 Desc.");
			helper.CreateCusCodeList("EUN", "CL148", "C1", "Country in the list CL148", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			helper.CreateCusCodeList("EUN", "CL148", "C2", "Country in the list CL148", new ZDateTime(2022, 06, 01), new ZDateTime(2022, 12, 31));
			helper.CreateCusCodeList("TST", "CL148", "C3", "Country with TST grouping", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			Factory.Save();

			CombineAssertions(() =>
			{
				var countries = Factory.GetCountryAddressPostcodeOnlyList();
				AssertEquals("Filtered by default EUN grouping.", true, countries.ContainsOnly("C1","C2"));

				countries = Factory.GetCountryAddressPostcodeOnlyList(dataGrouping: "TST");
				AssertEquals("Filtered by specified grouping.", true, countries.ContainsOnly("C3"));

				countries = Factory.GetCountryAddressPostcodeOnlyList(date: new ZDateTime(2022, 03, 01));
				AssertEquals("Filtered by date.", true, countries.ContainsOnly("C1"));
			});
		}

		public void TestGetCL030XMLErrorCodeListOnlyList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland);
			helper.CreateNewOrGetExistingCusCodeType("CL030", "CL030 Desc.");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL030", "12", "EU-Incorrect enumeration", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL030", "18", "EU-Unspecified Error / Other", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, "CL030", "22", "PL-Incorrect enumeration", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, "CL030", "28", "PL-Unspecified Error / Other", yesterday, tomorrow);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Filtered by default EUN grouping.", "12, 18", Factory.GetCL030XMLErrorCodeListOnlyList().CodesAsString);
				AssertEquals("Filtered by PL", "22, 28", Factory.GetCL030XMLErrorCodeListOnlyList(Core.Constants.CountryCodes.Poland).CodesAsString);
			});
		}

		public void TestGetCL180FunctionalErrorCodeListOnlyList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland);
			helper.CreateNewOrGetExistingCusCodeType("CL180", "CL180 Desc.");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL180", "14", "EU-Rule violation", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL180", "92", "EU-Message out of sequence", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, "CL180", "24", "PL-Rule violation", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, "CL180", "102", "PL-Message out of sequence", yesterday, tomorrow);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Filtered by default EUN grouping.", "14, 92", Factory.GetCL180FunctionalErrorCodeListOnlyList().CodesAsString);
				AssertEquals("Filtered by PL", "24, 102", Factory.GetCL180FunctionalErrorCodeListOnlyList(Core.Constants.CountryCodes.Poland).CodesAsString);
			});
		}

		public void TestGet104IMCodeListOnlyList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingCusCodeType("104IM", "104IM Desc.");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "104IM", "A", "EU-A", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "104IM", "B", "EU-B", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, "104IM", "C", "FR-C", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, "104IM", "D", "FR-D", yesterday, tomorrow);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Filtered by default EUN grouping.", "A, B", Factory.Get104IMCodeListOnlyList().CodesAsString);
				AssertEquals("Filtered by FR", "C, D", Factory.Get104IMCodeListOnlyList(Core.Constants.CountryCodes.France).CodesAsString);
			});
		}

		public void TestGetCountryNC008List_DateGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			CreateCountryListOfAU_DE_FR_ForCountryAndEU(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			var countryOfDestinationList = Factory.GetCachedCountryNC008List(Core.Constants.CountryCodes.Latvia);
			CombineAssertions(() =>
			{
				AssertEquals("AU, DE, FR", countryOfDestinationList.CodesAsString);
				AssertSame("Cached", countryOfDestinationList, Factory.GetCachedCountryNC008List(Core.Constants.CountryCodes.Latvia));
			});
		}

		public void TestGetCountryNC008List_DateGroup_NoDuplicates()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			CreateCountryListOfAU_DE_FR_ForCountryAndEU(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008, Core.Constants.CountryCodes.Germany);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var countryOfDestinationList = Factory.GetCachedCountryNC008List(Core.Constants.CountryCodes.Germany);
				CombineAssertions(() =>
				{
					AssertEquals("CodesAsString", "AU, DE, FR", countryOfDestinationList.CodesAsString);
					AssertEquals("No duplicates should be generated EU and Country contain 3", 3, countryOfDestinationList.Count);
					AssertSame("Cached", countryOfDestinationList, Factory.GetCachedCountryNC008List(Core.Constants.CountryCodes.Germany));
					AssertNotSame("Cached different country", countryOfDestinationList, Factory.GetCachedCountryNC008List(Core.Constants.CountryCodes.Latvia));
				});
			}
		}

		public void TestGetCountryNC008List_Fallback()
		{
			var countryOfDestinationList = Factory.GetCachedCountryNC008List(Core.Constants.CountryCodes.Latvia);
			var fallbackCountries = new RefCountryCollection(Factory, new ZQuery(RefCountrySchema.RN_IsActive, true));
			AssertContainsExactElementsInAnyOrder("RefCountry fallback", fallbackCountries.Select(x => x.Code).ToArray(), countryOfDestinationList.GetAllCodesZString());
		}

		static void SetEuropeanUnionAsParentIfRequired(BusinessObjectFactory factory)
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var eunCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(currentCountryCode))
			{
				var helper = new UniversalReferenceTestDataHelper(factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(eunCountryCode, "European Union");
				helper.CreateNewOrGetExistingDataGrouping(currentCountryCode, currentCountryCode, eun);
			}
		}

		void CreateCountryListOfAU_DE_FR_ForCountryAndEU(BusinessObjectFactory factory, string codeType, string countryOrDataGrouping = Core.Constants.CountryCodes.Latvia)
		{
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var eurpoeanUnionCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunId = helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);
			helper.CreateNewOrGetExistingDataGrouping(countryOrDataGrouping, parent: eunId);

			helper.CreateNewOrGetExistingCusCodeType(codeType, "CountryList – NCTS");
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Australia, "Australien", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Germany, "Deutschland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.France, "Frankreich", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(countryOrDataGrouping, codeType, Core.Constants.CountryCodes.Australia, "Australien", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(countryOrDataGrouping, codeType, Core.Constants.CountryCodes.Germany, "Deutschland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(countryOrDataGrouping, codeType, Core.Constants.CountryCodes.France, "Frankreich", startDate, endDate);
		}

		const string Level = RefCusCodeListAttributeTypes.Codes.Level;
		const string Direction = RefCusCodeListAttributeTypes.Codes.Direction;
		const string Item = UniversalReferenceConstants.RefCusCodeListLevelType.Item;
		const string Header = UniversalReferenceConstants.RefCusCodeListLevelType.Header;
		const string Import = UniversalReferenceConstants.RefCusCodeListDirectionType.Import;
		const string Export = UniversalReferenceConstants.RefCusCodeListDirectionType.Export;
		const string Both = UniversalReferenceConstants.RefCusCodeListLevelType.Both;
	}
}
