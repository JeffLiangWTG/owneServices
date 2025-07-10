using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public sealed class AutomaticSignatureExternalPasswordValidation : MasterFiles.Business.GlbExternalPasswordValidation
{
	public AutomaticSignatureExternalPasswordValidation(AutomaticSignatureExternalPassword parent) : base(parent)
	{
	}

	protected override void CheckGP_MailBoxID()
	{
		base.CheckGP_MailBoxID();

		if (IsConfigurationActive)
		{
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.GP_MailBoxIDInfo);
		}
	}

	protected override void CheckGP_UserID()
	{
		base.CheckGP_UserID();

		if (IsConfigurationActive)
		{
			MandatoryValidation.CheckEntered(Parent.GP_UserIDInfo);
		}
	}

	protected override void CheckGP_Name()
	{
		base.CheckGP_Name();

		if (IsConfigurationActive)
		{
			var parent = Parent;
			var name = parent.GP_Name;
			var targetInfo = parent.GP_NameInfo;
			if (name.IsEmpty)
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}
			else
			{
				var cusCodeValidator = (IITCusCodeValidator)new ITFiscalCodeValidator();
				if (cusCodeValidator.Validate(name) == ITCusCodeValidationResult.InvalidPattern)
				{
					targetInfo.AddMessageError(Res.GetString("4B0F7E1E-1115-43A0-A2C1-5454444905E2", "The User Fiscal Code pattern is invalid. Valid patterns are: {0}.", UserFiscalCodeValidPatterns));
				}
			}
		}
	}

	new AutomaticSignatureExternalPassword Parent => (AutomaticSignatureExternalPassword)base.Parent;

	ZBool IsConfigurationActive => Parent.IsConfigurationActive;

	const string UserFiscalCodeValidPatterns = "AAAAAAnnAnnAnnnA";
}
