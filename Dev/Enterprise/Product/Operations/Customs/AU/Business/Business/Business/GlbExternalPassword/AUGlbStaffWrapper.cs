using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUGlbStaffWrapper : GlbStaffWrapper, IAUGlbStaffWrapper
	{
		protected AUGlbStaffWrapper(GlbStaff staff)
			: base(staff)
		{
		}

		public static AUGlbStaffWrapper Get(GlbStaff staff)
		{
			return staff == null ? null : staff.Factory.GetCachedValue(staff.PK.ToString(), () => new AUGlbStaffWrapper(staff));
		}

		#region NEXDOCS User Token

		public GlbExternalPassword_NUT NUTPassword
		{
			get
			{
				if (nutPassword == null || nutPassword.IsDeleted)
				{
					nutPassword = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_NUT>(PasswordTypesList.Codes.NUT, GlbCompany.CurrentCompany.PK);
					RegisterEditableChildObject(nutPassword);
				}

				return nutPassword;
			}
		}
		GlbExternalPassword_NUT nutPassword;

		#endregion

		IGlbExternalPassword IAUGlbStaffWrapper.NUTPassword => NUTPassword;
	}
}
