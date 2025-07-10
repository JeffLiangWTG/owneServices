using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class EntryHeaderCommonSendMessageWrapper : IESEDIMessageCollectionProvider
	{
		public EntryHeaderCommonSendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.certificate = Argument.NotNull(certificate, nameof(certificate));
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		}
		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;
		protected readonly ICertificateProvider certificate;

		public ZBool IsTest
		{
			get
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
							&& (!registration.IsWiseTechGlobalInternalSystem()
				|| (bool)declaration.ZG_IsTrainingDeclaration);
			}
		}

		public ZString BusinessObjectReference => entryHeader.CH_BGMReference;

		public EDIMessageCollection Messages => messages ?? (messages = entryHeader.Messages);
		EDIMessageCollection messages;

		public BusinessObjectFactory Factory => entryHeader.Factory;

		public ZString BrokerCode => certificate.BrokerCode;

		public ZString CertificateName => certificate.CertificateName;

		public ZString CertificateThumbPrint => certificate.CertificateThumbPrint;

		public ZBlob CertificateBytes => certificate.CertificateBytes;

		public ZString DecryptedCertificatePassphrase => certificate.DecryptedCertificatePassphrase;

		public ZGuid CertificatePK => certificate.CertificatePK;

		public ZString CertificateID => certificate.CertificateID;
	}
}
