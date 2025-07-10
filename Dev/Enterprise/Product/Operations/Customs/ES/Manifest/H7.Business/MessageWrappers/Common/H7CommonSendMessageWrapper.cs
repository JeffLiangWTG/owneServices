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
	public abstract class H7CommonSendMessageWrapper : IESEDIMessageCollectionProvider
	{
		protected H7CommonSendMessageWrapper(AsycudaBill bill, ICertificateProvider certificate)
		{
			Bill = Argument.NotNull(bill, nameof(bill));
			Certificate = Argument.NotNull(certificate, nameof(certificate));
		}

		protected AsycudaBill Bill { get; }
		protected ICertificateProvider Certificate { get; }

		#region IESEDIMessageCollectionProvider

		public ZBool IsTest
		{
			get
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
							&& (!registration.IsWiseTechGlobalInternalSystem() || (bool)Bill.Header.TrainingEntry);
			}
		}

		public ZString BusinessObjectReference => Bill.ABL_BillNumber;

		#endregion

		#region IEDIMessageCollectionProvider

		public EDIMessageCollection Messages => messages ?? (messages = Bill.Messages);
		EDIMessageCollection messages;

		public BusinessObjectFactory Factory => Bill.Factory;

		#endregion

		#region ICertificateProvider

		public ZString BrokerCode => Certificate?.BrokerCode ?? ZString.Empty;

		public ZString CertificateName => Certificate?.CertificateName ?? ZString.Empty;

		public ZString CertificateThumbPrint => Certificate?.CertificateThumbPrint ?? ZString.Empty;

		public ZBlob CertificateBytes => Certificate?.CertificateBytes ?? ZBlob.Empty;

		public ZString DecryptedCertificatePassphrase => Certificate?.DecryptedCertificatePassphrase ?? ZString.Empty;

		public ZGuid CertificatePK => Certificate?.CertificatePK ?? ZGuid.Empty;

		public ZString CertificateID => Certificate?.CertificateID ?? ZString.Empty;

		#endregion
	}
}
