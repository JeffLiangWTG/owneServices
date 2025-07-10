using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonSendMessageWrapper : INCTSCommonDataProvider
	{
		public NCTS5CommonSendMessageWrapper(NctsHeader header, ICertificateProvider certificateData)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			certificate = Argument.NotNull(certificateData, nameof(certificateData));
		}
		protected NctsHeader nctsHeader;
		readonly ICertificateProvider certificate;

		public ZString MessageSender
		{
			get
			{
				var representativeId = nctsHeader.IsArrivalMovement ? nctsHeader.ArrivalMovementHeader?.Representative?.Address?.Header == null ? OrgHeaderExtension.GetIDCode(nctsHeader.DestinationTrader?.Address?.Header) : OrgHeaderExtension.GetIDCode(nctsHeader.ArrivalMovementHeader?.Representative?.Address?.Header) : OrgHeaderExtension.GetIDCode(nctsHeader.MovementHeader?.Representative?.Address?.Header);
				return representativeId.IsEmpty ? OrgHeaderExtension.GetIDCode(nctsHeader.Principal?.Address?.Header) : representativeId;
			}
		}

		public ZString MessageIdentification => EDIMessage.MessageNumberPlaceHolder;

		public ZBool IsTest
		{
			get
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
							&& (!registration.IsWiseTechGlobalInternalSystem()
								|| (bool)nctsHeader.TrainingEntry);
			}
		}

		public ZBool IsFinalPeriod => IsFinalPeriodAndTestDeclaration;

		public ZBool PhaseIDSpecified => IsFinalPeriodAndTestDeclaration;

		ZBool IsFinalPeriodAndTestDeclaration => IsTest && !nctsHeader.IsInPhase5TransitionPeriod;

		public ZString BusinessObjectReference => nctsHeader.BH_JobReference;

		public EDIMessageCollection Messages => nctsHeader.Messages;

		public BusinessObjectFactory Factory => nctsHeader.Factory;

		public ZString BrokerCode => certificate.BrokerCode;

		public ZString CertificateName => certificate.CertificateName;

		public ZString CertificateThumbPrint => certificate.CertificateThumbPrint;

		public ZBlob CertificateBytes => certificate.CertificateBytes;

		public ZString DecryptedCertificatePassphrase => certificate.DecryptedCertificatePassphrase;

		public ZGuid CertificatePK => certificate.CertificatePK;

		public ZString CertificateID => certificate.CertificateID;
	}
}
