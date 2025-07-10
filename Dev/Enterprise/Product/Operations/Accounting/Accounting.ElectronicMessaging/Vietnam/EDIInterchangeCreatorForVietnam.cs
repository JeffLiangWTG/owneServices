using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.FeatureConfiguration;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class EDIInterchangeCreatorForVietnam : GEIEDIInterchangeCreatorForTransactions
	{
		public EDIInterchangeCreatorForVietnam(GlbCompany company) : base(company)
		{
		}

		protected override IEDICommunicationsMode GetDefaultCommunicationsMode() => StandardCommunicationModeForEHubXml();

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => new TransactionBatchToGEIConverterForVietnam();

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

		protected override ZString GetEInvoicingServicePoint() => FeatureSettingsReader.Value.Transport.Destination.FallbackOnNullOrEmpty("XHUB_VN_EINVOICING");

		EInvoicingFeatureSettingsReader FeatureSettingsReader
			=> FeatureSettingsReaderUnsafe ??= new EInvoicingFeatureSettingsReader(Constants.CountryCodes.VietNam, ObjectFactory.Get<IFeatureControlManager>());
		EInvoicingFeatureSettingsReader FeatureSettingsReaderUnsafe;
	}
}
