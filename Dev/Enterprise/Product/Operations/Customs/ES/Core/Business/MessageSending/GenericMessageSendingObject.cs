using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class GenericMessageSendingObject : NonPersistentBusinessObject, ICertificateProvider
	{
		public GenericMessageSendingObject(GlbStaff broker, ZString brokerCertificate) : base(new BusinessObjectFactory())
		{
			Broker = Argument.NotNull(broker, nameof(broker));
			Certificate = CertificateHelper.GetCertificate(Broker, brokerCertificate);
		}
		public GlbStaff Broker { get; }
		public GlbExternalPassword Certificate { get; }

		#region Schema

		public static class Schema
		{
			public const string BrokerCertificate = "BrokerCertificate";
			public const string ShouldEditMessage = "ShouldEditMessage";
		}

		#endregion

		#region ShouldEditMessage

		public ZBool ShouldEditMessage
		{
			get => fShouldEditMessage;
			set => SetNonPersistentPropertyValue(ShouldEditMessageInfo, ref fShouldEditMessage, value);
		}
		ZBool fShouldEditMessage;

		public ZPropertyInfo ShouldEditMessageInfo => GetZPropertyInfo(Schema.ShouldEditMessage);

		#endregion

		public bool SendMessage()
		{
			var sendOK = false;
			RunPreSaveValidation();
			if (!HasNotifications())
			{
				sendOK = true;
			}
			return sendOK;
		}

		ZString ICertificateProvider.BrokerCode => Broker.GS_Code;

		ZString ICertificateProvider.CertificateName => Certificate?.GP_Name ?? ZString.Empty;

		ZString ICertificateProvider.CertificateThumbPrint => Certificate?.GP_UserID ?? ZString.Empty;

		ZBlob ICertificateProvider.CertificateBytes => Certificate?.GP_Certificate ?? ZBlob.Empty;

		ZString ICertificateProvider.DecryptedCertificatePassphrase => Certificate?.CurrentDecryptedCertificatePassphrase ?? ZString.Empty;

		public ZGuid CertificatePK => Certificate?.PK ?? ZGuid.Empty;

		ZString ICertificateProvider.CertificateID => Certificate?.GP_MailBoxID ?? ZString.Empty;
	}
}
