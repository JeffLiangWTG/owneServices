using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public abstract class G3CommonSendMessageWrapper : IG3CommonMessageDataProvider
	{
		protected G3CommonSendMessageWrapper(IEnumerable<AsycudaBill> bills, ICertificateProvider certificate, string localReferenceNumber)
		{
			this.bills = Argument.NotNull(bills, nameof(bills));
			this.certificate = Argument.NotNull(certificate, nameof(certificate));
			this.localReferenceNumber = Argument.NotNullOrEmpty(localReferenceNumber, nameof(localReferenceNumber));

			header = Argument.NotNull(bills.FirstOrDefault()?.Header, "bills[0].Header");
		}

		protected readonly IEnumerable<AsycudaBill> bills;

		protected readonly AsycudaManifestHeader header;

		protected readonly ICertificateProvider certificate;

		protected readonly string localReferenceNumber;

		#region IESEDIMessageCollectionProvider

		public ZBool IsTest
		{
			get
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
							&& (!registration.IsWiseTechGlobalInternalSystem() || (bool)header.TrainingEntry);
			}
		}

		public ZString BusinessObjectReference => header.AMA_JobReference;

		#endregion

		#region IEDIMessageCollectionProvider

		public EDIMessageCollection Messages => messages ??= new EDIMessageCollection(header);
		EDIMessageCollection messages;

		public BusinessObjectFactory Factory => header.Factory;

		#endregion

		#region ICertificateProvider

		public ZString BrokerCode => certificate?.BrokerCode ?? ZString.Empty;

		public ZString CertificateName => certificate?.CertificateName ?? ZString.Empty;

		public ZString CertificateThumbPrint => certificate?.CertificateThumbPrint ?? ZString.Empty;

		public ZBlob CertificateBytes => certificate?.CertificateBytes ?? ZBlob.Empty;

		public ZString DecryptedCertificatePassphrase => certificate?.DecryptedCertificatePassphrase ?? ZString.Empty;

		public ZGuid CertificatePK => certificate?.CertificatePK ?? ZGuid.Empty;

		public ZString CertificateID => certificate?.CertificateID ?? ZString.Empty;

		#endregion

		#region IG3CommonMessageDataProvider

		public IG3Message Message => CachedValueHelper.GetValue(ref message, () => new G3MessageWrapper(certificate, Factory));

		CachedValue<IG3Message> message;

		#endregion
	}
}
