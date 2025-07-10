using System;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;

namespace Enterprise.Customs.KR.GUI
{
	public partial class KRDigitalCertificateControl_p12 : Registry.GUI.DigitalCertificateControl_p12
	{
		public KRDigitalCertificateControl_p12()
		{
			InitializeComponent();
		}

		GlbCompanyWrapper CompanyWrapper => (GlbCompanyWrapper)DataSource;
		GlbCompanyCredential CompanyCredential => CompanyWrapper?.CertificateForUnipass;
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DataLoaded += CertificateLoaderUserControl_DataChanged;
			DataCleared += CertificateLoaderUserControl_DataChanged;
			OnDataView += OnCertificateDataView;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DataLoaded -= CertificateLoaderUserControl_DataChanged;
				DataCleared -= CertificateLoaderUserControl_DataChanged;
				OnDataView -= OnCertificateDataView;
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (CompanyCredential != null)
			{
				CertificatePassword = CompanyCredential.CurrentDecryptedCertificatePassphrase;
				SetFileData(CompanyCredential.GP_Certificate);
			}
		}

		#region Event Handlers

		void CertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
		{
			if (CompanyCredential != null)
			{
				CompanyCredential.GP_Certificate = (ZBlob)FileDataAsBinary();
			}
		}

		void OnCertificateDataView(object sender, EventArgs e)
		{
			if (CompanyCredential != null)
			{
				CertificatePassword = CompanyCredential.CurrentDecryptedCertificatePassphrase;
			}
		}

		#endregion
	}
}
