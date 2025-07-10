using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.GUI
{
	public class BRGlbStaffWrapperProvider : MasterFiles.GUI.GlbStaffWrapperProvider, MasterFiles.Integration.Customs.BR.IBRGlbStaffWrapperProvider
	{
		public override MenuItem GetNewTopLevelMenu(GlbStaffWrapper wrapper)
		{
			return new SubscriptionPlugInMenu(wrapper as BRGlbStaffWrapper);
		}

		protected override MasterFiles.GUI.StaffCredentialsUserControl GetNewUserControlCore()
		{
			return new StaffCredentialsUserControl();
		}

		protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
		{
			return BRGlbStaffWrapper.Get(staff);
		}
	}
}
