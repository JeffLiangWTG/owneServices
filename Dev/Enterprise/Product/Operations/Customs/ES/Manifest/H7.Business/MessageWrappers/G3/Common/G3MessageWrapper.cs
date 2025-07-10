using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3MessageWrapper : IG3Message
	{
		public G3MessageWrapper(ICertificateProvider certificate, BusinessObjectFactory factory)
		{
			this.certificate = Argument.NotNull(certificate, nameof(certificate));
			this.factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly ICertificateProvider certificate;
		readonly BusinessObjectFactory factory;

		public ZString Sender {
			get
			{
				var broker = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, certificate.BrokerCode);
				var glbExternalPassword = CertificateHelper.GetCertificate(broker, certificate.CertificateName);
				return glbExternalPassword?.GP_MailBoxID ?? ZString.Empty;
			}
		}
	}
}
