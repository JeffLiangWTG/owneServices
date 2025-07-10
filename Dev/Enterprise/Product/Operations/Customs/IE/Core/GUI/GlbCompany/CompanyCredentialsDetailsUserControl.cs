using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using GlbCompanyWrapper = Enterprise.Customs.IE.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IE.GUI
{
	public partial class CompanyCredentialsDetailsUserControl : ZUserControl
	{
		public CompanyCredentialsDetailsUserControl()
		{
			InitializeComponent();
		}

		GlbCompanyWrapper glbCompanyWrapper => DataSource as GlbCompanyWrapper;

		#region Event Handlers

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var isEMCSEnabled = glbCompanyWrapper?.Company is GlbCompany company && EmcsCustomsDataRegistry.Instance.EnableEmcsFunctions.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			EMCSROSCredentialsGroupBox.Visible = isEMCSEnabled;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataLoaded;
			CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataChanged;
			CertificateLoaderUserControl.OnDataView += OnCertificateDataView;

			EMCSCertificateLoaderUserControl.DataLoaded += EMCSCertificateLoaderUserControl_DataLoaded;
			EMCSCertificateLoaderUserControl.DataCleared += EMCSCertificateLoaderUserControl_DataChanged;
			EMCSCertificateLoaderUserControl.OnDataView += OnEMCSCertificateDataView;

			EMCSExternalPasswordCertificateGrid.SelectedRowsChangedInMouseDown += EMCSExternalPasswordCertificateGrid_SelectedRowsChanged;
			var emcsMailboxContextMenuItem = new ZMenuItem();
			emcsMailboxContextMenuItem.CaptionResourceString = Res.GetData("9D66953F-F6A5-4055-892A-D147CF7E5314", "Mailbox Request");
			emcsMailboxContextMenuItem.Click += EMCSMailboxRequest;
			EMCSExternalPasswordCertificateGrid.ContextMenu.MenuItems.Add(emcsMailboxContextMenuItem);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var password = glbCompanyWrapper?.GlbExternalPassword;
			if (password != null)
			{
				CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
				CertificateLoaderUserControl.SetFileData(password.GP_Certificate);
			}

			var emcsPassword = glbCompanyWrapper?.EMCSGlbExternalPasswordCollection.FirstOrDefault() as EMCSGlbCompanyCredential;
			if (emcsPassword != null)
			{
				EMCSCertificateLoaderUserControl.CertificatePassword = emcsPassword.CurrentDecryptedCertificatePassphrase;
				EMCSCertificateLoaderUserControl.SetFileData(emcsPassword.GP_Certificate);
			}
		}

		void CertificateLoaderUserControl_DataLoaded(object sender, EventArgs e)
		{
			var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();

			var password = glbCompanyWrapper?.GlbExternalPassword;
			if (password != null)
			{
				password.GP_Certificate = loadedData;
			}
		}

		void CertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
		{
			var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();

			var password = glbCompanyWrapper?.GlbExternalPassword;
			if (password != null)
			{
				password.GP_Certificate = loadedData;
			}
		}

		void OnCertificateDataView(object sender, EventArgs e)
		{
			var password = glbCompanyWrapper?.GlbExternalPassword;
			if (password != null)
			{
				CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
			}
		}

		void EMCSCertificateLoaderUserControl_DataLoaded(object sender, EventArgs e)
		{
			var loadedData = (ZBlob)EMCSCertificateLoaderUserControl.FileDataAsBinary();

			var password = GetSelectedEMCSCertificate();
			if (password != null)
			{
				password.GP_Certificate = loadedData;
			}
		}

		void EMCSCertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
		{
			var loadedData = (ZBlob)EMCSCertificateLoaderUserControl.FileDataAsBinary();

			var password = GetSelectedEMCSCertificate();
			if (password != null)
			{
				password.GP_Certificate = loadedData;
			}
		}

		void OnEMCSCertificateDataView(object sender, EventArgs e)
		{
			var password = GetSelectedEMCSCertificate();
			if (password != null)
			{
				CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
			}
		}

		void EMCSExternalPasswordCertificateGrid_SelectedRowsChanged(object sender, EventArgs e)
		{
			var password = GetSelectedEMCSCertificate();
			if (password != null)
			{
				EMCSCertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
				EMCSCertificateLoaderUserControl.SetFileData(password.GP_Certificate);
			}
		}

		EMCSGlbCompanyCredential GetSelectedEMCSCertificate()
		{
			var rowIndex = EMCSExternalPasswordCertificateGrid.CurrentRowIndex;
			return EMCSExternalPasswordCertificateGrid.ListManager.List[rowIndex] as EMCSGlbCompanyCredential;
		}

		void EMCSMailboxRequest(object sender, EventArgs e)
		{
			var certificate = GetSelectedEMCSCertificate();
			if (certificate != null)
			{
				SendMailboxRequest(certificate, true);
			}
		}

		void MailboxRequestButton_Click(object sender, EventArgs e)
		{
			var certificate = glbCompanyWrapper?.GlbExternalPassword;
			if (certificate != null)
			{
				SendMailboxRequest(certificate, false);
			}
		}

		void SendMailboxRequest(IGlbExternalPasswordWithCertificate credential, bool isEMCS)
		{
			if (!credential.GP_PasswordStatus.EqualsIgnoringCase(MasterFiles.Business.PasswordStatusList.Codes.Valid) || !credential.IsCertificateValid)
			{
				Globals.Message.ShowError(Res.GetString("{01252A19-9C48-4DCA-A19D-1CAC61CB041C}", "Cannot create a Mailbox Request for invalid credential."));
			}
			else
			{
				var interchange = MailboxRequester.RequestForSpecificCredentialPk(credential.PK, GetBranchPK(), isEMCS);
				if (interchange != null)
				{
					Globals.Message.Show(Res.GetString("{C7B03DC4-19AE-44CC-90EB-E1B19CDDC9CC}", "One Mailbox Request (Interchange Num = '{0}') created.", interchange.EI_InterchangeNum));
				}
			}
		}

		ZGuid GetBranchPK()
		{
			ZGuid result;
			if (GlbCompany.CurrentCompany.PK == glbCompanyWrapper.Company.PK)
			{
				result = GlbBranch.CurrentBranch.PK;
			}
			else
			{
				result = glbCompanyWrapper.Company.FirstActiveBranch.PK;
			}
			return result;
		}

		#endregion
	}
}
