using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.GUI
{
	public class GlbStaffWrapperProvider : MasterFiles.GUI.GlbStaffWrapperProvider, MasterFiles.Integration.Customs.ES.IGlbStaffWrapperProvider
	{
		protected override MasterFiles.GUI.StaffCredentialsUserControl GetNewUserControlCore()
			=> new GlbStaffForm_ESCredentialsUserControl();

		protected override MasterFiles.Business.GlbStaffWrapper GetWrapperCore(MasterFiles.Business.GlbStaff staff)
			=> GlbStaffWrapper.Get(staff);
	}
}
