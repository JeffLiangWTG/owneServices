using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class CusAuthorisationHeaderValidation : Customs.Business.CusAuthorisationHeaderValidation
{
	public CusAuthorisationHeaderValidation(CusAuthorisationHeader parent) : base(parent)
	{
	}

	protected override void CheckCPH_Number()
	{
		base.CheckCPH_Number();

		if (Parent.CPH_Type == CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar
			&& !Regex.IsMatch(Parent.CPH_Number, RegexForAuthorizedLocationCodeForPassar))
		{
			Parent.CPH_NumberInfo.AddMessageError(Res.GetString("BC6E169F-374F-4169-A320-1BBCD9823CB1", "ALP code should begin with ‘ZO’, followed by 10 numeric digits."));
		}
	}

	protected override void CheckCPH_PermitDescription()
	{
		base.CheckCPH_PermitDescription();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CPH_PermitDescriptionInfo);
	}

	const string RegexForAuthorizedLocationCodeForPassar = "^ZO[0-9]{10}$";
}
