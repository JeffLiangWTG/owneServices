using Enterprise.Customs.MX.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.GUI
{
	public class MXGlbStaffWrapperProvider : MasterFiles.GUI.GlbStaffWrapperProvider, MasterFiles.Integration.Customs.MX.IMXGlbStaffWrapperProvider
	{
		protected override MasterFiles.GUI.StaffCredentialsUserControl GetNewUserControlCore()
		{
			return new StaffCredentialsUserControl();
		}

		protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
		{
			return MXGlbStaffWrapper.Get(staff);
		}
	}
}
