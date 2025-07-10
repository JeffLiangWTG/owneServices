using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	public class GlobalEDIInterchangeCreator : GEIEDIInterchangeCreatorForTransactions
	{
		public GlobalEDIInterchangeCreator(GlbCompany company, ICountryEInvoicingObjectFactory countryFactory, ZString servicePointSuffix) : base(company)
		{
			CountryFactory = countryFactory;
			ServicePointSuffix = servicePointSuffix;
		}

		protected ICountryEInvoicingObjectFactory CountryFactory { get; }
		ZString ServicePointSuffix { get; }

		protected override ZString GetEInvoicingServicePoint() => CountryFactory.GetEInvoicingServicePoint(ServicePointSuffix);

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => new GlobalTransactionBatchToGEIConverter(CountryFactory);

		protected override string ParentTableCode => AccTransactionHeaderSchema.Constants.Prefix;

		protected override bool AddErrorToEDIMessageNotesIfAny => true;

		protected override IEDICommunicationsMode GetDefaultCommunicationsMode()
			=> CountryFactory.CommunicationTransport switch
			{
				EDICommunicationsModeCommunicationsTransportList.Codes.EHubService => StandardCommunicationModeForEHubXml(),
				EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface => StandardCommunicationModeForDirectXTXml(),
				_ => throw new NotSupportedException($"CountryFactory CommunicationTransport '{CountryFactory.CommunicationTransport}' is not supported. Only '{EDICommunicationsModeCommunicationsTransportList.Codes.EHubService}' and '{EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface}' are supported."),
			};

		protected override GEIDeliveryModeAndContextProvider GetDeliveryModeAndContextProvider(GEIProcessContext geiProcessContext, INotifications notifications)
		{
			SetPurposeForCurrentBatch(geiProcessContext.Batch);
			var deliveryContext = StandardDeliveryContext(geiProcessContext, notifications, "", "");
			return StandardDeliveryModeAndContextProviderForDefaultSerializer(deliveryContext);
		}
	}
}
