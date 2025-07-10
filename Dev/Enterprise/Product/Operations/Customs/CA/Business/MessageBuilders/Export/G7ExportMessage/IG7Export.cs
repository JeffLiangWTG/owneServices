namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;

	public interface IG7Export : ICAEDIFACTMessageAttachee
	{
		// BGM
		ZString DocumentMessageNumber { get; }

		// LOC
		ZString PortOfExit { get; }
		ZString PlaceOfReport { get; }

		// DTM
		ZDateTime DateOfExport { get; }//TODO: time also required

		// MEA
		ZDecimal CommodityGrossWeight { get; }
		ZString CommodityGrossWeightUnitOfMeasure { get; }

		// EQD
		IEnumerable<IG7Container> Containers { get; }

		// TDT
		ZString ModeOfTransport { get; }
		ZString VesselName { get; }
		ZString CarrierCode { get; }
		ZString CarrierName { get; }
		//IDLMOrganisation DLMServiceProvider { get; } //TODO:  get carrier code and name from arrier or forwarder

		// G01
		// RFF
		ZString TransactionNumber { get; }
		ZString TransportationDocumentNumber { get; }
		ZString ExportLicenceNumber { get; }
		ZString CAEDAuthorizationID { get; }
		OrgHeader ExportLicenceProxy { get; }

		// PAC
		ZInt NumberOfPackages { get; }
		ZString TypeOfPackages { get; }

		//CST
		ZString ServiceOption { get; }

		// G03
		// NAD
		IDocAddress Exporter { get; }
		ZString ExporterBusinessNumber { get; }
		IDocAddress DeliveryParty { get; }
		ZString DeliveryPartyBusinessNumber { get; }
		ZString CountryOfFinalDestination { get; }
		ZString BrokerSecurityNumber { get; }

		// G04
		// MOA
		ZDecimal InvoiceTotal { get; }
		ZString InvoiceCurrencyCode { get; }
		ZDecimal FreightChargesInCAD { get; }

		// G05
		// DMS 
		IEnumerable<ZString> References { get; } //previously called DLMReferences

		// GEI
		ZString ReasonForExport { get; }

		// NAD
		IDocAddress Vendor { get; }
		IDocAddress Consignee { get; }

		//	ZString NameOfExportingCompany { get; } ?? is used??

		IEnumerable<IG7ItemLine> Details { get; }

		// AUT
		ZString Authentication { get; }
	}
}
