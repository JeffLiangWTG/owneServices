using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class eAdaptorNextOutboundRegistryControl : RegistryZUserControl
	{
		public eAdaptorNextOutboundRegistryControl()
		{
			InitializeComponent();
		}

		public eAdaptorNextOutboundConfig Value
		{
			get { return oAuthConfig; }
			set
			{
				oAuthConfig = value;
				SetDataBinding(value, "");
				CurrentConfig = oAuthConfig.Clone();
				UpdateVisibilityOfElements();

				Value.HasChangesChanged += (_, args) => {
					if (!Value.Equals(CurrentConfig))
					{
						UpdateTick();
					}
				};
			}
		}
		eAdaptorNextOutboundConfig oAuthConfig;

		eAdaptorNextOutboundConfig CurrentConfig { get; set; }

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			if (readOnly)
			{
				Value = eAdaptorNextOutboundConfig.DefaultValue;
			}
			this.OptionGroupBox.Enabled = !readOnly;
			this.AuthorizationURL.Enabled = !readOnly;
			this.AuthorizationGrantTypeDropEdit.Enabled = !readOnly;
			this.OAuth2EnabledCheckBox.Enabled = !readOnly;
			this.VerifyButton.Enabled = !readOnly;
		}

		void UpdateNoteLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				WebUrlLauncher.Launch(UpdateNoteUrl);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message, Res.GetString("dc8f2605-a8d9-4780-be28-1024c5bd41f", "Error displaying update note"));
			}
		}

		const string UpdateNoteUrl = ""; //TODO: Put update note here

		ZString VerifyResText => Res.GetString("6badf676-d2b5-4fce-a4b5-f55122ff8344", "Verify");
		ZString CancelResText => Res.GetString("828acc38-41d5-4a01-90f5-b01587d257ce", "Cancel");

		async void VerifyButton_Click(object sender, EventArgs e)
		{
			ClearTick();
			switch (verifyState)
			{
				case VerifyState.None:
					verifyState = VerifyState.Verifying;

					VerifyButton.Text = CancelResText;

					Value.IsVerifiedInfo.ClearAllNotifications();

					using (cancellationTokenSource = new CancellationTokenSource())
					{
						try
						{
							var authToken = await OAuth2Connect.RequestAuthToken(Value, cancellationTokenSource.Token);
							if (authToken != null)
							{
								Value.IsVerified = true;
							}
						}
						catch (OAuth2Exception ex) when (ex.ErrorResponse != null)
						{
							var message = $"{ex.ErrorType}:\r\n{ex.ErrorResponse.ErrorDescription}\r\n{ex.ErrorResponse.ErrorURI}";
							Value.IsVerifiedInfo.AddError(message);
						}
						catch (OAuth2Exception ex)
						{
							Value.IsVerifiedInfo.AddError(ex.Message);
						}
						catch (Exception ex)
						{
							Value.IsVerifiedInfo.AddError(ex.Message);
						}
						finally
						{
							cancellationTokenSource = null;
						}
					}

					VerifyButton.Text = VerifyResText;
					verifyState = VerifyState.None;
					break;

				case VerifyState.Verifying:
					verifyState = VerifyState.None;

					cancellationTokenSource?.Cancel();
					VerifyButton.Text = VerifyResText;
					break;
			}
			UpdateTick();
		}

		CancellationTokenSource cancellationTokenSource;
		VerifyState verifyState = VerifyState.None;
		enum VerifyState
		{
			None,
			Verifying
		}

		void UpdateVisibilityOfElements()
		{
			if (Value == null)
			{
				return;
			}

			if (CurrentConfig.AuthorizationGrantTypeCode != Value.AuthorizationGrantTypeCode)
			{
				Value.Username = "";
				Value.Password = "";
				Value.ClientID = "";
				Value.ClientSecret = "";
				CurrentConfig = Value.Clone();
				Value.ClearAllNotifications();
				Value.IsVerified = false;
			}

			//Password || ClientCertificate
			if (Value.AuthorizationGrantTypeCode == eAdaptorNextOutboundGrantTypesList.Codes.Password ||
				Value.AuthorizationGrantTypeCode == eAdaptorNextOutboundGrantTypesList.Codes.ClientCertificate)
			{
				UsernameLabel.Visible = true;
				PasswordLabel.Visible = true;
			}
			else
			{
				UsernameLabel.Visible = false;
				PasswordLabel.Visible = false;
			}
			ClientSecretLabel.Text = ClientSecretResText;
			//ClientCertificate
			if (Value.AuthorizationGrantTypeCode == eAdaptorNextOutboundGrantTypesList.Codes.ClientCertificate)
			{
				PasswordLabel.Text = PrivateKeyText;
				Password.Multiline = true;
				Password.ScrollBars = ScrollBars.Vertical;

				UsernameLabel.Text = PublicKeyText;
				Username.Multiline = true;
				Username.ScrollBars = ScrollBars.Vertical;
				Username.PasswordChar = '*';

				ClientSecretLabel.Visible = false;
			}
			else
			{
				PasswordLabel.Text = PasswordText;
				Password.Multiline = false;
				Password.ScrollBars = ScrollBars.None;

				UsernameLabel.Text = UsernameText;
				Username.Multiline = false;
				Username.ScrollBars = ScrollBars.None;
				Username.PasswordChar = '\0';

				ClientSecretLabel.Visible = true;
			}
		}

		void UpdateTick()
		{
			if (Value.IsVerified)
			{
				Tick.ForeColor = Color.Green;
				Tick.Text = (NoResString)"✓";
			}
			else
			{
				Tick.ForeColor = Color.Red;
				Tick.Text = "X";
			}
		}

		void ClearTick()
		{
			Tick.Text = "";
		}
		ZString ClientSecretResText => ResString.GetMultilingualString("1eb151e0-f3e8-4d5e-8914-300d03ff682c", "Client Secret");

		ZString UsernameText => ResString.GetMultilingualString("289f4309-0965-4ecd-9635-3e61d834aff2", "Username");
		ZString PasswordText => ResString.GetMultilingualString("8a493bf1-db99-418d-b944-b6cfbae2877a", "Password");

		ZString PublicKeyText => (NoResString)"Public Key (X509 '.pem')";
		ZString PrivateKeyText => (NoResString)"Private Key (RSA PRIVATE KEY '.key')";

		void AuthorizationGrantTypeDropEdit_Leave(object sender, EventArgs e)
		{
			UpdateVisibilityOfElements();
		}
	}
}
