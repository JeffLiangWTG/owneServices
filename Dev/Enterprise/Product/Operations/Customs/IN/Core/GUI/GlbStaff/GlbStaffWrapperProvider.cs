using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.GUI
{
	public class GlbStaffWrapperProvider : MasterFiles.GUI.GlbStaffWrapperProvider, MasterFiles.Integration.Customs.IN.IINGlbStaffWrapperProvider
	{
		protected override MasterFiles.GUI.StaffCredentialsUserControl GetNewUserControlCore()
		{
			return new StaffCredentialsUserControl();
		}

		protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
		{
			return IN.Business.GlbStaffWrapper.Get(staff);
		}
	}
}
