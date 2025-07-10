using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI
{
	public sealed class TokenCertificateSelector
	{
		public TokenCertificateSelector(ZString libraryName)
		{
			this.libraryName = libraryName;
		}

		public CryptokiCertificate ChooseCertificate()
		{
			var certificates = CryptokiCertificateProvider.GetCertificateList(libraryName);
			var selectCertificateForm = new SelectCertificateForm(certificates);
			var editorDialogResult = ZFormModaliser.ShowDialogAndDispose(selectCertificateForm);

			return (editorDialogResult == DialogResult.OK)
				? selectCertificateForm.SelectedCertificate
				: null;
		}

		ICryptokiCertificateProvider CryptokiCertificateProvider => cryptokiCertificateProvider ??= new CryptokiCertificateProvider();
		ICryptokiCertificateProvider cryptokiCertificateProvider;

		readonly ZString libraryName;
	}
}
