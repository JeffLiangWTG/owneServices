using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IENSGenericMessageDataProvider : ISummaryDeclarationsCommonMessageDataProvider
	{
		ZString Priority { get; }
		ZString SenderName { get; }
	}

	public interface IENSCommonMessageDataProvider : IENSGenericMessageDataProvider
	{
		IENSAddressInformation Consignor { get; }
		IENSAddressInformation Consignee { get; }
		IENSAddressInformation NotifyParty { get; }
		IReadOnlyCollection<IENSCommonLine> Lines { get; }
		IReadOnlyCollection<ZString> ItineraryCountries { get; }
		IENSAddressInformation RepresentativeTrader { get; }
		IENSAddressInformation LodgingPerson { get; }
		IReadOnlyCollection<IENSSeal> Seals { get; }
		ZString FirstEntryCustomsOffice { get; }
		ZDateTime FirstEntryExpectedArrivalDate { get; }
		IReadOnlyCollection<ZString> SubsequentEntriesCustomsOffices { get; }
		IENSAddressInformation EntryCarrierTrader { get; }
	}

	public interface IENSCommonHeader
	{
		ZString BorderTransportMode { get; }
		IENSBorderTransport BorderTransportInfo { get; }
		ZInt TotalLinesNum { get; }
		ZInt TotalPackagesQty { get; }
		ZDecimal TotalGrossWeight { get; }
		ZString SpecificCircumstanceInd { get; }
		ZString MethodOfPayment { get; }
		ZString CommercialReferenceNumber { get; }
		ZString ConveyanceReferenceNumber { get; }
		ZString PlaceOfLoading { get; }
		ZString PlaceOfLoadingLanguage { get; }
		ZString PlaceOfUnloading { get; }
		ZString PlaceOfUnloadingLanguage { get; }
	}

	public interface IENSBorderTransport
	{
		ZString Nationality { get; }
		ZString Id { get; }
		ZString Language { get; }
	}

	public interface IENSAddressInformation : IPartyProvider
	{
		ZString Language { get; }
	}

	public interface IENSCommonLine
	{
		ZInt LineNumber { get; }
		ZString GoodsDescription { get; }
		ZString GoodsDescriptionLanguage { get; }
		ZDecimal GrossWeight { get; }
		ZString MethodOfPayment { get; }
		ZString CommercialReferenceNumber { get; }
		ZString UNDangerousCode { get; }
		ZString PlaceOfLoading { get; }
		ZString PlaceOfLoadingLanguage { get; }
		ZString PlaceOfUnloading { get; }
		ZString PlaceOfUnloadingLanguage { get; }
		IReadOnlyCollection<IENSDocument> Certificates { get; }
		IReadOnlyCollection<ZString> SpecialMentions { get; }
		IENSAddressInformation Consignor { get; }
		ZString CommodityCode { get; }
		IENSAddressInformation Consignee { get; }
		IReadOnlyCollection<ZString> Containers { get; }
		IReadOnlyCollection<IENSBorderTransport> BorderTransportMeans { get; }
		IReadOnlyCollection<IENSPackage> Packages { get; }
		IENSAddressInformation NotifyParty { get; }
	}

	public interface IENSDocument : IDocumentsCommon
	{
		ZString Language { get; }
	}

	public interface IENSPackage : IPackageCommonNumbers
	{
		ZString MarksLanguage { get; }
	}

	public interface IENSSeal
	{
		ZString Id { get; }
		ZString Language { get; }
	}
}
