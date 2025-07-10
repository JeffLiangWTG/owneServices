using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers
{
	public class GbCDSExportDeclarationWrapper : GbCDSImportDeclarationWrapper
	{
		public GbCDSExportDeclarationWrapper(CusEntryHeader header)
			: base(header)
		{
			exportHeader = CDSEntryHeaderWrapper.ExportHeaderWrapper;
		}

		protected new GbCDSExportEntryHeaderWrapper CDSEntryHeaderWrapper => (GbCDSExportEntryHeaderWrapper)base.CDSEntryHeaderWrapper;

		protected override GbCDSImportEntryHeaderWrapper GetHeaderWrapper(CusEntryHeader header)
		{
			return new GbCDSExportEntryHeaderWrapper(header);
		}

		protected override IAmountAndCurrency GetInvoiceAmount()
		{
			var invoiceCurrency = entryHeader.InvoiceHeaders.FirstOrDefault(x => x.Invoice_Currency != null)?.Invoice_Currency?.RX_Code ?? ZString.Empty;
			return entryHeader.IsMultiInvoiceCurrency ? AmountAndCurrencyWrapper.New(declaration.TotalInvoiceAmountInLocalCurrency, declaration.LocalCurrencyCode) : AmountAndCurrencyWrapper.New(declaration.TotalInvoiceAmount.Amount, invoiceCurrency);
		}

		//at this stage do not map EU codes to GB codes, just transmit verbatim
		protected override ZString GetExitOfficeID() => declaration.JE_CustomsOffice;

		protected override IOrganisation GetCarrier()
		{
			var carrierId = declaration.ShippingLine?.GetEuIdentificationNumber() ?? ZString.Empty;

			return carrierId.IsEmpty
				? OrganisationWrapper.New(declaration.ShippingLine?.MainAddress, declaration.ShippingLine?.GetEuIdentificationNumber())
				: OrganisationWrapper.New(ZString.Empty, carrierId);
		}

		protected override IOrganisation GetConsignor() => OrganisationWrapper.New(declaration.ShipperAddress, lineLength: 70);

		protected override ZString GetFreightPaymentMethodCode() => exportHeader.TransportChargesMethodOfPayment;

		protected override IEnumerable<ZString> GetItineraryRoutingCountryCodes() => exportHeader.CountriesOfRouting;

		protected override ZString GetSpecificCircumstancesCodeCode() => SpecificCircumstanceIndicatorCodeList.MapToSpecificCircumstanceCode(declaration.ZG_SpecificCircumstanceIndicator);

		protected override ZString BorderTransportMeansID => CDSEntryHeaderWrapper.GetArrivalTransportMeansId(declaration.JE_TransportMode);
		protected override ZString BorderTransportMeansIdType => CDSEntryHeaderWrapper.GetArrivalTransportMeansIdentificationTypeCode(declaration.JE_TransportMode);

		readonly GbCDSExportHeader exportHeader;
	}
}
