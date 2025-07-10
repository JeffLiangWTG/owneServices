using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.GUI
{
	public class GlbStaffWrapperProvider : MasterFiles.GUI.GlbStaffWrapperProvider, MasterFiles.Integration.Customs.IL.IILGlbStaffWrapperProvider
	{
		protected override MasterFiles.GUI.StaffCredentialsUserControl GetNewUserControlCore() => new StaffCredentialsUserControl();

		protected override MasterFiles.Business.GlbStaffWrapper GetWrapperCore(MasterFiles.Business.GlbStaff staff)
		{
			return GlbStaffWrapper.Get(staff);
		}
	}
}
