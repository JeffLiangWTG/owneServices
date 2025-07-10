using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This interface will be used in the future.")]
	public interface IIE815EMCSMessage : IEMCSMessage
	{
		IIE815Attributes Attributes { get; set; }

		IConsigneeTrader ConsigneeTrader { get; set; }

		IConsignorTrader ConsignorTrader { get; set; }

		IPlaceOfDispatchTrader PlaceOfDispatchTrader { get; set; }

		IOffice DispatchImportOffice { get; set; }

		IComplementConsigneeTrader ComplementConsigneeTrader { get; set; }

		IDeliveryPlaceTrader DeliveryPlaceTrader { get; set; }

		IOffice DeliveryPlaceCustomsOffice { get; set; }

		IOffice CompetentAuthorityDispatchOffice { get; set; }

		ITransportTrader TransportArrangerTrader { get; set; }

		ITransportTrader FirstTransporterTrader { get; set; }

		IDocumentCertificate DocumentCertificate { get; set; }

		IHeaderEad HeaderEad { get; set; }

		ITransportMode TransportMode { get; set; }

		IMovementGuarantee MovementGuarantee { get; set; }

		IIE815BodyEad BodyEad { get; set; }

		IEadDraft EadDraft { get; set; }

		ITransportDetails TransportDetails { get; set; }
	}

	public interface IIE815Attributes
	{
		ZString SubmissionMessageType { get; set; }

		ZString DeferredSubmissionFlag { get; set; }
	}

	public interface IComplementConsigneeTrader
	{
		ZString MemberStateCode { get; set; }

		ZString SerialNumberOfCertificateOfExemption { get; set; }
	}

	public interface IDocumentCertificate
	{
		ZString DocumentDescription { get; set; }

		ZString ReferenceOfDocument { get; set; }
	}

	public interface IHeaderEad
	{
		ZString DestinationTypeCode { get; set; }

		ZString JourneyTime { get; set; }

		ZString TransportArrangement { get; set; }
	}

	public interface ITransportMode
	{
		ZString TransportModeCode { get; set; }

		ZString ComplementaryInformation { get; set; }
	}

	public interface IMovementGuarantee
	{
		ZString GuarantorTypeCode { get; set; }

		IGuarantorTrader GuarantorTrader { get; set; }
	}

	public interface IIE815BodyEad : IBodyEad
	{
		ZString AlcoholicStrength { get; set; }

		ZString DegreePlato { get; set; }

		ZString DesignationOfOrigin { get; set; }

		ZString SizeOfProducer { get; set; }

		IWineProduct WineProduct { get; set; }
	}

	public interface IEadDraft
	{
		ZString LocalReferenceNumber { get; set; }

		ZString InvoiceNumber { get; set; }

		ZDate InvoiceDate { get; set; }

		ZString OriginTypeCode { get; set; }

		ZDate DateOfDispatch { get; set; }

		ZDateTime TimeOfDispatch { get; set; }

		IImportSad ImportSad { get; set; }
	}

	public interface IImportSad
	{
		ZString ImportSadNumber { get; set; }
	}
}
