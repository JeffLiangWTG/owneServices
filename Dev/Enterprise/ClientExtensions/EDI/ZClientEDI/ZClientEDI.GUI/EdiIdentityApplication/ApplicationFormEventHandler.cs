using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using WTG.AWSCertificateIntegration;

namespace Enterprise.Client.EDI.IdentityApplication.GUI
{
	class ApplicationFormEventHandler
	{
		public void GenerateCertificate(EdiIdentityApplication application)
		{
			if (application.Certificates.Any(cert => !cert.IsInDatabase))
			{
				Globals.Message.ShowError(Res.GetString("0D069521-1455-4EFC-A433-DF821B14696D", "Only one certificate can be added at a time."));
				return;
			}

			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.Multiselect = false;
				openFileDialog.Filter = string.Format(CultureInfo.CurrentCulture, "csr files (*.csr)|*.csr");

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(openFileDialog) == System.Windows.Forms.DialogResult.OK)
				{
					var fileName = openFileDialog.ForceLocalFile();
					var fileContent = File.ReadAllText(fileName);

					var query = new ZQuery(EdiIdentityCertificateSchema.ICE_CertificateSigningRequest, fileContent);
					query.AddToFilter(EdiIdentityCertificateSchema.ICE_IsActive, true);
					query.AddToFilter(EdiIdentityCertificateSchema.ICE_CertificateData, null);
					var certs = new BusinessObjectFactory().Load<EdiIdentityCertificate>(query);

					if (certs.Length >= 1)
					{
						Globals.Message.ShowError(Res.GetString("3B902070-56D1-4583-9A3F-AD0C88C9526E", "The certificate is already in use, please add a new certificate."));
						return;
					}

					if (AwsPcaManager.IsCertificateSignRequestValid(fileContent))
					{
						var certificate = application.Certificates.AddNew();
						certificate.ICE_CertificateSigningRequest = fileContent;
					}
				}
			}
		}

		public void RevokeCertificate(List<EdiIdentityCertificate> certificates, bool customerApplication = false)
		{
			if (certificates.Count > 0)
			{
				var shouldGlobalTip = certificates.All(t => t.ICE_ProcessingStatus == EdiIdentityCertificateProcessingStatus.Codes.COM);
				if (!shouldGlobalTip)
				{
					Globals.Message.Show(Res.GetString("8FA8CB37-BC7C-46ED-9E12-FE79C2DFA0E7", "All selected records must be in the completed state."));
				}
				else
				{
					var message = customerApplication
						? Res.GetString("1814B587-B075-4B6A-AA84-CAFE281E9171", $"You are about to revoke {certificates.Count} certificates.\r\nAre you sure you want to proceed?")
						: Res.GetString("AD614E71-CF13-4347-9B38-338566E5B640", $"You are about to revoke {certificates.Count} certificates and remove the corresponding certificate in application from our Azure AD B2C server.\r\nAre you sure you want to proceed?");
					if (Globals.Message.ShowConfirmation(message, Res.GetString("D35BE238-5DE5-40D0-AF84-37F12467E462", "Warning"), "Yes", ZMessageBoxIcon.Warning) == ZDialogResult.OK)
					{
						foreach (var certificate in certificates)
						{
							certificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("18370962-2964-4F0D-B10B-2909AC2862A0", "Please select at least one certificate to revoke."));
			}
		}

		public MultilingualString GenerateCertificateText => ResString.GetMultilingualString("96BA00D2-467D-4208-BFD1-7CE1E25CFBE7", "Generate Certificate (Upload CSR)");
		public MultilingualString RenewCertificateText => ResString.GetMultilingualString("AE8FCB0E-CEDB-4DC3-8C3C-541926A23586", "Renew Certificate (Upload CSR)");
		public MultilingualString RevokeCertificateText => ResString.GetMultilingualString("885D0537-8618-4583-8B66-EBB57242FA9A", "Revoke");
		public MultilingualString RollbackApplicationText => ResString.GetMultilingualString("BEA937DE-E718-48E5-A564-2A02660FA0B3", "Rollback Application");
	}
}
