using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	public partial class SystemUserAccountsAuditForm : ZTemplateForm
	{
		public SystemUserAccountsAuditForm(EdiCustomerUserAccount ediCustomerUserAccount)
			: base(ediCustomerUserAccount)
		{
			InitializeComponent();
			PlugIns.Add(ControllerIDs.Audit);

			var mainTabPage = MainTabControl.Controls.Find("MainTabPage", true);
			if(mainTabPage != null && mainTabPage.Length > 0)
			{
				MainTabControl.Controls.Remove(mainTabPage[0]);
			}
		}
	}
}

