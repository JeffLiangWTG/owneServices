using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class InboxNotificationDetailCommonSendMessageWrapper : IInboxNotificationDetailCommonMessageDataProvider
	{
		public InboxNotificationDetailCommonSendMessageWrapper(BusinessObjectFactory boFactory, IESResponseBusinessObject bo, ZBool isTest, ZString keyCode, ICertificateProvider certificateData)
		{
			businessObject = Argument.NotNull(bo, nameof(bo));
			certificate = Argument.NotNull(certificateData, nameof(certificateData));
			Key = Argument.NotNullOrEmpty(keyCode, nameof(keyCode));

			IsTest = isTest;
			Factory = boFactory;
		}

		readonly IESResponseBusinessObject businessObject;
		readonly ICertificateProvider certificate;

		public ZString Key { get; }

		public ZBool IsTest { get; }

		public ZString BusinessObjectReference => businessObject.EntryReference;

		public EDIMessageCollection Messages => businessObject.MessageCollection;

		public BusinessObjectFactory Factory { get; }

		public ZString BrokerCode => certificate.BrokerCode;

		public ZString CertificateName => certificate.CertificateName;

		public ZString CertificateThumbPrint => certificate.CertificateThumbPrint;

		public ZBlob CertificateBytes => certificate.CertificateBytes;

		public ZString DecryptedCertificatePassphrase => certificate.DecryptedCertificatePassphrase;

		public ZGuid CertificatePK => certificate.CertificatePK;

		public ZString CertificateID => certificate.CertificateID;
	}
}
