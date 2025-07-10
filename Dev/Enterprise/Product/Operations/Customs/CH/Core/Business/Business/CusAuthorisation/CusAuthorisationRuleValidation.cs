using System.Text.RegularExpressions;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class CusAuthorisationRuleValidation : Customs.Business.CusAuthorisationRuleValidation
{
	public CusAuthorisationRuleValidation(CusAuthorisationRule parent) : base(parent)
	{
	}

	protected override void CheckCPR_ValueFrom()
	{
		base.CheckCPR_ValueFrom();

		if (Parent.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Location
			&& Parent.AuthorisationHeader.CPH_Type == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec
			&& !Regex.IsMatch(Parent.CPR_ValueFrom, RegexForAuthorizedLocationCodeForEdec))
		{
			Parent.CPR_ValueFromInfo.AddMessageError(Res.GetString("F1B04F80-BEEE-4767-A894-FC2C800DB79A", "ALE code should begin with ‘CH’ and have the following structure: {0}.", "CHnnnnnnZOnnnnNnnnnnn"));
		}
	}

	const string RegexForAuthorizedLocationCodeForEdec = "^CH[0-9]{6}ZO[0-9]{4}N[0-9]{6}$";
}
