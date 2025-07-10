using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CusAuthorisationHeaderProvider))]
sealed class CusAuthorisationHeaderProviderTest : EU.Business.Testing.EUCusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
{
	protected override Type ExpectedHeaderLookupsType => typeof(CusAuthorisationHeaderLookups);
	protected override Type ExpectedRuleLookupsType => typeof(CusAuthorisationRuleLookups);
	protected override bool ExpectedShowCustomsCode => true;

	public void TestGetRuleDescriptionForDocumentRule()
	{
		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
		authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Document;
		authorisationRule.CPR_ValueFrom = "INVL";
		CusAuthorisationTestHelper.SetupSupportingDocumentData(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("Invalid rule", ZString.Empty, AuthorisationHeaderProvider.GetRuleDescription(authorisationRule));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authorisationRule.CPR_ValueFrom = "9112";
			AssertEquals("DC44E Description", "Drugs Precursor Chemicals Individual Licence", AuthorisationHeaderProvider.GetRuleDescription(authorisationRule));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			authorisationRule.CPR_ValueFrom = "9004";
			AssertEquals("DC44I Description", "Certificates of origin for steel quotas.", AuthorisationHeaderProvider.GetRuleDescription(authorisationRule));
		});
	}

	public void TestGetDocRuleValueFromMaxLength()
	{
		CombineAssertions(() =>
		{
			authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("RuleCode Location", CusAuthorisationRule.Schema.CPR_ValueFromMaxLength, AuthorisationHeaderProvider.GetRuleValueFromMaxLength(authorisationRule));
			authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Document;
			AssertEquals("RuleCode 'DOC'", 4, AuthorisationHeaderProvider.GetRuleValueFromMaxLength(authorisationRule));
		});
	}

	public void TestGetDocRuleValueFromFieldType()
	{
		authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Document;
		AssertEquals("RuleCode 'DOC'", nameof(FieldType.TextCodeFindBox), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
		authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Location;
		AssertEquals("RuleCode 'LOC'", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
		authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Use;
		AssertEquals("RuleCode 'USE'", nameof(FieldType.TextDropEdit), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
	}

	public void TestGetNewAuthorisationRuleValidRepetitionForAuthorisationTypeCWP()
	{
		var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
		AssertRuleRequirement(ITCusAuthorisationRuleTypeList.Codes.Document, validAuthorisationRuleRequirement.Single(x => x.RuleType == ITCusAuthorisationRuleTypeList.Codes.Document), 0, 1, false);
	}

	public void TestGetNewAuthorisationRuleValidRepetitionForAuthorisationTypeCW1()
	{
		var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1);
		AssertRuleRequirement(ITCusAuthorisationRuleTypeList.Codes.Document, validAuthorisationRuleRequirement.Single(x => x.RuleType == ITCusAuthorisationRuleTypeList.Codes.Document), 0, 1, false);
	}

	public void TestGetNewAuthorisationRuleValidRepetitionForAuthorisationTypeCW2()
	{
		var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2);
		AssertRuleRequirement(ITCusAuthorisationRuleTypeList.Codes.Document, validAuthorisationRuleRequirement.Single(x => x.RuleType == ITCusAuthorisationRuleTypeList.Codes.Document), 0, 1, false);
	}

	public void TestGetNewAuthorisationRuleValidRepetitionForAuthorisationTypeIPO()
	{
		var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
		AssertRuleRequirement(ITCusAuthorisationRuleTypeList.Codes.Document, validAuthorisationRuleRequirement.Single(x => x.RuleType == ITCusAuthorisationRuleTypeList.Codes.Document), 0, 1, false);
	}

	public void TestGetNewAuthorisationRuleValidRepetitionForAuthorisationTypeOPO()
	{
		var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
		AssertRuleRequirement(ITCusAuthorisationRuleTypeList.Codes.Document, validAuthorisationRuleRequirement.Single(x => x.RuleType == ITCusAuthorisationRuleTypeList.Codes.Document), 0, 1, false);
	}

	public void TestGetNewAuthorisationRuleValidRepetitionForAuthorisationTypeDOI()
	{
		var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent);
		AssertRuleRequirement(ITCusAuthorisationRuleTypeList.Codes.Use, validAuthorisationRuleRequirement.Single(x => x.RuleType == ITCusAuthorisationRuleTypeList.Codes.Use), 1, 1, false);
	}

	public void TestGetNewAuthorisationRuleValidRepetitionForAuthorisationTypeACR()
	{
		var validAuthorisationRuleRequirement = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
		AssertRuleRequirement(ITCusAuthorisationRuleTypeList.Codes.Use, validAuthorisationRuleRequirement.Single(x => x.RuleType == ITCusAuthorisationRuleTypeList.Codes.Document), 0, 1, false);
	}

	public void TestGetValidLinkedAuthorizationRuleRepetitionsLOC()
	{
		var validLinkedAuthorizationRuleRepetitions = AuthorisationHeaderProvider.GetValidLinkedAuthorizationRuleRepetitions(Factory);
		var locationRuleRanges = validLinkedAuthorizationRuleRepetitions[Customs.Business.CusAuthorisationRuleTypeList.Codes.Location];
		AssertEquals("Expected only one 'LOC' rule range", 1, locationRuleRanges.Count);
		AssertLinkedRuleRange(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, locationRuleRanges.Single(x => x.RuleType == LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice), 1, 1);
	}

	public void TestIsAuthorisationNumberValid_Default()
	{
		Assert("By default an authorisation number is valid. Other validation rules are implemented in specific tests.", AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));
	}

	public void TestIsAuthorisationNumberValid_ApprovedLocationForExport()
	{
		AssertAuthorisationNumberValidity(CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport);
	}

	public void TestIsAuthorisationNumberValid_ApprovedLocationForImport()
	{
		AssertAuthorisationNumberValidity(CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport);
	}

	public void TestIsAuthorisationNumberValid_DeclarationOfIntent()
	{
		const string validFirstBlock = "201231112233";
		const string invalidFirstBlock = "AABBCCDDEEFF";
		const string validSecondBlock = "12345123456";
		const string invalidSecondBlock = "AAAAABBBBBB";

		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent;
		authorisationHeader.CPH_Number = "";
		Assert("Empty authorisation number", !AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

		authorisationHeader.CPH_Number = validFirstBlock;
		Assert("Invalid length", !AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

		authorisationHeader.CPH_Number = invalidFirstBlock + invalidSecondBlock;
		Assert("Valid length, both blocks invalid", !AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

		authorisationHeader.CPH_Number = invalidFirstBlock + validSecondBlock;
		Assert("Valid length, wrong first block", !AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

		authorisationHeader.CPH_Number = validFirstBlock + invalidSecondBlock;
		Assert("Valid length, wrong second block", !AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

		authorisationHeader.CPH_Number = validFirstBlock + validSecondBlock;
		Assert("Valid length, both blocks valid", AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

		authorisationHeader.CPH_Number = "X";
		Assert("Valid placeholder", AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

		authorisationHeader.CPH_Number = "Y";
		Assert("Invalid placeholder", !AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));
	}

	public void TestAuthorisationNumberInvalidFormatMessage_Default()
	{
		AssertEquals("By default invalid format message error is empty. Other validation rules are implemented in specific tests.", ZString.Empty, AuthorisationHeaderProvider.GetAuthorisationNumberInvalidFormatMessage(authorisationHeader));
	}

	public void TestAuthorisationNumberInvalidFormatMessage_ApprovedLocationForExport()
	{
		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport;
		AssertEquals(AuthorisationNumberInvalidFormatMessage, AuthorisationHeaderProvider.GetAuthorisationNumberInvalidFormatMessage(authorisationHeader));
	}

	public void TestAuthorisationNumberInvalidFormatMessage_ApprovedLocationForImport()
	{
		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport;
		AssertEquals(AuthorisationNumberInvalidFormatMessage, AuthorisationHeaderProvider.GetAuthorisationNumberInvalidFormatMessage(authorisationHeader));
	}

	public void TestAuthorisationNumberInvalidFormatMessage_DeclarationOfIntent()
	{
		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent;
		AssertEquals(ValidationCaptions.CusAuthorisations.Header.DeclarationOfIntentNotValid, AuthorisationHeaderProvider.GetAuthorisationNumberInvalidFormatMessage(authorisationHeader));
	}

	protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => (CusAuthorisationHeaderProvider)authorisationHeader.Provider;

	protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.Italy;

	protected override CodeDescriptionPairList ExpectedRuleCodeListForModule
	{
		get
		{
			var expectedList = new CusAuthorisationRuleTypeList();
			expectedList.AddRangeOverwriteIfExists(new CusAuthorisationRuleTypeList());
			expectedList.Sort();
			return expectedList;
		}
	}

	protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
	{
		get
		{
			var authorizationTypes = new CusAuthorizationHeaderTypeList();
			authorizationTypes.Sort();
			return authorizationTypes;
		}
	}

	#region Implementation

	void AssertAuthorisationNumberValidity(ZString authorisationType)
	{
		authorisationHeader.CPH_Type = authorisationType;

		CombineAssertions(authorisationType, () =>
		{
			authorisationHeader.CPH_Number = string.Empty;
			Assert("Empty", !AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

			authorisationHeader.CPH_Number = "Q3496Q";
			Assert("Must start with a digit", !AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

			authorisationHeader.CPH_Number = "13496q";
			Assert("Must end with an uppercase letter", !AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

			authorisationHeader.CPH_Number = "13496Q";
			Assert("Valid with less than 6 characters", AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));

			authorisationHeader.CPH_Number = "1349645Q";
			Assert("Valid with more than 6 characters", AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));
		});
	}

	const string AuthorisationNumberInvalidFormatMessage = "The Authorization Number must start with one or more digits and end with one upper alphabetical character.";

	#endregion
}
