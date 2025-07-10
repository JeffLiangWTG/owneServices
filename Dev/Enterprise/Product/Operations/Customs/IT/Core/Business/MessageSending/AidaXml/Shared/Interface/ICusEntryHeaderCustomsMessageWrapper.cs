using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public interface ICusEntryHeaderCustomsMessageWrapper
{
	ZString InvoiceCurrencyCode { get; }
	decimal ExchangeRate { get; }
	decimal? InvoiceTotalAmount { get; }
	int NatureOfTransaction { get; }
	ITermsOfDelivery TermOfDelivery { get; }
	int NumberOfPackages { get; }
	IReadOnlyCollection<IAdditionalInformation> AdditionalInformation { get; }
	decimal GrossMass { get; }
	ZString TransportChargesMethodOfPayment { get; }
	IReadOnlyCollection<ITransportEquipment> TransportEquipment { get; }
	int? ExportNatureOfTransaction { get; }
	ZString CountryOfDestination { get; }
	ZString CountryOfExport { get; }
	IEoriTrader Consignor { get; }
	IEoriTrader Consignee { get; }
	ZString DeferredPayment { get; }
}
