using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoReportWorkflowTestCase : TestCaseWithFactory
	{
		public void TestScheduledCargoReportDateReturnsEmptyDateWhenThereIsAnSqlException()
		{
			var cargoReportWorkflow = new CargoReportWorkflowThatThrowsSqlExceptions(Factory);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			AssertEquals("cargoReportWorkflow.ScheduledCargoReportDate(shipment)", ZDateTime.Empty, cargoReportWorkflow.ScheduledCargoReportDate(shipment));
		}

		public void TestScheduledCargoReportDateWhenNoETAAvailable()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "G1";
			CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AUCustomsDataRegistry.Instance.AirMandatoryLatestCargoReportingTimeframe.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_HouseBill = "H1234";
			AssertEquals((ZInt)0, shipment1.WorkflowItems.Milestones.Count);
			cargoReportWorkflow.ProcessOneImportShipment(shipment1);
			AssertEquals("A new milestone created", (ZInt)1, shipment1.WorkflowItems.Milestones.Count);
			ProcessTask mileStone = shipment1.WorkflowItems.Milestones[0];
			AssertEquals("No date on milestone", ZDateTime.Empty, mileStone.P9_ScheduledDate.ToZDateTime());
		}

		public void TestScheduledCargoReportDateForAir()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "G1";
			CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AUCustomsDataRegistry.Instance.AirMandatoryLatestCargoReportingTimeframe.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);
			ZDateTime testingETA1 = new ZDateTime(2008, 1, 15, 1, 2, 0);
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_E_ARV = testingETA1;
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_HouseBill = "H1234";
			ProcessTask mileStone = shipment1.WorkflowItems.Milestones.AddNew();
			mileStone.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code;
			mileStone.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2007, 12, 31, 2, 2, 0)));
			ZInt exstingMilestoneCount = shipment1.WorkflowItems.Milestones.Count;
			cargoReportWorkflow.ProcessOneImportShipment(shipment1);
			AssertEquals("No new milestone", exstingMilestoneCount, shipment1.WorkflowItems.Milestones.Count);
			AssertEquals("Date updated", testingETA1.AddHours(-36), mileStone.P9_ScheduledDate.ToZDateTime());
		}

		public void TestScheduledCargoReportDateForSea()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "G1";
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AUCustomsDataRegistry.Instance.SeaMandatoryLatestCargoReportingTimeframe.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 36);

			ZDateTime testingETA1 = new ZDateTime(2008, 1, 15, 1, 2, 0);
			ZDateTime testingETA2 = new ZDateTime(2008, 1, 16, 1, 2, 0);
			ZDateTime testingETA3 = new ZDateTime(2008, 1, 17, 1, 2, 0);
			ZDateTime testingETA4 = new ZDateTime(2008, 1, 18, 1, 2, 0);
			ZDateTime testingETA5 = new ZDateTime(2008, 1, 19, 1, 2, 0);
			ZDateTime testingETA6 = new ZDateTime(2008, 1, 20, 1, 2, 0);
			ZDateTime testingETA7 = new ZDateTime(2008, 1, 21, 1, 2, 0);
			ZString testingLloyds = "1111111";
			ZString testingVoyage = "123";
			ZString otherLloyds = "2222222";
			ZString otherVoyage = "444";

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_E_ARV = testingETA6;
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment1.JS_HouseBill = "H1234";
			shipment1.JS_RL_NKDestination = "AUPTH";
			ForwardingConsol consol = shipment1.Consols.AddNew();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKPortOfFirstArrival = "AUBNE";
			consol.JK_DatePortOfFirstArrival = testingETA1;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "OBLTEST";
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_VoyageFlight = testingVoyage;
			RefVessel testVessel = RefVessel.New(Factory);
			testVessel.RV_Code = testingLloyds;
			transport.JW_Vessel = testVessel.RV_Code;
			consol.Vessel.RV_LloydsNumber = testingLloyds;
			ProcessTask mileStone = shipment1.WorkflowItems.Milestones.AddNew();
			mileStone.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code;
			mileStone.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(testingETA7));
			ZInt exstingMilestoneCount = shipment1.WorkflowItems.Milestones.Count;
			cargoReportWorkflow.ProcessOneImportShipment(shipment1);
			AssertEquals("No new milestone", exstingMilestoneCount, shipment1.WorkflowItems.Milestones.Count);
			AssertEquals("Date updated-no impending arrival", testingETA1.AddHours(-84), mileStone.P9_ScheduledDate.ToZDateTime());

			CMRSeaImpendingArrivals impendingArrival1 = Factory.New<CMRSeaImpendingArrivals>();
			impendingArrival1.SI_VesselID = testingLloyds;
			impendingArrival1.SI_VoyageNumber = testingVoyage;
			impendingArrival1.SI_OriginalFirstPortCode = "AUADL";
			impendingArrival1.SI_OriginalETA = testingETA2;
			CMRSeaImpendingArrivals impendingArrival2 = Factory.New<CMRSeaImpendingArrivals>();
			impendingArrival2.SI_VesselID = otherLloyds;
			impendingArrival2.SI_VoyageNumber = testingVoyage;
			impendingArrival2.SI_OriginalFirstPortCode = "AUADL";
			impendingArrival2.SI_OriginalETA = testingETA3;
			CMRSeaImpendingArrivals impendingArrival3 = Factory.New<CMRSeaImpendingArrivals>();
			impendingArrival3.SI_VesselID = testingLloyds;
			impendingArrival3.SI_VoyageNumber = otherVoyage;
			impendingArrival3.SI_OriginalFirstPortCode = "AUADL";
			impendingArrival3.SI_OriginalETA = testingETA4;
			Factory.Save();

			cargoReportWorkflow.ProcessOneImportShipment(shipment1);
			shipment1.Reload();
			AssertEquals("No new milestone", exstingMilestoneCount, shipment1.WorkflowItems.Milestones.Count);
			AssertEquals("Date updated-correct impending arrival", testingETA2.AddHours(-84), mileStone.P9_ScheduledDate.ToZDateTime());

			CMRSeaImpendingArrivals impendingArrival4 = Factory.New<CMRSeaImpendingArrivals>();
			impendingArrival4.SI_VesselID = testingLloyds;
			impendingArrival4.SI_VoyageNumber = testingVoyage;
			impendingArrival4.SI_OriginalFirstPortCode = "AUBNE";
			impendingArrival4.SI_OriginalETA = testingETA5;

			cargoReportWorkflow.ProcessOneImportShipment(shipment1);
			AssertEquals("No new milestone", exstingMilestoneCount, shipment1.WorkflowItems.Milestones.Count);
			AssertEquals("Date updated-correct impending arrival with first port", testingETA5.AddHours(-84), mileStone.P9_ScheduledDate.ToZDateTime());
		}

		[TestDate(2008, 01, 19, 11, 11, 11)]
		public void TestDoDailyProcessing()
		{
			// initialize the wrapped property cache first
			new BusinessObjectFactory().New<CMRSeaImpendingArrivals>();

			AUCustomsDataRegistry.Instance.SeaMandatoryLatestCargoReportingTimeframe.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 24);
			RefVessel testVessel = RefVessel.New(Factory);
			testVessel.RV_Code = "VESSEL NAME";
			testVessel.RV_LloydsNumber = "1111111";

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00000001";
			ZDateTime testingETA1 = new ZDateTime(2008, 1, 15, 1, 2, 0);
			SetupConsole(shipment1, testingETA1, "AUSYD");
			ProcessTask mileStone1 = shipment1.WorkflowItems.Milestones.AddNew();
			mileStone1.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code;
			ProcessTask mileStoneException1 = mileStone1.CreateMilestoneException();
			AssertEquals(((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent.Code, mileStoneException1.TriggerConditions.TriggerEventCode);
			mileStoneException1.IsExceptionActioned = true;

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00000002";
			ZDateTime testingETA2 = new ZDateTime(2008, 1, 16, 1, 2, 0);
			SetupConsole(shipment2, testingETA2, "AUMEL");
			ProcessTask mileStone2 = shipment2.WorkflowItems.Milestones.AddNew();
			mileStone2.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment2).CargoReportAcceptedEvent.Code;
			ProcessTask mileStoneException2 = mileStone2.CreateMilestoneException();
			AssertEquals(((IParentForCargoReporter)shipment2).CargoReportAcceptedEvent.Code, mileStoneException2.TriggerConditions.TriggerEventCode);

			ForwardingShipment shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S00000003";
			ZDateTime testingETA3 = new ZDateTime(2008, 1, 21, 1, 2, 0);
			SetupConsole(shipment3, testingETA3, "AUSYD");
			ProcessTask mileStone3 = shipment3.WorkflowItems.Milestones.AddNew();
			mileStone3.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment3).CargoReportAcceptedEvent.Code;
			ProcessTask mileStoneException3 = mileStone3.CreateMilestoneException();
			AssertEquals(((IParentForCargoReporter)shipment3).CargoReportAcceptedEvent.Code, mileStoneException3.TriggerConditions.TriggerEventCode);

			ForwardingShipment shipment4 = Factory.New<ForwardingShipment>();
			shipment4.JS_UniqueConsignRef = "S00000004";
			ZDateTime testingETA4 = new ZDateTime(2008, 1, 23, 1, 2, 0);
			SetupConsole(shipment4, testingETA4, "AUSYD");
			ProcessTask mileStone4 = shipment4.WorkflowItems.Milestones.AddNew();
			mileStone4.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment4).CargoReportAcceptedEvent.Code;

			ForwardingShipment shipment5 = Factory.New<ForwardingShipment>();
			shipment5.JS_UniqueConsignRef = "S00000005";
			SetupConsole(shipment5, testingETA2, "AUSYD");
			ProcessTask mileStone5 = shipment5.WorkflowItems.Milestones.AddNew();
			mileStone5.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment5).CargoReportAcceptedEvent.Code;
			ProcessTask mileStoneException5 = mileStone5.CreateMilestoneException();
			AssertEquals(((IParentForCargoReporter)shipment5).CargoReportAcceptedEvent.Code, mileStoneException5.TriggerConditions.TriggerEventCode);
			mileStoneException5.IsExceptionActioned = true;
			CusSCAOceanBill oceanBill5 = Factory.New<CusSCAOceanBill>();
			CusSCAHouse cusSCAHouse5 = oceanBill5.HouseBills.AddNew();
			cusSCAHouse5.CA_JS = shipment5.PK;
			cusSCAHouse5.CA_MessageStatus = "WTO";

			ForwardingShipment shipment6 = Factory.New<ForwardingShipment>();
			shipment6.JS_UniqueConsignRef = "S00000006";
			SetupConsole(shipment6, testingETA2, "AUSYD");
			ProcessTask mileStone6 = shipment6.WorkflowItems.Milestones.AddNew();
			mileStone6.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment6).CargoReportAcceptedEvent.Code;
			ProcessTask mileStoneException6 = mileStone6.CreateMilestoneException();
			AssertEquals(((IParentForCargoReporter)shipment6).CargoReportAcceptedEvent.Code, mileStoneException6.TriggerConditions.TriggerEventCode);
			mileStoneException6.IsExceptionActioned = true;
			CusSCAOceanBill oceanBill6 = Factory.New<CusSCAOceanBill>();
			CusSCAHouse cusSCAHouse6 = oceanBill6.HouseBills.AddNew();
			cusSCAHouse6.CA_JS = shipment6.PK;
			cusSCAHouse6.CA_MessageStatus = "RJO";

			ForwardingShipment shipment7 = Factory.New<ForwardingShipment>();
			shipment7.JS_UniqueConsignRef = "S00000007";
			SetupConsole(shipment7, testingETA3, "AUSYD");
			ProcessTask mileStone7 = shipment7.WorkflowItems.Milestones.AddNew();
			mileStone7.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment7).CargoReportAcceptedEvent.Code;
			ProcessTask mileStoneException7 = mileStone7.CreateMilestoneException();
			AssertEquals(((IParentForCargoReporter)shipment7).CargoReportAcceptedEvent.Code, mileStoneException7.TriggerConditions.TriggerEventCode);
			mileStoneException7.IsExceptionActioned = true;
			CusSCAOceanBill oceanBill7 = Factory.New<CusSCAOceanBill>();
			CusSCAHouse cusSCAHouse7 = oceanBill7.HouseBills.AddNew();
			cusSCAHouse7.CA_JS = shipment7.PK;
			cusSCAHouse7.CA_MessageStatus = "WTO";

			ForwardingShipment shipment8 = Factory.New<ForwardingShipment>();
			shipment8.JS_UniqueConsignRef = "S00000008";
			SetupConsole(shipment8, testingETA3, "AUSYD");
			ProcessTask mileStone8 = shipment8.WorkflowItems.Milestones.AddNew();
			mileStone8.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment8).CargoReportAcceptedEvent.Code;
			ProcessTask mileStoneException8 = mileStone8.CreateMilestoneException();
			AssertEquals(((IParentForCargoReporter)shipment8).CargoReportAcceptedEvent.Code, mileStoneException8.TriggerConditions.TriggerEventCode);
			mileStoneException8.IsExceptionActioned = true;
			CusSCAOceanBill oceanBill8 = Factory.New<CusSCAOceanBill>();
			CusSCAHouse cusSCAHouse8 = oceanBill8.HouseBills.AddNew();
			cusSCAHouse8.CA_JS = shipment8.PK;
			cusSCAHouse8.CA_MessageStatus = "RJO";

			ForwardingShipment shipment9 = Factory.New<ForwardingShipment>();
			shipment9.JS_UniqueConsignRef = "S00000009";
			SetupConsole(shipment9, testingETA1, "AUSYD");
			ProcessTask mileStone9 = shipment9.WorkflowItems.Milestones.AddNew();
			mileStone9.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment9).CargoReportAcceptedEvent.Code;
			ProcessTask mileStoneException9 = mileStone9.CreateMilestoneException();
			AssertEquals(((IParentForCargoReporter)shipment9).CargoReportAcceptedEvent.Code, mileStoneException9.TriggerConditions.TriggerEventCode);
			mileStoneException9.IsExceptionActioned = false;
			shipment9.Consols[0].Transports[0].JW_VoyageFlight = "222";

			ForwardingShipment shipment10 = Factory.New<ForwardingShipment>();
			shipment10.JS_UniqueConsignRef = "S00000010";
			SetupConsole(shipment10, testingETA2, "AUSYD");
			shipment10.JS_RL_NKOrigin = "AUHBT"; //domestic shipment
			shipment10.Consols[0].JK_RL_NKLoadPort = "AUHBT";
			ProcessTask mileStone10 = shipment10.WorkflowItems.Milestones.AddNew();
			mileStone10.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment10).CargoReportAcceptedEvent.Code;
			ProcessTask mileStoneException10 = mileStone10.CreateMilestoneException();
			AssertEquals(((IParentForCargoReporter)shipment10).CargoReportAcceptedEvent.Code, mileStoneException10.TriggerConditions.TriggerEventCode);
			Factory.Save();

			CustomsDataRegistry.Instance.CustomsIsRequiredForDomesticShipments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			cargoReportWorkflow.Logger = new LoggingInformation();
			cargoReportWorkflow.ExecuteBatch();
			mileStone1.Reload();
			mileStone2.Reload();
			mileStone3.Reload();
			mileStone4.Reload();
			mileStone5.Reload();
			mileStone6.Reload();
			mileStone7.Reload();
			mileStone8.Reload();
			AssertEquals("Shipment1 milestone date is reset", testingETA1.AddHours(-72), mileStone1.P9_ScheduledDate.ToZDateTime());
			AssertEquals("Shipment2 milestone date is reset", testingETA2.AddHours(-72), mileStone2.P9_ScheduledDate.ToZDateTime());
			AssertEquals("Shipment3 milestone date is reset", testingETA3.AddHours(-72), mileStone3.P9_ScheduledDate.ToZDateTime());
			AssertEquals("Shipment4 milestone date is reset", testingETA4.AddHours(-72), mileStone4.P9_ScheduledDate.ToZDateTime());
			AssertEquals("Shipment5 milestone date is reset", testingETA2.AddHours(-72), mileStone5.P9_ScheduledDate.ToZDateTime());
			AssertEquals("Shipment6 milestone date is reset", testingETA2.AddHours(-72), mileStone6.P9_ScheduledDate.ToZDateTime());
			AssertEquals("Shipment7 milestone date is reset", testingETA3.AddHours(-72), mileStone7.P9_ScheduledDate.ToZDateTime());
			AssertEquals("Shipment8 milestone date is reset", testingETA3.AddHours(-72), mileStone8.P9_ScheduledDate.ToZDateTime());
			AssertMultilineASCIIEquals("Email body", expectedExceptionsEmailBody.Replace("''", "\""), cargoReportWorkflow.cargoReportExceptionsEmailForTesting.Body);
			AssertEquals("Email subject", "Late and Pending Shipment Cargo Report Notifications", cargoReportWorkflow.cargoReportExceptionsEmailForTesting.Subject);

			CustomsDataRegistry.Instance.CustomsIsRequiredForDomesticShipments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			cargoReportWorkflow.Logger = new LoggingInformation();
			cargoReportWorkflow.ExecuteBatch();
			AssertMultilineASCIIEquals("Email body", expectedExceptionsEmailBody.Replace("''", "\""), cargoReportWorkflow.cargoReportExceptionsEmailForTesting.Body);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "G0";
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			cargoReportWorkflow = new CargoReportWorkflow(Factory);
			StmEvent cRAEvent = Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, Events.CargoReportAccepted.Code);
			cRAEvent.SE_AirExceptionSafetyMargin = 24;
			cRAEvent.SE_SeaExceptionSafetyMargin = 48;
			Factory.Save();
		}

		CargoReportWorkflow cargoReportWorkflow;
		int consoleCount = 1;

		void SetupConsole(ForwardingShipment shipment, ZDateTime testingETA, ZString firstPort)
		{
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "SGSIN";
			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKPortOfFirstArrival = firstPort;
			consol.JK_DatePortOfFirstArrival = testingETA;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			CMRSeaImpendingArrivals impendingArrival = Factory.New<CMRSeaImpendingArrivals>();
			impendingArrival.SI_VesselID = "1111111";
			impendingArrival.SI_VoyageNumber = "11" + consoleCount.ToString();
			impendingArrival.SI_OriginalFirstPortCode = firstPort;
			impendingArrival.SI_OriginalETA = testingETA;
			consol.JK_MasterBillNum = "OBL" + impendingArrival.SI_VoyageNumber;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_VoyageFlight = impendingArrival.SI_VoyageNumber;
			transport.JW_Vessel = "VESSEL NAME";
			consol.Vessel.RV_LloydsNumber = "1111111";
			consoleCount++;
		}

		readonly string expectedExceptionsEmailBody = string.Format(@"<html xmlns=''http://www.w3.org/1999/xhtml''>
<head>
  <title>Late and Pending Shipment Cargo Report Notifications</title>
  <style type=''text/css''>
  <!--
  {0}
  -->
</style>
</head>
<body>
  <table border=''0'' cellpadding=''0'' cellspacing=''0'' bgcolor=''#FFFFFF''>
    <tr>
      <td><img src=''cid:Banner.jpg'' alt=''Banner Image'' /></td>
    </tr>
    <tr>
      <td>
        <br />
        <strong>Late and Pending Shipment Cargo Report Notifications as at 19-Jan-08 11:11</strong>
        <br />
        <br />
        <hr />
        <br />
        The following is a list of shipments that appear to NOT have a matching Arrival Report in the Customs Supplied Impending Arrivals Reference File.<br />
        <br />
        <br />
        <table class=''table'' cellpadding=''1'' cellspacing=''0'' border=''1'' bgcolor=''#FFFFFF''>
          <tr class=''tableheadings''>
            <th>
              <b>&nbsp;Shipment&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Vessel/Voyage or Flight&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port ETA&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;CR Required By&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Comments&nbsp;</b>
            </th>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000009</a>&nbsp;</td>
            <td>&nbsp;VESSEL NAME/222&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;15-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;&nbsp;</td>
            <td>&nbsp;Impending arrivals for this vessel: 111/AUSYD/15-Jan-08 1110/AUSYD/16-Jan-08 112/AUMEL/16-Jan-08 113/AUSYD/21-Jan-08 114/AUSYD/23-Jan-08 115/AUSYD/16-Jan-08 116/AUSYD/16-Jan-08 117/AUSYD/21-Jan-08 118/AUSYD/21-Jan-08 119/AUSYD/15-Jan-08 &nbsp;</td>
          </tr>
        </table>
        <br />
        <hr />
        <br />
        The following is a list of shipments that appear to NOT have had a Cargo Report lodged and the due date for reporting has expired.<br />
        <br />
        <br />
        <table class=''table'' cellpadding=''1'' cellspacing=''0'' border=''1'' bgcolor=''#FFFFFF''>
          <tr class=''tableheadings''>
            <th>
              <b>&nbsp;Shipment&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Vessel/Voyage or Flight&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port ETA&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;CR Required By&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Comments&nbsp;</b>
            </th>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000002</a>&nbsp;</td>
            <td>&nbsp;VESSEL NAME/112&nbsp;</td>
            <td>&nbsp;AUMEL&nbsp;</td>
            <td>&nbsp;16-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;15-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;!!! LATE.&nbsp;</td>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000005</a>&nbsp;</td>
            <td>&nbsp;VESSEL NAME/115&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;16-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;15-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;!!! LATE, Waiting for response from Customs.&nbsp;</td>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000006</a>&nbsp;</td>
            <td>&nbsp;VESSEL NAME/116&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;16-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;15-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;!!! LATE, Original message rejected.&nbsp;</td>
          </tr>
        </table>
        <br />
        <hr />
        <br />
        The following is a list of shipments that appear to NOT have had a Cargo Report lodged and the due date for reporting is approaching.<br />
        <br />
        <br />
        <table class=''table'' cellpadding=''1'' cellspacing=''0'' border=''1'' bgcolor=''#FFFFFF''>
          <tr class=''tableheadings''>
            <th>
              <b>&nbsp;Shipment&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Vessel/Voyage or Flight&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port ETA&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;CR Required By&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Comments&nbsp;</b>
            </th>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000003</a>&nbsp;</td>
            <td>&nbsp;VESSEL NAME/113&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;21-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;20-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;DUE DATE APPROACHING.&nbsp;</td>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000007</a>&nbsp;</td>
            <td>&nbsp;VESSEL NAME/117&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;21-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;20-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;DUE DATE APPROACHING, Waiting for response from Customs.&nbsp;</td>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000008</a>&nbsp;</td>
            <td>&nbsp;VESSEL NAME/118&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;21-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;20-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;DUE DATE APPROACHING, Original message rejected.&nbsp;</td>
          </tr>
        </table>
        <br />
        <hr />
        <br />
        The Cargo Report Accepted exception, on the following shipments, has been actioned
        but the milestone has not been closed (the Cargo Report Accepted event has not occurred).
        This has probably been caused by manually setting the 'Actioned' tick box on the exception.
        Please check these shipments and, if required, close the milestone by setting the actual
        date on the milestone.<br />
        <br />
        <br />
        <table class=''table'' cellpadding=''1'' cellspacing=''0'' border=''1'' bgcolor=''#FFFFFF''>
          <tr class=''tableheadings''>
            <th>
              <b>&nbsp;Shipment&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Vessel/Voyage or Flight&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Shipment First Port ETA&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;CR Required By&nbsp;</b>
            </th>
            <th>
              <b>&nbsp;Comments&nbsp;</b>
            </th>
          </tr>
          <tr>
            <td>&nbsp;<a href=''(*ShipmentUrl*)''>S00000001</a>&nbsp;</td>
            <td>&nbsp;VESSEL NAME/111&nbsp;</td>
            <td>&nbsp;AUSYD&nbsp;</td>
            <td>&nbsp;15-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;14-Jan-08 01:02:00&nbsp;</td>
            <td>&nbsp;Exception manually actioned.&nbsp;</td>
          </tr>
        </table>
        <br />
        <hr />
        <br />
        Regards,<br />
        <br/>
        CargoWise One Shipment Cargo Report Notifications<br/>
       <br/>
      </td>
    </tr>
    <tr>
      <td><img src=''cid:Footer.jpg'' alt=''Footer Image'' /></td>
    </tr>
  </table>
</body>
</html>
", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);

		sealed class CargoReportWorkflowThatThrowsSqlExceptions : CargoReportWorkflow
		{
			internal CargoReportWorkflowThatThrowsSqlExceptions(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			internal new ZDateTime ScheduledCargoReportDate(ForwardingShipment shipment)
			{
				return base.ScheduledCargoReportDate(shipment);
			}

			protected override ZDateTime CustomsSuppliedImpendingArrivalDate(ForwardingShipment shipment)
			{
				DbConnection connection = ((IDbConnected)factory).Connection;
				using (DbCommand command = connection.Command("RAISERROR('Raise Error For Testing', 16, 1)"))
				{
					command.ExecuteNonQuery();
				}

				return ZDateTime.Now;
			}
		}
	}
}
