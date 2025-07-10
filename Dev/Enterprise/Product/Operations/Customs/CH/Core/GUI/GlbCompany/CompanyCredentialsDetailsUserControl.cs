using System;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class CompanyCredentialsDetailsUserControl : ZUserControl
{
	public CompanyCredentialsDetailsUserControl()
	{
		InitializeComponent();
	}

	GlbCompanyWrapper glbCompanyWrapper => DataSource as GlbCompanyWrapper;

	#region Event Handlers

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataLoaded;
		CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataChanged;
		CertificateLoaderUserControl.OnDataView += OnCertificateDataView;
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

	#endregion
}
