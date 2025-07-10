#if DEBUG

using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;

namespace Enterprise.Accounting.GUI
{
	public partial class SecurityOverrideProviderWithApprovalRequestSupport
	{
		public string GetSecurityOverrideMessage_ForTestOnly(SecurityCheckpoint checkPoint)
		{
			return GetSecurityOverrideMessage(checkPoint);
		}

		public bool IsApprovalRequestButtonVisible_ForTestOnly => IsApprovalRequestButtonVisible;

		public bool ShouldPromptForGranted_ForTestOnly => ShouldPromptForGranted;

		public DialogResult LastLoginFormResult_ForTestOnly
		{
			get { return LastLoginFormResult; }
			set { LastLoginFormResult = value; }
		}

		public LoginForm CreateNewLoginForm_ForTestOnly()
		{
			return CreateNewLoginForm();
		}
	}
}

#endif
