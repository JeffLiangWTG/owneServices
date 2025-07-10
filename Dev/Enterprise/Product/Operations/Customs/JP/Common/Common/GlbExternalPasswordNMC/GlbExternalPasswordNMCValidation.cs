using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common;

public class GlbExternalPasswordNMCValidation : GlbExternalPasswordWithPasswordTypeValidation
{
	public GlbExternalPasswordNMCValidation(GlbExternalPasswordNMC parent) : base(parent)
	{
	}

	protected override void CheckGP_MailBoxID()
	{
		if ((!string.IsNullOrWhiteSpace(Parent.GP_MailBoxID) || !string.IsNullOrWhiteSpace(Parent.CurrentDecryptedPassword)) && !new Regex("^[A-Z0-9]+$").IsMatch(Parent.GP_MailBoxID))
		{
			Parent.GP_MailBoxIDInfo.AddError(Res.GetString("D8036261-D22A-42B9-91BE-557E3133A937", "Mailbox must contain only capitalized characters or numbers."));
		}
	}

	protected override void CheckCurrentDecryptedPassword()
	{
		base.CheckCurrentDecryptedPassword();

		if (!string.IsNullOrWhiteSpace(Parent.GP_MailBoxID) && string.IsNullOrWhiteSpace(Parent.CurrentDecryptedPassword))
		{
			Parent.CurrentDecryptedPasswordInfo.AddError(Res.GetString("D7FEF9E1-4066-44CC-AF1F-19A3202AE1F3", "Please enter a valid password when the Mailbox is not empty."));
		}
	}

	protected override void CheckGP_UserID()
	{
	}

	protected new GlbExternalPasswordNMC Parent => (GlbExternalPasswordNMC)base.Parent;
}
