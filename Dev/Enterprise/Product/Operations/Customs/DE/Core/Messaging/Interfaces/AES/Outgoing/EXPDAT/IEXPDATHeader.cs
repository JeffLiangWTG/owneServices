using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IEXPDATHeader : IAESHeader
	{
		ZString DeclarationType { get; }
		string ExportDeclarationType { get; }
		ZString PartyConstellation { get; }
		ZDate DecisiveDate { get; }
		ZDate ExitDate { get; }
		DateTime PresentationStartDateAndTimeUtc { get; }
		DateTime LoadingEndDateAndTimeUtc { get; }
		string Security { get; }

		IReadOnlyCollection<IAuthorisation> Authorisations { get; }
		string CustomsOfficeOfPresentation { get; }
		IAESParty ContractualPartner { get; }
		IAESParty Exporter { get; }
		IAESParty SubContractor { get; }
		IOutwardProcessing OutwardProcessing { get; }
		IReadOnlyCollection<IPreviousDocument> PreviousDocuments { get; }
		IReadOnlyCollection<ISupportingDocument> SupportingDocuments { get; }
		IReadOnlyCollection<IReference> AdditionalReferences { get; }
		IReadOnlyCollection<IReference> AdditionalInformations { get; }
		IPartyID Carrier { get; }
		IAESParty Consignor { get; }
		IAESParty Consignee { get; }

		bool LocationOfGoodsSpecified { get; }
		string TypeOfLocation { get; }
		string QualifierOfIdentification { get; }
		string AuthorisationNumber { get; }
		string AdditionalIdentifier { get; }
		string UNLocode { get; }
		bool GNSSSpecified { get; }
		double GNSSLatitude { get; }
		double GNSSLongitude { get; }
		IPartyDocAddress LocationOfGoodsParty { get; }
		IAESPartyContactPerson LocationOfGoodsContactPerson { get; }
		IReadOnlyCollection<IReference> TransportDocuments { get; }
		ZString TransportChargesPaymentMethod { get; }

		ZString ExportCountry { get; }
		ZString DestinationCountry { get; }
		ZString DeclarationProcedure { get; }
		ZString DeclarationVariant { get; }
		ZString SpecificCircumstanceIndicator { get; }
		ZString RegistrationNumber { get; }
		ZString Annotation { get; }
		ZInt GoodsItemQuantity { get; }
		ZString LocalClearanceOutwardProcessingIDNumber { get; }
		ZString OutwardProcessingIDNumber { get; }
		ZString AccreditedExporterIDNumber { get; }
		IGoodsLoadingPlace GoodsLoadingPlace { get; }
		ZString IntendedExitCustomsOffice { get; }
		ZString ActualExitCustomsOffice { get; }
		IReadOnlyCollection<ZString> ItineraryCountries { get; }
		IDateTimeRange PresentationPackingLoading { get; }
		IAESParty Contractor { get; }
		IAESParty OutwardProcessingOwner { get; }
		IReadOnlyCollection<IEXPDATLine> Lines { get; }
		string InternalCurrency { get; }
		decimal ExchangeRate { get; }
		string DeferredPayment { get; }
		IReadOnlyCollection<ISupplyChainActor> AdditionalSupplyChainActors { get; }
		string WarehouseType { get; }
		string WarehouseIdentifier { get; }
	}
}
