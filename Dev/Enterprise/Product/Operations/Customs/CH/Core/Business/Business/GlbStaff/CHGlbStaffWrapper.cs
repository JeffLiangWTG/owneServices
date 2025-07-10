using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class CHGlbStaffWrapper : GlbStaffWrapper
{
	protected CHGlbStaffWrapper(GlbStaff staff)
			: base(staff)
	{
	}

	public static CHGlbStaffWrapper Get(GlbStaff staff)
	{
		return staff?.Factory.GetCachedValue(staff.PK.ToString(), () => new CHGlbStaffWrapper(staff));
	}

	#region CHDPassword

	public GlbExternalPassword_CHD CHDPassword
	{
		get
		{
			if (chdPassword == null)
			{
				chdPassword = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_CHD>(PasswordTypesList.Codes.CHD, GlbCompany.CurrentCompany.PK);
				RegisterEditableChildObject(chdPassword);
			}

			return chdPassword;
		}
	}
	GlbExternalPassword_CHD chdPassword;

	#endregion
}
