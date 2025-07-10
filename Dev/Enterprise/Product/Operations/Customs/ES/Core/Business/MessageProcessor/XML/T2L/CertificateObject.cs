using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class CertificateObject : ICertificateProvider
	{
		public CertificateObject(GlbStaff broker, ZString certificateName, ZString certificateTumbPrint)
		{
			Broker = Argument.NotNull(broker, nameof(broker));
			Certificate = CertificateHelper.GetCertificate(Broker, certificateName, certificateTumbPrint);
		}
		GlbStaff Broker { get; }
		GlbExternalPassword Certificate { get; }

		ZString ICertificateProvider.BrokerCode => Broker.GS_Code;

		ZString ICertificateProvider.CertificateName => Certificate?.GP_Name ?? ZString.Empty;

		ZString ICertificateProvider.CertificateThumbPrint => Certificate?.GP_UserID ?? ZString.Empty;

		ZBlob ICertificateProvider.CertificateBytes => Certificate?.GP_Certificate ?? ZBlob.Empty;

		ZString ICertificateProvider.DecryptedCertificatePassphrase => Certificate?.CurrentDecryptedCertificatePassphrase ?? ZString.Empty;

		public ZGuid CertificatePK => Certificate?.PK ?? ZGuid.Empty;

		ZString ICertificateProvider.CertificateID => Certificate?.GP_MailBoxID ?? ZString.Empty;
	}
}
