using System.Windows.Forms;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;

namespace Enterprise.Accounting.GUI
{
	public abstract partial class SecurityOverrideProviderWithApprovalRequestSupport : InteractiveSecurityOverrideProvider, ISecurityOverrideProviderWithApprovalRequest
	{
		public SecurityOverrideProviderWithApprovalRequestSupport(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false)
		{
			this.showApprovalRequestButton = showApprovalRequestButton;
			this.alwaysCreateApprovalRequest = alwaysCreateApprovalRequest;
			this.keepLoginFormResultAfterFirstUserAnswer = keepLoginFormResultAfterFirstUserAnswer;
		}
		public bool ShouldApprovalRequestBeCreated
		{
			get { return alwaysCreateApprovalRequest || LastLoginFormResult == DialogResult.Ignore; }
		}

		protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
		{
			if (!keepLoginFormResultAfterFirstUserAnswer || LastLoginFormResult == DialogResult.None)
			{
				return base.RequestLoginCredentials(checkPoint);
			}

			return UserSecurityOverride;
		}

		protected override bool ShouldPromptForGranted
		{
			get { return true; }
		}

		protected override LoginForm CreateNewLoginForm()
		{
			if (showApprovalRequestButton)
			{
				return new LoginFormWithRequest();
			}
			else
			{
				return base.CreateNewLoginForm();
			}
		}

		protected sealed override string GetSecurityOverrideMessage(SecurityCheckpoint checkPoint)
		{
			string result = GetSecurityOverrideMessageCore(checkPoint);

			if (!string.IsNullOrEmpty(result) && showApprovalRequestButton)
			{
				result += "\r\n\r\n" + Res.GetString("C3CAA84B-DEC8-45ED-9A50-ACCC9486553D", "To queue a request for approval and postpone posting, press 'Approval Request' button.");
			}

			return result;
		}

		protected virtual string GetSecurityOverrideMessageCore(SecurityCheckpoint checkPoint)
		{
			return base.GetSecurityOverrideMessage(checkPoint);
		}

		protected readonly bool showApprovalRequestButton;
		readonly bool alwaysCreateApprovalRequest;
		protected readonly bool keepLoginFormResultAfterFirstUserAnswer;

		protected bool IsApprovalRequestButtonVisible
		{
			get => showApprovalRequestButton;
		}
	}
}
