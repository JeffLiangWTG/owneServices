using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("ExportEntryHeader")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ExportEntryHeader : IExportEntryHeader
	{
		public string ExportDeclarationNumber { get; set; }
		public string TransactionType { get; set; }
		public string ExportTypeCode { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public string CountryOfDestination { get; set; }
		public string PortOfLoading { get; set; }
		public string GoodsLocationPostcode { get; set; }
		public string GoodsLocationAddress { get; set; }
		public string GoodsLocationAdditionalDetails { get; set; }
		public string SouthNorthTradeIdentification { get; set; }
		public string FinalLoadingPlace { get; set; }
		public string GoodsLocationBondedAreaCode { get; set; }
		public DateTime PreferredInspectionDate { get; set; }
		public DateTime BondedTransportationFromDate { get; set; }
		public DateTime BondedTransportationToDate { get; set; }
		public DateTime DepartureDate { get; set; }
		public string DrawbackApplicantType { get; set; }
		public string DeclarationProcedureType { get; set; }
		public string InvoicePaymentTerm { get; set; }
		public string ExporterType { get; set; }
		public string OutOfHoursDeclarationIndicator { get; set; }
		public string ReturnReason { get; set; }
		public string ReturnType { get; set; }
		public string GoodsStatus { get; set; }
		public string ApplicationForSimpleDrawback { get; set; }
		public bool ContainerizedIndicator { get; set; }
		public string SouthNorthTradeYN { get; set; }
		public string ContainerPackMode { get; set; }
		public ExportContainer[] Containers { get; set; }
		public string LCNo { get; set; }
		public ExportCargoMeanagement CargoManagement { get; set; }
		public string TransportMode { get; set; }
		public string UCR { get; set; }
		public string LocationIDInBondedArea { get; set; }
		public Organisation Declarant { get; set; }
		public string UnipassDeclarantID { get; set; }
		public Organisation Exporter { get; set; }
		public Organisation Supplier { get; set; }
		public Organisation Manufacturer { get; set; }
		public Organisation Importer { get; set; }
		public string FreightForwarderContactName { get; set; }
		public string CarrierID { get; set; }
		public string ShippingLineOrAirlineName { get; set; }
		public string VesselNameOrFlightNo { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public decimal TotalCustomsValue { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.CommercialChargeAmount)]
		public decimal Freight { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.CommercialChargeAmount)]
		public decimal Insurance { get; set; }
		public string Incoterm { get; set; }
		public string Currency { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.TotalInvoiceAmount)]
		public decimal TotalInvoiceAmount { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.ExchangeRate)]
		public decimal ExchangeRate { get; set; }
		public string DeclarantAdditionalDescription { get; set; }
		public ExportEntryLine[] EntryLines { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Qty)]
		public decimal TotalPackQty { get; set; }
		public string PackType { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		public decimal TotalGrossWeightInKG { get; set; }
		public string IndustrialParkCode { get; set; }

		ZString IExportEntryHeader.ExportDeclarationNumber => ExportDeclarationNumber;
		ZString IExportEntryHeader.TransactionType => TransactionType;
		ZString IExportEntryHeader.ExportTypeCode => ExportTypeCode;
		ZString IExportEntryHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IExportEntryHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		ZString IExportEntryHeader.CountryOfDestination => CountryOfDestination;
		ZString IExportEntryHeader.PortOfLoading => PortOfLoading;
		ZString IExportEntryHeader.GoodsLocationPostcode => GoodsLocationPostcode;
		ZString IExportEntryHeader.GoodsLocationAddress => GoodsLocationAddress;
		ZString IExportEntryHeader.GoodsLocationAdditionalDetails => GoodsLocationAdditionalDetails;
		ZString IExportEntryHeader.SouthNorthTradeIdentification => SouthNorthTradeIdentification;
		ZString IExportEntryHeader.FinalLoadingPlace => FinalLoadingPlace;
		ZString IExportEntryHeader.GoodsLocationBondedAreaCode => GoodsLocationBondedAreaCode;
		ZDate IExportEntryHeader.PreferredInspectionDate => (ZDate)PreferredInspectionDate;
		ZDate IExportEntryHeader.BondedTransportationFromDate => (ZDate)BondedTransportationFromDate;
		ZDate IExportEntryHeader.BondedTransportationToDate => (ZDate)BondedTransportationToDate;
		ZDate IExportEntryHeader.DepartureDate => (ZDate)DepartureDate;
		ZString IExportEntryHeader.DrawbackApplicantType => DrawbackApplicantType;
		ZString IExportEntryHeader.DeclarationProcedureType => DeclarationProcedureType;
		ZString IExportEntryHeader.InvoicePaymentTerm => InvoicePaymentTerm;
		ZString IExportEntryHeader.ExporterType => ExporterType;
		ZString IExportEntryHeader.OutOfHoursDeclarationIndicator => OutOfHoursDeclarationIndicator;
		ZString IExportEntryHeader.ReturnReason => ReturnReason;
		ZString IExportEntryHeader.ReturnType => ReturnType;
		ZString IExportEntryHeader.GoodsStatus => GoodsStatus;
		ZString IExportEntryHeader.ApplicationForSimpleDrawback => ApplicationForSimpleDrawback;
		bool IExportEntryHeader.ContainerizedIndicator => ContainerizedIndicator;
		ZString IExportEntryHeader.SouthNorthTradeYN => SouthNorthTradeYN;
		ZString IExportEntryHeader.ContainerPackMode => ContainerPackMode;
		IEnumerable<IExportContainer> IExportEntryHeader.Containers => Containers;
		ZString IExportEntryHeader.LCNo => LCNo;
		IExportCargoMeanagement IExportEntryHeader.CargoManagement => CargoManagement;
		ZString IExportEntryHeader.TransportMode => TransportMode;
		ZString IExportEntryHeader.UCR => UCR;
		ZString IExportEntryHeader.LocationIDInBondedArea => LocationIDInBondedArea;
		IOrganization IExportEntryHeader.Declarant => Declarant;
		IOrganization IExportEntryHeader.Exporter => Exporter;
		IOrganization IExportEntryHeader.Supplier => Supplier;
		IOrganization IExportEntryHeader.Manufacturer => Manufacturer;
		IOrganization IExportEntryHeader.Importer => Importer;
		ZString IExportEntryHeader.FreightForwarderContactName => FreightForwarderContactName;
		ZString IExportEntryHeader.CarrierID => CarrierID;
		ZString IExportEntryHeader.ShippingLineOrAirlineName => ShippingLineOrAirlineName;
		ZString IExportEntryHeader.VesselNameOrFlightNo => VesselNameOrFlightNo;
		ZDecimal IExportEntryHeader.TotalCustomsValue => TotalCustomsValue;
		ZDecimal IExportEntryHeader.Freight => Freight;
		ZDecimal IExportEntryHeader.Insurance => Insurance;
		ZString IExportEntryHeader.Incoterm => Incoterm;
		ZString IExportEntryHeader.Currency => Currency;
		ZDecimal IExportEntryHeader.TotalInvoiceAmount => TotalInvoiceAmount;
		ZDecimal IExportEntryHeader.ExchangeRate => ExchangeRate;
		ZString IExportEntryHeader.DeclarantAdditionalDescription => DeclarantAdditionalDescription;
		IEnumerable<IExportEntryLine> IExportEntryHeader.EntryLines => EntryLines;
		IEnumerable<IEntryLine> IEntryHeaderWithEntryLines.EntryLines => EntryLines;
		ZDecimal IExportEntryHeader.TotalPackQty => TotalPackQty;
		ZString IExportEntryHeader.PackType => PackType;
		ZDecimal IExportEntryHeader.TotalGrossWeightInKG => TotalGrossWeightInKG;
		ZString IExportEntryHeader.IndustrialParkCode => IndustrialParkCode;
	}
}
