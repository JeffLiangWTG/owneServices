using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class GlbLoginPasswordValidation : MasterFiles.Business.GlbExternalPasswordValidation
{
	public GlbLoginPasswordValidation(GlbLoginPassword parent)
		: base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateCopyToMailBox();
	}

	public void ValidateCopyToMailBox()
	{
		ValidateCalculatedProperty(Parent.CopyToMailBoxInfo);
	}

	protected new GlbLoginPassword Parent => (GlbLoginPassword)base.Parent;

	protected override void CheckGP_MailBoxID()
	{
		base.CheckGP_MailBoxID();
		var parent = Parent;
		var emailID = parent.GP_MailBoxID;
		if (!EmailAddressValidation.IsEmailAddressValid(emailID) || emailID.StartsWith("<", StringComparison.OrdinalIgnoreCase) && emailID.EndsWith(">", StringComparison.OrdinalIgnoreCase))
		{
			parent.GP_MailBoxIDInfo.AddMessageError(Res.GetString("01C920F6-FD89-4944-AA30-4531B1ACAD55", "{0} is not a valid email address.", emailID));
		}
	}

	protected void CheckCopyToMailBox()
	{
		var parent = Parent;
		if (parent.NeedCopyOfEmails && parent.CopyToMailBox.IsEmpty)
		{
			parent.CopyToMailBoxInfo.AddMessageError(Res.GetString("8C3C13A7-B5BD-407C-8260-A47018D2F866", "Email Address not found. Please update the email address under Staff Details."));
		}
	}
}
