using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Testing
{
	public static class GuaranteeTestHelper
	{
		public static CusGuaranteeHeader CreateGuaranteeHeader(BusinessObjectFactory factory, ZString type, ZString number, ZGuid orgHeaderPK, ZString ruleCode, ZString ruleValue, ZString additionalReferenceCode, ZString additionalreferenceData, ZString countryCode, bool hasEndDate = false, string entryTypeCode = GuaranteeEntryTypeList.Codes.BTH)
		{
			var result = factory.New<CusGuaranteeHeader>();
			result.CPH_OH_PermitHolder = orgHeaderPK;
			result.CPH_RN_NKCountryCode = countryCode;
			if (!hasEndDate)
			{
				result.CPH_StartDate = ZDate.Today;
			}
			else
			{
				result.CPH_StartDate = ZDate.Today.AddDays(-2);
				result.CPH_EndDate = ZDate.Today.AddDays(-1);
			}
			result.CPH_Type = type;
			result.CPH_Number = number;
			if (!string.IsNullOrEmpty(ruleCode))
			{
				var rule1 = result.CusGuaranteeRules.AddNew();
				rule1.CPR_RuleCode = ruleCode;
				rule1.CPR_ValueFrom = ruleValue;
			}

			if (type == GuaranteeTypeList.Codes.DEF || type == GuaranteeTypeList.Codes.COD)
			{
				var rule1 = result.CusGuaranteeRules.AddNew();
				rule1.CPR_RuleCode = PermitRuleCodeList.Codes.ENT;
				rule1.CPR_ValueFrom = entryTypeCode;
			}

			if (!string.IsNullOrEmpty(additionalReferenceCode))
			{
				var additionalRef = result.AdditionalGuaranteeReferences.AddNew();
				additionalRef.CY_Code = additionalReferenceCode;
				additionalRef.CY_Data = additionalreferenceData;
			}
			factory.Save();
			return result;
		}
	}
}
