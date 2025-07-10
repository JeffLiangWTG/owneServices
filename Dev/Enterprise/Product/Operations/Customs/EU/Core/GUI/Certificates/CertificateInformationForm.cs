using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Certificates
{
	public partial class CertificateInformationForm : ZChildForm
	{
		public CertificateInformationForm(CryptokiCertificate certificate)
		{
			certificateInfo = FormatCertificate(certificate);
			InitializeComponent();
		}

		readonly ZString certificateInfo;

		void ShowCertificate_Load(object sender, EventArgs e)
		{
			InfoZTextBox.Text = certificateInfo;
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		static string FormatCertificate(CryptokiCertificate certificate)
		{
			var sb = new StringBuilder();

			sb.AppendLine(GetCaptionForCertificateProperty(nameof(CryptokiCertificate.TokenManufacturerId)) + ":\t" + certificate.TokenManufacturerId);
			sb.AppendLine(GetCaptionForCertificateProperty(nameof(CryptokiCertificate.TokenModel)) + ":\t" + certificate.TokenModel);
			sb.AppendLine(GetCaptionForCertificateProperty(nameof(CryptokiCertificate.SerialNumber)) + ":\t" + certificate.SerialNumber);
			sb.AppendLine(GetCaptionForCertificateProperty(nameof(CryptokiCertificate.Thumbprint)) + ":\t" + certificate.Thumbprint);
			sb.AppendLine(GetCaptionForCertificateProperty(nameof(CryptokiCertificate.TokenChipset)) + ":\t" + certificate.TokenChipset);
			sb.AppendLine(GetCaptionForCertificateProperty(nameof(CryptokiCertificate.NotBefore)) + ":\t" + certificate.NotBefore);
			sb.AppendLine(GetCaptionForCertificateProperty(nameof(CryptokiCertificate.NotAfter)) + ":\t" + certificate.NotAfter);
			sb.AppendLine(GetCaptionForCertificateProperty(nameof(CryptokiCertificate.Owner)) + ":\t" + certificate.Owner);
			sb.AppendLine(GetCaptionForCertificateProperty(nameof(CryptokiCertificate.Issuer)) + ":\t" + certificate.Issuer);

			return sb.ToString();
		}

		static string GetCaptionForCertificateProperty(string propertyName)
		{
			return DataBoundResourceStrings.GetDataForProperty(typeof(CryptokiCertificate), propertyName).Caption;
		}
	}
}
