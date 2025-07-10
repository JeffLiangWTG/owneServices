using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CusAuthorisationHeaderProvider : EU.Business.CusAuthorisationHeaderProvider
	{
		protected CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
		{
		}

		protected override CodeDescriptionPairList GetRuleCodeListForModuleCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.DE.Business.CusAuthorisationHeaderProvider.GetRuleCodeListForModule", () =>
			{
				var result = new CodeDescriptionPairList(base.GetRuleCodeListForModuleCore(factory));
				result.AddRangeOverwriteIfExists(new CusAuthorisationRuleTypeList());
				result.Sort();
				return result;
			});
		}

		protected override ZInt GetRuleValueFromMaxLengthCore(CusAuthorisationRule cusAuthorisationRule) => cusAuthorisationRule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Usage ? (ZInt)3 : base.GetRuleValueFromMaxLengthCore(cusAuthorisationRule);

		protected override Customs.Business.CusAuthorisationHeaderLookups GetNewLookupsCore(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationHeaderLookups(cusAuthorisationHeader);

		protected override Customs.Business.CusAuthorisationRuleLookups GetNewLookupsCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleLookups(cusAuthorisationRule);

		protected override Customs.Business.CusAuthorisationRuleValidation GetNewValidationCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleValidation(cusAuthorisationRule);

		protected override Customs.Business.CusAuthorisationHeaderValidation GetNewValidationCore(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationHeaderValidation(cusAuthorisationHeader);

		protected override void SetupRuleDescriptionFunctions()
		{
			base.SetupRuleDescriptionFunctions();
			AddRuleDescriptionFunction(CusAuthorisationRuleTypeList.Codes.Usage, cusAuthorisationRule => ((CodeDescriptionPairList)cusAuthorisationRule.Lookups.ValueList).GetDescriptionFromCode(cusAuthorisationRule.CPR_ValueFrom));
			AddRuleDescriptionFunction(CusAuthorisationRuleTypeList.Codes.Release, cusAuthorisationRule => ((CodeDescriptionPairList)cusAuthorisationRule.Lookups.ValueList).GetDescriptionFromCode(cusAuthorisationRule.CPR_ValueFrom));
		}

		protected override Dictionary<ZString, FieldType> GetRuleValueFieldTypesCore()
		{
			var result = base.GetRuleValueFieldTypesCore();
			result[Customs.Business.CusAuthorisationRuleTypeList.Codes.Location] = FieldType.Text;
			result.Add(CusAuthorisationRuleTypeList.Codes.Usage, FieldType.TextDropEdit);
			result.Add(CusAuthorisationRuleTypeList.Codes.Release, FieldType.TextDropEdit);
			return result;
		}

		protected override Dictionary<ZString, List<CusAuthorisationRuleRequirement>> GetValidAuthorisationRuleRequirementsCore(CusAuthorisationHeader cusAuthorisationHeader)
		{
			var rules = base.GetValidAuthorisationRuleRequirementsCore(cusAuthorisationHeader);
			AddOrUpdateValidAuthorisationRuleRepititions(rules, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, new List<CusAuthorisationRuleRequirement>()
			{
				GetUSERuleRequirement(cusAuthorisationHeader),
				GetMRERuleRequirement()
			});
			AddOrUpdateValidAuthorisationRuleRepititions(rules, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, new List<CusAuthorisationRuleRequirement>()
			{
				GetUSERuleRequirement(cusAuthorisationHeader),
				new CusAuthorisationRuleRequirement(CusAuthorisationRuleTypeList.Codes.BusinessReference, 0, 1),
				GetMRERuleRequirement(),
				new CusAuthorisationRuleRequirement(CusAuthorisationRuleTypeList.Codes.Release, 1, 1, () => cusAuthorisationHeader.RequiresAuthorizationRuleRelease(), null)
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

		protected override ZBool IsAuthorisationNumberValidCore(CusAuthorisationHeader authorisationHeader)
			=> authorisationHeader.CPH_Number.IsAuthorisationNumberValid(authorisationHeader.CPH_Type);

		protected override ZString GetAuthorisationNumberInvalidFormatMessageCore(CusAuthorisationHeader authorisationHeader)
			=> authorisationHeader.CPH_Number.GetAuthorisationNumberInvalidFormatMessage(authorisationHeader.CPH_Type);

		CusAuthorisationRuleRequirement GetUSERuleRequirement(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationRuleRequirement(CusAuthorisationRuleTypeList.Codes.Usage, 1, 1, () => cusAuthorisationHeader.CPH_Number.StartsWith(Core.Constants.CountryCodes.Germany, StringComparison.CurrentCultureIgnoreCase), null);

		CusAuthorisationRuleRequirement GetMRERuleRequirement() => new CusAuthorisationRuleRequirement(CusAuthorisationRuleTypeList.Codes.MandateReference, 0, 1, null,
								value => value.Length == 10 && ((ZString)value).IsNumbersOnlyOrEmpty ? string.Empty : Res.GetString("4F32540D-1403-45B1-A2E2-11ACB6D54110", "The Reference length has to be 10 numeric digits."), NotificationType.MessageError);
	}
}
