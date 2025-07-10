using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class EmailSentNotification : BasePage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		protected void Page_Load(object sender, EventArgs e)
		{
			SetEmailVerificationSentValues();

			if (ShouldDisplayEmailVerificationSentMessage)
			{
				MessageLabel.Text = EmailVerificationSentSuccessfully
					? FormattableString.Invariant($"Account authentication is required to login from this system. Follow the link in the verification email sent to {EmailAddress} to confirm this is a valid account.")
					: EmailAddress.IsEmpty
						? "Account authentication is required to login from this system, but the account used for login has no email. Please enter an email address for the account before attempting login again."
						: FormattableString.Invariant($"Account authentication is required to login from this system, but the verification email failed to send to address: {EmailAddress}. Please attempt to login again and contact your system administrator if this issue persists.");
			}
			else
			{
				MessageLabel.Text = "Invalid Request!";
			}
		}

		#region Test User Email Verification

		public void SetEmailVerificationSentValues()
		{
			var queryStringData = Request.QueryString[SecureQueryString.QueryStringKey];

			if (!string.IsNullOrEmpty(queryStringData))
			{
				SecureQueryString queryString = null;

				try
				{
					queryString = new SecureQueryString(queryStringData);
				}
				catch (QueryStringException)
				{
					ShouldDisplayEmailVerificationSentMessage = false;
				}

				if (queryString != null)
				{
					try
					{
						EmailVerificationSentSuccessfully = new ZBool(queryString[UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey]);
						EmailAddress = queryString[UserEmailVerificationRoutingDescriptor.EmailVerificationAddressUrlKey];
					}
					catch (ZTypeValueException)
					{
						ShouldDisplayEmailVerificationSentMessage = false;
						return;
					}

					ShouldDisplayEmailVerificationSentMessage = true;
					return;
				}
			}

			ShouldDisplayEmailVerificationSentMessage = false;
		}

		public ZBool ShouldDisplayEmailVerificationSentMessage { get; private set; }
		public ZBool EmailVerificationSentSuccessfully { get; private set; }
		public ZString EmailAddress { get; private set; }

		#endregion
	}
}
