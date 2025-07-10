using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class CompanyCredentialsDetailsUserControl : ZUserControl
{
	public CompanyCredentialsDetailsUserControl()
	{
		InitializeComponent();
	}

	GlbMauExternalPassword CurrentExternalPassword => ITAccUserGrid.ListManager.GetCurrent() as GlbMauExternalPassword;

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataChanged;
		CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataChanged;
		CertificatePasswordTextBox.Leave += CertPasswordTextBox_Leave;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			CertificateLoaderUserControl.DataLoaded -= CertificateLoaderUserControl_DataChanged;
			CertificateLoaderUserControl.DataCleared -= CertificateLoaderUserControl_DataChanged;
			CertificatePasswordTextBox.Leave -= CertPasswordTextBox_Leave;
		}
		base.Dispose(disposing);
	}

	#region Event Handlers

	void CertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
	{
		UpdateExternalPasswordCertificateFileData();
	}

	void UpdateExternalPasswordCertificateFileData()
	{
		var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();
		var currentExternalPassword = CurrentExternalPassword;
		if (currentExternalPassword != null)
		{
			currentExternalPassword.GP_Certificate = loadedData;
		}
	}

	void ITAccUserGrid_AfterBind(object sender, EventArgs e)
	{
		var gridListManager = ITAccUserGrid.ListManager;
		if (gridListManager != null)
		{
			gridListManager.CurrentChanged += ITAccUserGrid_CurrentChanged;
			ITAccUserGrid_CurrentChanged(gridListManager, EventArgs.Empty);
		}
	}

	void ITAccUserGrid_CurrentChanged(object sender, EventArgs e)
	{
		UpdateCertificateLoaderUserControlFileData();
		UpdateCertificateLoaderUserControlPassword();
		ToggleCertificateLoaderUserControlStatus();
	}

	void ITAccUserGrid_CurrentCellChanged(object sender, EventArgs e)
	{
		ToggleCertificateLoaderUserControlStatus();
	}

	void CertPasswordTextBox_Leave(object sender, EventArgs e)
	{
		UpdateCertificateLoaderUserControlPassword();
	}

	void UpdateCertificateLoaderUserControlFileData()
	{
		CertificateLoaderUserControl.SetFileData(CurrentExternalPassword?.GP_Certificate ?? ZBlob.Empty);
	}

	void UpdateCertificateLoaderUserControlPassword()
	{
		CertificateLoaderUserControl.CertificatePassword = CurrentExternalPassword?.CurrentDecryptedCertificatePassphrase ?? ZString.Empty;
	}

	void ToggleCertificateLoaderUserControlStatus()
	{
		CertificateLoaderUserControl.Enabled = CurrentExternalPassword != null;
	}

	#endregion
}
