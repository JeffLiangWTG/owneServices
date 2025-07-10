using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UserPortal
{
	public partial class UserPortalDisclaimerForm : ZChildForm
	{
		public UserPortalDisclaimerForm(UserPortalDisclaimerBizO bizO)
			: base(bizO)
		{
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;
		const string PrivacyPolicyUrl = "https://www.wisetechglobal.com/privacy-policy";

		void BtnOK_Click(object sender, EventArgs e)
		{
			((UserPortalDisclaimerBizO)BusinessEntity).SaveToRegistry();
		}

		void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch(PrivacyPolicyUrl);
		}
	}
}
