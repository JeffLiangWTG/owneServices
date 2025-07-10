using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public static class TaxCoreCredentialHelper
	{
		const string PAC = nameof(PAC);

		public static string GetPAC(this EInvoicingCertificateCredential certificateCredential)
		{
			if (!string.IsNullOrEmpty(certificateCredential?.GP_MailBoxID))
			{
				return certificateCredential.GP_MailBoxID;
			}
			return null;
		}

		public static GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialCollection GetCredentials(this EInvoicingCertificateCredential certificateCredential)
		{
			return [new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential {
				Key = PAC,
				Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue {
					Encrypted = false,
					Value = certificateCredential.GetPAC()
				}
			}];
		}
	}
}
