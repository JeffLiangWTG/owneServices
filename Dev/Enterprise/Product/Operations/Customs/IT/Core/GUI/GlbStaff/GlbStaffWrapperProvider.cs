using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.IT.GUI;

public class GlbStaffWrapperProvider : MasterFiles.GUI.GlbStaffWrapperProvider, MasterFiles.Integration.CustomsIntegration.IT.IGlbStaffWrapperProvider
{
	protected override StaffCredentialsUserControl GetNewUserControlCore()
	{
		return new GlbStaffForm_ITCredentialsUserControl();
	}

	protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
	{
		return Business.GlbStaffWrapper.Get(staff);
	}
}
