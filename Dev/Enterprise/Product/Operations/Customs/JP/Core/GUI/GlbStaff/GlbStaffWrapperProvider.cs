using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.GUI
{
	public class GlbStaffWrapperProvider : MasterFiles.GUI.GlbStaffWrapperProvider
	{
		protected override MasterFiles.GUI.StaffCredentialsUserControl GetNewUserControlCore()
		{
			return new StaffCredentialsUserControl();
		}

		protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
		{
			return Common.GlbStaffWrapper.Get(staff);
		}
	}
}
