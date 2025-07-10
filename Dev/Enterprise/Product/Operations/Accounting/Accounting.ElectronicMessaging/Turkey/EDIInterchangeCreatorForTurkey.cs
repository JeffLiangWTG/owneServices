using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class EDIInterchangeCreatorForTurkey : GlobalEDIInterchangeCreator
	{
		public EDIInterchangeCreatorForTurkey(GlbCompany company, ICountryEInvoicingObjectFactory countryFactory) : base(company, countryFactory, ZString.Empty)
		{
		}

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

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => new TransactionBatchToGEIConverterForTurkey(CountryFactory);

		protected override GEIDeliveryModeAndContextProvider GetDeliveryModeAndContextProvider(GEIProcessContext geiProcessContext, INotifications notifications)
		{
			SetPurposeForCurrentBatch(geiProcessContext.Batch);

			return new GEIDeliveryModeAndContextProvider()
			{
				Serializer = new DefaultGlobalElectronicInvoiceSerializer(),
				Context = GetDeliveryContext(geiProcessContext, notifications),
				CreateEDIMessageEvenIfThereIsError = true,
				DeliverEDIMessageEvenIfThereIsError = ShouldEInvoiceBatchWithErrorBeSent,
				Modes = GetModes()
			};
		}

		DeliveryContext GetDeliveryContext(GEIProcessContext geiProcessContext, INotifications notifications)
		{
			var requestMessageType = geiProcessContext.EInvoice?.Header.ElectronicInvoiceBatchRequest.MessageType;
			var messageType = requestMessageType ?? TurkeyEInvoiceAPICommandList.GetMessageType(((AccEInvoicingTransactionPivot)geiProcessContext.Batch.TransactionPivots.FirstOrDefault())?.AIP_ActionType ?? ZString.Empty);

			var context = new DeliveryContext(geiProcessContext.EDIInterchangeCreationFactory)
			{
				ParentInfo = EntityInfo.New(geiProcessContext.Batch),
				PurposeCode = Purpose,
				ApplicationCode = ApplicationCodeList.Codes.GlobalElectronicInvoice,
				MessageTypeCode = messageType,
				MessageSubTypeCode = messageType,
				Notifications = notifications
			};
			return context;
		}

		protected override ZString GetEInvoicingServicePoint() => "XHUB_TR_EINVOICING";      // Constant string used by EServices for message routing.

		protected override bool AddErrorToEDIMessageNotesIfAny => false;
	}
}
