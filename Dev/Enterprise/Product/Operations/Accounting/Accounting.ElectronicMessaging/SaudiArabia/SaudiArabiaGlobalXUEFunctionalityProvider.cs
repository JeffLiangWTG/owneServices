using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia
{
	class SaudiArabiaGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public SaudiArabiaGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override bool IsBatchEventMessageProcessSupported(string eventType, string messageSubType)
			=> (eventType == AutoEvents.InterchangeAcknowledgedCode && messageSubType == EInvoiceAPICommandList.Codes.GenerateInvoiceRequest) || eventType == AutoEvents.InterchangeRejectedCode;
	}
}
