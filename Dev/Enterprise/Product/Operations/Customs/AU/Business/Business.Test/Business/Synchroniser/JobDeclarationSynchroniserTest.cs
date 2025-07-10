using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobDeclarationSynchroniserTest : TestCaseWithFactory
	{
		public void TestHookConsolContainerModeFAS()
		{
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "KRANY";
			shipment.JS_RL_NKDestination = localPort;
			shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = localPort;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "KRANY";
			consol1.JK_RL_NKDischargePort = "JPHIU";
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol1.JK_ConsolMode = Core.Constants.ContainerModes.Loose;

			var firstInternationalLegTransport = consol1.Transports[0];
			firstInternationalLegTransport.JW_RL_NKLoadPort = "KRANY";
			firstInternationalLegTransport.JW_RL_NKDiscPort = "JPHIU";
			firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
			firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
			firstInternationalLegTransport.JW_LegOrder = 1;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "JPHIU";
			consol2.JK_RL_NKDischargePort = localPort;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var secondInternationalLegTransport = consol2.Transports[0];
			secondInternationalLegTransport.JW_RL_NKLoadPort = "JPHIU";
			secondInternationalLegTransport.JW_RL_NKDiscPort = localPort;
			secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(2);
			secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(3);
			secondInternationalLegTransport.JW_LegOrder = 1;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKPortOfLoading = "";
			declaration.JE_RL_NKPortOfArrival = "";

			var decSynchroniser = declaration.ShipmentSynchroniser;
			AssertType<JobDeclarationSynchroniser>(decSynchroniser);
			decSynchroniser.Synchronise(true);

			AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", "JPHIU", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", localPort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals(Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.FCL, declaration.JE_ContainerMode);

			consol2.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("JK_ConsolMode has value changed event hooked up", Core.Constants.ContainerModes.LCL, declaration.JE_ContainerMode);

			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Consol resets mode when transport changes", Core.Constants.ContainerModes.Loose, consol2.JK_ConsolMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Shipment Packing Mode set from 1st Consol by Shipment Synchroniser", Core.Constants.ContainerModes.Loose, shipment.JS_PackingMode);
			AssertEquals(Core.Constants.TransportModes.Air, declaration.JE_TransportMode);
			AssertEquals("Reverts to HookContainerMode value", Core.Constants.ContainerModes.AIR, declaration.JE_ContainerMode);

			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Consol resets mode when transport changes", Core.Constants.ContainerModes.FCL, consol2.JK_ConsolMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertEquals("Shipment Packing Mode", Core.Constants.ContainerModes.Loose, shipment.JS_PackingMode);
			AssertEquals(Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
			AssertEquals("Back to Consol Mode", Core.Constants.ContainerModes.FCL, declaration.JE_ContainerMode);
		}

		public void TestHookConsolContainerModeFSA()
		{
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "KRANY";
			shipment.JS_RL_NKDestination = localPort;
			shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = localPort;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "KRANY";
			consol1.JK_RL_NKDischargePort = "JPHIU";
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var firstInternationalLegTransport = consol1.Transports[0];
			firstInternationalLegTransport.JW_RL_NKLoadPort = "KRANY";
			firstInternationalLegTransport.JW_RL_NKDiscPort = "JPHIU";
			firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
			firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
			firstInternationalLegTransport.JW_LegOrder = 1;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "JPHIU";
			consol2.JK_RL_NKDischargePort = localPort;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_ConsolMode = Core.Constants.ContainerModes.Loose;

			var secondInternationalLegTransport = consol2.Transports[0];
			secondInternationalLegTransport.JW_RL_NKLoadPort = "JPHIU";
			secondInternationalLegTransport.JW_RL_NKDiscPort = localPort;
			secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(2);
			secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(3);
			secondInternationalLegTransport.JW_LegOrder = 1;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKPortOfLoading = "";
			declaration.JE_RL_NKPortOfArrival = "";

			var decSynchroniser = declaration.ShipmentSynchroniser;
			AssertType<JobDeclarationSynchroniser>(decSynchroniser);
			decSynchroniser.Synchronise(true);

			AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", "JPHIU", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", localPort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals(Core.Constants.TransportModes.Air, declaration.JE_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.AIR, declaration.JE_ContainerMode);

			consol2.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("JK_ConsolMode has value changed event hooked up", Core.Constants.ContainerModes.AIR, declaration.JE_ContainerMode);

			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Consol resets mode when transport changes", Core.Constants.ContainerModes.FCL, consol2.JK_ConsolMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Shipment Packing Mode set from Consol by Shipment Synchroniser", Core.Constants.ContainerModes.FCL, shipment.JS_PackingMode);
			AssertEquals("Shipment Packing Mode applied to declaration", Core.Constants.ContainerModes.FCL, declaration.JE_ContainerMode);

			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Consol resets mode when transport changes", Core.Constants.ContainerModes.Loose, consol2.JK_ConsolMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertEquals("Shipment Packing Mode set from 2nd Consol by Shipment Synchroniser", Core.Constants.ContainerModes.LCL, shipment.JS_PackingMode);
			AssertEquals("Back to 2nd Consol Mode", Core.Constants.ContainerModes.AIR, declaration.JE_ContainerMode);
		}

		public void TestHookConsolContainerModeSingleConsolWithMultipleLegs_Air()
		{
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "KRANY";
			shipment.JS_RL_NKDestination = localPort;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = localPort;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "KRANY";
			consol1.JK_RL_NKDischargePort = localPort;
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol1.JK_ConsolMode = Core.Constants.ContainerModes.Loose;

			var firstInternationalLegTransport = consol1.Transports[0];
			firstInternationalLegTransport.JW_RL_NKLoadPort = "KRANY";
			firstInternationalLegTransport.JW_RL_NKDiscPort = "JPHIU";
			firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
			firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
			firstInternationalLegTransport.JW_LegOrder = 1;

			var secondInternationalLegTransport = consol1.Transports.AddNew();
			secondInternationalLegTransport.JW_RL_NKLoadPort = "JPHIU";
			secondInternationalLegTransport.JW_RL_NKDiscPort = localPort;
			secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(2);
			secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(3);
			secondInternationalLegTransport.JW_LegOrder = 2;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKPortOfLoading = "";
			declaration.JE_RL_NKPortOfArrival = "";

			var decSynchroniser = declaration.ShipmentSynchroniser;
			AssertType<JobDeclarationSynchroniser>(decSynchroniser);
			decSynchroniser.Synchronise(true);

			AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", "KRANY", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", localPort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals(Core.Constants.TransportModes.Air, declaration.JE_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.AIR, declaration.JE_ContainerMode);
		}

		public void TestHookConsolContainerModeSingleConsolWithMultipleLegs_Sea()
		{
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "KRANY";
			shipment.JS_RL_NKDestination = localPort;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = localPort;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "KRANY";
			consol1.JK_RL_NKDischargePort = localPort;
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var firstInternationalLegTransport = consol1.Transports[0];
			firstInternationalLegTransport.JW_RL_NKLoadPort = "KRANY";
			firstInternationalLegTransport.JW_RL_NKDiscPort = "JPHIU";
			firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
			firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
			firstInternationalLegTransport.JW_LegOrder = 1;

			var secondInternationalLegTransport = consol1.Transports.AddNew();
			secondInternationalLegTransport.JW_RL_NKLoadPort = "JPHIU";
			secondInternationalLegTransport.JW_RL_NKDiscPort = localPort;
			secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(2);
			secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(3);
			secondInternationalLegTransport.JW_LegOrder = 2;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKPortOfLoading = "";
			declaration.JE_RL_NKPortOfArrival = "";

			var decSynchroniser = declaration.ShipmentSynchroniser;
			AssertType<JobDeclarationSynchroniser>(decSynchroniser);
			decSynchroniser.Synchronise(true);

			AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", "KRANY", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", localPort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals(Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.FCL, declaration.JE_ContainerMode);
		}

		public void TestLoadAndDischargeSyncroniseToCorrectTransport_Import()
		{
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRANY";
			consol.JK_RL_NKDischargePort = localPort;

			var firstInternationalLegTransport = consol.Transports[0];
			firstInternationalLegTransport.JW_RL_NKLoadPort = "KRANY";
			firstInternationalLegTransport.JW_RL_NKDiscPort = "JPHIU";
			firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
			firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
			firstInternationalLegTransport.JW_LegOrder = 1;

			var secondInternationalLegTransport = consol.Transports.AddNew();
			secondInternationalLegTransport.JW_RL_NKLoadPort = "JPHIU";
			secondInternationalLegTransport.JW_RL_NKDiscPort = localPort;
			secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(7);
			secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(8);
			secondInternationalLegTransport.JW_LegOrder = 2;

			var shipment = consol.Shipments.AddNew();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = localPort;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKPortOfLoading = "";
			declaration.JE_RL_NKPortOfArrival = "";

			var decSynchroniser = declaration.ShipmentSynchroniser;
			AssertType<JobDeclarationSynchroniser>(decSynchroniser);
			decSynchroniser.Synchronise(true);

			AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", "KRANY", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", localPort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("JE_ExportDate is that of the first international leg's departure", ZDateTime.BrettsBirthday, declaration.JE_ExportDate);
			AssertEquals("JE_DateOfArrival is that of the last international leg's arrival", ZDateTime.BrettsBirthday.AddDays(8), declaration.JE_DateOfArrival);

			var preCarriageTransport = consol.Transports.AddNew();
			preCarriageTransport.JW_RL_NKLoadPort = "KRXXX";
			preCarriageTransport.JW_RL_NKDiscPort = "KRANY";
			preCarriageTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(-5);
			preCarriageTransport.JW_LegOrder = 0;

			var postCarriageTransport = consol.Transports.AddNew();
			postCarriageTransport.JW_RL_NKLoadPort = localPort;
			postCarriageTransport.JW_RL_NKDiscPort = localPort.Left(2) + "YYY";
			postCarriageTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(10);
			postCarriageTransport.JW_LegOrder = 3;

			decSynchroniser.Synchronise(true);
			AssertEquals("Declaration.JE_RL_NKPortOfLoading is not the load of the new first leg (the domestic leg). DIFFERENT TO BASE.", "KRXXX", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Declaration.JE_RL_NKPortOfArrival - ditto", localPort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("JE_ExportDate is that of the domestic leg's departure. DIFFERENT TO BASE.", ZDateTime.BrettsBirthday.AddDays(-5), declaration.JE_ExportDate);
			AssertEquals("JE_DateOfArrival is that of the last international leg's arrival (still)", ZDateTime.BrettsBirthday.AddDays(8), declaration.JE_DateOfArrival);
		}

		public void TestContainersAreSynchronisedWhenShipmentTypeChanges()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08112345678";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "ULD1";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "ULD2";
			container2.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HB1";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKOrigin = "KRBUS";
			shipment.Consols.Add(consol);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.CurrentConsol = consol;
			packLine.JL_Calc_ContainerNumber = "ULD1";
			packLine.JL_PackageCount = 1;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.CurrentConsol = consol;
			packLine.JL_Calc_ContainerNumber = "ULD2";
			packLine.JL_PackageCount = 1;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.CurrentConsol = consol;
			packLine.JL_Calc_ContainerNumber = "ULD1";
			packLine.JL_PackageCount = 1;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
			AssertEquals("For this AIR job, CusContainers are deleted", 0, declaration.CusContainers.Count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("For this Quarantine AIR job, CusContainers are NOT deleted", 2, declaration.CusContainers.Count);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
			AssertEquals("For this Quarantine AIR job, CusContainers are NOT deleted on saving", 2, declaration.CusContainers.Count);
		}

		public void TestPackingSynchroniser()
		{
			var declaration = JobDeclaration.New(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			var decSynchroniser = new JobDeclarationSynchroniserForTest(declaration);
			AssertEquals(typeof(PackingSynchroniser), decSynchroniser.GetPackingSynchroniserExtend().GetType());
		}

		public void TestPortOfLoadingAndDateOfArrivalForImport()
		{
			var consol = Factory.New<ForwardingConsol>();
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_E_ARV = new ZDateTime(2007, 06, 30);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);

			var decSynchroniser = new JobDeclarationSynchroniserForTest(declaration);
			decSynchroniser.Consol = consol;
			decSynchroniser.SetEnabled(true, false);

			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(new ZDateTime(2007, 06, 30), declaration.JE_DateOfArrival);

			consol.JK_RL_NKLoadPort = "CASYD";
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_ETD = new ZDateTime(2007, 07, 01);
			transport1.JW_ATA = new ZDateTime(2007, 07, 03);
			transport1.JW_ETA = new ZDateTime(2007, 07, 04);
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_ATD = new ZDateTime(2007, 07, 05);
			transport2.JW_ETD = new ZDateTime(2007, 07, 06);
			transport2.JW_ATA = new ZDateTime(2007, 07, 07);
			transport2.JW_ETA = new ZDateTime(2007, 07, 08);
			transport2.JW_RL_NKDiscPort = "AUSYD";
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("USLAX", declaration.JE_RL_NKPortOfLoading);
			AssertEquals(new ZDateTime(2007, 07, 01), declaration.JE_ExportDate);
			transport1.JW_ATD = new ZDateTime(2007, 07, 02);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(new ZDateTime(2007, 07, 02), declaration.JE_ExportDate);
			AssertEquals(new ZDateTime(2007, 07, 07), declaration.JE_DateOfArrival);
			var transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "AUSYD";
			transport3.JW_ATD = new ZDateTime(2007, 07, 09);
			transport3.JW_ETD = new ZDateTime(2007, 07, 10);
			transport3.JW_ATA = new ZDateTime(2007, 07, 11);
			transport3.JW_ETA = new ZDateTime(2007, 07, 12);
			transport3.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_LegOrder = 1;
			transport2.JW_LegOrder = 2;
			transport3.JW_LegOrder = 3;
			transport2.JW_ATA = ZDateTime.Empty;
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(new ZDateTime(2007, 07, 8), declaration.JE_DateOfArrival);
		}

		public void TestFCLUnits()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "C1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "C2";
			Factory.Save();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Shipment1";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			shipment.JS_F3_NKPackType = "123";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 12;
			packLine1.JL_F3_NKPackType = "AA";
			packLine1.JL_JC = container1.PK;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 14;
			packLine2.JL_F3_NKPackType = "BB";
			packLine2.JL_JC = container2.PK;

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;

			var decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.SetEnabled(true, false);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Units set to number of containers-FCL", 2, declaration.JE_TotalNoOfPieces);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			declaration.JE_TotalNoOfPieces = 0;
			var decSynchroniser2 = new JobDeclarationSynchroniser(declaration);
			decSynchroniser2.SetEnabled(true, false);
			decSynchroniser2.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Only for FCL and FCX", 0, declaration.JE_TotalNoOfPieces);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCLMixedShipper;
			declaration.JE_TotalNoOfPieces = 0;
			var decSynchroniser3 = new JobDeclarationSynchroniser(declaration);
			decSynchroniser3.SetEnabled(true, false);
			decSynchroniser3.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Units set to number of containers-FCX", 2, declaration.JE_TotalNoOfPieces);
		}

		public void TestContainerCount()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "C1";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Shipment1";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			shipment.JS_F3_NKPackType = "123";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 12;
			packLine1.JL_F3_NKPackType = "AA";
			packLine1.JL_JC = container1.PK;

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_JS = shipment.PK;

			var decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.SetEnabled(true, false);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("1 container for FCL", (ZInt)1, declaration.JE_ContainerCount);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;

			AssertEquals("now 0 containers", ZInt.Zero, declaration.JE_ContainerCount);
		}

		public void TestSyncDataWhenDeclarationIsAir()
		{
			var consol = Factory.New<ForwardingConsol>();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "C1";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKLoadPort = "SGSIN";
			shipment.JS_RL_NKDischargePort = "AUSYD";
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUSYD";

			shipment.JS_TransportMode = Core.Constants.TransportModes.Courier;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.OnBoardCourier;

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_Vessel = "QA001";

			Factory.Save();

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;

			var decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.SetEnabled(true, false);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			CombineAssertions(@"In AU, a declaration always clear these values of Vessel, Container Count and Container Mode when it's saving.
The JobDeclarationSynchroniser should matched this rule for prevent some unnecessary data changes.", () =>
			{
				AssertEquals("Pre-Condition", Core.Constants.TransportModes.Air, declaration.JE_TransportMode);
				AssertEquals("Vessel is not used for Air.", string.Empty, declaration.JE_VesselName);
				AssertEquals("Container Count is not used for Air.", ZShort.Zero, declaration.JE_ContainerCount);
				AssertEquals("Container Mode is not used for Air.", Core.Constants.ContainerModes.AIR, declaration.JE_ContainerMode);
			});
		}

		sealed class JobDeclarationSynchroniserForTest : JobDeclarationSynchroniser
		{
			public JobDeclarationSynchroniserForTest(JobDeclaration destination)
				: base(destination)
			{
			}

			internal Customs.Business.PackingSynchroniser GetPackingSynchroniserExtend() => GetPackingSynchroniser();

			internal ForwardingConsol Consol
			{
				get => hookedConsol;
				set => hookedConsol = value;
			}
		}
	}
}
