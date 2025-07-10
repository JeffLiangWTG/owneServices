using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.Testing.ContainerWrappers
{
	sealed class DocPackUnpackContainerRegoTest : TestCaseWithFactory
	{
		#region Wrapper Fields

		public void TestSailingLine()
		{
			AssertNull("SailingLine", PackUnpackWrapper.SailingLine);

			CreateSailing();
			Voyage.JV_OH_Line = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			Factory.Save();
			AssertNotNull("SailingLine", PackUnpackWrapper.SailingLine);
			AssertEquals("SailingLine ss of type DocOrganistion", typeof(DocOrganisation), PackUnpackWrapper.SailingLine.GetType());
		}

		public void TestLoadListConsol()
		{
			AddContainerToLoadListConsolAndShipment();
			AssertNotNull("LoadListConsol", PackUnpackWrapper.LoadListConsol);
			AssertEquals("LoadListConsol is of type DocLoadListConsol", typeof(DocLoadListConsol), PackUnpackWrapper.LoadListConsol.GetType());
		}

		public void TestArrivalTransport()
		{
			PackUnpackContainer.JC_ArrivalTransportPK = ZGuid.Empty;
			AssertEquals("Arrival Transport Name", null, PackUnpackWrapper.ArrivalTransport);

			var transport = Factory.New<OrgHeader>();
			transport.OH_Code = "ARV";
			transport.OH_FullName = "arrival transport name";
			PackUnpackContainer.JC_ArrivalTransportPK = transport.PK;
			AssertEquals("Arrival Transport Name", "arrival transport name", PackUnpackWrapper.ArrivalTransport.Name);
		}

		public void TestDepartureTransport()
		{
			PackUnpackContainer.JC_DepartureTransportPK = ZGuid.Empty;
			AssertEquals("Departure Transport Name", null, PackUnpackWrapper.DepartureTransport);

			var transport = Factory.New<OrgHeader>();
			transport.OH_Code = "DEP";
			transport.OH_FullName = "departure transport name";
			PackUnpackContainer.JC_DepartureTransportPK = transport.PK;
			AssertEquals("Departure Transport Name", "departure transport name", PackUnpackWrapper.DepartureTransport.Name);
		}

		#endregion

		#region ZInt Fields

		public void TestTotalShipmentPacks()
		{
			AssertEquals("TotalShipmentPacks", PackUnpackContainer.TotalShipmentPacks, PackUnpackWrapper.TotalShipmentPacks);
		}

		public void TestTotalPillagedShipmentPacks()
		{
			var shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_OuterPacks = 12;
			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 12;
			pack1.JL_Pillaged = 10;
			PackUnpackContainer.PackUnpackShipments.Add(shipment1);

			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_OuterPacks = 3;
			PackLine pack2 = shipment2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			PackLine pack3 = shipment2.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 2;
			pack3.JL_Pillaged = 1;
			PackUnpackContainer.PackUnpackShipments.Add(shipment2);

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			AssertEquals(11, PackUnpackWrapper.TotalPillagedShipmentPacks);
		}

		public void TestTotalSurplusShipmentPacks()
		{
			var shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_OuterPacks = 5;
			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_Outturn = 10;
			PackUnpackContainer.PackUnpackShipments.Add(shipment1);

			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_OuterPacks = 5;
			PackLine pack2 = shipment2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			pack2.JL_Outturn = 1;
			PackLine pack3 = shipment2.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 4;
			pack3.JL_Outturn = 5;
			PackUnpackContainer.PackUnpackShipments.Add(shipment2);

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			AssertEquals(6, PackUnpackWrapper.TotalSurplusShipmentPacks);
		}

		public void TestTotalShortShipmentPacks()
		{
			PackUnpackContainer.PackUnpackShipments.RemoveAll();

			var shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_OuterPacks = 5;
			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_Outturn = 3;
			PackUnpackContainer.PackUnpackShipments.Add(shipment1);

			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_OuterPacks = 5;
			PackLine pack2 = shipment2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			pack2.JL_Outturn = 0;
			PackLine pack3 = shipment2.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 4;
			pack3.JL_Outturn = 4;
			PackUnpackContainer.PackUnpackShipments.Add(shipment2);

			DocPackUnpackContainerRego packUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			AssertEquals(3, packUnpackWrapper.TotalShortShipmentPacks);
		}

		#endregion

		#region ZDecimal Fields

		public void TestTotalShipmentWeight()
		{
			AssertEquals("TotalShipmentWeight", PackUnpackContainer.TotalShipmentWeight, PackUnpackWrapper.TotalShipmentWeight);
		}

		public void TestTotalShipmentVolume()
		{
			AssertEquals("TotalShipmentVolume", PackUnpackContainer.TotalShipmentVolume, PackUnpackWrapper.TotalShipmentVolume);
		}

		#endregion

		#region ZDateTime Fields

		public void TestPackUnpackDate()
		{
			AssertEquals("PackUnpackDate", PackUnpackContainer.JC_PackUnpackDate, PackUnpackWrapper.PackUnpackDate);
		}

		public void TestLCLAvailablReadonly()
		{
			AssertEquals("LCLAvailablReadonly", PackUnpackContainer.JC_LCLAvailable_Readonly, PackUnpackWrapper.LCLAvailablReadonly);
		}

		public void TestLCLStorageCommenceReadonly()
		{
			AssertEquals("LCLStorageCommenceReadonly", PackUnpackContainer.JC_LCLStorageCommences_Readonly, PackUnpackWrapper.LCLStorageCommenceReadonly);
		}

		public void TestFumigationCompletionEventTime()
		{
			AssertEquals("FumigationCompletionEventTime", PackUnpackContainer.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Fumigation), PackUnpackWrapper.FumigationCompletionEventTime);
		}

		public void TestQuarantineCompletionEventTime()
		{
			AssertEquals("QuarantineCompletionEventTime", PackUnpackContainer.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.QuarantineInspection), PackUnpackWrapper.QuarantineCompletionEventTime);
		}

		public void TestCustomsHoldCompletionEventTime()
		{
			AssertEquals("CustomsHoldCompletionEventTime", PackUnpackContainer.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.CustomsHold), PackUnpackWrapper.CustomsHoldCompletionEventTime);
		}

		public void TestWashingCompletionEventTime()
		{
			AssertEquals("WashingCompletionEventTime", PackUnpackContainer.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Washing), PackUnpackWrapper.WashingCompletionEventTime);
		}

		public void TestSteamCleanCompletionEventTime()
		{
			AssertEquals("SteamCleanCompletionEventTime", PackUnpackContainer.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.SteamCleaning), PackUnpackWrapper.SteamCleanCompletionEventTime);
		}

		public void TestExtraInspectionCompletionEventTime()
		{
			AssertEquals("ExtraInspectionCompletionEventTime", PackUnpackContainer.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.ExtraInspection), PackUnpackWrapper.ExtraInspectionCompletionEventTime);
		}

		public void TestCleaningCompletionEventTime()
		{
			AssertEquals("CleaningCompletionEventTime", PackUnpackContainer.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Cleaning), PackUnpackWrapper.CleaningCompletionEventTime);
		}

		public void TestTailgateCompletionEventTime()
		{
			AssertEquals("TailgateCompletionEventTime", PackUnpackContainer.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Tailgate), PackUnpackWrapper.TailgateCompletionEventTime);
		}

		public void TestQuarantineUnpackCompletionEventTime()
		{
			AssertEquals("QuarantineUnpackCompletionEventTime", PackUnpackContainer.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.QuarantineUnpack), PackUnpackWrapper.QuarantineUnpackCompletionEventTime);
		}

		public void TestSailingETA()
		{
			CreateSailing();
			Destination.JB_E_ARV = ZDateTime.Empty;
			AssertEquals("SailingETA", ZString.Empty, PackUnpackWrapper.SailingETA.ToString());

			ZDateTime sailingETA = new ZDateTime(2004, 04, 04);
			Destination.JB_E_ARV = sailingETA;
			AssertEquals("SailingETA", sailingETA, PackUnpackWrapper.SailingETA);
		}

		public void TestSailingETD()
		{
			CreateSailing();
			Origin.JA_E_DEP = ZDateTime.Empty;
			AssertEquals("SailingETD", ZString.Empty, PackUnpackWrapper.SailingETD.ToString());

			ZDateTime sailingETD = new ZDateTime(2004, 04, 04);
			Origin.JA_E_DEP = sailingETD;
			AssertEquals("SailingETD", sailingETD, PackUnpackWrapper.SailingETD);
		}

		public void TestContainerRegBookingCutOffDate()
		{
			CreateSailing();
			AssertEquals(ZDateTime.Empty, PackUnpackWrapper.BookingCutOffDate);

			PackUnpackContainer.JC_JK = Sailing.PK;
			Sailing.Origin.JA_CutOff = ZDateTime.Today;
			Sailing.JX_DepotCutOff = ZDateTime.Today.AddDays(2);

			PackUnpackContainer.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(ZDateTime.Today.AddDays(2), PackUnpackWrapper.BookingCutOffDate);

			PackUnpackContainer.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(ZDateTime.Today, PackUnpackWrapper.BookingCutOffDate);
		}

		public void TestLoadListConsolBookingCutOffDate()
		{
			AddContainerToLoadListConsolAndShipment();
			AssertEquals(ZDateTime.Empty, PackUnpackWrapper.BookingCutOffDate);

			Sailing = Factory.New<JobSailing>();
			Voyage = Factory.New<JobVoyage>();
			Origin = Factory.New<VoyageOrigin>();
			Destination = Factory.New<VoyageDestination>();
			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;
			Origin.JA_JV = Voyage.PK;
			Destination.JB_JV = Voyage.PK;

			Transport transport = LoadList.Transports[0];
			transport.JW_JX = Sailing.PK;

			LoadList.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			Sailing.JX_DepotCutOff = ZDateTime.Today.AddDays(1);
			Sailing.Origin.JA_CutOff = ZDateTime.Today;
			AssertEquals(ZDateTime.Today.AddDays(1), PackUnpackWrapper.BookingCutOffDate);

			LoadList.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			Sailing.JX_DepotCutOff = ZDateTime.Today.AddDays(1);
			Sailing.Origin.JA_CutOff = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, PackUnpackWrapper.BookingCutOffDate);
		}

		#endregion

		#region Collections

		public void TestConsigneeAndConsignorLabels()
		{
			CreateSailing();

			Origin.JA_RL_NKPortOfLoading = "USLAX";
			Destination.JB_RL_NKPortOfDischarge = "AUMEL";
			PackUnpackContainer.JC_JX = Sailing.PK;

			CFSShipment shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_RL_NKDestination = "AUBNE";
			shipment1.JS_CartageWaybill = "TEST";

			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 1;
			PackUnpackContainer.AddPackLine(pack1);

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_OuterPacks = 1;
			shipment2.JS_RL_NKDestination = "AUSYD";

			PackLine pack2 = shipment2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			PackUnpackContainer.AddPackLine(pack2);

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);

			PackUnpackWrapper.SetReportNameForTesting(DocBaseWrapper.OnForwardingLabel);
			AssertLabels(PackUnpackWrapper.CFSShipments);

			PackUnpackWrapper.SetReportNameForTesting(DocBaseWrapper.ImportLabel);
			AssertLabels(PackUnpackWrapper.CFSShipments);
		}

		void AssertLabels(DocCFSShipmentCollection cfsShipments)
		{
			AssertEquals("CON Heading", "CON", cfsShipments[0].LabelConNoteHeading);
			AssertEquals("Cartage waybill", "TEST", cfsShipments[0].LabelConNote);
			AssertEquals("CNE Heading", "CNE", cfsShipments[0].LabelConsigneeHeading);

			AssertEquals("CON Heading (Blank)", ZString.Empty, cfsShipments[1].LabelConNoteHeading);
			AssertEquals("Cartage waybill (Blank)", ZString.Empty, cfsShipments[1].LabelConNote);
			AssertEquals("Consignee (Blank)", ZString.Empty, cfsShipments[1].LabelConsignee);
		}

		public void TestContainerShipments()
		{
			PackUnpackContainer.PackUnpackShipments.RemoveAll();

			var shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_OuterPacks = 2;
			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 2;
			PackUnpackContainer.PackUnpackShipments.Add(shipment1);

			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_OuterPacks = 3;
			PackLine pack2 = shipment2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			PackLine pack3 = shipment2.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 2;
			PackUnpackContainer.PackUnpackShipments.Add(shipment2);

			DocPackUnpackContainerRego containerRegoWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			AssertEquals(2, PackUnpackWrapper.ContainerShipments.Count);
		}

		public void TestCFSShipmentsCollection()
		{
			PackUnpackContainer.PackUnpackShipments.RemoveAll();

			var shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_OuterPacks = 2;
			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 2;
			PackUnpackContainer.AddPackLine(pack1);

			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_OuterPacks = 3;
			PackLine pack2 = shipment2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			PackLine pack3 = shipment2.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 2;
			PackUnpackContainer.AddPackLine(pack2);
			PackUnpackContainer.AddPackLine(pack3);

			DocPackUnpackContainerRego containerRegoWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			AssertEquals(5, PackUnpackWrapper.CFSShipmentsCollection.Count);
		}

		public void TestCFSShipmentsCollectionWithNullShipment()
		{
			PackUnpackContainer.PackUnpackShipments.RemoveAll();

			CFSShipment shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_OuterPacks = 2;
			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 2;
			PackUnpackContainer.AddPackLine(pack1);

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_OuterPacks = 3;
			PackLine pack2 = shipment2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			PackLine pack3 = shipment2.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 2;
			PackUnpackContainer.AddPackLine(pack2);
			PackUnpackContainer.AddPackLine(pack3);

			shipment1.OuterPackLines.Remove(pack1);

			DocPackUnpackContainerRego containerRegoWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			AssertEquals(3, PackUnpackWrapper.CFSShipmentsCollection.Count);
		}

		public void TestCFSShipmentsForImportShipments()
		{
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);

			CreateSailing();
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);

			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Destination.JB_RL_NKPortOfDischarge = "USCHI";
			PackUnpackContainer.JC_JX = Sailing.PK;

			var shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_RL_NKDestination = "USCHI";
			var pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 1;
			PackUnpackContainer.AddPackLine(pack1);

			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_OuterPacks = 1;
			shipment2.JS_RL_NKDestination = "SGSIN";
			var pack2 = shipment2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			PackUnpackContainer.AddPackLine(pack2);

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			PackUnpackWrapper.SetReportNameForTesting("IMPORT");
			AssertEquals(1, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals("Using sailing's discharge to calculate if cfsshipment is import", "USCHI", PackUnpackWrapper.CFSShipments[0].Dest);

			PackUnpackWrapper.SetReportNameForTesting("TRANSHIPMENT");
			AssertEquals(1, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals("SGSIN", PackUnpackWrapper.CFSShipments[0].Dest);

			PackUnpackWrapper.SetReportNameForTesting("ONFORWARDING");
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals(2, PackUnpackWrapper.CFSShipmentsCollection.Count);

			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.Containers.Add(PackUnpackContainer);

			PackUnpackWrapper.SetReportNameForTesting("IMPORT");
			AssertEquals(1, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals("Using consol's discharge to calculate if cfsshipment is import", "SGSIN", PackUnpackWrapper.CFSShipments[0].Dest);

			PackUnpackWrapper.SetReportNameForTesting("TRANSHIPMENT");
			AssertEquals(1, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals("USCHI", PackUnpackWrapper.CFSShipments[0].Dest);

			PackUnpackWrapper.SetReportNameForTesting("ONFORWARDING");
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals(2, PackUnpackWrapper.CFSShipmentsCollection.Count);

			PackUnpackContainer.JC_JX = ZGuid.Empty;

			PackUnpackWrapper.SetReportNameForTesting("IMPORT");
			AssertEquals(1, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals("SGSIN", PackUnpackWrapper.CFSShipments[0].Dest);

			PackUnpackWrapper.SetReportNameForTesting("TRANSHIPMENT");
			AssertEquals(1, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals("USCHI", PackUnpackWrapper.CFSShipments[0].Dest);

			PackUnpackWrapper.SetReportNameForTesting("ONFORWARDING");
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals(2, PackUnpackWrapper.CFSShipmentsCollection.Count);
		}

		public void TestCFSShipmentsForTranshipmentShipments()
		{
			PackUnpackWrapper.SetReportNameForTesting(DocBaseWrapper.TranshipmentLabel);
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);

			CreateSailing();
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);

			Origin.JA_RL_NKPortOfLoading = "AUBNE";
			Destination.JB_RL_NKPortOfDischarge = "USLAX";
			PackUnpackContainer.JC_JX = Sailing.PK;

			var shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_RL_NKDestination = "MYKUL";
			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 1;
			PackUnpackContainer.AddPackLine(pack1);

			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_OuterPacks = 1;
			shipment2.JS_RL_NKDestination = "SGSIN";
			PackLine pack2 = shipment2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			PackUnpackContainer.AddPackLine(pack2);

			var shipment3 = Factory.New<CFSShipment>();
			shipment3.JS_OuterPacks = 1;
			shipment3.JS_RL_NKDestination = "USCHI";
			PackLine pack3 = shipment3.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 1;
			PackUnpackContainer.AddPackLine(pack3);

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			PackUnpackWrapper.SetReportNameForTesting("TRANSHIPMENT");
			AssertEquals(2, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals("MYKUL", PackUnpackWrapper.CFSShipments[0].Dest);
			AssertEquals("SGSIN", PackUnpackWrapper.CFSShipments[1].Dest);

			PackUnpackWrapper.SetReportNameForTesting("IMPORT");
			AssertEquals(1, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals("USCHI", PackUnpackWrapper.CFSShipments[0].Dest);

			PackUnpackWrapper.SetReportNameForTesting("ONFORWARDING");
			AssertEquals(1, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals(3, PackUnpackWrapper.CFSShipmentsCollection.Count);
		}

		public void TestCFSShipmentsForOnForwardingShipments()
		{
			PackUnpackWrapper.SetReportNameForTesting(DocBaseWrapper.OnForwardingLabel);
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);

			CreateSailing();
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);

			Origin.JA_RL_NKPortOfLoading = "USLAX";
			Destination.JB_RL_NKPortOfDischarge = "AUMEL";
			PackUnpackContainer.JC_JX = Sailing.PK;

			var shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_RL_NKDestination = "AUBNE";
			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 1;
			PackUnpackContainer.AddPackLine(pack1);

			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_OuterPacks = 1;
			shipment2.JS_RL_NKDestination = "SGSIN";
			PackLine pack2 = shipment2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			PackUnpackContainer.AddPackLine(pack2);

			var shipment3 = Factory.New<CFSShipment>();
			shipment3.JS_OuterPacks = 1;
			shipment3.JS_RL_NKDestination = "AUSYD";
			PackLine pack3 = shipment3.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 1;
			PackUnpackContainer.AddPackLine(pack3);

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			PackUnpackWrapper.SetReportNameForTesting("ONFORWARDING");
			AssertEquals(2, PackUnpackWrapper.CFSShipments.Count);
			AssertEquals("AUBNE", PackUnpackWrapper.CFSShipments[0].Dest);
			AssertEquals("AUSYD", PackUnpackWrapper.CFSShipments[1].Dest);
			AssertEquals(3, PackUnpackWrapper.CFSShipmentsCollection.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestCFSShipmentsShipmentDetails()
		{
			PackUnpackWrapper.SetReportNameForTesting(DocBaseWrapper.OnForwardingLabel);
			CreateSailing();
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);

			Origin.JA_RL_NKPortOfLoading = "AUBNE";
			Destination.JB_RL_NKPortOfDischarge = "USLAX";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			Voyage.JV_VoyageFlight = "123456";
			PackUnpackContainer.JC_ContainerNum = "CONT2382832";

			var consol = Factory.New<CFSLoadListConsol>();
			consol.Containers.Add(PackUnpackContainer);

			Transport transport = consol.Transports[0];
			transport.JW_JX = Sailing.PK;
			var client = Factory.New<OrgHeader>();
			consol.JK_OH_Forwarder = client.PK;

			CFSShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00029385";
			OrgHeader consignee = GlbCompany.CurrentCompany.OrgProxy;
			shipment1.ConsigneePK = consignee.PK;
			shipment1.JS_E_ARV = ZDateTime.Today.AddDays(3);
			shipment1.JS_OuterPacks = 2;
			shipment1.JS_HouseBill = "HBL123";
			shipment1.JS_RL_NKDestination = "USCHI";
			shipment1.JS_CartageWaybill = "WWWEEE";
			shipment1.JS_MarksAndNumbers = "Marks and numbers lalala";
			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 2;
			pack1.JL_F3_NKPackType = "DRM";
			PackUnpackContainer.AddPackLine(pack1);

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			PackUnpackWrapper.SetReportNameForTesting(DocBaseWrapper.OnForwardingLabel);
			DocCFSShipmentCollection coll = PackUnpackWrapper.CFSShipments;
			AssertEquals(2, coll.Count);
			AssertEquals("Voyage", "123456", coll[0].CFSVoyage);
			AssertEquals("Vessel", vessel.RV_Code, coll[0].CFSVessel);
			AssertEquals("Destination", "USCHI", coll[0].Dest);
			AssertEquals("ETA", ZDateTime.Today.AddDays(3).ToString("dd-MMM-yy"), coll[0].ETA);
			AssertEquals("Shipment Number", "S00029385", coll[0].ShipmentNumber);
			AssertEquals("HBL", "HBL123", coll[0].HBL);
			AssertEquals("Marks and numbers", "Marks and numbers lalala", coll[0].Marks);
			AssertEquals("CON Heading", "CON", coll[0].LabelConNoteHeading);
			AssertEquals("Cartage waybill", "WWWEEE", coll[0].LabelConNote);
			AssertEquals("CNE Heading", "CNE", coll[0].LabelConsigneeHeading);
			AssertEquals("Consignee", consignee.OH_FullName, coll[0].LabelConsignee);
			AssertEquals("Packages", 2, coll[0].TotalPackages);
			AssertEquals("Pack type", "DRM", coll[0].CFSPackageType);

			AssertEquals("Voyage", "123456", coll[1].CFSVoyage);
			AssertEquals("Vessel", vessel.RV_Code, coll[1].CFSVessel);
			AssertEquals("Destination", "USCHI", coll[1].Dest);
			AssertEquals("ETA", ZDateTime.Today.AddDays(3).ToString("dd-MMM-yy"), coll[1].ETA);
			AssertEquals("Shipment Number", "S00029385", coll[0].ShipmentNumber);
			AssertEquals("HBL", "HBL123", coll[1].HBL);
			AssertEquals("Marks and numbers", "Marks and numbers lalala", coll[1].Marks);
			AssertEquals("CON Heading", "CON", coll[1].LabelConNoteHeading);
			AssertEquals("Cartage waybill", "WWWEEE", coll[1].LabelConNote);
			AssertEquals("CNE Heading", "CNE", coll[1].LabelConsigneeHeading);
			AssertEquals("Consignee", consignee.OH_FullName, coll[1].LabelConsignee);
			AssertEquals("Packages", 2, coll[1].TotalPackages);
			AssertEquals("Pack type", "DRM", coll[1].CFSPackageType);

			shipment1.JS_RL_NKDestination = "USLAX";
			consol.JK_RL_NKDischargePort = "USLAX";

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			PackUnpackWrapper.SetReportNameForTesting("IMPORT");
			coll = PackUnpackWrapper.CFSShipments;
			AssertEquals(2, coll.Count);
			AssertEquals("CON Heading", "CON", coll[0].LabelConNoteHeading);
			AssertEquals("Cartage waybill", "WWWEEE", coll[0].LabelConNote);
			AssertEquals("CNE Heading", "CNE", coll[0].LabelConsigneeHeading);
			AssertEquals("Consignee", "EDI CUSTOMS BROKERS", coll[0].LabelConsignee);
			AssertEquals("CON Heading", "CON", coll[1].LabelConNoteHeading);
			AssertEquals("Cartage waybill", "WWWEEE", coll[1].LabelConNote);
			AssertEquals("CNE Heading", "CNE", coll[1].LabelConsigneeHeading);
			AssertEquals("Consignee", "EDI CUSTOMS BROKERS", coll[1].LabelConsignee);
		}

		public void TestCFSShipmentsTotalPackageAndPackType()
		{
			PackUnpackWrapper.SetReportNameForTesting("IMPORT");
			CreateSailing();
			AssertEquals(0, PackUnpackWrapper.CFSShipments.Count);

			Origin.JA_RL_NKPortOfLoading = "AUBNE";
			Destination.JB_RL_NKPortOfDischarge = "USLAX";
			PackUnpackContainer.JC_JX = Sailing.PK;

			var shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_UniqueConsignRef = "S00029385";
			shipment1.JS_OuterPacks = 5;
			shipment1.JS_RL_NKDestination = "USLAX";
			PackLine pack1 = shipment1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 3;
			pack1.JL_F3_NKPackType = "DRM";

			PackLine pack2 = shipment1.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 2;
			pack2.JL_F3_NKPackType = "DRM";
			PackUnpackContainer.AddPackLine(pack1);
			PackUnpackContainer.AddPackLine(pack2);

			var shipment2 = Factory.New<CFSShipment>();
			shipment1.JS_UniqueConsignRef = "S00099982";
			shipment2.JS_OuterPacks = 3;
			shipment2.JS_RL_NKDestination = "USLAX";
			PackLine pack3 = shipment2.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 1;
			pack3.JL_F3_NKPackType = "BOX";
			PackLine pack4 = shipment2.OuterPackLines.AddNew();
			pack4.JL_PackageCount = 2;
			pack4.JL_F3_NKPackType = "CTN";
			PackUnpackContainer.AddPackLine(pack3);
			PackUnpackContainer.AddPackLine(pack4);

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Containers.Add(PackUnpackContainer);

			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			PackUnpackWrapper.SetReportNameForTesting("IMPORT");
			DocCFSShipmentCollection coll = PackUnpackWrapper.CFSShipments;
			AssertEquals(8, coll.Count);
			AssertEquals("Total Packs of Pack1 & 2", 5, coll[0].TotalPackages);
			AssertEquals("Total Packs of Pack1 & 2", 5, coll[1].TotalPackages);
			AssertEquals("Total Packs of Pack1 & 2", 5, coll[2].TotalPackages);
			AssertEquals("Total Packs of Pack1 & 2", 5, coll[3].TotalPackages);
			AssertEquals("Total Packs of Pack1 & 2", 5, coll[4].TotalPackages);
			AssertEquals("Total Packs of Pack3 & 4", 3, coll[5].TotalPackages);
			AssertEquals("Total Packs of Pack3 & 4", 3, coll[6].TotalPackages);
			AssertEquals("Total Packs of Pack3 & 4", 3, coll[7].TotalPackages);

			AssertEquals("PackType for Pack1 & 2", "DRM", coll[0].CFSPackageType);
			AssertEquals("PackType for Pack1 & 2", "DRM", coll[1].CFSPackageType);
			AssertEquals("PackType for Pack1 & 2", "DRM", coll[2].CFSPackageType);
			AssertEquals("PackType for Pack1 & 2", "DRM", coll[3].CFSPackageType);
			AssertEquals("PackType for Pack1 & 2", "DRM", coll[4].CFSPackageType);
			AssertEquals("PackType for Pack3 & 4", "PKG", coll[5].CFSPackageType);
			AssertEquals("PackType for Pack3 & 4", "PKG", coll[6].CFSPackageType);
			AssertEquals("PackType for Pack3 & 4", "PKG", coll[7].CFSPackageType);
		}

		public void TestContainerPackCount()
		{
			PackUnpackContainer.PackUnpackShipments.RemoveAll();
			var masterShipment1 = Factory.New<CFSShipment>();
			var subShipment2 = Factory.New<CFSShipment>();
			var subShipment3 = Factory.New<CFSShipment>();

			masterShipment1.JS_UniqueConsignRef = "S111000";
			subShipment2.JS_UniqueConsignRef = "S333000";
			subShipment3.JS_UniqueConsignRef = "S222000";

			PackLine line1Ship1 = masterShipment1.OuterPackLines.AddNew();
			PackLine line2Ship1 = masterShipment1.OuterPackLines.AddNew();
			PackLine line1Ship2 = subShipment2.OuterPackLines.AddNew();
			PackLine line1Ship3 = subShipment3.OuterPackLines.AddNew();
			PackLine line2Ship3 = subShipment3.OuterPackLines.AddNew();
			PackLine line3Ship3 = subShipment3.OuterPackLines.AddNew();
			PackUnpackContainer.AddPackLine(line1Ship1);
			PackUnpackContainer.AddPackLine(line2Ship1);
			PackUnpackContainer.AddPackLine(line1Ship2);
			PackUnpackContainer.AddPackLine(line1Ship3);
			PackUnpackContainer.AddPackLine(line2Ship3);
			PackUnpackContainer.AddPackLine(line3Ship3);

			DocPackUnpackContainerRego containerRegoWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
			DocPackLinesCollection coll = PackUnpackWrapper.ContainerPackCount;
			AssertEquals("Expecting 6 packlines", 6, coll.Count);
			AssertEquals("Shipment number should be sorted", "S111000", coll[0].ShipmentNumber);
			AssertEquals("Shipment number should be sorted", "S111000", coll[1].ShipmentNumber);
			AssertEquals("Shipment number should be sorted", "S222000", coll[2].ShipmentNumber);
			AssertEquals("Shipment number should be sorted", "S222000", coll[3].ShipmentNumber);
			AssertEquals("Shipment number should be sorted", "S222000", coll[4].ShipmentNumber);
			AssertEquals("Shipment number should be sorted", "S333000", coll[5].ShipmentNumber);
		}

		#endregion

		#region ZBool Fields

		public void TestIsFumigationCompleted()
		{
			Assert("!IsFumigationCompleted", !PackUnpackWrapper.IsFumigationCompleted);

			JobService fumigation = PackUnpackContainer.Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Assert("!IsFumigationCompleted", !PackUnpackWrapper.IsFumigationCompleted);

			fumigation.ES_Completed = ZDateTime.Now;
			Assert("IsFumigationCompleted", PackUnpackWrapper.IsFumigationCompleted);
		}

		public void TestIsQuarantineCompleted()
		{
			Assert("!IsQuarantineCompleted", !PackUnpackWrapper.IsQuarantineCompleted);

			JobService quarantine = PackUnpackContainer.Services.AddNew();
			quarantine.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			Assert("!IsQuarantineCompleted", !PackUnpackWrapper.IsQuarantineCompleted);

			quarantine.ES_Completed = ZDateTime.Now;
			Assert("IsQuarantineCompleted", PackUnpackWrapper.IsQuarantineCompleted);
		}

		public void TestIsCustomsHoldCompleted()
		{
			Assert("!IsCustomsHoldCompleted", !PackUnpackWrapper.IsCustomsHoldCompleted);

			JobService customsHold = PackUnpackContainer.Services.AddNew();
			customsHold.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.CustomsHold;
			Assert("!IsCustomsHoldCompleted", !PackUnpackWrapper.IsCustomsHoldCompleted);

			customsHold.ES_Completed = ZDateTime.Now;
			Assert("IsCustomsHoldCompleted", PackUnpackWrapper.IsCustomsHoldCompleted);
		}

		public void TestIsWashingCompleted()
		{
			Assert("!IsWashingCompleted", !PackUnpackWrapper.IsWashingCompleted);

			JobService washing = PackUnpackContainer.Services.AddNew();
			washing.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Washing;
			Assert("!IsWashingCompleted", !PackUnpackWrapper.IsWashingCompleted);

			washing.ES_Completed = ZDateTime.Now;
			Assert("IsWashingCompleted", PackUnpackWrapper.IsWashingCompleted);
		}

		public void TestIsSteamCleanCompleted()
		{
			Assert("!IsSteamCleanCompleted", !PackUnpackWrapper.IsSteamCleanCompleted);

			JobService steamClean = PackUnpackContainer.Services.AddNew();
			steamClean.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.SteamCleaning;
			Assert("!IsSteamCleanCompleted", !PackUnpackWrapper.IsSteamCleanCompleted);

			steamClean.ES_Completed = ZDateTime.Now;
			Assert("IsSteamCleanCompleted", PackUnpackWrapper.IsSteamCleanCompleted);
		}

		public void TestIsExtraInspectionCompleted()
		{
			Assert("!IsExtraInspectionCompleted", !PackUnpackWrapper.IsExtraInspectionCompleted);

			JobService extraInspection = PackUnpackContainer.Services.AddNew();
			extraInspection.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.ExtraInspection;
			Assert("!IsExtraInspectionCompleted", !PackUnpackWrapper.IsExtraInspectionCompleted);

			extraInspection.ES_Completed = ZDateTime.Now;
			Assert("IsExtraInspectionCompleted", PackUnpackWrapper.IsExtraInspectionCompleted);
		}

		public void TestIsCleaningCompleted()
		{
			Assert("!IsCleaningCompleted", !PackUnpackWrapper.IsCleaningCompleted);

			JobService cleaning = PackUnpackContainer.Services.AddNew();
			cleaning.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Cleaning;
			Assert("!IsCleaningCompleted", !PackUnpackWrapper.IsCleaningCompleted);

			cleaning.ES_Completed = ZDateTime.Now;
			Assert("IsCleaningCompleted", PackUnpackWrapper.IsCleaningCompleted);
		}

		public void TestIsTailgateCompleted()
		{
			Assert("!IsTailgateCompleted", !PackUnpackWrapper.IsTailgateCompleted);

			JobService tailgate = PackUnpackContainer.Services.AddNew();
			tailgate.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Tailgate;
			Assert("!IsTailgateCompleted", !PackUnpackWrapper.IsTailgateCompleted);

			tailgate.ES_Completed = ZDateTime.Now;
			Assert("IsTailgateCompleted", PackUnpackWrapper.IsTailgateCompleted);
		}

		public void TestIsQuarantineUnpackCompleted()
		{
			Assert("!IsQuarantineUnpackCompleted", !PackUnpackWrapper.IsQuarantineUnpackCompleted);

			JobService unpack = PackUnpackContainer.Services.AddNew();
			unpack.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineUnpack;
			Assert("!IsQuarantineUnpackCompleted", !PackUnpackWrapper.IsQuarantineUnpackCompleted);

			unpack.ES_Completed = ZDateTime.Now;
			Assert("IsQuarantineUnpackCompleted", PackUnpackWrapper.IsQuarantineUnpackCompleted);
		}

		#endregion

		#region ZString Fields

		public void TestUNDG_Codes()
		{
			var dGS1 = Factory.New<UNDGSubstance>();
			dGS1.DG_UNNO = "9001";
			dGS1.DG_Variant = "a";

			var dGS2 = Factory.New<UNDGSubstance>();
			dGS2.DG_UNNO = "9001";
			dGS2.DG_Variant = "b";

			var dGS3 = Factory.New<UNDGSubstance>();
			dGS3.DG_UNNO = "9002";
			dGS3.DG_Variant = "";

			PackLine line;

			CommonShipment shipment = Factory.New<CommonShipment>();
			line = shipment.OuterPackLines.AddNew();
			line.Containers.Add(PackUnpackContainer);

			line = shipment.OuterPackLines.AddNew();
			line.Containers.Add(PackUnpackContainer);

			AssertEquals("Incorrect Code Set", "", PackUnpackWrapper.UNDG_Codes);

			line = shipment.OuterPackLines.AddNew();
			line.Containers.Add(PackUnpackContainer);
			line.UNDGs.AddNew().DI_DG = dGS1.PK;

			AssertEquals("Incorrect Code Set", "9001a", PackUnpackWrapper.UNDG_Codes);

			line = shipment.OuterPackLines.AddNew();
			line.Containers.Add(PackUnpackContainer);
			line.UNDGs.AddNew().DI_DG = dGS1.PK;

			line = shipment.OuterPackLines.AddNew();
			line.Containers.Add(PackUnpackContainer);
			line.UNDGs.AddNew().DI_DG = dGS3.PK;

			line = shipment.OuterPackLines.AddNew();
			line.Containers.Add(PackUnpackContainer);
			line.UNDGs.AddNew().DI_DG = dGS2.PK;

			line = shipment.OuterPackLines.AddNew();
			line.Containers.Add(PackUnpackContainer);
			line.UNDGs.AddNew().DI_DG = dGS1.PK;

			line = shipment.OuterPackLines.AddNew();
			line.Containers.Add(PackUnpackContainer);

			AssertEquals("Incorrect Code Set", "9001a, 9001b, 9002", PackUnpackWrapper.UNDG_Codes);
		}

		public void TestHandlingInstructions()
		{
			AssertEquals("HandlingInstructions", ZString.Empty, PackUnpackWrapper.HandlingInstructions);

			bool found = false;
			foreach (PredefinedNoteType note in PackUnpackContainer.NoteTypes)
			{
				if (note == PredefinedNoteTypes.Instance.HandlingInstructions)
				{
					found = true;
					break;
				}
			}

			Assert("PackUnpackContainer must be able to have notes of type HandlingInstructions.", found);
			var handlingInstructions = PackUnpackContainer.Notes.AddNew();
			handlingInstructions.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			handlingInstructions.ST_Table = PackUnpackContainer.TableName;
			handlingInstructions.ST_ParentID = PackUnpackContainer.PK;
			handlingInstructions.ST_NoteDataAsText = "Handling Instruction\nLine Two";

			PackUnpackWrapper.SetReportNameForTesting("Load List Report");
			AssertEquals("HandlingInstructions", handlingInstructions.ST_NoteDataAsText, PackUnpackWrapper.HandlingInstructions);

			PackUnpackWrapper.SetReportNameForTesting("CFS Cartage Advice");
			AssertEquals("HandlingInstructions", handlingInstructions.ST_NoteDataAsText, PackUnpackWrapper.HandlingInstructions);

			AddContainerToLoadListConsolAndShipment();
			PackUnpackContainer.JC_JK = LoadList.PK;

			var loadListHandlingInstructions = LoadList.Notes.AddNew();
			loadListHandlingInstructions.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			loadListHandlingInstructions.ST_Table = LoadList.TableName;
			loadListHandlingInstructions.ST_ParentID = LoadList.PK;
			loadListHandlingInstructions.ST_NoteDataAsText = "Handling Instructions from load list\nLine Two";

			PackUnpackWrapper.SetReportNameForTesting("CFS Cartage Advice");
			AssertEquals("HandlingInstructions from load list", loadListHandlingInstructions.ST_NoteDataAsText, PackUnpackWrapper.HandlingInstructions);
		}

		public void TestTransportMode()
		{
			ZString transportMode = new ZString("TTT");
			PackUnpackContainer.JC_TransportMode = transportMode;
			AssertEquals("TransportMode", transportMode, PackUnpackWrapper.TransportMode);
		}

		public void TestWarningMessage()
		{
			AssertEquals("WarningMessage", PackUnpackContainer.WarningMessage, PackUnpackWrapper.WarningMessage);
		}

		public void TestContainerParkName()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Container Park";
			CreateSailing();
			Sailing.Origin.JA_RL_NKPortOfLoading = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Sailing.Destination.JB_RL_NKPortOfDischarge = "SGSIN";
			PackUnpackContainer.ContainerYardAddress = org.MainAddress.PK;
			AssertEquals("ContainerParkName", "Container Park", PackUnpackWrapper.ContainerParkName);
		}

		public void TestUnpackDepotName()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.Containers.Add(PackUnpackContainer);
			var depot = Factory.New<OrgHeader>();
			depot.OH_FullName = "Depot";
			PackUnpackContainer.Consol.JK_OA_UnpackDepotAddress = depot.MainAddress.PK;
			AssertEquals("UnpackDepotName", "Depot", PackUnpackWrapper.UnpackDepotName);
		}

		public void TestGrossWeight()
		{
			PackUnpackContainer.JC_GrossWeight = 1.453M;
			AssertEquals("GrossWeight", "1.45", PackUnpackWrapper.GrossWeight);
		}

		public void TestCTOName()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			CreateSailing();

			Transport transport = consol.Transports[0];
			transport.JW_JX = PackUnpackContainer.JC_JX;
			consol.Containers.Add(PackUnpackContainer);
			Origin.JA_RL_NKPortOfLoading = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Destination.JB_RL_NKPortOfDischarge = "CHRRC";

			var cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "CTO";
			PackUnpackContainer.Consol.JK_OA_ArrivalCTOAddress = cTO.MainAddress.PK;
			AssertEquals("CTOName", "CTO", PackUnpackWrapper.CTOName);
		}

		public void TestClientName()
		{
			PackUnpackContainer.JC_OH_CFSClient = ZGuid.Empty;
			AssertEquals("Client Name", ZString.Empty, PackUnpackWrapper.ClientName);

			var header = Factory.New<OrgHeader>();
			PackUnpackContainer.JC_OH_CFSClient = header.PK;
			AssertEquals("Client Name", header.OH_FullName, PackUnpackWrapper.ClientName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestETA()
		{
			AssertEquals("ETA", ZString.Empty, PackUnpackWrapper.ETA);

			CreateSailing();
			ZDateTime eTA = new ZDateTime(2004, 04, 04);
			Destination.JB_E_ARV = eTA;
			AssertEquals("ETA", eTA.ToString("dd-MMM-yy"), PackUnpackWrapper.ETA);

			AddContainerToLoadListConsolAndShipment();
			Transport transport = LoadList.Transports[0];
			ZDateTime eTA2 = new ZDateTime(2005, 05, 05);
			transport.JW_ETA = eTA2;

			AssertEquals("ETA", eTA2.ToString("dd-MMM-yy"), PackUnpackWrapper.ETA);
			AssertNotEquals("ETA", Destination.JB_E_ARV.ToString("dd-MMM-yy"), eTA2);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestETD()
		{
			AssertEquals("ETD", ZString.Empty, PackUnpackWrapper.ETD);

			CreateSailing();
			ZDateTime eTD = new ZDateTime(2004, 04, 04);
			Origin.JA_E_DEP = eTD;
			AssertEquals("ETD", eTD.ToString("dd-MMM-yy"), PackUnpackWrapper.ETD);

			AddContainerToLoadListConsolAndShipment();
			Transport transport = LoadList.Transports[0];
			ZDateTime eTD2 = new ZDateTime(2005, 05, 05);
			transport.JW_ETD = eTD2;

			AssertEquals("ETD", transport.JW_ETD.ToString("dd-MMM-yy"), PackUnpackWrapper.ETD);
			AssertNotEquals("ETD", eTD2, Origin.JA_E_DEP.ToString("dd-MMM-yy"));
		}

		public void TestCFSName()
		{
			AssertEquals("CFSName", PackUnpackWrapper.CurrentCompany.Name, PackUnpackWrapper.CFSName);
		}

		public void TestVessel()
		{
			AssertEquals("Vessel", ZString.Empty, PackUnpackWrapper.Vessel);

			CreateSailing();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			AssertEquals("Vessel", vessel.RV_Code, PackUnpackWrapper.Vessel);
		}

		public void TestVesselWhenNotLinked()
		{
			AssertEquals("Vessel", ZString.Empty, PackUnpackWrapper.Vessel);

			var consol = Factory.New<CFSLoadListConsol>();
			CreateSailing();

			RefVessel vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Voyage.JV_RV_NKVessel = vessel.RV_FK;

			Transport transport = Factory.LoadTop1<Transport>(new ZQuery());
			transport.JW_JX = PackUnpackContainer.JC_JX;
			transport.JW_IsLinked = false;

			consol.Containers.Add(PackUnpackContainer);

			AssertEquals("Vessel", vessel.RV_Code, PackUnpackWrapper.Vessel);
		}

		public void TestSailingVoyage()
		{
			AssertEquals("Voyage", ZString.Empty, PackUnpackWrapper.SailingVoyage);

			CreateSailing();
			Voyage.JV_VoyageFlight = "Voyage";
			AssertEquals("Voyage", "Voyage", PackUnpackWrapper.SailingVoyage);
		}

		public void TestDeliveryInstructions()
		{
			AssertEquals("", PackUnpackWrapper.DeliveryInstructions);
			StmNote note = PackUnpackContainer.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note.ST_NoteDataAsText = "Delivery Instructions.\nInstructions are to be followed prompty.\n";
			AssertEquals("Delivery Instructions.\nInstructions are to be followed prompty.", PackUnpackWrapper.DeliveryInstructions);
		}

		public void TestECNAndDeliveryInstructions()
		{
			AssertEquals("", PackUnpackWrapper.ECNAndDeliveryInstructions);
			StmNote note = PackUnpackContainer.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note.ST_NoteDataAsText = "Testing delivery instructions.\nSecond line here.\nThird line here.\n";
			AssertEquals("Testing delivery instructions.\nSecond line here.\nThird line here.", PackUnpackWrapper.ECNAndDeliveryInstructions);

			PackUnpackContainer.JC_ExportDepotCustomsReference = "TESTING ECN123";
			AssertEquals("ECN: TESTING ECN123\n\nTesting delivery instructions.\nSecond line here.\nThird line here.", PackUnpackWrapper.ECNAndDeliveryInstructions);
		}

		public void TestArrivalTruckDriversLicense()
		{
			PackUnpackContainer.JC_ArrivalTruckDriversLicense = ZString.Empty;
			AssertEquals("ArrivalTruckDriversLicense", ZString.Empty, PackUnpackWrapper.ArrivalTruckDriversLicense);

			PackUnpackContainer.JC_ArrivalTruckDriversLicense = "ARR License";
			AssertEquals("ArrivalTruckDriversLicense", "ARR License", PackUnpackWrapper.ArrivalTruckDriversLicense);
		}

		public void TestDepartureTruckDriversLicense()
		{
			PackUnpackContainer.JC_DepartureTruckDriversLicense = ZString.Empty;
			AssertEquals("DepartureTruckDriversLicense", ZString.Empty, PackUnpackWrapper.DepartureTruckDriversLicense);

			PackUnpackContainer.JC_DepartureTruckDriversLicense = "DEP License";
			AssertEquals("DepartureTruckDriversLicense", "DEP License", PackUnpackWrapper.DepartureTruckDriversLicense);
		}

		#endregion

		#region Implementation

		CFSContainer PackUnpackContainer;
		DocPackUnpackContainerRego PackUnpackWrapper;
		JobSailing Sailing;
		JobVoyage Voyage;
		VoyageOrigin Origin;
		VoyageDestination Destination;
		CFSLoadListConsol LoadList;
		CFSShipment ShipReceival;
		CFSPackLine PackLine;

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			PackUnpackContainer = Factory.New<CFSContainer>();
			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);

			base.SetUp();
		}

		void AddContainerToLoadListConsolAndShipment()
		{
			LoadList = Factory.New<CFSLoadListConsol>();
			LoadList.Containers.Add(PackUnpackContainer);
			ShipReceival = LoadList.Shipments.AddNew();

			PackLine = ShipReceival.OuterPackLines.AddNew();
			PackLine.SetContainer(LoadList, PackUnpackContainer);
			PackUnpackWrapper = DocPackUnpackContainerRego.New(PackUnpackContainer, Factory);
		}

		void CreateSailing()
		{
			Sailing = Factory.New<JobSailing>();
			Voyage = Factory.New<JobVoyage>();
			Origin = Factory.New<VoyageOrigin>();
			Destination = Factory.New<VoyageDestination>();

			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;
			Origin.JA_JV = Voyage.PK;
			Destination.JB_JV = Voyage.PK;
			PackUnpackContainer.JC_JX = Sailing.PK;
		}

		#endregion
	}
}
