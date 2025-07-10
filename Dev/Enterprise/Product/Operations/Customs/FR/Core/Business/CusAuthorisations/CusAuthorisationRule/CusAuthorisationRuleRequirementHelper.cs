using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business
{
	public static class CusAuthorisationRuleRequirementHelper
	{
		public static CusAuthorisationRuleRequirement GetRequirement(CusAuthorisationHeader authorisationheader, ZString authorisationType, ZString ruleType, INotificationType notification = null)
		{
			var minRulesAllowed = GetMinRulesAllowed(authorisationheader, authorisationType, ruleType);
			var maxRulesAllowed = GetMaxRulesAllowed(authorisationheader, authorisationType, ruleType);
			var additionalValidator = GetAdditionalValidator(ruleType);
			return notification != null ? new CusAuthorisationRuleRequirement(ruleType, minRulesAllowed, maxRulesAllowed, null, additionalValidator, notification) : new CusAuthorisationRuleRequirement(ruleType, minRulesAllowed, maxRulesAllowed, null, additionalValidator);
		}

		static int GetMinRulesAllowed(CusAuthorisationHeader authorisationheader, ZString authorisationType, ZString ruleCode)
		{
			switch (ruleCode)
			{
				case CusAuthorisationRuleTypeList.Codes.CON:
				case CusAuthorisationRuleTypeList.Codes.NAT:
					return IsLocationRuleMandatory(authorisationType) && !IsCustomsWarehouseAuthorisation(authorisationType) && !IsEndUse(authorisationType) ? 1 : 0;
				case CusAuthorisationRuleTypeList.Codes.OFC:
				case CusAuthorisationRuleTypeList.Codes.STO:
					return IsLocationRuleMandatory(authorisationType) && !IsCustomsWarehouseAuthorisation(authorisationType) ? 1 : 0;
				case Customs.Business.CusAuthorisationRuleTypeList.Codes.Location:
					return IsLocationRuleMandatory(authorisationType) ? 1 : 0;
				case CusAuthorisationRuleTypeList.Codes.USE:
					return IsWarehouseUsage(authorisationType) ? 1 : 0;
				case CusAuthorisationRuleTypeList.Codes.AUT:
					return IsAuthorizationShortCodeMandatory(authorisationheader, authorisationType) ? 1 : 0;
				case CusAuthorisationRuleTypeList.Codes.PCP:
				case CusAuthorisationRuleTypeList.Codes.PCV:
				case CusAuthorisationRuleTypeList.Codes.PCD:
					return IsPercentLiability(authorisationType) ? 1 : 0;
				default:
					return 0;
			}
		}

		static int GetMaxRulesAllowed(CusAuthorisationHeader authorisationheader, ZString authorisationType, ZString ruleCode)
		{
			switch (ruleCode)
			{
				case CusAuthorisationRuleTypeList.Codes.Location:
					return IsTransitAuthorisation(authorisationType) ? 9999 : 1;
				default:
					return 1;
			}
		}

		static bool IsEndUse(string authorisationType) => authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse;

		static bool IsLocationRuleMandatory(string authorisationType) => authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse;

		static bool IsPercentLiability(string authorisationType) => authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;

		static bool IsWarehouseUsage(string authorisationType) => authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;

		static bool IsCustomsWarehouseAuthorisation(string authorisationType) => authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;

		static bool IsTransitAuthorisation(string authorisationType) => authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;

		public static bool IsAuthorizationShortCodeMandatory(CusAuthorisationHeader authorisationheader, string authorisationType) => !authorisationheader.CPH_IsAdHoc && IsLocationRuleMandatory(authorisationType);

		public static bool IsDescriptionMandatory(ZString authorisationType, ZString ruleCode) => (authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit
															|| authorisationType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir)
															&& ruleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;

		static Func<string, string> GetAdditionalValidator(ZString ruleCode)
		{
			switch (ruleCode)
			{
				case CusAuthorisationRuleTypeList.Codes.CNT:
					return valueFrom => int.TryParse(valueFrom, out var result) && result > -1 ? string.Empty : ErrorMessageIfNegative(ruleCode);
				case CusAuthorisationRuleTypeList.Codes.STO:
					return valueFrom => int.TryParse(valueFrom, out var result) && result > 1 ? result > 99 ? ErrorMessageIfNotLessThan100(ruleCode) : string.Empty : ErrorMessageIfNotGreaterThan1(ruleCode);
				case CusAuthorisationRuleTypeList.Codes.WAR:
					return valueFrom => int.TryParse(valueFrom, out var result) && result > 1 ? string.Empty : ErrorMessageIfNotGreaterThan1(ruleCode);
				case CusAuthorisationRuleTypeList.Codes.PCD:
				case CusAuthorisationRuleTypeList.Codes.PCP:
				case CusAuthorisationRuleTypeList.Codes.PCV:
					return valueFrom => int.TryParse(valueFrom, out var result) && result >= 0 && result <= 100 ? string.Empty : ErrorMessageIfOutOfBoundaries(ruleCode);
				default:
					return valueFrom => string.Empty;
			}
		}

		public static string ErrorMessageIfNotGreaterThan1(ZString ruleCode) => Res.GetString("A0D154BB-4887-4D2C-8167-70DCCA9EADC3", "Rule code: {0}. The value entered in Value From field should be greater than 1.", ruleCode);

		public static string ErrorMessageIfOutOfBoundaries(ZString ruleCode) => Res.GetString("D8B86082-3C78-4FB8-B980-0AF292AB475E", "Rule code: {0}. The value entered in Value From field should be comprise between 0 and 100 included.", ruleCode);

		public static string ErrorMessageIfNegative(ZString ruleCode) => Res.GetString("4C385B71 - 3EAF - 48F7 - B33D - CC5511E4A458", "Rule code: {0}. The value entered in Value From field should be greater than -1.", ruleCode);

		public static string ErrorMessageIfNotLessThan100(ZString ruleCode) => Res.GetString("d070ccf8-2be4-46a0-bd6c-2a1d4eec5d0d", "Rule code: {0}. The value entered in Value From field should be less than 100.", ruleCode);
	}
}
