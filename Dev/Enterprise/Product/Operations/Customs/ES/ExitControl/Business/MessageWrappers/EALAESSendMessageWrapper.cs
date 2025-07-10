using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALAESSendMessageWrapper : IEALAESMessageDataProvider
	{
		public EALAESSendMessageWrapper(CusExitReport exitReport, ICertificateProvider certificateData)
		{
			this.exitReport = Argument.NotNull(exitReport, nameof(exitReport));
			this.certificateData = Argument.NotNull(certificateData, nameof(certificateData));
			exitConsignment = Argument.NotNull(exitReport.Consignment, nameof(exitReport.Consignment));
		}
		readonly CusExitReport exitReport;
		readonly ICertificateProvider certificateData;
		readonly CusExitConsignment exitConsignment;

		public IEALAESExportOperation ExportOperation => exportOperation ?? (exportOperation = new EALAESExportOperationWrapper(exitReport));
		EALAESExportOperationWrapper exportOperation;

		public ZString CustomsOfficeOfExitActual => exitReport.CER_OfficeOfExit;

		public IEALAESGoodsShipment GoodsShipment => goodsShipment ?? (goodsShipment = new EALAESGoodsShipmentWrapper(exitReport));
		EALAESGoodsShipmentWrapper goodsShipment;

		public IAESCommonMessage Message => message ?? (message = new EALAESMessageWrapper(exitReport.Header));
		EALAESMessageWrapper message;

		public ZBool IsFinalPeriod => IsFinalPeriodAndTestDeclaration;

		public ZBool PhaseIDSpecified => IsFinalPeriodAndTestDeclaration;

		ZBool IsFinalPeriodAndTestDeclaration => IsTest && MessageVersionRegistryProvider.IsExportVersionAes11();

		public ZBool IsTest
		{
			get
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
							&& (!registration.IsWiseTechGlobalInternalSystem()
								|| (bool)exitReport.Header.TrainingEntry);
			}
		}

		public ZString BusinessObjectReference => exitConsignment.CXC_MovementReference;

		public EDIMessageCollection Messages => messages ?? (messages = exitReport.Messages);
		EDIMessageCollection messages;

		public BusinessObjectFactory Factory => exitReport.Factory;

		public ZString BrokerCode => certificateData.BrokerCode;

		public ZString CertificateName => certificateData.CertificateName;

		public ZString CertificateThumbPrint => certificateData.CertificateThumbPrint;

		public ZBlob CertificateBytes => certificateData.CertificateBytes;

		public ZString DecryptedCertificatePassphrase => certificateData.DecryptedCertificatePassphrase;

		public ZGuid CertificatePK => certificateData.CertificatePK;

		public ZString CertificateID => certificateData.CertificateID;
	}
}
