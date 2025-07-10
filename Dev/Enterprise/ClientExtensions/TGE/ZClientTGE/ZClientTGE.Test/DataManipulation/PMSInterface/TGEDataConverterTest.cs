using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.TGE.PMS
{
	public class TGEDataConverterTest : TestCaseWithFactory
	{
		#region TestMapImport
		public void TestMapImport()
		{
			FlatFileDataRowCollection rows = SetupFlatDataRowCollection();
			Xsd.Consols consols = new Xsd.Consols();
			DataConverterTestClass dataConverter = new DataConverterTestClass(Factory);
			dataConverter.MapImport(consols, rows);
			Xsd.Consol consol = consols.Consol[0];
			AssertEquals("One Shipment Attached to the Consol", 1, consol.Shipments.Count);
			AssertEquals("Consol was created", 1, consols.Consol.Count);
		}

		#endregion
		#region TestProcessConsols
		public void TestProcessConsol()
		{
			FlatFileDataRowCollection rows = SetupFlatDataRowCollection();
			ConsolAndShipmentRecord row = rows[1] as ConsolAndShipmentRecord;
			Xsd.Consol consol = new Consol();
			DataConverterTestClass dataConverter = new DataConverterTestClass(Factory);
			Record = new ConsolAndShipmentRecord(row);
			dataConverter.ProcessConsolAndShipment(consol, Record);
			AssertEquals("Consol Type", Xsd.ConsolType.Agent, consol.ConsolDetail.ConsolType);
			AssertEquals("Consol Identifier Type", Xsd.ConsolIdentifierType.MasterWaybill, consol.ConsolIdentifier[0].ConsolIdentifierType);
			AssertEquals("Bill of Lading|MasterWayBill", "8112349301", consol.ConsolIdentifier[0].Value);
			AssertEquals("Consol Created Date", ConsolDate, consol.ConsolDetail.DateCreated.ToString("yyyyMMdd"));
			AssertEquals("Transport Mode", nameof(Xsd.TransportMode.AIR), consol.ConsolDetail.TransportMode.ToString());
			AssertEquals("Container Mode", nameof(Xsd.ContainerMode.LSE), consol.ConsolDetail.ContainerMode.ToString());
			AssertProcessOrganisation(consol.ConsolDetail.SendingAgent, "SendingAgent");
			AssertProcessOrganisation(consol.ConsolDetail.Creditor, "FRTCREDITORCODE");
			AssertProcessOrganisation(consol.ConsolDetail.Carrier, "AirlineCode");
			AssertProcessOrganisation(consol.ConsolDetail.ReceivingAgent, "ReceivingAgent");
		}

		#endregion
		#region TestProcessPortInformation
		public void TestProcessPortInformation()
		{
			FlatFileDataRowCollection rows = SetupFlatDataRowCollection();
			ConsolAndShipmentRecord row = rows[1] as ConsolAndShipmentRecord;
			Record = new ConsolAndShipmentRecord(row);
			Xsd.Consol consol = new Consol();
			DataConverterTestClass dataConverter = new DataConverterTestClass(Factory);
			dataConverter.ProcessPortsInformation(consol, Record);
			AssertEquals("Port of Loading", "AUSYD", consol.ConsolDetail.PortOfLoading.Port.Value.ToString());
			AssertEquals("Port of Discharge", "NZAKL", consol.ConsolDetail.PortOfDischarge.Port.Value.ToString());
			AssertEquals("ETD", new ZDateTime(2005, 3, 25, 12, 12, 0), consol.ConsolDetail.PortOfLoading.EstimatedDateTime);
			AssertEquals("ETA", new ZDateTime(2005, 3, 25, 0, 0, 0), consol.ConsolDetail.PortOfDischarge.EstimatedDateTime);
		}

		#endregion
		#region TestProcessFlightInformation
		public void TestProcessFlightInformation()
		{
			FlatFileDataRowCollection rows = SetupFlatDataRowCollection();
			ConsolAndShipmentRecord row = rows[1] as ConsolAndShipmentRecord;
			Record = new ConsolAndShipmentRecord(row);
			Xsd.Consol consol = new Consol();
			DataConverterTestClass dataConverter = new DataConverterTestClass(Factory);
			dataConverter.ProcessFlightInformation(consol, Record);
			Xsd.FlightWithFlightNumber flight = new Xsd.FlightWithFlightNumber();
			flight = (Xsd.FlightWithFlightNumber)consol.ConsolDetail.Item;
			AssertEquals("Flight No", "QF123", flight.FlightNoJourneyNoTruckRegNo);
			AssertEquals("ETA", new ZDateTime(2005, 3, 25, 0, 0, 0), flight.ETA);
			AssertEquals("ETD", new ZDateTime(2005, 3, 25, 12, 12, 0), flight.ETD);
		}

		#endregion
		#region TestProcessShipperInformation
		public void TestProcessShipperInformation()
		{
			FlatFileDataRowCollection rows = SetupFlatDataRowCollection();
			ConsolAndShipmentRecord row = rows[1] as ConsolAndShipmentRecord;
			Record = new ConsolAndShipmentRecord(row);
			Xsd.Consol consol = new Consol();
			Xsd.Shipment shipment = consol.Shipments.AddNew();
			DataConverterTestClass dataConverter = new DataConverterTestClass(Factory);
			dataConverter.ProcessShipperInformation(shipment, Record);
			Xsd.Organisation shipper = consol.Shipments[0].ShipmentDetails.Consignor;
			Xsd.OrgAddress addr = shipper.OrganisationDetails.Addresses.GetMainAddress();
			AssertEquals("Shipper Name", "WIG GALLERY", shipper.OrganisationDetails.Name);
			AssertEquals("Shipper Code", "63761", shipper.EDICode);
			AssertEquals("Shipper's address line1", "73-77 SACKVILLE ST", addr.AddressLine1);
			AssertEquals("Shipper's address line2", "COLLINGWOOD", addr.AddressLine2);
			AssertEquals("Shipper's address city", "CITY", addr.CityOrSuburb);
			AssertEquals("Post Code", "3066", addr.PostCode);
			AssertEquals("Phone No", "03 9419 8266", addr.TelephoneNumbers[0].Value);
			AssertEquals("ABN", "ABN1", shipper.OrganisationDetails.RegistrationNumbers[0].Number.ToString());
			AssertEquals("City", "Auckland", shipper.OrganisationDetails.Location.City);
		}

		#endregion
		#region TestProcessConsigneeInformation
		public void TestProcessConsigneeInformation()
		{
			FlatFileDataRowCollection rows = SetupFlatDataRowCollection();
			ConsolAndShipmentRecord row = rows[1] as ConsolAndShipmentRecord;
			Xsd.Consol consol = new Consol();
			Xsd.Shipment shipment = consol.Shipments.AddNew();
			DataConverterTestClass dataConverter = new DataConverterTestClass(Factory);
			ConsolAndShipmentRecord record = new ConsolAndShipmentRecord(row);
			dataConverter.ProcessConsigneeInformation(shipment, record);
			Xsd.Organisation consignee = consol.Shipments[0].ShipmentDetails.Consignee;
			Xsd.OrgAddress addr = consignee.OrganisationDetails.Addresses.GetMainAddress();
			AssertEquals("Consignee Name", "CONSIGN-1", consignee.OrganisationDetails.Name);
			AssertEquals("Consignee Code", "CONSIGNEE", consignee.EDICode);
			AssertEquals("Consignee's address line1", "CONSIGN ADDR1-1", addr.AddressLine1);
			AssertEquals("Consignee's address line2", "CONSIGN ADDR2-1", addr.AddressLine2);
			AssertEquals("Consignee's address City", "CONSIGN ADDR3-1", addr.CityOrSuburb);
			AssertEquals("Post Code", "PCODE-1", addr.PostCode);
			AssertEquals("Phone No", "PHONE-1", addr.TelephoneNumbers[0].Value);
			AssertEquals("City", "Auckland", consignee.OrganisationDetails.Location.City);
		}

		#endregion
		#region TestProcessGoodsAndPackagesInformation
		public void TestProcessGoodsAndPackagesInformation()
		{
			FlatFileDataRowCollection rows = SetupFlatDataRowCollection();
			ConsolAndShipmentRecord row = rows[1] as ConsolAndShipmentRecord;
			Record = new ConsolAndShipmentRecord(row);
			Xsd.Consol consol = new Consol();
			Xsd.Shipment shipment = consol.Shipments.AddNew();
			DataConverterTestClass dataConverter = new DataConverterTestClass(Factory);
			dataConverter.ProcessGoodsAndPackagesInformation(shipment, Record);
			AssertEquals("Pack Type", "PKG", shipment.ShipmentDetails.Packages[0].PackType);
			AssertEquals("Number of Packs", 1, shipment.ShipmentDetails.Packages[0].NumberOfPacks);
			AssertEquals("Number of OuterPacks", 1m, shipment.ShipmentDetails.TotalOuterPacksQty.Value);
			AssertEquals("Goods Desc", "Goods1", shipment.ShipmentDetails.GoodsDescription);
			AssertEquals("Goods Value", 1000m, shipment.ShipmentDetails.GoodsValue.Value);
			AssertEquals("Goods Value Currency", "AUD", shipment.ShipmentDetails.GoodsValue.CurrencyCode);
			AssertEquals("Weight", 1.1m, shipment.ShipmentDetails.Weight.Value);
			AssertEquals("Weight UQ", Core.Constants.Weight.Kilograms, shipment.ShipmentDetails.Weight.DimensionType);
			AssertEquals("Volume UQ", Core.Constants.Volume.CubicMetres, shipment.ShipmentDetails.Volume.DimensionType);
		}

		#endregion
		#region TestProcessShipment
		public void TestProcessShipment()
		{
			FlatFileDataRowCollection rows = SetupFlatDataRowCollection();
			ConsolAndShipmentRecord row = rows[1] as ConsolAndShipmentRecord;
			Record = new ConsolAndShipmentRecord(row);
			Xsd.Consol consol = new Consol();
			Xsd.Shipment shipment = consol.Shipments.AddNew();
			DataConverterTestClass dataConverter = new DataConverterTestClass(Factory);
			dataConverter.ProcessPortsInformation(consol, Record);
			dataConverter.ProcessShipment(consol, shipment, Record);
			AssertEquals("Container Mode", "LSE", shipment.ShipmentDetails.PackingMode.ToString());
			AssertEquals("Transport Mode", "AIR", shipment.ShipmentDetails.TransportMode.ToString());
			AssertEquals("Shipment Identifier Type", Xsd.ShipmentIdentifierType.Housebill, shipment.ShipmentIdentifier[0].ShipmentIdentifierType);
			AssertEquals("Housebill", "7712349301", shipment.ShipmentIdentifier[0].Value);
			AssertEquals("Port of Origin", "NZAKL", shipment.ShipmentDetails.PortOfOrigin.Port.Value.ToString());
			AssertEquals("Port of Destination", "JPTYO", shipment.ShipmentDetails.PortofDestination.Port.Value.ToString());
			AssertEquals("Order References", "OR1", shipment.ShipmentDetails.OrderReferences[0]);
			AssertEquals("Entry Number", "XLV", shipment.ShipmentDetails.CustomsEntryNumbers[0].Type.ToString());
			AssertEquals("Service Level", "STD", shipment.ShipmentDetails.ServiceLevel);
			AssertEquals("INCOTerm", Core.Constants.IncoTerms.FreeOnBoard, shipment.ShipmentDetails.Incoterm);
		}

		#endregion
		#region Implementation
		#region AssertProcessOrganisation
		void AssertProcessOrganisation(Xsd.Organisation organisation, ZString code)
		{
			AssertEquals("Organisation Name", ZString.Empty, organisation.OrganisationDetails.Name);
			AssertEquals("Addressline1", DataConverter.NotSpecified, organisation.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("Owner Code", code, organisation.OwnerCode);
		}

		#endregion
		ZString ConsolDate;
		ConsolAndShipmentRecord Record;
		protected FlatFileDataRowCollection SetupFlatDataRowCollection()
		{
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
			ConsolDate = ZDateTime.Now.ToString("yyyyMMdd");
			string[] lineHeader = { "Branch", "ConsolDate", "ConsolType", "ConsolMode", "ConsolOriginCountry", "ConsolOriginPort", "ConsolDestCountry", "ConsolDestPort", "OriginETD", "OriginETA", "OriginATD", "ConsolAgent", "BkRef", "Flight1Carrier", "Flight1Number", "Flight1Dest", "MAWB", "DischargeCountry", "DischargePort", "AirlineCode", "FrtCreditorCode", "RecvDepot", "CurrencyCode", "ShipperCode", "ShipperName", "ShipperAddress1", "ShipperAddress2", "ShipperAddress3", "ShipperPostcode", "ShipperPhone", "ShipperABN", "ConsigneeCode", "ConsigneeName", "ConsigneeAddress1", "ConsigneeAddress2", "ConsigneeAddress3", "ConsigneePostcode", "ConsigneePhone", "ConsigneeCity", "BillCode", "DelivAgent", "SalesPerson", "OpsPerson", "Quote", "AsAgreed", "OrderNo", "HAWB", "HAWBOriginCountry", "HAWBOriginPort", "HAWBDestCountry", "HAWBDestPort", "HAWBETA", "Qty", "PackCode", "ServiceLevel", "INCOTerm", "CommCodes", "CustomsValue", "CustomsCurr", "DescOfGoods", "ActualWgt", "ActualVol", "ChgWgt", "HAWBKgRate", "AirPrepaid", "FrtCurr", "FrtPrepaidAmt", "CAN", "WHC", "Type", "CollectDate", "TransportCarrierCode", "CheckInLoc", "CheckInDate" };
			ConsolAndShipmentRecord header = new ConsolAndShipmentRecord(lineHeader);
			string[] lineRow1 = { "13", ConsolDate, "Agent2Agent", "LOOSE", "AU", "SYD", "NZ", "AKL", "20050325121200", "20050325000000", "20050325000000", "SendingAgent", "bkref1", "QF", "123", "AKL", "8112349301", "NZ", "AKL", "AirlineCode", "FrtCreditorCode", "ReceivingAgent", "AUD", "63761", "WIG GALLERY", "73-77 SACKVILLE ST", "COLLINGWOOD", "CITY", "3066", "03 9419 8266", "ABN1", "Consignee", "CONSIGN-1", "CONSIGN ADDR1-1", "CONSIGN ADDR2-1", "CONSIGN ADDR3-1", "PCODE-1", "PHONE-1", "NZAKL", "63761", "12micon", "STD", "DM", "QTE1", "Y", "OR1", "7712349301", "NZ", "AKL", "JP", "TYO", "20050325", "1", "PK", "STD", "FOB", "FRA", "1000", "AUD", "Goods1", "1.1", "0.001", "2", "0", "Prepaid", "AUD", "10", "EXLV", "FD69N", "OT", "20050323", "TCC", "CIL", "20050323" };
			ConsolAndShipmentRecord row1 = new ConsolAndShipmentRecord(lineRow1);
			dataRows.Add(header);
			dataRows.Add(row1);
			return dataRows;
		}

		public class DataConverterTestClass : DataConverter
		{
			public DataConverterTestClass(BusinessObjectFactory factory) : base(new NotificationBuffer(), factory)
			{
			}

			public new void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
			{
				base.MapImport(valueObject, fileLines);
			}

			public new void ProcessConsolAndShipment(Xsd.Consol consol, ConsolAndShipmentRecord row)
			{
				base.ProcessConsolAndShipment(consol, row);
			}

			public new void ProcessPortsInformation(Xsd.Consol consol, ConsolAndShipmentRecord row)
			{
				base.ProcessPortsInformation(consol, row);
			}

			public new void ProcessFlightInformation(Xsd.Consol consol, ConsolAndShipmentRecord row)
			{
				base.ProcessFlightInformation(consol, row);
			}

			public new void ProcessConsigneeInformation(Xsd.Shipment shipment, ConsolAndShipmentRecord row)
			{
				base.ProcessConsigneeInformation(shipment, row);
			}

			public new void ProcessShipperInformation(Xsd.Shipment shipment, ConsolAndShipmentRecord row)
			{
				base.ProcessShipperInformation(shipment, row);
			}

			public new void ProcessGoodsAndPackagesInformation(Xsd.Shipment shipment, ConsolAndShipmentRecord row)
			{
				base.ProcessGoodsAndPackagesInformation(shipment, row);
			}

			public new void ProcessShipment(Xsd.Consol consol, Xsd.Shipment shipment, ConsolAndShipmentRecord row)
			{
				base.ProcessShipment(consol, shipment, row);
			}
		}
	}
}
#endregion
