using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AUGlbStaffWrapperProvider : GlbStaffWrapperProvider, MasterFiles.Integration.Customs.AU.IAUGlbStaffWrapperProvider
	{
		protected override StaffCredentialsUserControl GetNewUserControlCore()
		{
			return new AUStaffCredentialsUserControl();
		}

		protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
		{
			return AUGlbStaffWrapper.Get(staff);
		}
	}
}
