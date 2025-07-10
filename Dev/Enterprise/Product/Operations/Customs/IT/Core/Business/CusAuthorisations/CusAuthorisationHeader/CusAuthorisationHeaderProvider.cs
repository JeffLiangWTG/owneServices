using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class CusAuthorisationHeaderProvider : EU.Business.CusAuthorisationHeaderProvider
{
	protected CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
	{
	}

	protected override CodeDescriptionPairList GetRuleCodeListForModuleCore(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue("Enterprise.Customs.IT.Business.CusAuthorisationHeaderProvider.GetRuleCodeListForModule", () =>
		{
			var result = new CodeDescriptionPairList(base.GetRuleCodeListForModuleCore(factory));
			result.AddRangeOverwriteIfExists(new CusAuthorisationRuleTypeList());
			result.Sort();
			return result;
		});
	}

	protected override Dictionary<ZString, FieldType> GetRuleValueFieldTypesCore()
	{
		var result = base.GetRuleValueFieldTypesCore();
		result[ITCusAuthorisationRuleTypeList.Codes.Location] = FieldType.Text;
		result.Add(ITCusAuthorisationRuleTypeList.Codes.Document, FieldType.TextCodeFindBox);
		result.Add(ITCusAuthorisationRuleTypeList.Codes.Use, FieldType.TextDropEdit);
		return result;
	}

	protected override void SetupRuleDescriptionFunctions()
	{
		base.SetupRuleDescriptionFunctions();
		AddRuleDescriptionFunction(ITCusAuthorisationRuleTypeList.Codes.Document, rule => GetDocumentDescription(rule));
	}

	protected override Dictionary<ZString, List<CusAuthorisationRuleRequirement>> GetValidAuthorisationRuleRequirementsCore(CusAuthorisationHeader cusAuthorisationHeader)
	{
		var rules = base.GetValidAuthorisationRuleRequirementsCore(cusAuthorisationHeader);

		var authorisationRuleRequirementProvider = new CusAuthorisationRuleRequirementProvider(cusAuthorisationHeader);

		new[]
		{
			  CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP,
			  CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1,
			  CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2,
			  CusAuthorizationHeaderTypeList.Codes.InwardProcessing,
			  CusAuthorizationHeaderTypeList.Codes.OutwardProcessing,
			  CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit,
		}.ForEach(x => AddOrUpdateValidAuthorisationRuleRepititions(rules, x, new[] { authorisationRuleRequirementProvider.DocRuleRequirement }.ToList()));

		AddOrUpdateValidAuthorisationRuleRepititions(rules, CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent, new List<CusAuthorisationRuleRequirement>()
		{
			authorisationRuleRequirementProvider.UseRuleRequiremnt,
		});
		AddOrUpdateValidAuthorisationRuleRepititions(rules, KeyForGeneralHeaderType, new List<CusAuthorisationRuleRequirement>()
		{
			authorisationRuleRequirementProvider.LocRuleRequirement,
		});
		return rules;
	}

	protected override Dictionary<ZString, List<LinkedCusAuthorisationRuleRange>> GetValidLinkedAuthorizationRuleRepetitionsCore()
	{
		var rules = base.GetValidLinkedAuthorizationRuleRepetitionsCore();
		AddOrUpdateValidLinkedAuthorizationRuleRepetitions(rules, Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, new List<LinkedCusAuthorisationRuleRange>()
		{
			new LinkedCusAuthorisationRuleRange(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, 1, 1)
		});
		return rules;
	}

	protected override ZInt GetRuleValueFromMaxLengthCore(CusAuthorisationRule cusAuthorisationRule) => cusAuthorisationRule.CPR_RuleCode == ITCusAuthorisationRuleTypeList.Codes.Document ? (ZInt)DocRuleMaxLength : base.GetRuleValueFromMaxLengthCore(cusAuthorisationRule);

	protected override Customs.Business.CusAuthorisationRuleLookups GetNewLookupsCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleLookups(cusAuthorisationRule);

	protected override Customs.Business.CusAuthorisationHeaderLookups GetNewLookupsCore(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationHeaderLookups(cusAuthorisationHeader);

	protected override ZBool IsAuthorisationNumberValidCore(CusAuthorisationHeader authorisationHeader)
	{
		switch (authorisationHeader.CPH_Type)
		{
			case CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport:
			case CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport:
				return ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter(authorisationHeader.CPH_Number);
			case CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent:
				return new DeclarationOfIntentValueObject(authorisationHeader.CPH_Number).IsValid(true);
		}

		return ZBool.True;
	}

	protected override ZString GetAuthorisationNumberInvalidFormatMessageCore(CusAuthorisationHeader authorisationHeader)
	{
		switch (authorisationHeader.CPH_Type)
		{
			case CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport:
			case CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport:
				return ValidationCaptions.CusAuthorisations.InvalidApprovedLocationAuthorisationNumber;
			case CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent:
				return ValidationCaptions.CusAuthorisations.Header.DeclarationOfIntentNotValid;
		}
		return ZString.Empty;
	}

	protected override CodeDescriptionPairList GetAuthorisationTypeListCore(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue("Enterprise.Customs.IT.Business.CusAuthorisationHeaderProvider.GetAuthorisationTypeListCore", () =>
		{
			var authorizationTypes = new CusAuthorizationHeaderTypeList();
			authorizationTypes.Sort();
			return authorizationTypes;
		});
	}

	protected override bool ShowCustomsCodeCore => true;

	#region Implementation

	ZString GetDocumentDescription(CusAuthorisationRule rule)
	{
		var description = ZString.Empty;
		if (rule?.AuthorisationHeader != null)
		{
			var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(rule.Factory, rule.CPR_ValueFrom, CountryCode, rule.AuthorisationHeader.GetSupportedDocumentRefCusCodeListTypeCode(), ZDateTime.Now);
			description = cusCode?.ZZD_Description ?? ZString.Empty;
		}
		return description;
	}

	const int DocRuleMaxLength = 4;
	#endregion
}
