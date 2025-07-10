using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderProvider))]
	class CusAuthorisationHeaderProviderTest : EU.Business.Testing.EUCusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
	{
		protected override Type ExpectedRuleLookupsType => typeof(CusAuthorisationRuleLookups);
		protected override Type ExpectedRuleValidationType => typeof(CusAuthorisationRuleValidation);
		protected override Type ExpectedHeaderLookupsType => typeof(CusAuthorisationHeaderLookups);
		protected override Type ExpectedHeaderValidationType => typeof(CusAuthorisationHeaderValidation);

		[ExpectNoExceptions]
		public void TestGetRuleDescriptionForUsageRule()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescription(authorisationRule), Is.EqualTo(ZString.Empty), "Empty rule");
				authorisationRule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescription(authorisationRule), Is.EqualTo(CusAuthorisationUsageRuleList.Descriptions.AccreditedExporter).Using(CustomComparers.TypeComparison), "AEX Description");
			});
		}

		[ExpectNoExceptions]
		public void TestGetRuleDescriptionForReleaseRule()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Release;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescription(authorisationRule), Is.EqualTo(ZString.Empty), "Empty rule");
				authorisationRule.CPR_ValueFrom = CusAuthorisationReleaseRuleList.Codes._2;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleDescription(authorisationRule), Is.EqualTo(CusAuthorisationReleaseRuleList.Descriptions._2).Using(CustomComparers.TypeComparison), "Description");
			});
		}

		[ExpectNoExceptions]
		public void TestGetRuleValueFromMaxLength()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleValueFromMaxLength(authorisationRule), Is.EqualTo(CusAuthorisationRule.Schema.CPR_ValueFromMaxLength).Using(CustomComparers.TypeComparison), "RuleCode Location");
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleValueFromMaxLength(authorisationRule), Is.EqualTo(3).Using(CustomComparers.TypeComparison), "RuleCode 'USE'");
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.BusinessReference;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleValueFromMaxLength(authorisationRule), Is.EqualTo(CusAuthorisationRule.Schema.CPR_ValueFromMaxLength).Using(CustomComparers.TypeComparison), "RuleCode 'BRE'");
			});
		}

		[ExpectNoExceptions]
		public void TestGetRuleValueFromFieldType()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule), Is.EqualTo(nameof(FieldType.TextDropEdit)), "RuleCode 'USE'");
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.BusinessReference;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule), Is.EqualTo(nameof(FieldType.Text)), "RuleCode 'BRE'");
				authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule), Is.EqualTo(nameof(FieldType.Text)), "RuleCode 'LOC'");
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Release;
				NUnit.Framework.Assert.That(AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule), Is.EqualTo(nameof(FieldType.TextDropEdit)), "RuleCode 'REL'");
			});
		}

		[ExpectNoExceptions]
		public void TestGetNewAuthorisationRuleValidRepetitionForAuthorisationTypeSDE()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration).ToArray();
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(validAuthorisationRuleRequirement.Length, Is.EqualTo(2), "Number of rules for SDE");
				AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.Usage, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.Usage), 1, 1, true);
				AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.MandateReference, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.MandateReference), 0, 1, false,
					"1234567890", "1ABC456789", "The Reference length has to be 10 numeric digits.", NotificationType.MessageError);
			});
		}

		[ExpectNoExceptions]
		public void TestGetNewAuthorisationRuleValidRepetitionForForAuthorisationTypeEIR()
		{
			var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords).ToArray();
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(validAuthorisationRuleRequirement.Length, Is.EqualTo(4), "Number of rules for EIR");
				AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.Usage, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.Usage), 1, 1, true);
				AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.BusinessReference, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.BusinessReference), 0, 1, false);
				AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.MandateReference, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.MandateReference), 0, 1, false,
					"1234567890", "1ABC456789", "The Reference length has to be 10 numeric digits.", NotificationType.MessageError);
				AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.Release, validAuthorisationRuleRequirement.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.Release), 1, 1, true);
			});
		}

		[ExpectNoExceptions]
		public void TestGetValidLinkedAuthorizationRuleRepetitions_Location()
		{
			var repetitions = AuthorisationHeaderProvider.GetValidLinkedAuthorizationRuleRepetitions(Factory);
			var ruleRanges = repetitions[Customs.Business.CusAuthorisationRuleTypeList.Codes.Location];
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ruleRanges.Count, Is.EqualTo(1), "Number of rules for LOC");
				AssertLinkedRuleRange(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, ruleRanges.Single(x => x.RuleType == LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice), 1, 1);
			});
		}

		public void TestAdditionalMinAuthorisationCheck_USE()
		{
			const string message = "You are required to have at least 1 authorization rule of type 'USE'";
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			NUnit.Framework.Assert.Multiple(() =>
			{
				authorisationHeader.CPH_Number = "123";
				AssertNoErrorContaining("CPH_Number doesn't start with DE", authorisationHeader.CPH_NumberInfo, message);

				authorisationHeader.CPH_Number = "DE123";
				AssertHasErrorContaining("CPH_Number start with DE", authorisationHeader.CPH_NumberInfo, message);

				authorisationHeader.CPH_Number = "de123";
				AssertHasErrorContaining("CPH_Number start with de", authorisationHeader.CPH_NumberInfo, message);
			});
		}

		public void TestAdditionalMinAuthorisationCheck_REL()
		{
			const string message = "You are required to have at least 1 authorization rule of type 'REL'";
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			NUnit.Framework.Assert.Multiple(() =>
			{
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				authorisationRule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				authorisationHeader.Validation.ValidateCPH_Number();
				AssertNoErrorContaining("Has rule 'USE' with value 'AEX'", authorisationHeader.CPH_NumberInfo, message);

				authorisationRule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.FreeCirculation;
				authorisationHeader.Validation.ValidateCPH_Number();
				AssertHasErrorContaining("Has rule 'USE' with value 'IMP'", authorisationHeader.CPH_NumberInfo, message);

				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.BusinessReference;
				authorisationHeader.Validation.ValidateCPH_Number();
				AssertNoErrorContaining("Rule <> 'USE'", authorisationHeader.CPH_NumberInfo, message);

				authorisationHeader.CusAuthorisationRules.DeleteAll();
				authorisationHeader.Validation.ValidateCPH_Number();
				AssertNoErrorContaining("No rules", authorisationHeader.CPH_NumberInfo, message);
			});
		}

		protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => (CusAuthorisationHeaderProvider)authorisationHeader.Provider;

		protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.Germany;

		protected override CodeDescriptionPairList ExpectedRuleCodeListForModule
		{
			get
			{
				var expectedList = new Customs.Business.CusAuthorisationRuleTypeList();
				expectedList.AddRangeOverwriteIfExists(new CusAuthorisationRuleTypeList());
				expectedList.Sort();
				return expectedList;
			}
		}

		protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("OTH", "Other DE");
				result.AddPair("SAS", "Self-Assessment");
				result.AddPair("DPO", "Deferred");
				result.Sort();
				return result;
			}
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, null, grouping);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, "AUTH", "OTH", "Other DE", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
			base.SetUp();
		}
	}
}
