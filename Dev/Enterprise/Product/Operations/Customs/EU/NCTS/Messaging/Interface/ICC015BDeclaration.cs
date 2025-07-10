using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface ICC015BDeclaration : IDeclaration
	{
		ZString DeclarantTIN { get; }
		ZString PrincipalTIN { get; }
		ZString TypeOfDeclaration { get; }
		ZString CountryOfDestinationCode { get; }
		ZString AgreedLocationOfGoodsCode { get; }
		ZString AgreedLocationOfGoods { get; }
		ZString AgreedLocationOfGoodsLanguage { get; }
		ZString AuthorisedLocationOfGoodsCode { get; }
		ZString PlaceOfLoadingCode { get; }
		ZString PlaceOfLoading { get; }
		ZString CountryOfDispatchExportCode { get; }
		ZString CustomsSubPlace { get; }
		ZString InlandTransportMode { get; }
		ZString TransportModeAtBorder { get; }
		ZString IdentityOfMeansOfTransportAtDeparture { get; }
		ZString IdentityOfMeansOfTransportAtDepartureLanguage { get; }
		ZString NationalityOfMeansOfTransportAtDeparture { get; }
		ZString IdentityOfMeansOfTransportCrossingBorder { get; }
		ZString IdentityOfMeansOfTransportCrossingBorderLanguage { get; }
		ZString NationalityOfMeansOfTransportCrossingBorder { get; }
		ZString TypeOfMeansOfTransportCrossingBorder { get; }
		ZBool IsContainerised { get; }
		ZString DialogLanguageIndicatorAtDeparture { get; }
		ZString AccompanyingDocumentLanguage { get; }
		ZInt TotalNumberOfItems { get; }
		ZLong TotalNumberOfPackages { get; }
		ZDecimal TotalGrossMass { get; }
		ZDecimal TotalInvoiceValue { get; }
		ZString DeclarationDate { get; }
		ZString DeclarationPlace { get; }
		ZString DeclarationPlaceLanguage { get; }
		ZString SpecificCircumstanceIndicator { get; }
		ZString TransportChargesMethodOfPayment { get; }
		ZString CommercialReferenceNumber { get; }
		ZBool SecurityIndicator { get; }
		ZBool SafetyAndSecurityData { get; }
		ZString ProvisionalTransitDepartureDate { get; }
		ZString ModeOfRepresentation { get; }
		ZString ConveyanceReferenceNumber { get; }
		ZString TransportReferenceNumber { get; }
		ZString PlaceOfUnloadingCode { get; }
		ZString PlaceOfUnloading { get; }
		ZString PlaceOfUnloadingCodeLanguage { get; }
		ITrader Consignor { get; }
		ZString AuthorisedConsigneeTIN { get; }
		IReadOnlyCollection<ICustomsOffice> TransitCustomsOffices { get; }
		ZString ControlResultCode { get; }
		ZString ControlResultDateLimit { get; }
		ITrader Declarant { get; }
		ZInt NumberOfSeals { get; }
		IReadOnlyCollection<ISealID> Seals { get; }
		IReadOnlyCollection<IGuarantee> Guarantees { get; }
		IReadOnlyCollection<IDepartureGoodsItem> GoodsItems { get; }
		IReadOnlyCollection<ZString> Itinerary { get; }
		ITrader SecurityConsignor { get; }
		ITrader SecurityConsignee { get; }
		SecurityTraderCountryGroup SecurityConsignorCountryGroup { get; }
		SecurityTraderCountryGroup SecurityConsigneeCountryGroup { get; }
		ZString SecurityCarrierEORI { get; }
		ZBool IsTIRDeclaration { get; }
	}
}
