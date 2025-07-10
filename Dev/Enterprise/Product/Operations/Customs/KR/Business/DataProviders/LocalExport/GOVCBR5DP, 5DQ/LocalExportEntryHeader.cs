using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("LocalExportEntryHeader")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class LocalExportEntryHeader : ILocalExportEntryHeader
	{
		public string DeclarationType { get; set; }
		public string DeclarationNumber { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public string BondedAreaCode { get; set; }
		public DateTime DeclarationDate { get; set; }
		public Organisation Supplier { get; set; }
		public Organisation Exporter { get; set; }
		public Organisation Manufacturer { get; set; }
		public Organisation Importer { get; set; }
		public string FlightNoOrVesselName { get; set; }
		public string MRNNo { get; set; }
		public int CrewCount { get; set; }
		public int ScheduledSailingDays { get; set; }
		public string GoodsType { get; set; }
		public string DrawbackApplicantType { get; set; }
		public string VesselRadioCallSign { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.TotalInvoiceAmount)]
		public decimal TotalDeclarationAmount { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		public decimal TotalGrossWeight { get; set; }
		public int TotalPackages { get; set; }
		public LocalExportStevedore[] Stevedores { get; set; }
		public LocalExportEntryLine[] EntryLines { get; set; }
		public LocalExportOtherTransportMeans[] OtherTransportMeans { get; set; }

		ZString ILocalExportEntryHeader.DeclarationType => DeclarationType;
		ZString ILocalExportEntryHeader.DeclarationNumber => DeclarationNumber;
		ZString ILocalExportEntryHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString ILocalExportEntryHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		ZString ILocalExportEntryHeader.BondedAreaCode => BondedAreaCode;
		ZDate ILocalExportEntryHeader.DeclarationDate => new ZDate(DeclarationDate);
		IOrganization ILocalExportEntryHeader.Supplier => Supplier;
		IOrganization ILocalExportEntryHeader.Exporter => Exporter;
		IOrganization ILocalExportEntryHeader.Manufacturer => Manufacturer;
		IOrganization ILocalExportEntryHeader.Importer => Importer;
		ZString ILocalExportEntryHeader.FlightNoOrVesselName => FlightNoOrVesselName;
		ZString ILocalExportEntryHeader.MRNNo => MRNNo;
		ZInt ILocalExportEntryHeader.CrewCount => CrewCount;
		ZInt ILocalExportEntryHeader.ScheduledSailingDays => ScheduledSailingDays;
		ZString ILocalExportEntryHeader.GoodsType => GoodsType;
		ZString ILocalExportEntryHeader.DrawbackApplicantType => DrawbackApplicantType;
		ZString ILocalExportEntryHeader.VesselRadioCallSign => VesselRadioCallSign;
		ZDecimal ILocalExportEntryHeader.TotalDeclarationAmount => TotalDeclarationAmount;
		ZDecimal ILocalExportEntryHeader.TotalGrossWeight => TotalGrossWeight;
		ZInt ILocalExportEntryHeader.TotalPackages => TotalPackages;
		IEnumerable<ILocalExportStevedore> ILocalExportEntryHeader.Stevedores => Stevedores;
		IEnumerable<ILocalExportEntryLine> ILocalExportEntryHeader.EntryLines => EntryLines;
		IEnumerable<IEntryLine> IEntryHeaderWithEntryLines.EntryLines => EntryLines;
		IEnumerable<ILocalExportOtherTransportMeans> ILocalExportEntryHeader.OtherTransportMeans => OtherTransportMeans;
	}
}
