using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	public class GlobalElectronicPaymentEDIInterchangeCreator
	{
		public const string DestinationParty = "XHUB_OFX_EPAYMENT"; // DestinationParty does not need to be translated.

		public void CreateInterchangeAndDeliver(BusinessObjectFactory factory, IEPaymentDeliveryContextValueProvider deliveryContextValueProvider, GlobalElectronicPayment.GlobalElectronicPayment ePayment, INotifications notifications)
		{
			var deliveryContext = GetDeliveryContext(factory, deliveryContextValueProvider, ePayment, notifications);
			var mode = GetStandardCommunicationsMode(deliveryContextValueProvider.Purpose);
			var serializedInvoice = new GlobalElectronicPaymentSerializer().Serialize(ePayment);
			using (var ms = (SubStreamableStream)new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(serializedInvoice)))
			{
				new GlobalElectronicPaymentDelivery().Deliver(deliveryContext, mode, new DeliveryStreamWrapperUXML(ms, deliveryContext.ParentInfo));
			}
		}

		DeliveryContext GetDeliveryContext(BusinessObjectFactory factory, IEPaymentDeliveryContextValueProvider deliveryContextValueProvider, GlobalElectronicPayment.GlobalElectronicPayment ePayment, INotifications notifications)
		{
			var context = new DeliveryContext(factory)
			{
				ParentInfo = deliveryContextValueProvider.EntityInfo,
				PurposeCode = deliveryContextValueProvider.Purpose,
				ApplicationCode = ApplicationCodeList.Codes.GlobalElectronicPayment,
				MessageTypeCode = ePayment.Header.ElectronicPaymentRequest.MessagingSystem,
				MessageSubTypeCode = ePayment.Header.ElectronicPaymentRequest.MessageType,
				Notifications = notifications
			};
			return context;
		}

		IEDICommunicationsMode GetStandardCommunicationsMode(ZString purpose)
		{
			return new NonPersistentEDICommunicationMode
			{
				EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
				EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
				EK_Destination = DestinationParty,
				EK_MessagePurpose = purpose
			};
		}
	}
}
