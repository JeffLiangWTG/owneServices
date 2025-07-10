using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderProvider))]
	public class CusAuthorisationHeaderProviderTest : EU.Business.Testing.EUCusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
	{
		protected override Type ExpectedHeaderLookupsType => typeof(CusAuthorisationHeaderLookups);

		protected override Type ExpectedRuleLookupsType => typeof(CusAuthorisationRuleLookups);

		protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => (CusAuthorisationHeaderProvider)authorisationHeader.Provider;

		protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.UnitedKingdom;

		protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
		{
			get
			{
				var authorizationTypes = new CDSAuthorisationHeaderTypeList();
				authorizationTypes.Sort();
				return authorizationTypes;
			}
		}

		protected override CodeDescriptionPairList ExpectedRuleCodeListForModule => new GBCusAuthorisationRuleTypeList();

		public void TestGetRuleDescription()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "AA01";
			header.OH_FullName = "AA01 Description";

			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No Rule", ZString.Empty, AuthorisationHeaderProvider.GetRuleDescription(null));
				authorisationRule.CPR_RuleCode = GBCusAuthorisationRuleTypeList.Codes.ORG;
				authorisationRule.CPR_ValueFrom = "AA01";
				AssertEquals("Correct Description", "AA01 Description", AuthorisationHeaderProvider.GetRuleDescription(authorisationRule));

				authorisationRule.CPR_RuleCode = GBCusAuthorisationRuleTypeBaseList.Codes.CTY;
				authorisationRule.CPR_ValueFrom = GBCusAuthorisationCountryCodePrefixList.Codes.GB;
				AssertEquals("Correct Description", GBCusAuthorisationCountryCodePrefixList.Descriptions.GB, AuthorisationHeaderProvider.GetRuleDescription(authorisationRule));
				authorisationRule.CPR_ValueFrom = GBCusAuthorisationCountryCodePrefixList.Codes.XI;
				AssertEquals("Correct Description", GBCusAuthorisationCountryCodePrefixList.Descriptions.XI, AuthorisationHeaderProvider.GetRuleDescription(authorisationRule));
			});
		}

		public void TestGetRuleValueFromFieldType()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No Rule", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(null));
				authorisationRule.CPR_RuleCode = GBCusAuthorisationRuleTypeList.Codes.ORG;
				AssertEquals("RuleCode 'ORG'", nameof(FieldType.TextCodeFindBox), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = GBCusAuthorisationRuleTypeBaseList.Codes.CTY;
				AssertEquals("RuleCode 'CTY'", nameof(FieldType.TextDropEdit), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

				authorisationRule.CPR_RuleCode = GBCusAuthorisationRuleTypeBaseList.Codes.Location;
				AssertEquals("RuleCode 'LOC'", nameof(FieldType.TextCodeFindBox), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationHeader.CPH_Type = CDSAuthorisationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				AssertEquals("RuleCode 'LOC'", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
			});
		}
		public void TestShowRelatedAuthorisationWithoutReference()
		{
			AssertEquals("ShowRelatedAuthorisationWithoutReference should be true for GB", true, AuthorisationHeaderProvider.ShowRelatedAuthorisationWithoutReference);
		}

		public void TestIsAgcNumberFieldALookup()
		{
			AssertEquals("IsAgcNumberFieldALookup should be false for GB", false, AuthorisationHeaderProvider.IsAgcNumberFieldALookup);
		}

		public void TestAuthorizationTypeDescription()
		{
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			CombineAssertions(() =>
			{
				var authorisationTypeList = AuthorisationHeaderProvider.GetAuthorisationTypeList(Factory);
				AssertArrayEqualsByElements("List", authorisationTypeList.GetAllCodes(), ExpectedAuthorisationTypeList.GetAllCodes());

				authorizationHeader.CPH_Type = CDSAuthorisationHeaderTypeList.Codes.AdvanceTariffRuling;
				AssertEquals("Description from CPH_Type", CDSAuthorisationHeaderTypeList.Descriptions.AdvanceTariffRuling, DescriptionPropertyAttribute.DescriptionFromBusinessObject(authorizationHeader));

				authorizationHeader.CPH_Type = CDSAuthorisationHeaderTypeList.Codes.AdvanceOriginRuling;
				AssertEquals("Description from CPH_Type", CDSAuthorisationHeaderTypeList.Descriptions.AdvanceOriginRuling, DescriptionPropertyAttribute.DescriptionFromBusinessObject(authorizationHeader));

				authorizationHeader.CPH_Type = CDSAuthorisationHeaderTypeList.Codes.AdvanceValuationRuling;
				AssertEquals("Description from CPH_Type", CDSAuthorisationHeaderTypeList.Descriptions.AdvanceValuationRuling, DescriptionPropertyAttribute.DescriptionFromBusinessObject(authorizationHeader));
			});
		}

		public void TestAllowMixedCaseAuthorisationNumbers()
		{
			var provider = authorisationHeader.Provider;
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			AssertEquals("AllowMixedCaseAuthorisationNumbers should be true for GB (ACR)", true, provider.AllowMixedCaseAuthorisationNumbers(authorisationHeader));
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			AssertEquals("AllowMixedCaseAuthorisationNumbers should be true for GB (ACE)", true, provider.AllowMixedCaseAuthorisationNumbers(authorisationHeader));
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			AssertEquals("AllowMixedCaseAuthorisationNumbers should be false for GB other types", false, provider.AllowMixedCaseAuthorisationNumbers(authorisationHeader));
		}
	}
}
