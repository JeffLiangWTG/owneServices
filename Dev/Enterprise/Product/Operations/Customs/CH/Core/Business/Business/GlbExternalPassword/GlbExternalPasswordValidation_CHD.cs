using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class GlbExternalPasswordValidation_CHD : GlbExternalPasswordValidation
{
	public GlbExternalPasswordValidation_CHD(GlbExternalPassword_CHD parent)
		: base(parent)
	{
	}

	protected new GlbExternalPassword_CHD Parent => (GlbExternalPassword_CHD)base.Parent;

	protected override void CheckGP_UserID()
	{
		base.CheckGP_UserID();
		var userID = Parent.GP_UserID;
		var userIDInfo = Parent.GP_UserIDInfo;

		if (!userID.IsNumbersOnlyOrEmpty)
		{
			userIDInfo.AddError(Res.GetString("87F8A7C0-332A-44A7-873B-93942275DD6A", "Declarant number must be digits only."));
		}
	}
}
