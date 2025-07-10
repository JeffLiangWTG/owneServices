using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface IIE815EMCSMessage
{
	IIE815EMCSMessageHeader Header { get; }
	IEnumerable<IIE815EMCSMessageContinuation> Continuations { get; }
}

public interface IIE815EMCSMessageHeader
{
	IIE815Attributes Attributes { get; }
	IIE815MessageHeaderDetailPart DetailPart { get; }
	IHeaderEad HeaderEad { get; }
	IConsignorTrader ConsignorTrader { get; }
	IConsigneeTrader ConsigneeTrader { get; }
	ITransportMode TransportMode { get; }
	ITransportTrader TransportArrangerTrader { get; }
	ITransportTrader FirstTransporterTrader { get; }
	IMovementGuarantee MovementGuarantee { get; }
	IImportSadContainer ImportSads { get; }
	ITransportDetailContainer TransportDetailContainer { get; }
	ICertificates Certificates { get; }
	IIE815BodyEad BodyEad { get; }
	IPlaceOfDispatchTrader PlaceOfDispatchTrader { get; }
	IDeliveryPlaceTrader DeliveryPlaceTrader { get; }
	IDocumentCertificate DocumentCertificate { get; }
	IOffice CompetentAuthorityDispatchOffice { get; }
	IOffice DeliveryPlaceCustomsOffice { get; }
	IComplementConsigneeTrader ComplementConsigneeTrader { get; }
	IOffice DispatchImportOffice { get; }
	IEadDraft EadDraft { get; }
}

public interface IIE815EMCSMessageContinuation
{
	IIE815Attributes Attributes { get; }
	IIE815BodyEad BodyEad { get; }
}

public interface IIE815Attributes
{
	ZString IDRegistrantCode { get; }
	ZDate DateOfTrasmission { get; }
	ZString LocalIdentityNumber { get; }
}

public interface IIE815BodyEad
{
	ZInt BodyRecordUniqueReference { get; }
	ZString ExciseProductCode { get; }
	ZString CnCode { get; }
	ZDecimal Quantity { get; }
	ZDecimal GrossWeight { get; }
	ZDecimal NetWeight { get; }
	ZString FiscalMark { get; }
	ZBool FiscalMarkUsedFlag { get; }
	ZDecimal Density { get; }
	ZString CommercialDescription { get; }
	ZString BrandNameOfProducts { get; }
	ZDecimal AlcoholicStrength { get; }
	ZDecimal DegreePlato { get; }
	ZString DesignationOfOrigin { get; }
	ZInt SizeOfProducer { get; }
	ZString FiscalMarkLanguage { get; }
	ZInt TaricCode { get; }
	ZString CaddCode { get; }
	ZString AamsCode { get; }
	ZString CommercialDescriptionLanguage { get; }
	ZString BrandNameOfProductsLanguage { get; }
	ZString DesignationOfOriginLanguage { get; }
	IEnumerable<IPackage> Packages { get; }
	IWineProduct WineProduct { get; }
}

public interface IIE815MessageHeaderDetailPart
{
	ZInt MessageType { get; }
}
