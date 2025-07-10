using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderProvider))]
	public class CusAuthorisationHeaderProviderTest : EUCusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
	{
		[ExpectNoExceptions]
		public void TestGetAuthorisationTypeListWithCountry()
		{
			authorisationRule.Delete();
			linkedAuthorisationRule.Delete();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, "AUTH", "OTH", "Other EU", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, "AUTH", "OTI", "Other IT", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			CombineAssertions(() =>
			{
				var authorisationTypeListEU = AuthorisationHeaderProvider.GetAuthorisationTypeList(Factory);
				NUnit.Framework.Assert.That(authorisationTypeListEU.GetAllCodes(), NUnit.Framework.Is.EqualTo(new string[] { "DPO", "OTH", "SAS" }), "List for EU");
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetAuthorisationTypeList(Factory), NUnit.Framework.Is.SameAs(authorisationTypeListEU), "Cached List for EU");
			});
		}

		[ExpectNoExceptions]
		public void TestGetAuthorisationTypeListWithEuropeanUnionCustomsCodeInDescription()
		{
			authorisationRule.Delete();
			linkedAuthorisationRule.Delete();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: grouping);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, "AUTH", "OTE", "Other ES", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, "AUTH", "OTI", "Other IT", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "DPO", "C506", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
			helper.CreateCusMap("EUNAU", "SAS", "DUP", ZDateTime.MinSmallDateTimeValue.Date.AddDays(1), ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
			helper.CreateCusMap("EUNAU", "SAS", "C515", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
			helper.CreateCusMap("EUNAU", "SAS", "CSAS", ZDateTime.MinSmallDateTimeValue.Date.AddDays(1), ZDateTime.MaxSmallDateTimeValue.Date, Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap("EUNAU", "OTE", "CES", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap("EUNAU", "OTI", "CIT", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, Core.Constants.CountryCodes.Italy);
			Factory.Save();

			CombineAssertions(() =>
			{
				var authorisationHeaderSpain = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				authorisationHeaderSpain.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
				var authorisationHeaderProviderSpain = (CusAuthorisationHeaderProvider)authorisationHeaderSpain.Provider;
				var resultSpain = authorisationHeaderProviderSpain.GetAuthorisationTypeListWithEuropeanUnionCustomsCodeInDescription(Factory);
				NUnit.Framework.Assert.That(resultSpain.CodesAsString, NUnit.Framework.Is.EqualTo("DPO, OTE, SAS"), "CodesAsString for Spain");
				NUnit.Framework.Assert.That(resultSpain.GetDescriptionFromCode("DPO"), NUnit.Framework.Is.EqualTo("C506 - Deferred"), "Description for DPO for Spain");
				NUnit.Framework.Assert.That(resultSpain.GetDescriptionFromCode("SAS"), NUnit.Framework.Is.EqualTo("CSAS - Self-Assessment"), "Description for SAS for Spain");
				NUnit.Framework.Assert.That(resultSpain.GetDescriptionFromCode("OTE"), NUnit.Framework.Is.EqualTo("CES - Other ES"), "Description for OTE for Spain");
				NUnit.Framework.Assert.That(authorisationHeaderProviderSpain.GetAuthorisationTypeListWithEuropeanUnionCustomsCodeInDescription(Factory), NUnit.Framework.Is.SameAs(resultSpain), "Cached for Spain");

				var authorisationHeaderItaly = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				authorisationHeaderItaly.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
				var authorisationHeaderProviderItaly = (CusAuthorisationHeaderProvider)authorisationHeaderItaly.Provider;
				var resultItaly = authorisationHeaderProviderItaly.GetAuthorisationTypeListWithEuropeanUnionCustomsCodeInDescription(Factory);
				NUnit.Framework.Assert.That(resultItaly.CodesAsString, NUnit.Framework.Is.EqualTo("DPO, OTI, SAS"), "CodesAsString for Italy");
				NUnit.Framework.Assert.That(resultItaly.GetDescriptionFromCode("DPO"), NUnit.Framework.Is.EqualTo("C506 - Deferred"), "Description for DPO for Italy");
				NUnit.Framework.Assert.That(resultItaly.GetDescriptionFromCode("SAS"), NUnit.Framework.Is.EqualTo("C515 - Self-Assessment"), "Description for SAS for Italy");
				NUnit.Framework.Assert.That(resultItaly.GetDescriptionFromCode("OTI"), NUnit.Framework.Is.EqualTo("CIT - Other IT"), "Description for OTI for Italy");
				NUnit.Framework.Assert.That(authorisationHeaderProviderItaly.GetAuthorisationTypeListWithEuropeanUnionCustomsCodeInDescription(Factory), NUnit.Framework.Is.SameAs(resultItaly), "Cached for Italy");

				var resultEU = AuthorisationHeaderProvider.GetAuthorisationTypeListWithEuropeanUnionCustomsCodeInDescription(Factory);
				NUnit.Framework.Assert.That(resultEU.CodesAsString, NUnit.Framework.Is.EqualTo("DPO, SAS"), "CodesAsString for EU");
				NUnit.Framework.Assert.That(resultEU.GetDescriptionFromCode("DPO"), NUnit.Framework.Is.EqualTo("C506 - Deferred"), "Description for DPO for EU");
				NUnit.Framework.Assert.That(resultEU.GetDescriptionFromCode("SAS"), NUnit.Framework.Is.EqualTo("C515 - Self-Assessment"), "Description for SAS for EU");
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetAuthorisationTypeListWithEuropeanUnionCustomsCodeInDescription(Factory), NUnit.Framework.Is.SameAs(resultEU), "Cached for EU");
			});
		}

		[ExpectNoExceptions]
		public void TestShowRelatedAuthorisationWithoutReference()
		{
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.ShowRelatedAuthorisationWithoutReference, NUnit.Framework.Is.EqualTo(false), "ShowRelatedAuthorisationWithoutReference default should be false for EU");
		}

		[ExpectNoExceptions]
		public void TestIsAgcNumberFieldALookup()
		{
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.IsAgcNumberFieldALookup, NUnit.Framework.Is.EqualTo(true), "IsAgcNumberFieldALookup default should be true for EU");
		}

		[ExpectNoExceptions]
		public void TestGetRuleValueFromFieldType()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleValueFromFieldType(null), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "No Rule");
				authorisationHeader.CPH_Type = UniversalReferenceConstants.CusAuthorisationHeaderType.CustomsAuthorizedLocations;
				authorisationRule.CPR_RuleCode = ZString.Empty;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescriptionFieldType(authorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "Type 'CLO', Empty RuleCode");
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.Guid)), "Type 'CLO' & RuleCode 'LOC'");
			});
		}

		[ExpectNoExceptions]
		public void TestGetRuleDescriptionFieldType()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescriptionFieldType(null), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "No Rule");
				authorisationHeader.CPH_Type = UniversalReferenceConstants.CusAuthorisationHeaderType.CustomsAuthorizedLocations;
				authorisationRule.CPR_RuleCode = ZString.Empty;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescriptionFieldType(authorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "Type 'CLO', Empty RuleCode");
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescriptionFieldType(authorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.GuidDropEdit)), "Type 'CLO' & RuleCode 'LOC'");
			});
		}

		[ExpectNoExceptions]
		public void TestGetLinkedRuleValueFromFieldType_NoRule()
		{
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetLinkedRuleValueFromFieldType(null), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "No Rule");
		}

		[ExpectNoExceptions]
		public void TestGetLinkedRuleValueFromFieldType_UnknownRule()
		{
			linkedAuthorisationRule.CPR_RuleCode = "ABC";
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetLinkedRuleValueFromFieldType(linkedAuthorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "Unknown RuleCode");
		}

		[ExpectNoExceptions]
		public void TestGetLinkedRuleValueFromFieldType_ACT()
		{
			linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.Active;
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetLinkedRuleValueFromFieldType(linkedAuthorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.TextDropEdit)), "RuleCode 'ACT'");
		}

		[ExpectNoExceptions]
		public void TestGetLinkedRuleValueFromFieldType_IEB()
		{
			linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.IEB;
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetLinkedRuleValueFromFieldType(linkedAuthorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.TextDropEdit)), "RuleCode 'IEB'");
		}

		[ExpectNoExceptions]
		public void TestGetLinkedRuleValueFromFieldType_OFT()
		{
			linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.OfficeType;
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetLinkedRuleValueFromFieldType(linkedAuthorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.TextDropEdit)), "RuleCode 'OFT'");
		}

		[ExpectNoExceptions]
		public void TestSetupLinkedRuleDescriptionFunctions_NoRule()
		{
			NUnit.Framework.Assert.That(linkedAuthorisationRule.CPR_Description, NUnit.Framework.Is.EqualTo(ZString.Empty), "No Rule");
		}

		[ExpectNoExceptions]
		public void TestSetupLinkedRuleDescriptionFunctions_Active_Y()
		{
			linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.Active;
			linkedAuthorisationRule.CPR_ValueFrom = YesNoList.Codes.Yes;
			NUnit.Framework.Assert.That(linkedAuthorisationRule.CPR_Description, NUnit.Framework.Is.EqualTo(YesNoList.Descriptions.Yes).Using(CustomComparers.TypeComparison), "Rule Active, Value Y");
		}

		[ExpectNoExceptions]
		public void TestSetupLinkedRuleDescriptionFunctions_IEB_I()
		{
			linkedAuthorisationRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
			linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.IEB;
			linkedAuthorisationRule.CPR_ValueFrom = ImportExportList.Codes.Import;
			NUnit.Framework.Assert.That(linkedAuthorisationRule.CPR_Description, NUnit.Framework.Is.EqualTo(ImportExportList.Descriptions.Import).Using(CustomComparers.TypeComparison), "Rule Import/Export/Both, Value I");
		}

		[ExpectNoExceptions]
		public void TestSetupLinkedRuleDescriptionFunctions_OFT_A()
		{
			linkedAuthorisationRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
			linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.OfficeType;
			linkedAuthorisationRule.CPR_ValueFrom = LocationQualifierList.Codes.DesignatedPlace;
			NUnit.Framework.Assert.That(linkedAuthorisationRule.CPR_Description, NUnit.Framework.Is.EqualTo("Designated place").Using(CustomComparers.TypeComparison), "Rule Office Type, Value A");
		}

		[ExpectNoExceptions]
		public void TestGetLinkedRuleDescriptionFieldType_NoRule()
		{
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescriptionFieldType(null), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "No Rule");
		}

		[ExpectNoExceptions]
		public void TestGetLinkedRuleDescriptionFieldType_UnknownRule()
		{
			authorisationRule.CPR_RuleCode = "ABC";
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescriptionFieldType(authorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "Unknown RuleCode");
		}

		[ExpectNoExceptions]
		public void TestGetLinkedRuleDescriptionFieldType_ACT()
		{
			authorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.Active;
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescriptionFieldType(authorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "RuleCode 'ACT'");
		}

		[ExpectNoExceptions]
		public void TestGetLinkedRuleDescriptionFieldType_IEB()
		{
			authorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.IEB;
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescriptionFieldType(authorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "RuleCode 'IEB'");
		}

		[ExpectNoExceptions]
		public void TestGetLinkedRuleDescriptionFieldType_OFT()
		{
			authorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.OfficeType;
			NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescriptionFieldType(authorisationRule), NUnit.Framework.Is.EqualTo(nameof(FieldType.Text)), "RuleCode 'OFT'");
		}

		[ExpectNoExceptions]
		protected override void TestLinkedRulesRuleCodeReadOnly()
		{
			NUnit.Framework.Assert.That(authorisationHeader.Provider.IsLinkedRuleCodeReadOnly(linkedAuthorisationRule), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "LinkedRulesRuleCodeReadOnly");

			var linkedAuthorisationRuleWithoutHeader = Factory.New<LinkedCusAuthorisationRule>();
			NUnit.Framework.Assert.That(linkedAuthorisationRuleWithoutHeader.CPR_RuleCode_ReadOnly, NUnit.Framework.Is.EqualTo(true), "LinkedRulesRuleCodeReadOnly when AuthorisationHeader is not available");
		}
	}
}
