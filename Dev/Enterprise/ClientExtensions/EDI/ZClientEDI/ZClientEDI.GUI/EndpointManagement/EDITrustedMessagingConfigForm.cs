using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.EndpointManagement.GUI
{
	public partial class EDITrustedMessagingConfigForm : ZTemplateForm
	{
		public EDITrustedMessagingConfigForm(EdiTrustedMessagingConfig config) : base(config)
		{
			ZFormMenuStrategy.AddActionsMenuItem(this, "Generate New Certificate from CA...", OnGenerateCertificate);
			ZFormMenuStrategy.AddActionsMenuItem(this, "Export Certificate (.cer)...", OnExportCertificate);
		}

		EdiTrustedMessagingConfig Config => (EdiTrustedMessagingConfig)DataSource;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CertificateControl.SetFileData(Config?.ETM_CertificateData ?? ZBlob.Empty);
			CertificateControl.CertificatePassword = Config?.ETM_CertificatePassword ?? null;
			if (Config != null)
			{
				Config.ETM_CertificatePasswordInfo.ValueChanged += ETM_CertificatePasswordInfo_ValueChanged;
			}
		}

		void ETM_CertificatePasswordInfo_ValueChanged(object sender, EventArgs e)
		{
			CertificateControl.CertificatePassword = Config?.ETM_CertificatePassword ?? null;
		}

		void CertificateControl_DataChanged(object sender, EventArgs e)
		{
			if (Config != null)
			{
				Config.ETM_CertificateData = CertificateControl.FileDataAsBinary();
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			if (Config != null)
			{
				Config.ETM_CertificatePasswordInfo.ValueChanged -= ETM_CertificatePasswordInfo_ValueChanged;
			}
		}

		protected void OnGenerateCertificate(object sender, EventArgs e)
		{
			if (EDISecurityCheckpoints.EdiTrustedMessagingConfig.IsAllowed)
			{
				if (Config != null)
				{
					var args = new UserResponseArgument
					{
						DefaultAnswer = "ediProd Central system-Product ***",
						MinimumResponseLength = 3,
						Caption = "New Certificate",
						Message = "Warning: New Certificate will overwrite existing certificate on this form.\r\n\r\nPlease Enter a Certificate Subject Name.",
						Buttons = ZMessageBoxButtons.OKCancel,
						Icon = ZMessageBoxIcon.Question,
						UserResponseTextBoxCharactersCasing = ZCharacterCasing.Normal,
					};

					var subjectName = Globals.Message.QueryUserResponse(args);

					if (!string.IsNullOrWhiteSpace(subjectName))
					{
						try
						{
							Cursor.Current = Cursors.WaitCursor;
							if (Config.TryGenerateCertificate(subjectName, out var errorMessage))
							{
								CertificateControl.SetFileData(Config.ETM_CertificateData);
								CertificateControl.CertificatePassword = Config.ETM_CertificatePassword;
								Globals.Message.ShowInformation("The operation completed successfully.");
							}
							else
							{
								Globals.Message.ShowError(errorMessage);
							}
						}
						finally
						{
							Cursor.Current = Cursors.Default;
						}
					}
				}
			}
			else
			{
				EDISecurityCheckpoints.EdiTrustedMessagingConfig.ShowError();
			}
		}

		protected void OnExportCertificate(object sender, EventArgs e)
		{
			if (EDISecurityCheckpoints.EdiTrustedMessagingConfig.IsAllowed)
			{
				var cert = Config?.GetCertificate();
				if (cert != null)
				{
					using (var dialog = GetSaveFileDialog(cert.Subject))
					{
						if (dialog.ShowDialog(this) == DialogResult.OK)
						{
							using (var writer = new BinaryWriter(dialog.OpenFile()))
							{
								writer.Write(cert.Export(X509ContentType.Cert));
							}
							Globals.Message.ShowInformation("The operation completed successfully.");
						}
					}
				}
				else
				{
					Globals.Message.ShowError("Invalid Certificate");
				}
			}
			else
			{
				EDISecurityCheckpoints.EdiTrustedMessagingConfig.ShowError();
			}
		}

		protected virtual IFileDialog GetSaveFileDialog(string defaultFileName)
		{
			return new ZSaveFileDialog()
			{
				Filter = "Certificate files (*.cer)|*.cer",
				FileName = defaultFileName,
				CheckPathExists = true,
				AddExtension = true,
				OverwritePrompt = true,
				RestoreDirectory = true,
				Title = "Export Certificate",
			};
		}
	}
}
