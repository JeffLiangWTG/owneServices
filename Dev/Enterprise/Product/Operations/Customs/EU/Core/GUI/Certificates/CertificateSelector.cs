using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Certificates
{
	public abstract class CertificateSelector
	{
		protected CertificateSelector(string chipset)
		{
			this.chipset = chipset;
		}

		protected readonly string chipset;

		public CryptokiCertificate ChooseCertificate() => ChooseCertificate(CryptokiCertificateProvider.GetCertificateList(chipset));

		CryptokiCertificate ChooseCertificate(IReadOnlyList<CryptokiCertificate> certificates)
		{
			var selectCertificateForm = GetSelectCertificateForm(certificates);
			var editorDialogResult = ZFormModaliser.ShowDialogAndDispose(selectCertificateForm);

			return (editorDialogResult == DialogResult.OK)
				? selectCertificateForm.SelectedCertificate
				: null;
		}

		protected virtual SelectCertificateForm GetSelectCertificateForm(IReadOnlyList<CryptokiCertificate> certificates) => new SelectCertificateForm(certificates);

		public void ShowCertificateInfo(string serialNumberText)
		{
			var certificate = ReadCertificate(serialNumberText);
			ZFormModaliser.ShowDialogAndDispose(new CertificateInformationForm(certificate));
		}

		public bool CanLocateCertificate(string serialNumberText)
		{
			try
			{
				return (ReadCertificate(serialNumberText) != null);
			}
			catch (CryptographicException)
			{
				return false;
			}
		}

		CryptokiCertificate ReadCertificate(string serialNumberText)
		{
			if (!CertificateHelper.TryGetHexValue(serialNumberText, out var serialNumber))
			{
				throw new CryptographicException(InvalidSerialNumberMsg);
			}

			return CryptokiCertificateProvider.ReadCertificate(chipset, serialNumber)
				?? throw new CryptographicException(CannotFindCertificateMsg(serialNumberText, chipset));
		}

		ICryptokiCertificateProvider CryptokiCertificateProvider => cryptokiCertificateProvider ??= GetNewCryptokiCertificateProvider(CertificateSource);
		ICryptokiCertificateProvider cryptokiCertificateProvider;

		protected virtual ICryptokiCertificateProvider GetNewCryptokiCertificateProvider(string certificateSource)
			=> CertificateHelper.GetNewCryptokiCertificateProvider(certificateSource);

		protected abstract string CertificateSource { get; }

		static string InvalidSerialNumberMsg => Res.GetString("3ED22199-3518-4D36-BF83-F1D7B08C836E", "Serial number must have even number of hexadecimal digits.");

		static string CannotFindCertificateMsg(string serialNumberText, string chipset)
		{
			return Res.GetString("10C31757-907A-4334-9599-09E1D098E888", "Cannot find certificate with serial number '{0}' using {1} library.", serialNumberText, chipset);
		}
	}
}
