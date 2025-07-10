using System.Collections.Generic;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE801 : IEMCSInboundProvider
	{
		ZString MessageSender { get; }
		ZString LocalReferenceNumber { get; }
		ZDateTime DateAndTimeOfValidationOfEad { get; }
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
		IEMCSEvent ExciseMovementEad { get; }
		IReadOnlyCollection<IIE801Line> Lines { get; }
	}
}
