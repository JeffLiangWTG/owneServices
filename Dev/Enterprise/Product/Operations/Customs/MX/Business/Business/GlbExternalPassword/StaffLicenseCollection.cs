using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.Business
{
	public class StaffLicenseCollection : DependentBusinessObjectCollection<GlbExternalPassword_MXL, GlbStaff>
	{
		public StaffLicenseCollection(GlbStaff staff)
			: base(staff, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.MXL))
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((GlbExternalPassword_MXL)child).GP_PasswordType = PasswordTypesList.Codes.MXL;
		}
	}
}
