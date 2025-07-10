using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class GEIMessageHelper
	{
		public static GlbCompanySignatureCredential GetCompanySignatureCredential(this GlbCompany company)
		{
			return company.SignatureCredentials.OfType<GlbCompanySignatureCredential>().FirstOrDefault();
		}

		public static string GetEncryptedPassphraseForEHub(this GlbCompanySignatureCredential signatureCredential) =>
			!string.IsNullOrEmpty(signatureCredential?.CurrentDecryptedPassword) ? CredentialSender.EncryptPasswordAsString(signatureCredential.CurrentDecryptedPassword) : string.Empty;

		public static string GetCertificateAsBase64String(this GlbExternalPassword externalPassword, bool supportCertificateBase64EncodedTwice = false)
		{
			var certificateString = string.Empty;
			if (supportCertificateBase64EncodedTwice)
			{
				var decodedStr = System.Text.Encoding.UTF8.GetString(externalPassword?.GP_Certificate);  //expected string starts with "TUI...."
				try
				{
					var bytes = Convert.FromBase64String(decodedStr);
					certificateString = System.Text.Encoding.UTF8.GetString(bytes);  //expected string starts with "MII...."
				}
				catch (FormatException)
				{
					//if invalid Base-64 string, then return empty string
				}
			}
			else
			{
				var bytes = (byte[])externalPassword?.GP_Certificate;
				if (bytes != null)
				{
					certificateString = Convert.ToBase64String(bytes);
				}
			}
			return certificateString;
		}

		public static string GetEncryptedPassphraseForEHub(this GlbExternalPassword credential) =>
			!string.IsNullOrEmpty(credential?.CurrentDecryptedCertificatePassphrase) ? CredentialSender.EncryptPasswordAsString(credential.CurrentDecryptedCertificatePassphrase) : string.Empty;

		public static string GetEncryptedPassphraseForEHub(this EInvoicingCredentials credential) =>
			!string.IsNullOrEmpty(credential?.ClientSecret) ? CredentialSender.EncryptPasswordAsString(credential.ClientSecret) : string.Empty;

		public static string ToISO8601StringWithZeroOffsetSymbol(this ZDateTime dateTime) => string.Concat(dateTime.ToISO8601String(), "Z");

		public static bool GetIsProductionSystem(GlbCompany company)
		{
			switch (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				case AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem:
					return true;
				case AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem:
					return false;
				case AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default:
				default:
					return Env.Instance.IsProductionSystem;
			}
		}
	}
}
