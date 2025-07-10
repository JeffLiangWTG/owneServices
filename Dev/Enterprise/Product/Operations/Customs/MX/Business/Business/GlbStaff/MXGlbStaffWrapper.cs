using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.MX;

namespace Enterprise.Customs.MX.Business
{
	public class MXGlbStaffWrapper : GlbStaffWrapper, IMXGlbStaffWrapper
	{
		public MXGlbStaffWrapper(GlbStaff staff)
			: base(staff)
		{
		}

		public static MXGlbStaffWrapper Get(GlbStaff staff)
		{
			return staff?.Factory.GetCachedValue(staff.PK.ToString(), () => new MXGlbStaffWrapper(staff));
		}

		#region StaffLicenseCollection

		[ChildEditable]
		public StaffLicenseCollection StaffLicenses
		{
			get
			{
				if (staffLicenseCollection == null)
				{
					staffLicenseCollection = new StaffLicenseCollection(Staff);
					staffLicenseCollection.Load();
					RegisterEditableChildObject(staffLicenseCollection);
				}

				return staffLicenseCollection;
			}
		}

		StaffLicenseCollection staffLicenseCollection;

		#endregion
	}
}
