using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.GUI
{
	public partial class OutboundOAuthUserControl : ZUserControl
	{
		public OutboundOAuthUserControl()
		{
			InitializeComponent();
		}

		protected void GrantType_OnChangeChanged(object sender, EventArgs e)
		{
			UpdateVisibilityOfElements();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (Config != null)
			{
				UpdateVisibilityOfElements();
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource is EDICommunicationParty party)
			{
				party.OutboundConfig.Auth.ECA_FlowCodeInfo.AdditionalValidation += CheckValidConfig;
				party.OutboundConfig.Auth.ECA_FlowCodeInfo.ValueChanged += OnFlowCodeChanged;
				party.OutboundConfig.Auth.ECA_FlowCodeInfo.ValueChanged += ClearVerifyResult;
				party.OutboundConfig.Auth.ECA_AuthorizationEndpointInfo.ValueChanged += ClearVerifyResult;
				party.OutboundConfig.Auth.ECA_AuthorizationModeInfo.ValueChanged += ClearVerifyResult;
				party.OutboundConfig.Auth.ECA_CertificateInfo.ValueChanged += ClearVerifyResult;
				party.OutboundConfig.Auth.ECA_OperationIdInfo.ValueChanged += ClearVerifyResult;
				party.OutboundConfig.Auth.ECA_RenewalEncodedPrivateKeyInfo.ValueChanged += ClearVerifyResult;
				party.OutboundConfig.Auth.ECA_ClientIDInfo.ValueChanged += ClearVerifyResult;
				party.OutboundConfig.Auth.ECA_ClientSecretInfo.ValueChanged += ClearVerifyResult;
				party.OutboundConfig.Auth.ECA_PasswordInfo.ValueChanged += ClearVerifyResult;
				party.OutboundConfig.Auth.ECA_UsernameInfo.ValueChanged += ClearVerifyResult;
				party.OutboundConfig.Auth.ECA_ScopesInfo.ValueChanged += ClearVerifyResult;
			}
		}

		void EnableAuthInput(bool enable )
		{
			ClientID.Enabled = enable;
			AuthorizationURL.Enabled = enable;
			AuthorizationGrantTypeDropEdit.Enabled = enable;
			ClientSecret.Enabled = enable;
			Username.Enabled = enable;
			Password.Enabled = enable;
			ScopesGroupBox.Enabled = enable;
			GenerateCSRButton.Enabled = enable;
			CertificateGroupBox.Enabled = enable;
			SetCertificateButton.Enabled = enable;
			ShowCertificateButton.Enabled = enable;
		}

		public void ClearVerifyResult(object sender, EventArgs eventArgs)
		{
			Tick.ForeColor = Color.Red;
			Tick.Text = "X";
			Config.Auth.IsVerified = false;
			lastValidConfig = null;
		}

		public void VerifyButton_Click(object sender, EventArgs e)
		{
			VerifyConfig();
		}

		async void VerifyConfig()
		{
			Tick.Text = "";

			if (isVerifying)
			{
				isVerifying = false;
				EnableAuthInput(true);
				cancellationTokenSource?.Cancel();
				VerifyButton.Text = VerifyResText;
			}
			else
			{
				isVerifying = true;
				EnableAuthInput(false);
				VerifyButton.Text = CancelResText;
				lastValidationErrorMessage = null;

				using (cancellationTokenSource = new CancellationTokenSource())
				{
					try
					{
						((IBusinessObjectInternals)Config.Auth).EnsureBlobField(EDICommunicationAuthSchema.ECA_EncodedPrivateKey);
						((IBusinessObjectInternals)Config.Auth).EnsureBlobField(EDICommunicationAuthSchema.ECA_Certificate);
						var authToken = await GetAuthToken(Config);
						if (authToken != null)
						{
							lastValidConfig = new CachedOutboundOAuthConfig(Config);
						}
					}
					catch (OAuth2Exception ex) when (ex.ErrorResponse != null)
					{
						lastValidConfig = null;
						lastValidationErrorMessage = $"{ex.ErrorType}:\r\n{ex.ErrorResponse.ErrorDescription}\r\n{ex.ErrorResponse.ErrorURI}";
					}
					catch (OAuth2Exception ex)
					{
						lastValidConfig = null;
						lastValidationErrorMessage = ex.Message;
					}
					catch (Exception ex)
					{
						lastValidConfig = null;
						lastValidationErrorMessage = ex.Message;
						ErrorReporter.ReportOnce(Res.GetString("DA1CAA2C-D302-4737-97AA-3352AA2BDCA1", "Error on config verification: {0}.", ex.Message), ex);
					}
					finally
					{
						cancellationTokenSource = null;
					}
				}

				isVerifying = false;
				EnableAuthInput(true);
				VerifyButton.Text = VerifyResText;
				UpdateVerifyResult();
			}
		}

		bool isVerifying;
		CachedOutboundOAuthConfig lastValidConfig;
		string lastValidationErrorMessage;
		CancellationTokenSource cancellationTokenSource;

		protected virtual async Task<AuthToken> GetAuthToken(EDICommunicationPartyConfig config) => await OAuth2Connect.RequestAuthToken(Config.Auth, cancellationTokenSource.Token);

		void UpdateVerifyResult()
		{
			if (lastValidConfig != null && lastValidConfig.IsCached(Config))
			{
				Tick.ForeColor = Color.Green;
				Tick.Text = (NoResString)"✓";
				Config.Auth.IsVerified = true;
			}
			else
			{
				Tick.ForeColor = Color.Red;
				Tick.Text = "X";
				Config.Auth.IsVerified = false;
				lastValidConfig = null;
			}
			Config.Auth.Validation.ValidateECA_FlowCode();
		}

		void OnFlowCodeChanged(object sender, EventArgs eventArgs)
		{
			UpdateVisibilityOfElements();
		}

		void UpdateVisibilityOfElements()
		{
			if (Config.Auth.ECA_FlowCode == EDICommunicationAuthOutboundGrantTypesList.Codes.Password)
			{
				Username.Visible = true;
				Password.Visible = true;
			}
			else
			{
				Username.Visible = false;
				Password.Visible = false;
			}
			if (Config.Auth.ECA_FlowCode == EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate)
			{
				ClientSecret.Visible = false;
				CertificateGroupBox.Visible = true;
				SetCertificateButton.Enabled = !EDICommunicationParty.OutboundConfig.Auth.ECA_RenewalEncodedPrivateKey.IsEmpty;
				ShowCertificateButton.Enabled = !string.IsNullOrEmpty(EDICommunicationParty.OutboundConfig.Auth.Certificate);
			}
			else
			{
				ClientSecret.Visible = true;
				CertificateGroupBox.Visible = false;
			}
		}

		void CheckValidConfig()
		{
			if (Config.Party.ECP_IsActive && Config.ECC_IsActive)
			{
				if (Config.Auth.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.OAuthAuthentication)
				{
					if (!string.IsNullOrEmpty(lastValidationErrorMessage))
					{
						Config.Auth.ECA_FlowCodeInfo.AddError(Res.GetString("430F82C6-BDA5-474D-9744-029E3C8B61FC", "Verification Error: ") + lastValidationErrorMessage);
					}
				}
			}
		}

		public EDICommunicationParty EDICommunicationParty => (EDICommunicationParty)DataSource;

		protected void GenerateCsrButton_Click(object sender, EventArgs e)
		{
			if (EDICommunicationParty.ECP_Name.IsEmpty)
			{
				Globals.Message.ShowError((NoResString)"Please enter an EDI Client Name.");
				return;
			}
			if (EDICommunicationParty.IsPartyNameDuplicate(EDICommunicationParty.PK, EDICommunicationParty.ECP_Name))
			{
				Globals.Message.ShowError(Res.GetString("2A20CF82-B20E-47E1-B1F1-35A1EBA16B28", "The name '{0}' is already in use by another EDI Client. Please select a different name.", EDICommunicationParty.ECP_Name));
				return;
			}
			var csrPem = EDICommunicationParty.OutboundConfig.Auth.SetPrivateKeyAndCsr(EDICommunicationParty.ECP_Name);
			SetCertificateButton.Enabled = true;
			AuthorizationGrantTypeDropEdit.Focus();
			AuthorizationURL.Focus();
			GenerateCSRButton.Focus();
			using (var form = new TextDialogForm((NoResString)"Showing CSR", csrPem, TextDialogForm.SecretLabelTypes.CSR))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void ShowCertificateButton_Click(object sender, EventArgs e)
		{
			OpenCertificateForm();
		}

		TwoWayEncoder TwoWayEncoder => TwoWayEncoder.NewWithStandardInitialisationVector();

#if DEBUG
		public
		#endif
		void OpenCertificateForm()
		{
			using (var form = new TextDialogForm((NoResString)"Showing Certificate", EDICommunicationParty.OutboundConfig.Auth.Certificate, TextDialogForm.SecretLabelTypes.Certificate)
			)
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		string ValidateCertificate(string certificatePem)
		{
			var privateKey = Encoding.Unicode.GetString(TwoWayEncoder.Decrypt(EDICommunicationParty.OutboundConfig.Auth.ECA_RenewalEncodedPrivateKey));
			if (!EDIClientOutboundCsrGenerator.IsCertificateValid(certificatePem))
			{
				return Res.GetString("5115bb8e-2e4f-4036-a44d-2e1a9ada6f7b", "Certificate is not valid.");
			}
			if (!EDIClientOutboundCsrGenerator.IsCertificateMatchingPrivateKey(certificatePem, privateKey))
			{
				return Res.GetString("7cf664f6-2e0d-4bbb-badd-150a1eeff612", "Certificate does not match the Private Key.");
			}

			if (!EDIClientOutboundCsrGenerator.IsCertificateMatchingCsrSubject(certificatePem, EDICommunicationParty.ECP_Name))
			{
				return Res.GetString("7f9b1a45-88c7-4147-a8f1-8bd997b6ec1f", "Certificate does not match the CSR.");
			}
			return string.Empty;
		}

		protected void SetCertificateButton_Click(object sender, EventArgs e)
		{
			using (
				var form = new TextDialogForm((NoResString)"Setting Certificate", Res.GetString("818ceaaf-5458-487e-a12b-4f5c9e326432", "Certificate has been set successfully."), ValidateCertificate, TextDialogForm.SecretLabelTypes.Certificate)
			)
			{
				var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form);
				if (dialogResult == DialogResult.OK)
				{
					EDICommunicationParty.OutboundConfig.Auth.SetCertificate(form.SecretString, EDICommunicationParty.ECP_Name);
					SetCertificateButton.Enabled = false;
					ShowCertificateButton.Enabled = true;
					AuthorizationGrantTypeDropEdit.Focus();
					AuthorizationURL.Focus();
					SetCertificateButton.Focus();
				}
			}
		}

		readonly ZString VerifyResText = ResString.GetMultilingualString("663B06EA-DD46-4390-938F-D257E1400208", "Verify");
		readonly ZString CancelResText = ResString.GetMultilingualString("7C3B591F-F06C-4577-8001-659B351A6237", "Cancel");

		public EDICommunicationPartyConfig Config => ((EDICommunicationParty)DataSource)?.OutboundConfig;
	}
}
