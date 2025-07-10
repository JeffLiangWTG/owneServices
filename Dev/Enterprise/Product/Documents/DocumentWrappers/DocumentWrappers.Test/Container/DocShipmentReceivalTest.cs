using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container
{
	[TestedType(typeof(DocShipmentReceival))]
	sealed class DocShipmentReceivalTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocShipmentReceival.New(Shipment, Factory), DocShipmentReceival.New(Factory, Shipment.PK)
			};
		}

		public void TestClientName()
		{
			AssertEquals(ZString.Empty, ShipmentWrapper.ClientName);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.JS_OH_HandledOnBehalfOfForwarder = org.PK;
			AssertEquals(org.OH_FullName, ShipmentWrapper.ClientName);
		}

		public void TestDestinationCode()
		{
			Shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals("USLAX", ShipmentWrapper.DestinationCode);
		}

		public void TestConNote()
		{
			AssertEquals(ZString.Empty, ShipmentWrapper.ConNote);
			Shipment.JS_CartageWaybill = "Con note test";
			AssertEquals("Con note test", ShipmentWrapper.ConNote);
		}

		public void TestExportVessel()
		{
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "SGSIN";
			AssertEquals("Vessel should be Empty", ZString.Empty, ShipmentWrapper.ExportVessel);

			LinkSailing(Shipment, "VESSEL 1", "VOYAGE 1");
			AssertEquals("Vessel from Shipment.Sailing", "VESSEL 1", ShipmentWrapper.ExportVessel);

			var transport = AddTransport(Shipment, "VESSEL 2", "VOYAGE 2");
			AssertEquals("Vessel from Shipment.Transport", "VESSEL 2", ShipmentWrapper.ExportVessel);

			LinkSailing(transport, "VESSEL 3", "VOYAGE 3");
			AssertEquals("Vessel from Shipment.Transport.Sailing", "VESSEL 3", ShipmentWrapper.ExportVessel);

			var consol = Shipment.Consols.AddNew();
			transport = AddTransport(consol, "VESSEL 4", "VOYAGE 4", "USCHI", "USLAX");
			AssertEquals("Vessel from Consol.Transport", "VESSEL 4", ShipmentWrapper.ExportVessel);

			LinkSailing(transport, "VESSEL 5", "VOYAGE 5");
			AssertEquals("Vessel from Consol.Transport.Sailing", "VESSEL 5", ShipmentWrapper.ExportVessel);

			Shipment.JS_JX = ZGuid.Empty;
			AssertEquals("Vessel from Consol.Transport.Sailing", "VESSEL 5", ShipmentWrapper.ExportVessel);
		}

		public void TestExportVoyage()
		{
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "AUMEL";
			AssertEquals("Voyage should be Empty", ZString.Empty, ShipmentWrapper.ExportVessel);

			LinkSailing(Shipment, "VESSEL 1", "VOYAGE 1");
			AssertEquals("Voyage from Shipment.Sailing", "VOYAGE 1", ShipmentWrapper.ExportVoyage);

			var transport = AddTransport(Shipment, "VESSEL 2", "VOYAGE 2");
			AssertEquals("Voyage from Shipment.Transport", "VOYAGE 2", ShipmentWrapper.ExportVoyage);

			LinkSailing(transport, "VESSEL 3", "VOYAGE 3");
			AssertEquals("Voyage from Shipment.Transport.Sailing", "VOYAGE 3", ShipmentWrapper.ExportVoyage);

			var consol = Shipment.Consols.AddNew();
			transport = AddTransport(consol, "VESSEL 4", "VOYAGE 4", "USCHI", "USLAX");
			AssertEquals("Voyage from Consol.Transport", "VOYAGE 4", ShipmentWrapper.ExportVoyage);

			LinkSailing(transport, "VESSEL 5", "VOYAGE 5");
			AssertEquals("Voyage from Consol.Transport.Sailing", "VOYAGE 5", ShipmentWrapper.ExportVoyage);

			Shipment.JS_JX = ZGuid.Empty;
			AssertEquals("Voyage from Consol.Transport.Sailing", "VOYAGE 5", ShipmentWrapper.ExportVoyage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestImportShipments()
		{
			var client = Factory.LoadTop1<OrgHeader>(new ZQuery());
			SetShipmentForLabelTest("USLAX", "AUMEL", "HBL000", "Marks 000", "PLT", 13, client, "USCHI", "AUMEL");

			Shipment.OuterPackLines.RemoveAndDeleteAll();
			var line1 = Shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 5;
			line1.JL_F3_NKPackType = "PLT";
			var line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 8;
			line2.JL_F3_NKPackType = "PLT";

			CFSLoadListConsol consol = Shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.Transports.MostInterestingTransport.JW_JX = Shipment.JS_JX;

			AssertEquals("Precondition: country of consol's final discharge", "AU", ShipmentWrapper.Consol.LastDischargePort.CountryCode);
			AssertEquals("Precondition: country of shipment's discharge", "AU", ShipmentWrapper.DestinationLoco.CountryCode);

			ShipmentWrapper.SetReportNameForTesting("IMPORT");

			AssertEquals(13, ShipmentWrapper.CFSShipments.Count);
			foreach (DocCFSShipment cFSWrapper in ShipmentWrapper.CFSShipments)
			{
				AssertLabelProperties(cFSWrapper, Shipment.JS_UniqueConsignRef, "Vessel", "Voyage No", client.OH_FullName, Today.ToString("dd-MMM-yy"),
					"AUMEL", "HBL000", "Marks 000", "", "", "", "");
				AssertEquals("PackageType", "PLT", cFSWrapper.CFSPackageType);
			}

			Shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("CFSShipments populated: same discharge country of consol and shipment", 13, ShipmentWrapper.CFSShipments.Count);

			consol.JK_RL_NKDischargePort = "CRAPO";
			AssertEquals("CFSShipments not populated: different discharge countries of consol and shipment", 0, ShipmentWrapper.CFSShipments.Count);

			consol.JK_RL_NKDischargePort = "AUMEL";
			AssertEquals("CFSShipments populated: same discharge country of consol and shipment", 13, ShipmentWrapper.CFSShipments.Count);

			Shipment.Consols.RemoveAndDeleteAll();
			AssertEquals("CFSShipments not populated: no consol found", 0, ShipmentWrapper.CFSShipments.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestTranshipmentShipments()
		{
			var client = Factory.LoadTop1<OrgHeader>(new ZQuery());
			SetShipmentForLabelTest("USLAX", "AUMEL", "HBL123", "Marks 123", "PLT", 4, client, "USCHI", "NZAKL");

			Shipment.OuterPackLines.RemoveAndDeleteAll();
			var line1 = Shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 1;
			line1.JL_F3_NKPackType = "PKG";
			var line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 3;
			line2.JL_F3_NKPackType = "BOX";

			ShipmentWrapper.SetReportNameForTesting("TRANSHIPMENT");
			AssertEquals(4, ShipmentWrapper.CFSShipments.Count);
			foreach (DocCFSShipment cFSWrapper in ShipmentWrapper.CFSShipments)
			{
				AssertLabelProperties(cFSWrapper, Shipment.JS_UniqueConsignRef, "Vessel", "Voyage No", client.OH_FullName, Today.ToString("dd-MMM-yy"),
					"AUMEL", "HBL123", "Marks 123", "", "", "", "");
				AssertEquals("PackageType", "PKG", cFSWrapper.CFSPackageType);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestOnForwardingShipments()
		{
			var client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			SetShipmentForLabelTest("USLAX", "AUMEL", "HBL123", "Marks 123", "PLT", 4, client, "USCHI", "AUPER");
			((ISupportDataImporting)Shipment).IsImportingData = true;
			Shipment.ConsigneePK = consignee.PK;
			Shipment.JS_CartageWaybill = "CONNOTE";

			Shipment.OuterPackLines.RemoveAndDeleteAll();
			var line1 = Shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 4;
			line1.JL_F3_NKPackType = "PLT";
			var line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 2;
			line2.JL_F3_NKPackType = "PLT";

			ShipmentWrapper.SetReportNameForTesting("ONFORWARDING");
			AssertEquals(6, ShipmentWrapper.CFSShipments.Count);
			foreach (DocCFSShipment cFSWrapper in ShipmentWrapper.CFSShipments)
			{
				AssertLabelProperties(cFSWrapper, Shipment.JS_UniqueConsignRef, "Vessel", "Voyage No", client.OH_FullName, Today.ToString("dd-MMM-yy"),
					"AUMEL", "HBL123", "Marks 123", "CONNOTE", "CON", consignee.OH_FullName, "CNE");
				AssertEquals("PackageType", "PLT", cFSWrapper.CFSPackageType);
			}
		}

		public void TestExportShipments()
		{
			var sailing = GetSailing("Vessel", "Voyage No", "USCHI", "AUPER");

			Shipment.JS_RL_NKDestination = "AUMEL";
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_JX = sailing.PK;
			AssertEquals(0, ShipmentWrapper.CFSShipments.Count);

			var line1 = Shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 4;
			var line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 2;

			ShipmentWrapper.SetReportNameForTesting("CFS EXPORT LABEL");
			AssertEquals("Should only print one label", 1, ShipmentWrapper.CFSShipments.Count);
			AssertEquals("Number of packages should be total package count", 6, ShipmentWrapper.CFSShipments[0].TotalPackages);
		}

		public void TestMostInterestingTransport()
		{
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "AUMEL";
			AssertEquals("Transport should be Empty", null, ShipmentWrapper.MostInterestingTransport);

			LinkSailing(Shipment, "VESSEL 1", "VOYAGE 1");
			AssertEquals("Transport from Shipment.Sailing", "VESSEL 1", ShipmentWrapper.MostInterestingTransport.VesselName);

			var transport = AddTransport(Shipment, "VESSEL 2", "VOYAGE 2");
			AssertEquals("Transport from Shipment.Transport", "VESSEL 2", ShipmentWrapper.MostInterestingTransport.VesselName);

			LinkSailing(transport, "VESSEL 3", "VOYAGE 3");
			AssertEquals("Transport from Shipment.Transport.Sailing", "VESSEL 3", ShipmentWrapper.MostInterestingTransport.VesselName);

			var consol = Shipment.Consols.AddNew();
			transport = AddTransport(consol, "VESSEL 4", "VOYAGE 4", "USCHI", "USLAX");
			AssertEquals("Transport from Consol.Transport", "VESSEL 4", ShipmentWrapper.MostInterestingTransport.VesselName);

			LinkSailing(transport, "VESSEL 5", "VOYAGE 5");
			AssertEquals("Transport from Consol.Transport.Sailing", "VESSEL 5", ShipmentWrapper.MostInterestingTransport.VesselName);

			Shipment.JS_JX = ZGuid.Empty;
			AssertEquals("Transport from Consol.Transport.Sailing", "VESSEL 5", ShipmentWrapper.MostInterestingTransport.VesselName);
		}

		#region IDocCartageAdvice

		public void TestPrintAsContainers()
		{
			AssertEquals("Print as container should always be false", false, ShipmentWrapper.PrintAsContainers);
		}
		#endregion

		#region Implementation

		CFSShipment Shipment;
		DocShipmentReceival ShipmentWrapper
		{
			get { return DocShipmentReceival.New(Shipment, Factory); }
		}

		readonly ZDateTime Today = ZDateTime.Today;

		Transport AddTransport(CFSShipment shipment, ZString vessel, ZString voyage)
		{
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = shipment.JS_RL_NKOrigin;
			transport.JW_RL_NKDiscPort = shipment.JS_RL_NKDestination;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyage;

			return transport;
		}

		Transport AddTransport(CFSLoadListConsol consol, ZString vessel, ZString voyage, ZString load, ZString discharge)
		{
			var transport = consol.Transports[0];
			transport.JW_IsLinked = false;
			transport.JW_RL_NKLoadPort = load;
			transport.JW_RL_NKDiscPort = discharge;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyage;

			return transport;
		}

		void LinkSailing(CFSShipment shipment, ZString vessel, ZString voyage)
		{
			var sailing = GetSailing(vessel, voyage, shipment.JS_RL_NKOrigin, shipment.JS_RL_NKDestination);
			shipment.JS_JX = sailing.PK;
		}

		void LinkSailing(Transport transport, ZString vessel, ZString voyage)
		{
			var sailing = GetSailing(vessel, voyage, transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort);
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
		}

		JobSailing GetSailing(ZString vesselCode, ZString voyageNo, ZString load, ZString discharge)
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = vesselCode;

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_Code;
			voyage.JV_VoyageFlight = voyageNo;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = Today;

			Factory.Save();

			return voyage.Sailings[0];
		}

		void AssertLabelProperties(DocCFSShipment wrapper, ZString shipmentNo, ZString vessel, ZString voyage,
			ZString clientName, ZString eTA, ZString destination, ZString hBL, ZString marks, ZString conNote, ZString conNoteHeading, ZString consignee, ZString consigneeHeading)
		{
			AssertEquals("ShipmentNumber", shipmentNo, wrapper.ShipmentNumber);
			AssertEquals("ClientName", clientName, wrapper.ClientName);
			AssertEquals("ETA", eTA, wrapper.ETA);
			AssertEquals("Destination", destination, wrapper.Dest);
			AssertEquals("HBL", hBL, wrapper.HBL);
			AssertEquals("Marks", marks, wrapper.Marks);
			AssertEquals("Vessel", vessel, wrapper.CFSVessel);
			AssertEquals("Voyage", voyage, wrapper.CFSVoyage);
			AssertEquals("Con Note", conNote, wrapper.LabelConNote);
			AssertEquals("Con Note Heading", conNoteHeading, wrapper.LabelConNoteHeading);
			AssertEquals("Consignee", consignee, wrapper.LabelConsignee);
			AssertEquals("Consignee Heading", consigneeHeading, wrapper.LabelConsigneeHeading);
		}

		void SetShipmentForLabelTest(ZString origin, ZString destination, ZString hBL, ZString marks, ZString packType, ZInt packs,
			OrgHeader client, ZString load, ZString discharge)
		{
			var sailing = GetSailing("Vessel", "Voyage No", load, discharge);

			Shipment.JS_RL_NKDestination = destination;
			Shipment.JS_RL_NKOrigin = origin;
			Shipment.JS_HouseBill = hBL;
			Shipment.JS_MarksAndNumbers = marks;
			Shipment.JS_F3_NKPackType = packType;
			Shipment.JS_OuterPacks = packs;
			Shipment.JS_OH_HandledOnBehalfOfForwarder = client.PK;
			Shipment.JS_JX = sailing.PK;

			AssertEquals(0, ShipmentWrapper.CFSShipments.Count);
		}

		protected override void SetUp()
		{
			Shipment = Factory.New<CFSShipment>();
			base.SetUp();
		}

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Eritrea; }
		}

		#endregion
	}
}
