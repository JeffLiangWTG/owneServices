using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETMessageHeader : IMessageHeader
{
	public ETMessageHeader(IETHeader iETHeader, ISadMessageSendingObject sadMessageSendingObject)
	{
		this.iETHeader = Argument.NotNull(iETHeader, nameof(iETHeader));
		this.sadMessageSendingObject = Argument.NotNull(sadMessageSendingObject, nameof(sadMessageSendingObject));
	}

	readonly IETHeader iETHeader;
	readonly ISadMessageSendingObject sadMessageSendingObject;

	[MessageLayout(Order = 0)]
	public ISadMessageFixedPart FixedPart => SadMessageFixedPartFactory.GetSadMessageFixedPart
		(isHeader: true
		, sadMessageSendingObject: sadMessageSendingObject
		, messageCode: SadMessageFixedPart.Constants.MessageCode.ET.Header
		, annualProgressiveNumber: iETHeader.AnnualProgressiveNumber
		, progressiveNumber: 0);

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 7, false)]
	[MessageFieldExportRules("D", "CN91")]
	[MessageFieldExportWithTransitRules("D", "CN91")]
	[MessageFieldTransitRules("D", "CN91")]
	public ZString AuthorizationNumberAndCIN => FormattableString.Invariant($"{iETHeader.AuthorizationNo}{iETHeader.AuthorizationCIN}");

	[MessageLayout(Order = 2)]
	[MessageFieldExportRules("R", "O")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R", "RN5")]
	public ETHeaderDeclaration Declaration => new ETHeaderDeclaration(iETHeader.Declaration);

	[MessageLayout(Order = 3)]
	[MessageFieldDateDDMMYYYYRepresentation]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZDate AcceptanceDate => iETHeader.AcceptanceDate;

	[MessageLayout(Order = 4)]
	[MessageFieldBoolRepresentation()]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O", "RN7")]
	public ZBool HeaderDataDeclaredOnItems => iETHeader.HeaderDataDeclaredOnItems;

	[MessageLayout(Order = 5)]
	[MessageFieldBoolRepresentation()]
	[MessageFieldExportWithTransitRules("O", "R229")]
	[MessageFieldTransitRules("O", "R229")]
	[MessageFieldInternationalRoadTransportsRules("O", "R229")]
	public ZBool? SecurityFlag => iETHeader.SecurityData;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("O", "R839")]
	[MessageFieldExportWithTransitRules("D", "C186", "R825")]
	[MessageFieldTransitRules("D", "C186", "R825")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186", "R825")]
	public ZString SpecificCircumstanceIndicator => iETHeader.SecurityBlock.SpecificCircumstanceIndicator;

	[MessageLayout(Order = 7)]
	[MessageFieldIntegerRepresentation(5, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZInt TotalNumberOfItems => iETHeader.TotalItems;

	[MessageLayout(Order = 8)]
	[MessageFieldExportRules("D", "R10")]
	[MessageFieldExportWithTransitRules("D", "R10")]
	[MessageFieldTransitRules("D", "R10")]
	[MessageFieldInternationalRoadTransportsRules("D", "R10")]
	public ETHeaderConsignor Consignor => new ETHeaderConsignor(iETHeader.Consignor);

	[MessageLayout(Order = 9)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 22, false)]
	public ZString ReferenceNumber => ZString.Empty;

	[MessageLayout(Order = 10)]
	[MessageFieldExportRules("O", "R11", "RN11")]
	[MessageFieldExportWithTransitRules("O", "R11", "RN11")]
	[MessageFieldTransitRules("D", "R11", "C1", "RN11")]
	[MessageFieldInternationalRoadTransportsRules("D", "R11", "C1", "RN11")]
	public ETHeaderConsignee Consignee => new ETHeaderConsignee(iETHeader.Consignee);

	[MessageLayout(Order = 11)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ETHeaderDeclarantTrader DeclarantTrader => new ETHeaderDeclarantTrader(iETHeader.DeclarantTrader);

	[MessageLayout(Order = 12)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("D", "C135")]
	[MessageFieldTransitRules("D", "C135")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString CountryOfDispatch => iETHeader.CountryOfDispatch;

	[MessageLayout(Order = 13)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("D", "C140")]
	[MessageFieldTransitRules("D", "C140")]
	[MessageFieldInternationalRoadTransportsRules("D", "C140")]
	public ZString CountryOfDestination => iETHeader.CountryOfDestination;

	[MessageLayout(Order = 14)]
	[MessageFieldExportRules("D", "C5", "R831", "R838", "O")]
	[MessageFieldExportWithTransitRules("O", "D", "TR90", "95")]
	[MessageFieldTransitRules("D", "TR90", "90")]
	[MessageFieldInternationalRoadTransportsRules("D", "TR90", "90", "95")]
	public ETHeaderMeansOfTransportAtDeparture MeansOfTransportAtDeparture => new ETHeaderMeansOfTransportAtDeparture(iETHeader.MeansOfTransportAtDeparture);

	[MessageLayout(Order = 15)]
	[MessageFieldBoolRepresentation()]
	[MessageFieldExportRules("R", "R230")]
	[MessageFieldExportWithTransitRules("R", "R230")]
	[MessageFieldTransitRules("R", "R230")]
	[MessageFieldInternationalRoadTransportsRules("R", "R230")]
	public ZBool? IsContainerizedTransport => iETHeader.IsContainerizedTransport;

	[MessageLayout(Order = 16)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	public ETHeaderTermOfDeliveryGroup TermsOfDelivery => new ETHeaderTermOfDeliveryGroup(iETHeader.TermsOfDelivery);

	[MessageLayout(Order = 17)]
	[MessageFieldExportRules("D", "R838", "C10")]
	[MessageFieldExportWithTransitRules("D", "R838", "C10")]
	[MessageFieldTransitRules("D", "C11", "C10", "R36")]
	public ETHeaderMeansOfTransportCrossingBorder MeansOfTransportCrossingBorder => new ETHeaderMeansOfTransportCrossingBorder(iETHeader.MeansOfTransportCrossingBorder);

	[MessageLayout(Order = 18)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	public ZString DialogLanguageIndicatorAtDeparture => iETHeader.DialogLanguageIndicatorAtDeparture;

	[MessageLayout(Order = 19)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString LanguageCodeUsedForDATAndDAE => ZString.Empty;

	[MessageLayout(Order = 20)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("O")]
	public ETHeaderTransactionData TransactionData => new ETHeaderTransactionData(iETHeader.TransactionData);

	[MessageLayout(Order = 21)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString TransportModeAtBorder => iETHeader.TransportModeAtBorder;

	[MessageLayout(Order = 22)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	[MessageFieldExportRules("D", "C557")]
	[MessageFieldExportWithTransitRules("D", "C557")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString InlandTransportMode => iETHeader.InlandTransportMode;

	[MessageLayout(Order = 23)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldExportWithTransitRules("D", "C191")]
	[MessageFieldTransitRules("D", "C191")]
	[MessageFieldInternationalRoadTransportsRules("D", "C191")]
	public ZString PlaceOfLoadingCode => iETHeader.SecurityBlock.PlaceOfLoadingCode;

	[MessageLayout(Order = 24)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldExportRules("D", "CN2")]
	[MessageFieldExportWithTransitRules("D", "CN2")]
	public ZString ExitCustomsOffice => iETHeader.ExitCustomsOffice;

	[MessageLayout(Order = 25)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("D", "C186")]
	[MessageFieldTransitRules("D", "C186")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186")]
	public ZString TransportChargesMethodOfPayment => iETHeader.SecurityBlock.TransportChargesMethodOfPayment;

	[MessageLayout(Order = 26)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 70, false)]
	[MessageFieldExportRules("D", "C567", "R876")]
	[MessageFieldExportWithTransitRules("D", "C186", "C547", "R876")]
	[MessageFieldTransitRules("D", "C186", "C547", "R876")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186", "C547", "R876")]
	public ZString CommercialReferenceNumber => iETHeader.SecurityBlock.CommercialReferenceNumber;

	[MessageLayout(Order = 27)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportWithTransitRules("D", "C531")]
	[MessageFieldTransitRules("D", "C531")]
	[MessageFieldInternationalRoadTransportsRules("D", "C531")]
	public ZString ConveyanceReferenceNumber => iETHeader.SecurityBlock.ConveyanceReferenceNumber;

	[MessageLayout(Order = 28)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportWithTransitRules("D", "C186")]
	[MessageFieldTransitRules("D", "C186", "C589")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186", "C589")]
	public ZString PlaceOfUnloadingCode => iETHeader.SecurityBlock.PlaceOfUnloadingCode;

	[MessageLayout(Order = 29)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString PlaceOfUnloadingCodeLng => ZString.Empty;

	[MessageLayout(Order = 30)]
	[MessageFieldExportRules("D", "CN92")]
	[MessageFieldExportWithTransitRules("D", "CN92")]
	[MessageFieldTransitRules("D", "CN92")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN92")]
	public ETHeaderAgreedLocationOfGoods AgreedLocationOfGoods => new ETHeaderAgreedLocationOfGoods(iETHeader.AgreedLocationOfGoods);

	[MessageLayout(Order = 31)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("D", "C186", "C587")]
	[MessageFieldTransitRules("D", "C186", "C587")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186", "C587")]
	public ETHeaderSecurityBlockItinerary Itinerary => new ETHeaderSecurityBlockItinerary(iETHeader.SecurityBlock.TransitCountries);

	[MessageLayout(Order = 32)]
	[MessageFieldExportWithTransitRules("D", "C186", "C187")]
	[MessageFieldTransitRules("D", "C186", "C187")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186", "C187")]
	public ETHeaderSecurityBlockConsignor SecurityBlockConsignor => new ETHeaderSecurityBlockConsignor(iETHeader.SecurityBlock.Consignor);

	[MessageLayout(Order = 33)]
	[MessageFieldExportWithTransitRules("D", "C186", "C188")]
	[MessageFieldTransitRules("D", "C186", "C188")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186", "C188")]
	public ETHeaderSecurityBlockConsignee SecurityBlockConsignee => new ETHeaderSecurityBlockConsignee(iETHeader.SecurityBlock.Consignee);

	[MessageLayout(Order = 34)]
	[MessageFieldExportWithTransitRules("D", "C186", "C181")]
	[MessageFieldTransitRules("D", "C186", "C181")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186", "C181")]
	public ETHeaderSecurityBlockCarrier SecurityBlockCarrier => new ETHeaderSecurityBlockCarrier(iETHeader.SecurityBlock.Carrier);

	[MessageLayout(Order = 35)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportRules("D", "C558")]
	[MessageFieldExportWithTransitRules("D", "C558")]
	[MessageFieldTransitRules("D", "C558")]
	[MessageFieldInternationalRoadTransportsRules("D", "C558")]
	public ZString DeferredPayment => FormattableString.Invariant($"{iETHeader.DeferredPayment.AuthorizationReference}{iETHeader.DeferredPayment.CinOfAuthorizationReference}");

	[MessageLayout(Order = 36)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ETHeaderWarehouseIdentification WarehouseIdentification => new ETHeaderWarehouseIdentification(iETHeader.WarehouseIdentification);

	[MessageLayout(Order = 37)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ETHeaderPrincipalTrader PrincipalTrader => new ETHeaderPrincipalTrader(iETHeader.PrincipalTrader);

	[MessageLayout(Order = 38)]
	[MessageFieldExportWithTransitRules("D", "C30")]
	[MessageFieldTransitRules("D", "C30")]
	public ETHeaderTransitCustomsOfficeCollection TransitCustomsOffices => new ETHeaderTransitCustomsOfficeCollection(iETHeader.TransitCustomsOffices);

	[MessageLayout(Order = 39)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	public ETHeaderGuaranteeCollection Guarantees => new ETHeaderGuaranteeCollection(iETHeader.Guarantees);

	[MessageLayout(Order = 40)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R", "RN6")]
	public ZString DestinationCustomsOffice => iETHeader.DestinationCustomsOffice;

	[MessageLayout(Order = 41)]
	[MessageFieldIntegerRepresentation(4, false)]
	[MessageFieldExportRules("O", "R165")]
	[MessageFieldExportWithTransitRules("O", "R165")]
	[MessageFieldTransitRules("O", "R165")]
	public ZInt SealsNumber => iETHeader.SecurityBlock.SealsNumber;

	[MessageLayout(Order = 42)]
	[MessageFieldExportRules("D", "C569")]
	[MessageFieldExportWithTransitRules("R", "C569")]
	[MessageFieldTransitRules("R", "C569")]
	[MessageFieldInternationalRoadTransportsRules("R", "C569")]
	public ETHeaderSealCollection Seals => new ETHeaderSealCollection(iETHeader.Seals);

	[MessageLayout(Order = 43)]
	[MessageFieldDateDDMMYYYYRepresentation]
	[MessageFieldExportRules("D", "CN4")]
	[MessageFieldExportWithTransitRules("D", "CN4")]
	public ZDate DateLimitOfTemporaryOperation => iETHeader.DateLimitOfTemporaryOperation;

	[MessageLayout(Order = 44)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ETHeaderControlResult ControlResult => new ETHeaderControlResult(iETHeader.ControlResult);
}
