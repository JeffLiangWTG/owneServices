using System.Collections.Generic;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE813 : IEMCSInboundProvider
	{
		IEMCSEvent UpdateEad { get; }
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
		ZString AdministrativeReferenceCode { get; }
	}
}
