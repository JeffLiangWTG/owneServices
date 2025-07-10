using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public class EDIInterchangeCreatorForTaxCore : GEIEDIInterchangeCreatorForTransactions
	{
		public EDIInterchangeCreatorForTaxCore(GlbCompany company, ITaxCoreCountryEInvoicingObjectFactory countryEInvoicingObjectFactory) : base(company)
		{
			Argument.NotNull(countryEInvoicingObjectFactory, nameof(countryEInvoicingObjectFactory));
			CountryEInvoicingObjectFactory = countryEInvoicingObjectFactory;
		}

		protected readonly ITaxCoreCountryEInvoicingObjectFactory CountryEInvoicingObjectFactory;

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => new TransactionBatchToGEIConverterForTaxCore(CountryEInvoicingObjectFactory);

		protected override IEDICommunicationsMode GetDefaultCommunicationsMode() => StandardCommunicationModeForEHubXml();

		protected override GEIDeliveryModeAndContextProvider GetDeliveryModeAndContextProvider(GEIProcessContext geiProcessContext, INotifications notifications)
		{
			SetPurposeForCurrentBatch(geiProcessContext.Batch);
			var deliveryContext = StandardDeliveryContext(geiProcessContext, notifications, ZString.Empty, ZString.Empty);
			return StandardDeliveryModeAndContextProviderForDefaultSerializer(deliveryContext);
		}
	}
}
