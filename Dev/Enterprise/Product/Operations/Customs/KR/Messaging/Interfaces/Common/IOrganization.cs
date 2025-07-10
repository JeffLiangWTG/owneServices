using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IOrganization
	{
		[ID()]
		RoleType Role { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Exporter, ExportAmendmentDataItemIDList.Codes.A201)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Supplier, ExportAmendmentDataItemIDList.Codes.A301)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Manufacturer, ExportAmendmentDataItemIDList.Codes.A401)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Importer, ExportAmendmentDataItemIDList.Codes.A501)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Importer, ImportAmendmentDataItemIDList.Codes.A201)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A310)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Forwarder, ImportAmendmentDataItemIDList.Codes.A613)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.OnlineTradeDistributor, ImportAmendmentDataItemIDList.Codes.A620)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Supplier, ImportAmendmentDataItemIDList.Codes.A601)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Shipper, ImportAmendmentDataItemIDList.Codes.A616)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.OnlineTradeSeller, ImportAmendmentDataItemIDList.Codes.A623)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.OnlineTradeSellingAgent, ImportAmendmentDataItemIDList.Codes.A625)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03A)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04A)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Manufacturer, FTAAmendmentDataItemIDList.Codes._05A)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03A)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04A)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Manufacturer, FTAAmendmentDataItemIDList.Codes._05A)]
		ZString CompanyName { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Supplier, ExportAmendmentDataItemIDList.Codes.A308)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A311)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03B)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04B)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Manufacturer, FTAAmendmentDataItemIDList.Codes._05B)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03B)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04B)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Manufacturer, FTAAmendmentDataItemIDList.Codes._05B)]
		ZString RepresentativeName { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Supplier, ExportAmendmentDataItemIDList.Codes.A303)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A308)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03F)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04C)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Manufacturer, FTAAmendmentDataItemIDList.Codes._05C)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03F)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04C)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Manufacturer, FTAAmendmentDataItemIDList.Codes._05C)]
		ZString AddressLine1 { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Supplier, ExportAmendmentDataItemIDList.Codes.A304)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A309)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03G)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03G)]
		ZString AddressLine2 { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Supplier, ExportAmendmentDataItemIDList.Codes.A305)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Manufacturer, ExportAmendmentDataItemIDList.Codes.A406)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A305)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03C)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03C)]
		ZString Postcode { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Supplier, ExportAmendmentDataItemIDList.Codes.A306)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A306)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03D)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03D)]
		ZString RoadNameCode { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Supplier, ExportAmendmentDataItemIDList.Codes.A307)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A307)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03E)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03E)]
		ZString BuildingNumber { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Supplier, ImportAmendmentDataItemIDList.Codes.A602)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Shipper, ImportAmendmentDataItemIDList.Codes.A617)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04G)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04G)]
		ZString CountryCode { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Declarant, ImportAmendmentDataItemIDList.Codes.A101)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A312)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03H)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04D)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Manufacturer, FTAAmendmentDataItemIDList.Codes._05D)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03H)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04D)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Manufacturer, FTAAmendmentDataItemIDList.Codes._05D)]
		ZString PhoneNumber { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Declarant, ImportAmendmentDataItemIDList.Codes.A102)]
		ZString ExtensionNumber { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Declarant, ImportAmendmentDataItemIDList.Codes.A103)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A314)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03J)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03J)]
		ZString Email { get; }
		ZString MobileNumber { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04E)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Manufacturer, FTAAmendmentDataItemIDList.Codes._05E)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03I)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Supplier, FTAAmendmentDataItemIDList.Codes._04E)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Manufacturer, FTAAmendmentDataItemIDList.Codes._05E)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03I)]
		ZString FaxNumber { get; }
		ZBool IsIndividual { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Supplier, ExportAmendmentDataItemIDList.Codes.A310)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A304)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03K)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03K)]
		ZString BusinessRegNo { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A303)]
		ZString KoreanRegNoForResident { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Exporter, ExportAmendmentDataItemIDList.Codes.A202)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Supplier, ExportAmendmentDataItemIDList.Codes.A302)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Manufacturer, ExportAmendmentDataItemIDList.Codes.A402)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Importer, ImportAmendmentDataItemIDList.Codes.A202)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A301)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._5SC, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03L)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._DHR, RoleType.Importer, FTAAmendmentDataItemIDList.Codes._03L)]
		ZString UnipassIDForOrganization { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Supplier, ImportAmendmentDataItemIDList.Codes.A603)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Shipper, ImportAmendmentDataItemIDList.Codes.A618)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Importer, ExportAmendmentDataItemIDList.Codes.A502)]
		ZString ForeignCompanyID { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A315)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Exporter, ExportAmendmentDataItemIDList.Codes.A204)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Supplier, ExportAmendmentDataItemIDList.Codes.A311)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, RoleType.Manufacturer, ExportAmendmentDataItemIDList.Codes.A412)]
		ZString OfficeID { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A303)]
		ZString KoreanRegNoForForeigner { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A303)]
		ZString PassportNo { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Payer, ImportAmendmentDataItemIDList.Codes.A303)]
		ZString UnipassIDForIndividual { get; }
		ZString CertificateOfOriginExporterNumber { get; }
		ZString CorporationCode { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.Forwarder, ImportAmendmentDataItemIDList.Codes.A612)]
		ZString CarrierCode { get; }
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.OnlineTradeDistributor, ImportAmendmentDataItemIDList.Codes.A619)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.OnlineTradeSeller, ImportAmendmentDataItemIDList.Codes.A622)]
		[OrganisationDataItemID(ElectronicDocumentTypeList.Codes._929, RoleType.OnlineTradeSellingAgent, ImportAmendmentDataItemIDList.Codes.A624)]
		ZString ECommerceCompanyID { get; }
	}

	public enum RoleType
	{
		None,
		Buyer,
		Declarant,
		Exporter,
		Importer,
		Manufacturer,
		Payer,
		Seller,
		Supplier,
		Broker,
		Consignee,
		Forwarder,
		OnlineTradeDistributor,
		Shipper,
		OnlineTradeSeller,
		OnlineTradeSellingAgent
	}
}
