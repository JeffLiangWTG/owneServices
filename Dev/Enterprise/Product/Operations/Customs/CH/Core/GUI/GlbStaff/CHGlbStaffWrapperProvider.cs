using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration.Customs.CH;

namespace Enterprise.Customs.CH.GUI;

public class CHGlbStaffWrapperProvider : GlbStaffWrapperProvider, ICHGlbStaffWrapperProvider
{
	protected override StaffCredentialsUserControl GetNewUserControlCore()
	{
		return new CHStaffCredentialsUserControl();
	}

	protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
	{
		return CHGlbStaffWrapper.Get(staff);
	}
}
