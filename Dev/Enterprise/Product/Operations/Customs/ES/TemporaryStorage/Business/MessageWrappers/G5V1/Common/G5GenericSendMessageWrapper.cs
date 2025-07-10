using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5GenericSendMessageWrapper : IG5GenericMessageDataProvider
	{
		public G5GenericSendMessageWrapper(TemporaryStorageHeader tempHeader, ICertificateProvider certificateData)
		{
			this.tempHeader = Argument.NotNull(tempHeader, nameof(tempHeader));
			certificate = Argument.NotNull(certificateData, nameof(certificateData));
		}
		protected TemporaryStorageHeader tempHeader;
		readonly ICertificateProvider certificate;

		public ZString SenderId => (tempHeader.Declarant?.Header).GetIDCode();

		public ZBool IsTest
		{
			get
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
							&& (!registration.IsWiseTechGlobalInternalSystem()
								|| (bool)tempHeader.TrainingEntry);
			}
		}

		public ZString BusinessObjectReference => tempHeader.AMA_JobReference;

		public EDIMessageCollection Messages => messages ??= tempHeader.Messages;
		EDIMessageCollection messages;

		public BusinessObjectFactory Factory => tempHeader.Factory;

		public ZString BrokerCode => certificate.BrokerCode;

		public ZString CertificateName => certificate.CertificateName;

		public ZString CertificateThumbPrint => certificate.CertificateThumbPrint;

		public ZBlob CertificateBytes => certificate.CertificateBytes;

		public ZString DecryptedCertificatePassphrase => certificate.DecryptedCertificatePassphrase;

		public ZGuid CertificatePK => certificate.CertificatePK;

		public ZString CertificateID => certificate.CertificateID;
	}
}
