using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExportEntryHeader : IMessageDataProvider, IEntryHeaderWithEntryLines
	{
		[XmlIgnore]
		ZString ExportDeclarationNumber { get; }
		[DataItemID("A105")]
		ZString TransactionType { get; }
		[DataItemID("A106")]
		ZString ExportTypeCode { get; }
		[XmlIgnore]
		ZString DeclarationCustomsOffice { get; }
		[XmlIgnore]
		ZString DeclarationCustomsDivision { get; }
		[DataItemID("A601")]
		ZString CountryOfDestination { get; }
		[DataItemID("A602")]
		ZString PortOfLoading { get; }
		[DataItemID("A801")]
		ZString GoodsLocationPostcode { get; }
		[DataItemID("A805")]
		ZString GoodsLocationAddress { get; }
		[DataItemID("A802")]
		ZString GoodsLocationAdditionalDetails { get; }
		[DataItemID("AB08")]
		ZString SouthNorthTradeIdentification { get; }
		[DataItemID("A607")]
		ZString FinalLoadingPlace { get; }
		[DataItemID("A901")]
		ZString GoodsLocationBondedAreaCode { get; }
		[DataItemID("AB01")]
		ZDate PreferredInspectionDate { get; }
		[DataItemID("A904")]
		ZDate BondedTransportationFromDate { get; }
		[DataItemID("A905")]
		ZDate BondedTransportationToDate { get; }
		[DataItemID("A606")]
		ZDate DepartureDate { get; }
		[DataItemID("AC01")]
		ZString DrawbackApplicantType { get; }
		[DataItemID("A104")]
		ZString DeclarationProcedureType { get; }
		[DataItemID("A107")]
		ZString InvoicePaymentTerm { get; }
		[DataItemID("A203")]
		ZString ExporterType { get; }
		[DataItemID("AB02")]
		ZString OutOfHoursDeclarationIndicator { get; }
		[DataItemID("AB03")]
		ZString ReturnReason { get; }
		[DataItemID("AB09")]
		ZString ReturnType { get; }
		[DataItemID("A804")]
		ZString GoodsStatus { get; }
		[DataItemID("AC02")]
		ZString ApplicationForSimpleDrawback { get; }
		[DataItemID("AB04")]
		bool ContainerizedIndicator { get; }
		[DataItemID("AB07")]
		ZString SouthNorthTradeYN { get; }
		[DataItemID("A702")]
		ZString ContainerPackMode { get; }
		IEnumerable<IExportContainer> Containers { get; }
		[DataItemID("A803")]
		ZString LCNo { get; }
		[DataItemID("A701")]
		ZString TransportMode { get; }
		[XmlIgnore]
		ZString UCR { get; }
		[DataItemID("A902")]
		ZString LocationIDInBondedArea { get; }
		[XmlIgnore]
		IOrganization Declarant { get; }
		[XmlIgnore]
		string UnipassDeclarantID { get; }
		IOrganization Exporter { get; }
		IOrganization Supplier { get; }
		IOrganization Manufacturer { get; }
		IOrganization Importer { get; }
		[DataItemID("A903")]
		ZString FreightForwarderContactName { get; }
		[DataItemID("A604")]
		ZString CarrierID { get; }
		[DataItemID("A603")]
		ZString ShippingLineOrAirlineName { get; }
		[DataItemID("A605")]
		ZString VesselNameOrFlightNo { get; }
		[XmlIgnore]
		ZDecimal TotalCustomsValue { get; }
		[DataItemID("A703")]
		ZDecimal Freight { get; }
		[DataItemID("A704")]
		ZDecimal Insurance { get; }
		[DataItemID("A705")]
		ZString Incoterm { get; }
		[DataItemID("A707")]
		ZString Currency { get; }
		[DataItemID("A706")]
		ZDecimal TotalInvoiceAmount { get; }
		[XmlIgnore]
		ZDecimal ExchangeRate { get; }
		[DataItemID("AB05")]
		ZString DeclarantAdditionalDescription { get; }
		new IEnumerable<IExportEntryLine> EntryLines { get; }
		[DataItemID("AA04")]
		ZDecimal TotalPackQty { get; }
		[DataItemID("AA05")]
		ZString PackType { get; }
		[DataItemID("AA02")]
		ZDecimal TotalGrossWeightInKG { get; }
		ZString IndustrialParkCode { get; }
		IExportCargoMeanagement CargoManagement { get; }
	}
}
