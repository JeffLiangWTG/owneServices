using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class JobFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestExportShipments()
		{
			JobFlatFileConverterTestClass converter = new JobFlatFileConverterTestClass(new NotificationBuffer(), Factory, "");
			Xsd.Consol consol = CreateConsolXSD();
			consol.Shipments.Add(CreateShipmentXSD());
			consol.Shipments.Add(CreateShipmentXSD());
			FlatFileDataRowCollection dataRows = converter.MapExport(consol);
			AssertEquals("Should be 2 rows in the collection", 2, dataRows.Count);
			AssertRow((JobFlatFileDataRow)dataRows[0]);
			AssertRow((JobFlatFileDataRow)dataRows[1]);
		}

		public void TestExportOneShipment()
		{
			JobFlatFileConverterTestClass converter = new JobFlatFileConverterTestClass(new NotificationBuffer(), Factory, "S000203498");
			Xsd.Consol consol = CreateConsolXSD();
			Xsd.Shipment shipment = CreateShipmentXSD();
			shipment.ShipmentDetails.AgentReference = "S000000121";
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(CreateShipmentXSD());
			FlatFileDataRowCollection dataRows = converter.MapExport(consol);
			AssertEquals("Should be 1 rows in the collection", 1, dataRows.Count);
			AssertRow((JobFlatFileDataRow)dataRows[0]);
		}

		public void TestExportDeclaration()
		{
			Xsd.ConsolAndShipment declaration = new Xsd.ConsolAndShipment();
			declaration.Consol = CreateConsolXSD();
			declaration.Shipment = CreateShipmentXSD();
			JobFlatFileConverterTestClass converter = new JobFlatFileConverterTestClass(new NotificationBuffer(), Factory, "");
			FlatFileDataRowCollection dataRows = converter.MapExport(declaration);
			AssertEquals("Should be 1 rows in the collection", 1, dataRows.Count);
			AssertRow((JobFlatFileDataRow)dataRows[0]);
		}

		public void TestInvalidValueObject()
		{
			JobFlatFileConverterTestClass converter = new JobFlatFileConverterTestClass(new NotificationBuffer(), Factory, "");
			FlatFileDataRowCollection dataRows = converter.MapExport(new Xsd.TxnHeader());
			AssertEquals("Should be 0 rows in the collection", 0, dataRows.Count);
		}

		public void TestInvalidToExportForShipments()
		{
			JobFlatFileConverterTestClass converter = new JobFlatFileConverterTestClass(new NotificationBuffer(), Factory, "");
			Xsd.Consol consol = CreateConsolXSD();
			consol.Shipments.Add(CreateShipmentXSD());
			consol.ConsolDetail.PortOfLoading.EstimatedDateTime = ZDateTime.Empty;
			AssertEquals("Should be 0 rows in the collection", 0, converter.MapExport(consol).Count);
			consol.ConsolDetail.PortOfLoading.EstimatedDateTime = ZDateTime.Now;
			consol.ConsolDetail.PortOfDischarge.EstimatedDateTime = ZDateTime.Empty;
			AssertEquals("Should be 0 rows in the collection", 0, converter.MapExport(consol).Count);
			consol.ConsolDetail.PortOfLoading.EstimatedDateTime = ZDateTime.Empty;
			AssertEquals("Should be 0 rows in the collection", 0, converter.MapExport(consol).Count);
		}

		public void TestInvalidToExportForDeclaration()
		{
			JobFlatFileConverterTestClass converter = new JobFlatFileConverterTestClass(new NotificationBuffer(), Factory, "");
			Xsd.ConsolAndShipment declaration = new Xsd.ConsolAndShipment();
			declaration.Consol = CreateConsolXSD();
			declaration.Shipment = CreateShipmentXSD();
			declaration.Consol.ConsolDetail.PortOfLoading.EstimatedDateTime = ZDateTime.Empty;
			AssertEquals("Should be 0 rows in the collection", 0, converter.MapExport(declaration).Count);
			declaration.Consol.ConsolDetail.PortOfLoading.EstimatedDateTime = ZDateTime.Now;
			declaration.Consol.ConsolDetail.PortOfDischarge.EstimatedDateTime = ZDateTime.Empty;
			AssertEquals("Should be 0 rows in the collection", 0, converter.MapExport(declaration).Count);
			declaration.Consol.ConsolDetail.PortOfLoading.EstimatedDateTime = ZDateTime.Empty;
			AssertEquals("Should be 0 rows in the collection", 0, converter.MapExport(declaration).Count);
			declaration.Consol.ConsolDetail.PortOfLoading.EstimatedDateTime = ZDateTime.Now;
			declaration.Shipment.ShipmentDetails.ShipmentType = Xsd.ShipmentType.EXW;
			declaration.Shipment.ShipmentDetails.ShipmentTypeSpecified = true;
			declaration.Shipment.ShipmentDetails.PortofDestination.Port.Value = "";
			AssertEquals("Should be 0 rows in the collection", 0, converter.MapExport(declaration).Count);
			declaration.Shipment.ShipmentDetails.PortofDestination.Port.Value = "AUSYD";
			declaration.Shipment.ShipmentDetails.Consignee = new Xsd.Organisation();
			AssertEquals("Should be 0 rows in the collection", 0, converter.MapExport(declaration).Count);
		}

		#region Implementation
		void AssertRow(JobFlatFileDataRow row)
		{
			AssertEquals("Job Number", "S000203498", row.JobNumber);
			AssertEquals("Description", "646A8DSF M00U98UIH JDFJ8", row.Description);
			AssertEquals("Description2", "CONSIGNEE", row.Description2);
			AssertEquals("Creation Date", new ZDateTime(2006, 1, 1), row.CreationDate);
			AssertEquals("Owner Reference", "OWNIHEEFDSFSLA", row.OwnerReference);
			AssertEquals("Consignor Name", "CONSIGNOR", row.ConsignorName);
			AssertEquals("MasterAWB", "2983D47289O3RF", row.MasterAWB);
			AssertEquals("HouseAWB", "209349W83UXOI47P9U", row.HouseAWB);
			AssertEquals("Vessel/Flight", "02394UXAWCI", row.VesselFlight);
			AssertEquals("Transport Mode", Core.Constants.TransportModeDescriptions.Sea.Replace(" Freight", ""), row.TransportMode);
			AssertEquals("Container Numbers", "M000239894 JIUWKDH000U", row.ContainerNumbers);
			AssertEquals("Loading Port", "GBLON", row.LoadingPort);
			AssertEquals("Discharge Port", "AUSYD", row.DischargePort);
			AssertEquals("Goods Description", "weapons of mass destruction", row.GoodsDescription);
			AssertEquals("Consolidated ETA", new ZDateTime(2006, 2, 22), row.ConsolidatedETA);
			AssertEquals("Consolidated ETD", new ZDateTime(2006, 2, 21), row.ConsolidatedETD);
			AssertEquals("Completion Percentage", Constants.CompletionPercentage, row.CompletionPercentage);
			AssertEquals("Status", Constants.Status, row.Status);
			AssertEquals("Application Method", Constants.ApplicationMethod, row.ApplicationMethod);
			AssertEquals("Job Usage Posting", Constants.JobUsagePosting, row.JobUsagePosting);
		}

		#region Create IValueObjects
		Xsd.Consol CreateConsolXSD()
		{
			Xsd.Consol consol = new Xsd.Consol();
			consol.ConsolDetail.PortOfLoading.EstimatedDateTime = new ZDateTime(2006, 2, 21, 18, 34, 3);
			consol.ConsolDetail.PortOfDischarge.EstimatedDateTime = new ZDateTime(2006, 2, 22, 12, 42, 56);
			consol.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "GBLON");
			consol.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			Xsd.SailingWithVesselVoyage vesselVoyage = new Xsd.SailingWithVesselVoyage();
			vesselVoyage.VoyageNo = "02394UXAWCI";
			consol.ConsolDetail.Item = vesselVoyage;
			Xsd.ConsolIdentifier identifier = consol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.Value = "2983D47289O3RF";
			consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			Xsd.Container container = consol.ConsolDetail.Containers.AddNew();
			container.ContainerNumber = "M000239894";
			container = consol.ConsolDetail.Containers.AddNew();
			container.ContainerNumber = "JIUWKDH000U";
			return consol;
		}

		Xsd.Shipment CreateShipmentXSD()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			shipment.ShipmentDetails.AgentReference = "S000203498";
			shipment.ShipmentDetails.Consignee.OrganisationDetails.Name = "CONSIGNEE";
			shipment.ShipmentDetails.Consignor.OrganisationDetails.Name = "CONSIGNOR";
			shipment.ShipmentDetails.GoodsDescription = "weapons of mass destruction";
			shipment.ShipmentDetails.OrderReferences = new string[] { "646A8DSF", "M00U98UIH", "JDFJ8" };
			shipment.ShipmentDetails.OwnerReference = "OWNIHEEFDSFSLA";
			shipment.ShipmentDetails.PortofDestination.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			shipment.ShipmentDetails.PortOfOrigin.Port = Xsd.UNLOCO.FromPortCode(Factory, "GBLON");
			shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipment.ShipmentDetails.Volume.Value = 234m;
			shipment.ShipmentDetails.Weight.Value = 982m;
			Xsd.Order order = shipment.Orders.AddNew();
			order.OrderIdentifier.OrderNumber = "SDHFKLAWS98FDY";
			order = shipment.Orders.AddNew();
			order.OrderIdentifier.OrderNumber = "PAOIERJASER2309";
			Xsd.ShipmentIdentifier identifier = shipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "209349W83UXOI47P9U";
			Xsd.Event @event = shipment.Events.Event.AddNew();
			@event.Code = Events.AddedARecordToTheSystem.Code;
			@event.CodeDescription = Events.AddedARecordToTheSystem.Description;
			@event.DateTime = new ZDateTime(2006, 1, 1, 1, 1, 1);
			@event = shipment.Events.Event.AddNew();
			@event.Code = Events.AddedARecordToTheSystem.Code;
			@event.CodeDescription = Events.AddedARecordToTheSystem.Description;
			@event.DateTime = new ZDateTime(2006, 1, 1, 1, 4, 56);
			@event = shipment.Events.Event.AddNew();
			@event.Code = Events.DataImport.Code;
			@event.CodeDescription = Events.DataImport.Description;
			@event.DateTime = new ZDateTime(2005, 12, 31, 23, 59, 59);
			return shipment;
		}

		#endregion
		class JobFlatFileConverterTestClass : JobFlatFileConverter
		{
			public JobFlatFileConverterTestClass(INotifications notification, BusinessObjectFactory factory, ZString shipmentNumber) : base(notification, factory, shipmentNumber)
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}
		}
		#endregion
	}
}
