using CargoWise.ComponentModel;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class EDIInterchangeCreatorForItaly : GEIEDIInterchangeCreatorForTransactions
	{
		public EDIInterchangeCreatorForItaly(GlbCompany company)
			: base(company)
		{ }

		protected override IEDICommunicationsMode GetDefaultCommunicationsMode()
		{
			return new NonPersistentEDICommunicationMode
			{
				EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
				EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
				EK_Destination = EInvoicingServicePoint,
				EK_MessagePurpose = Purpose
			};
		}

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter =>
			new TransactionBatchToGEIConverterForItaly();

		protected override GEIDeliveryModeAndContextProvider GetDeliveryModeAndContextProvider(GEIProcessContext geiProcessContext, INotifications notifications)
		{
			SetPurposeForCurrentBatch(geiProcessContext.Batch);

			return new GEIDeliveryModeAndContextProvider()
			{
				Serializer = new GlobalElectronicInvoiceWithCDataPayloadSerializer(),

				Context = GetDeliveryContext(geiProcessContext, notifications),

				CreateEDIMessageEvenIfThereIsError = true,

				DeliverEDIMessageEvenIfThereIsError = ShouldEInvoiceBatchWithErrorBeSent,

				Modes = GetModes()
			};
		}

		DeliveryContext GetDeliveryContext(GEIProcessContext geiProcessContext, INotifications notifications)
		{
			var context = new DeliveryContext(geiProcessContext.EDIInterchangeCreationFactory)
			{
				ParentInfo = EntityInfo.New(geiProcessContext.Batch),
				PurposeCode = Purpose,
				ApplicationCode = ApplicationCodeList.Codes.GlobalElectronicInvoice,
				MessageTypeCode = geiProcessContext.EInvoice.Header.ElectronicInvoiceBatchRequest.MessageType,
				MessageSubTypeCode = geiProcessContext.EInvoice.Header.ElectronicInvoiceBatchRequest.MessageType,
				Notifications = notifications
			};
			return context;
		}
	}
}
