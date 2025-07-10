using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.EDICommunicationAuthInbound;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class InboundOAuthUserControl : ZUserControl
	{
		string fileContent;
		bool formLoaded;
		public InboundOAuthUserControl()
		{
			InitializeComponent();
		}

		CertificateDataCollection inboundCertificates;

		CertificateDataCollection InboundCertificates
		{
			get
			{
				if (inboundCertificates == null)
				{
					inboundCertificates = new CertificateDataCollection(EDICommunicationParty.InboundConfig.Auth.ClientID);
					inboundCertificates.LoadCollection(out Exception exception);
					if (formLoaded && exception != null)
					{
						Globals.Message.ShowError($"Error in displaying certificates: {exception.Message}");
					}
				}
				return inboundCertificates;
			}
		}

		ScopeDataCollection scope;

		ScopeDataCollection Scope
		{
			get
			{
				if (scope == null)
				{
					scope = new ScopeDataCollection(EDICommunicationParty.InboundConfig.Auth.ClientID);
					scope.LoadCollection();
				}
				return scope;
			}
		}

		HashSet<string> KnownCodes
		{
			get
			{
				return new HashSet<string>
				{
					CertificateProcessingCodes.Revoked,
					CertificateProcessingCodes.Completed,
					CertificateProcessingCodes.Failed,
					CertificateProcessingCodes.Processing,
					CertificateProcessingCodes.Queued
				};
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				RefreshCertificates();
			}
		}

		public EDICommunicationParty EDICommunicationParty => (EDICommunicationParty)DataSource;

		void OpenFileButton_Click(object sender, EventArgs e)
		{
			OpenFile();
		}

		void OpenFile()
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.CheckFileExists = true;
				dialog.Multiselect = false;

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					CertificateRequestFileTextBox.Text = dialog.UnmappedFileName;
					using (var stream = dialog.OpenFile())
					{
						using (var reader = new StreamReader(stream, Encoding.UTF8))
						{
							fileContent = reader.ReadToEnd();
						}
					}
				}
			}
		}

		void VerifyButton_Click(object sender, EventArgs e)
		{
			VerifyCsr(fileContent);
		}

		bool VerifyCsr(string content)
		{
			if (CertificateUtility.IsCsrValid(content))
			{
				Tick.ForeColor = Color.Green;
				Tick.Text = (NoResString)"✓";
				return true;
			}
			else
			{
				Tick.ForeColor = Color.Red;
				Tick.Text = "X";
				Globals.Message.ShowWarning((NoResString)"Csr is not valid.");
				return false;
			}
		}

		void RegisterCertificateButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(EDICommunicationParty.InboundConfig.Party.Name))
			{
				Globals.Message.ShowError((NoResString)"Please enter an EDI Client Name.");
				return;
			}

			if (EDICommunicationParty.IsPartyNameDuplicate(EDICommunicationParty.InboundConfig.Party.PK, EDICommunicationParty.InboundConfig.Party.Name))
			{
				Globals.Message.ShowError(Res.GetString("62299E7F-77CA-4ECB-818A-969660549087", "The name '{0}' is already in use by another EDI Client. Please select a different name.", EDICommunicationParty.ECP_Name));
				return;
			}

			try
			{
				if (RegisterCertificate(fileContent))
				{
					Globals.Message.Show((NoResString)"Request sent successfully.");
				}
			}
			catch (Exception ex) when (ex is ArgumentNullException || ex is InvalidOperationException || ex is CertificateManagementException)
			{
				Globals.Message.ShowWarning(ex.Message);
			}
		}

		#if DEBUG
		public
		#endif
		bool RegisterCertificate(string cstrPem)
		{
			if (!VerifyCsr(cstrPem))
			{
				return false;
			}

			EDICommunicationParty.InboundConfig.Auth.RegisterCertificate(cstrPem, EDICommunicationParty.InboundConfig.Party.Name);
			SetGenerateCertificateVisibility();
			return true;
		}

		#if DEBUG
		public
		#endif
		bool ClientIdIsReadOnly => ClientIdTextBox.ReadOnly;

		#if DEBUG
		public
		#endif
		bool AuthorityUrlIsReadOnly => AuthorityUrlTextBox.ReadOnly;

		void RefreshCertificatesButton_Click(object sender, EventArgs e)
		{
			RefreshCertificates();
		}

		void RefreshCertificates()
		{
			try
			{
				var operationId = string.Empty;
				if (!string.IsNullOrEmpty(EDICommunicationParty.InboundConfig.Auth.ECA_RenewalOperationId))
				{
					operationId = EDICommunicationParty.InboundConfig.Auth.ECA_RenewalOperationId;
				}
				else if (!string.IsNullOrEmpty(EDICommunicationParty.InboundConfig.Auth.ECA_OperationId) && EDICommunicationParty.InboundConfig.Auth.ECA_Certificate.IsEmpty)
				{
					operationId = EDICommunicationParty.InboundConfig.Auth.ECA_OperationId;
				}
				if (!string.IsNullOrEmpty(operationId))
				{
					var result = ObjectFactory.Get<ICertificateManager>();
					var response = result.DownloadCertificate(operationId);

					if (!KnownCodes.Contains(response.StatusCode))
					{
						ErrorReporter.ReportDeveloperExceptionOnce($"Unexpected code {response.StatusCode} was found. Please check if this code should be added to {nameof(CertificateProcessingCodes)}.", new DeveloperNotificationException($"Unexpected code {response.StatusCode} was found."));
					}

					if (response.StatusCode != CertificateProcessingCodes.Failed)
					{
						EDICommunicationParty.InboundConfig.Auth.SetInboundCertificate(response.TenantId, response.ClientId, response.Certificate, EDICommunicationParty.InboundConfig.Auth.ECA_RenewalOperationId);
						scope = null;
						var scopeData = (ScopeData)Scope.FirstOrDefault();
						ScopeTextBox.SetDataBinding(scopeData?.Name, string.Empty);
					}
					else
					{
						Globals.Message.ShowWarning(Res.GetString("EFB60D0F-2DA6-418C-BB17-64DD444F67EE", "The certificate generation has failed. Please try generating a new certificate with another certificate request file."));
						EDICommunicationParty.InboundConfig.Auth.RevertFailedCertificateRegistration();
					}
				}
			}
			catch (Exception ex) when (ex is ArgumentNullException || ex is CertificateManagementException)
			{
				if (formLoaded)
				{
					Globals.Message.ShowWarning(ex.Message);
				}
			}
			try
			{
				inboundCertificates = null;
				CertificatesGrid.SetDataBinding(InboundCertificates, string.Empty);
			}
			catch (Exception ex) when (ex is ArgumentNullException || ex is InvalidOperationException || ex is CertificateManagementException)
			{
				if (formLoaded)
				{
					Globals.Message.ShowWarning(ex.Message);
				}
			}
			SetGenerateCertificateVisibility();
			formLoaded = true;
		}

		bool isCertificateInProgress()
		{
			if (string.IsNullOrEmpty(EDICommunicationParty.InboundConfig.Auth.ECA_OperationId))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(EDICommunicationParty.InboundConfig.Auth.ECA_ClientID) && string.IsNullOrEmpty(EDICommunicationParty.InboundConfig.Auth.ECA_RenewalOperationId))
			{
				return false;
			}
			return true;
		}

		void SetGenerateCertificateVisibility()
		{
			var certificateInProgress = isCertificateInProgress();
			CertificateRequestFileTextBox.Enabled = !certificateInProgress;
			OpenFileButton.Enabled = !certificateInProgress;
			VerifyButton.Enabled = !certificateInProgress;
			Tick.Enabled = !certificateInProgress;
			RegisterCertificateButton.Enabled = !certificateInProgress;
			if (certificateInProgress)
			{
				awaitingCertificateGenerationLabel.Visible = true;
				RegisterCertificateButton.Text = "Waiting...";
			}
			else
			{
				awaitingCertificateGenerationLabel.Visible = false;
				RegisterCertificateButton.Text = (NoResString)"Generate Certificate";
			}
		}

		void DownloadCertificateButton_Click(object sender, EventArgs e)
		{
			DownloadCertificate();
		}

		void DownloadCertificate()
		{
			if (CertificatesGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowWarning((NoResString)"No row selected.");
				return;
			}

			using (var fileDialog = new ZSaveFileDialog
			{
				DefaultExt = (NoResString)"pem",
				Filter = (NoResString)"Privacy enhanced mail files|*.pem"
			})
			{
				if (fileDialog.ShowDialog() == DialogResult.OK)
				{
					var certificate = (CertificateData)CertificatesGrid.SelectedElements[0];
					SaveContentToFile(fileDialog.OpenFile(), certificate.CertificatePem);
				}
			}
		}

		void SaveContentToFile(Stream fileStream, string content)
		{
			using (var sw = new StreamWriter(fileStream))
			{
				sw.Write(content);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
			{
				IsSelfManagedCheckBox.Visible = !EnvProxy.IsHostedWithCargowise;
			}
		}

		protected void IsSelfManagedCheckBox_OnCheckedChanged(object sender, EventArgs e)
		{
			AuthorityUrlTextBox.ReadOnly = !IsSelfManagedCheckBox.Checked;
			ClientIdTextBox.ReadOnly = !IsSelfManagedCheckBox.Checked;
			ScopeTextBox.Visible = !IsSelfManagedCheckBox.Checked;
			CertificateGroupBox.Visible = !IsSelfManagedCheckBox.Checked;
		}

		#if DEBUG
		public
		#endif
		bool IsSelfManagedCheckBoxVisible => IsSelfManagedCheckBox.Visible;

		#if DEBUG
				public
		#endif
		bool ScopeTextBoxVisible => ScopeTextBox.Visible;

		#if DEBUG
		public
		#endif
		bool CertificateGroupBoxVisible => CertificateGroupBox.Visible;
	}
}
