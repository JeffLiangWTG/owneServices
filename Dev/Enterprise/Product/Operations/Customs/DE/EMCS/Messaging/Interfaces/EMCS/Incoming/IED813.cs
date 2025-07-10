using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED813 : IEmcsDataProvider
	{
		IEMCSEvent UpdateEadEsad { get; }
		ZString NewDestinationCode { get; }
		ZString JourneyTime { get; }
		ZString TransportModeCode { get; }
		ZString TransportArrangement { get; }
		ZString ComplementaryInfo { get; }
		ZString InvoiceNumber { get; }
		ZDate InvoiceDate { get; }
		ZString DestinationTypeCode { get; }
		ZString GuarantorTypeCode { get; }
		IEMCSPartyGuarantor Guarantor { get; }
		IEMCSPartyDeliveryPlace DeliveryPlace { get; }
		IEMCSPartyTransporter NewTransportArranger { get; }
		IEMCSPartyTransporter NewTransporter { get; }
		IReadOnlyCollection<IEMCSTransportDetails> TransportDetails { get; }
		IEMCSPartyConsignee Consignee { get; }
	}
}
