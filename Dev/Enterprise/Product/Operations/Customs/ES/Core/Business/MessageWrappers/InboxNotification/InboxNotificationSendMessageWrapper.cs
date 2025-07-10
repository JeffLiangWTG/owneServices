using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class InboxNotificationSendMessageWrapper : IInboxNotificationMessageDataProvider
	{
		public InboxNotificationSendMessageWrapper(BusinessObjectFactory boFactory, OrgHeader declarant, ZBool isTest, ZString mesType, ICertificateProvider certificateData)
		{
			certificate = Argument.NotNull(certificateData, nameof(certificateData));
			messageType = Argument.NotNullOrEmpty(mesType, nameof(mesType));
			this.declarant = declarant;

			IsTest = isTest;
			Factory = boFactory;
		}

		readonly OrgHeader declarant;
		readonly ICertificateProvider certificate;
		readonly ZString messageType;

		public ZString ResponseType => GetResponseType();

		public ZString DeclarantName => declarant?.OH_FullName ?? ZString.Empty;

		public ZString DeclarantID => declarant?.GetNIFCode() ?? ZString.Empty;

		public ZBool IsTest { get; }

		public ZString BusinessObjectReference => ZString.Empty;

		public EDIMessageCollection Messages => null;

		public BusinessObjectFactory Factory { get; }

		public ZString BrokerCode => certificate.BrokerCode;

		public ZString CertificateName => certificate.CertificateName;

		public ZString CertificateThumbPrint => certificate.CertificateThumbPrint;

		public ZBlob CertificateBytes => certificate.CertificateBytes;

		public ZString DecryptedCertificatePassphrase => certificate.DecryptedCertificatePassphrase;

		public ZGuid CertificatePK => certificate.CertificatePK;

		public ZString CertificateID => certificate.CertificateID;

		ZString GetResponseType()
		{
			switch (messageType)
			{
				case DeclarationMessageTypeList.Codes.InBoxNotificationForExport:
					return InboxNotificationResponseTypes.Export;
				case DeclarationMessageTypeList.Codes.InBoxNotificationForImport:
					return InboxNotificationResponseTypes.Import;
				case DeclarationMessageTypeList.Codes.ExportInvalidationCommunication:
					return InboxNotificationResponseTypes.AESInvalidation;
				case DeclarationMessageTypeList.Codes.ExportClearanceCommunication:
					return InboxNotificationResponseTypes.AESClearance;
				case DeclarationMessageTypeList.Codes.ExportNonConformityCommunication:
					return InboxNotificationResponseTypes.AESNonConformity;
				case DeclarationMessageTypeList.Codes.ExportCceControlCommunication:
					return InboxNotificationResponseTypes.AESCceControl;
				case DeclarationMessageTypeList.Codes.ExportExitResultCommunication:
					return InboxNotificationResponseTypes.AESExitResult;
				case DeclarationMessageTypeList.Codes.ExportExitClearanceNotification:
					return InboxNotificationResponseTypes.AESExitClearance;
				case DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification:
					return InboxNotificationResponseTypes.AESExitNonConformity;
				case DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2:
					return InboxNotificationResponseTypes.DVD;
				case DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture:
					return InboxNotificationResponseTypes.NCTSNonConformity;
				case DeclarationMessageTypeList.Codes.InboxNotificationNctsControls:
					return InboxNotificationResponseTypes.NCTSCceControl;
				case DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance:
					return InboxNotificationResponseTypes.NCTSClearance;
				case DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation:
					return InboxNotificationResponseTypes.NCTSInvalidation;
				default:
					return ZString.Empty;
			}
		}
	}
}
