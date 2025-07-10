using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.FeatureConfiguration;
using Enterprise.Accounting.Business.EInvoicing.Germany;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Germany
{
	public class EDIInterchangeCreatorForGermany : GEIEDIInterchangeCreatorForTransactions
	{
		readonly EInvoicingFeatureSettingsReader settingsReader;

		public EDIInterchangeCreatorForGermany(GlbCompany company)
			: base(company)
		{
			var featureControlManager = ObjectFactory.Get<IFeatureControlManager>();
			settingsReader = new EInvoicingFeatureSettingsReader(CountryCodes.Germany, featureControlManager);
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

		IEDICommunicationsMode[] GetDirectXtCommunicationModes()
			=> [
				new NonPersistentEDICommunicationMode
				{
					EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
					EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface,
					EK_Destination = EInvoicingServicePoint,
					EK_MessagePurpose = Purpose
				}
			];

		protected override ZString GetEInvoicingServicePoint() => "XHUB_DE_EINVOICING";      // Constant string used by EServices for message routing.
		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter =>
			new TransactionBatchToGEIConverterForGermany();

		protected override GEIDeliveryModeAndContextProvider GetDeliveryModeAndContextProvider(GEIProcessContext geiProcessContext, INotifications notifications)
		{
			SetPurposeForCurrentBatch(geiProcessContext.Batch);

			var transactionCategory = GetAdditionalDataItemValue(geiProcessContext.EInvoice, GermanyEInvoicingDataItems.TransactionCategory);
			var isB2G = string.Equals(transactionCategory, OrgConstants.Category.Government, StringComparison.OrdinalIgnoreCase)
				|| string.IsNullOrEmpty(transactionCategory);

			var isB2GinXTEnabled = settingsReader.HasFeature(GermanyEInvoicingFeatureFlags.B2GinXT);

			return new GEIDeliveryModeAndContextProvider()
			{
				Serializer = new GlobalElectronicInvoiceWithCDataPayloadSerializer(),
				Context = GetDeliveryContext(geiProcessContext, notifications),
				CreateEDIMessageEvenIfThereIsError = true,
				DeliverEDIMessageEvenIfThereIsError = ShouldEInvoiceBatchWithErrorBeSent,
				Modes = isB2G && !isB2GinXTEnabled ? GetModes() : GetDirectXtCommunicationModes(),
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

		string GetAdditionalDataItemValue(GlobalElectronicInvoicing eInvoice, string dataItemKey)
		{
			if (eInvoice?.Header?.ElectronicInvoiceBatchRequest?.AdditionalDataItems?.Count == null)
			{
				return null;
			}

			var items = eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems;
			for (var i = 0; i < items.Count; i++)
			{
				if (string.Equals(items[i].Key, dataItemKey, StringComparison.OrdinalIgnoreCase))
				{
					return items[i].Value;
				}
			}

			return null;
		}
	}
}
