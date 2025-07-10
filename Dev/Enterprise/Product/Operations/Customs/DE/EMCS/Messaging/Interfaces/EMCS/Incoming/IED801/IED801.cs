using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED801 : IEmcsDataProvider
	{
		ZString MessageSender { get; }
		ZString LocalReferenceNumber { get; }
		ZString MessageRecipient { get; }
		ZDateTime DateAndTimeOfValidationOfEadEsad { get; }
		ZString DispatchImportOffice { get; }
		ZString DeliveryPlaceCustomsOffice { get; }
		ZString CompetentAuthorityDispatchOffice { get; }
		ZString JourneyTime { get; }
		ZString DestinationTypeCode { get; }
		ZString TransportArrangement { get; }
		ZDateTime DispatchTime { get; }
		ZString OriginTypeCode { get; }
		ZString InvoiceNumber { get; }
		ZDate InvoiceDate { get; }
		IReadOnlyCollection<ZString> ImportSadNumbers { get; }
		ZString GuarantorTypeCode { get; }
		IEMCSPartyGuarantor Guarantor { get; }
		IEMCSPartyConsignee Consignee { get; }
		IEMCSPartyConsignor Consignor { get; }
		IEMCSPartyPlaceOfDispatch PlaceOfDispatch { get; }
		IEMCSPartyDeliveryPlace DeliveryPlace { get; }
		IEMCSPartyTransporter TransportArranger { get; }
		IEMCSPartyTransporter FirstTransporter { get; }
		ZString TransportModeCode { get; }
		ZString ComplementaryInfo { get; }
		IReadOnlyCollection<IEMCSDocumentCert> DocumentCertificates { get; }
		ZString MemberStateCode { get; }
		ZString CertificateOfExemption { get; }
		IReadOnlyCollection<IEMCSTransportDetails> TransportDetails { get; }
		IEMCSEvent ExciseMovement { get; } 
		IReadOnlyCollection<IED801Line> Lines { get; }
	}
}
