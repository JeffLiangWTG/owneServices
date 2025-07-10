using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

// These live in GUI cos we need a form in order to trigger them
namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	class AutoFrcTests : TestCaseWithFactory
	{
		public void TestDoNotKeepTemporaryFriMessagesIfRedStopOccursDuringGeneration()
		{
			var basic = GetBasic(Factory);
			basic.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			Factory.Save();
			basic.AirportOfArrival = "XXX";
			basic.CM_MAWB = "TSTCRASH";  // See CUSCARGeneratorFRI for why this is set
			MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB awb)
			{ return new CcsukAirInventoryForm(awb); });
			AssertEquals("A red error during generatiopn should result in the temporary FRIs being deleted", 1, basic.Messages.Count);
		}

		public void TestBasicForm()
		{
			var basic = GetBasic(Factory);
			basic.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			Factory.Save();
			basic.NumberOfPiecesExpected = 100; // trigger a change
			MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB awb)
			{ return new CcsukAirInventoryForm(awb); });
			AssertEquals(2, basic.Messages.Count);
			AssertEquals("FRC", basic.Messages[1].EM_MessageSubType);
			AssertNotContains("Common access reference placeholder should have been flipped out OK", "<<SYSCAR>>", basic.Messages[1].EM_MessageText);
			UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("111-22222222");
		}

		public void TestBasicFormWithIrrelevantChange()
		{
			var basic = GetBasic(Factory);
			basic.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			Factory.Save();
			basic.CM_MessageReference = "Any change"; // trigger a change BUT not one that is involved in the messaging
			MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB awb)
			{ return new CcsukAirInventoryForm(awb); });
			AssertEquals("A change is made, but it does not go into the message, so no amendment is needed", 1, basic.Messages.Count);
		}

		public void TestBasicInConsolPlugin()
		{
			var basic = GetBasic(Factory);
			var consol = CreateConsolGoodEnoughToSave(basic);
			basic.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			Factory.Save();
			basic.NumberOfPiecesExpected = 100; // trigger a change
			MakeFormAddPluginShowFormAndSaveForm(consol, ZArchitecture.Modules.ControllerIDs.Customs.GB.CcsukAirInventory);
			AssertEquals(2, basic.Messages.Count);
			AssertEquals("FRC", basic.Messages[1].EM_MessageSubType);
			UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("111-22222222");
		}

		public void TestBasicFormWithSplits()
		{
			var basic = GetBasic(Factory);
			SplitConsignment split01;
			SplitConsignment split03;
			AddSplits(basic.Splits, out split01, out split03);
			Factory.Save();
			basic.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			basic.NumberOfPiecesExpected = 100; // trigger a change
			split01.NumberOfPiecesExpected = 9;
			split03.NumberOfPiecesExpected = 9;
			MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB awb)
			{ return new CcsukAirInventoryForm(awb); });
			AssertSplitMessageResults(basic.Messages, "ACD::");
			UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("111-22222222/01");
			UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("111-22222222/03");
		}

		public void TestHouseForm()
		{
			var mawb = GetBasic(Factory);
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_PiecesManifested = 10;
			hawb.CS_HAWB = "DANIEL01";
			hawb.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			Factory.Save();
			hawb.CS_PiecesManifested = 100; // trigger a change
			MakeFormShowFormAndSaveForm(hawb, delegate(CusHAWB awb)
			{ return new CcsukAirInventoryFormHouse(awb); });
			AssertEquals(2, hawb.Messages.Count);
			AssertEquals("FRC", hawb.Messages[1].EM_MessageSubType);
			UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("111-22222222-DANIEL01");
		}

		public void TestHouseFormWithSplits()
		{
			var mawb = GetBasic(Factory);
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_PiecesManifested = 10;
			hawb.CS_HAWB = "DANIEL01";
			SplitConsignment split01;
			SplitConsignment split03;
			AddSplits(hawb.Splits, out split01, out split03);
			Factory.Save();
			hawb.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			hawb.CS_PiecesManifested = 100; // trigger a change
			split01.NumberOfPiecesExpected = 9;
			split03.NumberOfPiecesExpected = 9;
			MakeFormShowFormAndSaveForm(hawb, delegate(CusHAWB awb)
			{ return new CcsukAirInventoryFormHouse(awb); });
			AssertSplitMessageResults(hawb.Messages, "HWB:DANIEL01:");
			UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("111-22222222-DANIEL01/01");
			UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("111-22222222-DANIEL01/03");
		}

		public void TestHouseInShipmentPlugin()
		{
			var mawb = GetBasic(Factory);
			var consol = CreateConsolGoodEnoughToSave(mawb);
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignor = true;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			shipment.JS_RL_NKDestination = "GBLHR";
			var hawb = mawb.ChildBills.AddNew();
			hawb.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			hawb.CS_HAWB = "DANIEL01";
			shipment.JS_HouseBill = hawb.CS_HAWB;
			hawb.CS_JS = shipment.PK;
			Factory.Save();
			hawb.CS_PiecesManifested = 100; // trigger a change
			MakeFormAddPluginShowFormAndSaveForm(shipment, ZArchitecture.Modules.ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
			AssertEquals(2, hawb.Messages.Count);
			AssertEquals("FRC", hawb.Messages[1].EM_MessageSubType);
			UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("111-22222222-DANIEL01");
		}

		public void TestRegistryDisables()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowAutoFrc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var basic = GetBasic(Factory);
			basic.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			Factory.Save();
			basic.NumberOfPiecesExpected = 100; // trigger a change
			MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB awb)
			{ return new CcsukAirInventoryForm(awb); });
			AssertEquals("No new message", 1, basic.Messages.Count);
			AssertEquals("Saved OK", false, basic.HasChanges);
		}

		public void TestAgentDoesNothing()
		{
			var basic = GetBasic(Factory);
			basic.Profile = "CUKFFW98000LXA";
			basic.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			Factory.Save();
			basic.NumberOfPiecesExpected = 100; // trigger a change
			MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB awb)
			{ return new CcsukAirInventoryForm(awb); });
			AssertEquals("For agents, no new message", 1, basic.Messages.Count);
			AssertEquals("Saved OK", false, basic.HasChanges);
		}

		public void TestArchivedDoesNothing()
		{
			var basic = GetBasic(Factory);
			basic.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			basic.PresenceOnNetworkStatus = "ARC";
			Factory.Save();
			basic.NumberOfPiecesExpected = 100; // trigger a change
			MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB awb)
			{ return new CcsukAirInventoryForm(awb); });
			AssertEquals("For archived jobs, not auto frc", 1, basic.Messages.Count);
			AssertEquals("Saved OK", false, basic.HasChanges);
		}

		public void TestNoFrcWithoutFirstMessage()
		{
			var basic = GetBasic(Factory);
			Factory.Save();
			basic.NumberOfPiecesExpected = 100; // trigger a change

			MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB awb)
			{ return new CcsukAirInventoryForm(awb); });
			AssertEquals(0, basic.Messages.Count);
			basic.Messages.AddNew().EM_ReceiveTransmit = "RCV";
			Factory.Save();
			basic.NumberOfPiecesExpected = 200; // trigger a change			
			MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB awb)
			{ return new CcsukAirInventoryForm(awb); });
			AssertEquals(2, basic.Messages.Count);
		}

		public void TestNonPersistentSplitsOnlySendsFrcIfNprIsChanging()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);

			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.AirportOfArrival = "LHR";
			basic.Profile = "CUKAIR98LHRBAC";  //shed
			var ot = basic.OutTurns.AddNew();
			ot.C5_MarksAndNumbers = "Red";
			ot.C5_PackagesOutturned = basic.NumberOfPiecesExpected;  //15
			basic.Factory.Save();
			var controller = new NonPersistentSplitLineOrchestrator(basic);
			controller.GetNonPersistentSplitsAndFlightData();

			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);
			var npbo1 = controller.SplitsAndFlightData.SplitLines.AddNew();
			var npbo2 = controller.SplitsAndFlightData.SplitLines.AddNew();
			npbo1.NumberOfPieces = 10;
			npbo1.NumberOfPiecesReceived = 10;
			npbo1.HandlingDetail = "Blue";
			npbo1.WarehouseLocationID = locationBac1.PK;
			npbo1.Weight = 10m;
			npbo2.NumberOfPieces = 5;
			npbo2.NumberOfPiecesReceived = 5;
			npbo2.HandlingDetail = "Green";
			npbo2.Weight = 5m;
			npbo2.WarehouseLocationID = locationBac2.PK;

			// Make 2 splits
			var guiSender = new SendsMessagesToCustomsGUI();
			AssertEquals("Pre-req - no splits on local awb", 0, basic.Splits.Count);
			controller.PerformSplit_FCS(guiSender);
			AssertEquals("Only FCS message was created - NO FRC messages yet", 1, basic.Messages.Count);
			var msg = basic.Messages[0];
			AssertContains("EM_MessageText is edifact", "BGM+:::FCS+", msg.EM_MessageText);
			AssertEquals("Split count", 2, basic.Splits.Count);
			AssertEquals("Changes saved to database", true, basic.Splits[1].IsInDatabase);
			AssertEquals(2, basic.OutTurns.Count);
			AssertEquals("Blue", basic.OutTurns[0].C5_MarksAndNumbers);
			AssertEquals("Green", basic.OutTurns[1].C5_MarksAndNumbers);
			AssertEquals(10, basic.OutTurns[0].C5_PackagesOutturned);
			AssertEquals(5, basic.OutTurns[1].C5_PackagesOutturned);
			basic.Factory.Save();

			// Now re-distribute the splits.  FRCs will be sent because NPR is changing for them.
			controller = new NonPersistentSplitLineOrchestrator(basic);
			controller.GetNonPersistentSplitsAndFlightData();
			npbo1 = controller.SplitsAndFlightData.SplitLines[0];
			npbo2 = controller.SplitsAndFlightData.SplitLines[1];
			npbo1.NumberOfPieces = 9;
			npbo1.NumberOfPiecesReceived = 9;
			npbo2.NumberOfPieces = 6;
			npbo2.NumberOfPiecesReceived = 6;
			guiSender = new SendsMessagesToCustomsGUI();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			controller.PerformSplit_FCS(guiSender);
			AssertEquals("4 messages - original FCS, second FCS, plus an FRC for each split (none for parent)", 4, basic.Messages.Count);
			AssertEquals("FCS", basic.Messages[1].EM_MessageSubType);
			AssertEquals("FRC", basic.Messages[2].EM_MessageSubType);
			AssertEquals("FRC", basic.Messages[3].EM_MessageSubType);
			AssertContains("ACD::01", basic.Messages[2].EM_MessageText);
			AssertContains("ACD::02", basic.Messages[3].EM_MessageText);
			AssertEquals(2, basic.OutTurns.Count);
			basic.Factory.Save();

			// Reduce NPR all round
			basic.OutTurns[0].C5_PackagesOutturned = 1;
			basic.OutTurns[1].C5_PackagesOutturned = 1;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB b)
			{ return new CcsukAirInventoryForm(b); });
			basic.Factory.Save();
			AssertEquals("7 messages - 4, plus three new FRCs (two splits plus parent)", 7, basic.Messages.Count);

			// Re-arrange the splits, without changing NPR, should see only FCS and not FRC
			controller = new NonPersistentSplitLineOrchestrator(basic);
			controller.GetNonPersistentSplitsAndFlightData();
			npbo1 = controller.SplitsAndFlightData.SplitLines[0];
			npbo2 = controller.SplitsAndFlightData.SplitLines[1];
			AssertEquals(9, (ZInt)npbo1.NumberOfPiecesExpected);
			AssertEquals(1, npbo1.NumberOfPiecesReceived);
			AssertEquals(6, (ZInt)npbo2.NumberOfPiecesExpected);
			AssertEquals(1, npbo2.NumberOfPiecesReceived);
			npbo1.NumberOfPieces = 10;
			npbo2.NumberOfPieces = 5;
			controller.PerformSplit_FCS(guiSender);
			AssertEquals("One new message, FCS, no new FRCs", 8, basic.Messages.Count);
			AssertEquals("FCS", basic.Messages.LastOutgoingMessage.EM_MessageSubType);
			AssertEquals(2, basic.OutTurns.Count);
			AssertEquals(2, basic.Splits.Count);
			basic.Factory.Save();

			// Increase number of splits, should give only FCS and not FRC
			controller = new NonPersistentSplitLineOrchestrator(basic);
			controller.GetNonPersistentSplitsAndFlightData();
			npbo1 = controller.SplitsAndFlightData.SplitLines[0];
			npbo1.NumberOfPieces = 9;
			var npbo3 = controller.SplitsAndFlightData.SplitLines.AddNew();
			npbo3.NumberOfPieces = 1;
			npbo3.Weight = 1m;
			npbo3.HandlingDetail = "Extended";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			controller.PerformSplit_FCS(guiSender);
			AssertEquals("One new FCS message, no FRC", 9, basic.Messages.Count);
			AssertEquals("FCS", basic.Messages.LastOutgoingMessage.EM_MessageSubType);
			AssertEquals(3, basic.OutTurns.Count);
			AssertEquals(3, basic.Splits.Count);
			basic.Factory.Save();

			// Increase number of splits again, taking on received piece away from SRF02, should see an FRC
			controller = new NonPersistentSplitLineOrchestrator(basic);
			controller.GetNonPersistentSplitsAndFlightData();
			npbo1 = controller.SplitsAndFlightData.SplitLines[0];
			npbo1.NumberOfPiecesReceived = 2;
			npbo2 = controller.SplitsAndFlightData.SplitLines[1];
			npbo2.NumberOfPieces = 4;
			npbo2.NumberOfPiecesReceived = 0;  // decrease... FRC
			npbo2.WarehouseLocationID = ZGuid.Empty;
			var npbo4 = controller.SplitsAndFlightData.SplitLines.AddNew();
			npbo4.NumberOfPieces = 1;
			npbo4.Weight = 1m;
			npbo4.HandlingDetail = "Extended too";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			controller.PerformSplit_FCS(guiSender);
			AssertEquals("Three new messages, an FCS and an FRC each for SRFs 01 and 02... and no FRIs for splits", 12, basic.Messages.Count);
			AssertEquals("FCS", basic.Messages[09].EM_MessageSubType);
			AssertEquals("FRC", basic.Messages[10].EM_MessageSubType);
			AssertEquals("FRC", basic.Messages[11].EM_MessageSubType);
			AssertContains("ACD::01", basic.Messages[10].EM_MessageText);
			AssertContains("NPX 9 still", "QTY+118:9", basic.Messages[10].EM_MessageText);
			AssertContains("NPR now 2", "QTY+48:2", basic.Messages[10].EM_MessageText);
			AssertContains("ACD::02", basic.Messages[11].EM_MessageText);
			AssertContains("NPX 4", "QTY+118:4", basic.Messages[11].EM_MessageText);
			AssertContains("NPR 0", "QTY+48:0", basic.Messages[11].EM_MessageText);
			AssertEquals(4, basic.OutTurns.Count);
			AssertEquals(4, basic.Splits.Count);

			basic.NumberOfPiecesExpected = 10;
			basic.Splits.RemoveAndDeleteAll();
			basic.OutTurns.RemoveAndDeleteAll();
			basic.Messages.RemoveAndDeleteAllFromTest();

			var receivedSomePieces = basic.OutTurns.AddNew();
			receivedSomePieces.C5_PackagesOutturned = 2;
			basic.Factory.Save();
			controller = new NonPersistentSplitLineOrchestrator(basic);
			controller.GetNonPersistentSplitsAndFlightData();
			var npboA = controller.SplitsAndFlightData.SplitLines.AddNew();
			npboA.NumberOfPieces = 6;
			npboA.Weight = 6;
			npboA.NumberOfPiecesReceived = 2;
			npboA.HandlingDetail = "X";
			npboA.WarehouseLocationID = locationBac1.PK;
			var npboB = controller.SplitsAndFlightData.SplitLines.AddNew();
			npboB.NumberOfPieces = 4;
			npboB.Weight = 4;
			npboB.HandlingDetail = "X";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			controller.PerformSplit_FCS(guiSender);
			AssertEquals("FCS to split, then FRC for first split", 2, basic.Messages.Count);
			AssertEquals("FCS", basic.Messages[0].EM_MessageSubType);
			AssertEquals("FRC", basic.Messages[1].EM_MessageSubType);
			AssertContains("ACD::01", basic.Messages[1].EM_MessageText);
			AssertContains("NPX 6", "QTY+118:6", basic.Messages[1].EM_MessageText);
			AssertContains("NPR 2", "QTY+48:2", basic.Messages[1].EM_MessageText);
			AssertEquals(2, (int)basic.NumberOfPiecesReceived);
			AssertEquals(2, (int)basic.Splits["01"].NumberOfPiecesReceived);
		}

		internal delegate ZForm CreateFormDelegate<TAwb>(TAwb awb);

		internal static void MakeFormShowFormAndSaveForm<TAwb>(TAwb awb, CreateFormDelegate<TAwb> makeForm)
		{
			using (var form = makeForm(awb))
			{
				form.Show();
				form.FireSaveButton();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H", portName: "Heathrow");
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);   // yes, I want to save even though it will make a message
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);   // yes, I want to send despite blue validations						
		}

		static void MakeFormAddPluginShowFormAndSaveForm(BusinessObject freight, ZArchitecture.Modules.ControllerID controllerId)
		{
			using (var form = new ZForm(freight))
			{
				form.PlugIns.Add(controllerId);
				form.Show();
				form.FireSaveButton();
			}
		}

		ForwardingConsol CreateConsolGoodEnoughToSave(CusMAWB basic)
		{
			var consol = Factory.New<ForwardingConsol>();
			basic.CM_JK = consol.PK;
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "ZAJNB";
			var transport = consol.MostInterestingTransportForBinding[0];
			transport.JW_VoyageFlight = "BA001";
			transport.JW_ETA = ZDateTime.Today;
			transport.JW_ETD = ZDateTime.Today;
			consol.JK_MasterBillNum = basic.CM_MAWB;
			return consol;
		}

		static void AddSplits(SplitCollection splits, out SplitConsignment split01, out SplitConsignment split03)
		{
			split01 = splits.AddNew();
			split01.SplitReference = "01";
			split01.NumberOfPiecesExpected = 1;
			var split02 = splits.AddNew();
			split02.SplitReference = "02";
			split02.NumberOfPiecesExpected = 2;
			split03 = splits.AddNew();
			split03.SplitReference = "03";
			split03.NumberOfPiecesExpected = 3;
		}

		void AssertSplitMessageResults(Messaging.Business.EDIMessageCollection messages, string splitNumberElementIntroText)
		{
			AssertEquals(4, messages.Count);
			AssertEquals("FRC", messages[1].EM_MessageSubType);
			AssertEquals("FRC", messages[2].EM_MessageSubType);
			AssertEquals("FRC", messages[3].EM_MessageSubType);
			AssertEquals("Message for splits", 2, messages.Find(m => m.EM_MessageText.Contains(splitNumberElementIntroText) && m.EM_MessageSubType == "FRC").Count());
			AssertEquals("Message for split 03", 1, messages.Find(m => m.EM_MessageText.Contains(splitNumberElementIntroText + "03")).Count());
			AssertEquals("Message for split 01", 1, messages.Find(m => m.EM_MessageText.Contains(splitNumberElementIntroText + "01")).Count());
			AssertEquals("Message for basic", 1, messages.Find(m => !m.EM_MessageText.Contains(splitNumberElementIntroText) && m.EM_MessageSubType == "FRC").Count());
		}

		static CusMAWB GetBasic(BusinessObjectFactory factory)
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);
			var basic = factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			basic.CM_MAWB = "11122222222";
			basic.NumberOfPiecesExpected = 10;
			basic.ShipmentDescriptionCode = "T";
			return basic;
		}

		class AutoFriTests : TestCaseWithFactory
		{
			public void TestSendFriForBrandNewRecord_Basic()
			{
				var basic = GetBasic(Factory);
				MakeFormShowFormAndSaveForm(basic, delegate(CusMAWB awb)
				{ return new CcsukAirInventoryForm(awb); });
				AssertEquals(1, basic.Messages.Count);
				AssertEquals("FRI", basic.Messages[0].EM_MessageSubType);
				AssertNotContains("Common access reference placeholder should have been flipped out OK (done by Messaging.Biz)", "<<SYSCAR>>", basic.Messages[0].EM_MessageText);
				UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("111-22222222");
			}

			public void TestSendFriForBrandNewRecord_ConsolOfHouses()
			{
				var mawb = GetBasic(Factory);
				var house1 = mawb.ChildBills.AddNew();
				house1.CS_PiecesManifested = 1;
				house1.CS_HAWB = "33333333";
				MakeFormShowFormAndSaveForm(mawb, delegate(CusMAWB awb)
				{ return new CcsukAirInventoryForm(awb); });
				AssertEquals(1, mawb.Messages.Count);
				AssertEquals(1, house1.Messages.Count);
				AssertEquals("FRI", mawb.Messages[0].EM_MessageSubType);
				AssertEquals("FRI", house1.Messages[0].EM_MessageSubType);
				AssertNotContains("Common access reference placeholder should have been flipped out OK (done by Messaging.Biz)", "<<SYSCAR>>", mawb.Messages[0].EM_MessageText);
				AssertNotContains("Common access reference placeholder should have been flipped out OK (done by Messaging.Biz)", "<<SYSCAR>>", house1.Messages[0].EM_MessageText);
				UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("111-22222222");
				UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("33333333");

				var house2 = mawb.ChildBills.AddNew();
				house2.CS_PiecesManifested = 1;
				house2.CS_HAWB = "44444444";
				MakeFormShowFormAndSaveForm(mawb, delegate(CusMAWB awb)
				{ return new CcsukAirInventoryForm(awb); });
				AssertEquals("No new mawb message", 1, mawb.Messages.Count);
				AssertEquals("No new message for first child", 1, house1.Messages.Count);
				AssertEquals("Message for first child", 1, house2.Messages.Count);
				AssertEquals("FRI", house2.Messages[0].EM_MessageSubType);
			}

			protected override void SetUp()
			{
				base.SetUp();

				ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);   // yes, I want to save even though it will make a message
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);   // yes, I want to send despite blue validations	
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);   // yes, I want to save even though it will make a message for second house
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);   // yes, I want to send despite blue validations for second house
			}
		}
	}

	class FrcAndFcsTestsFromNonPersistentSplitPopup : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);   // yes, I want to save even though it will make a message
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);   // yes, I want to send despite blue validations
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);
		}

		IWhsLocation locationBac1;

		public void TestMakeUsingFcs_NoPiecesReceived_CheckingInFewerThanTotal()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var awb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			awb.Messages.AddNew().EM_ReceiveTransmit = "RCV"; // Existing message to make it look like need FRC
			awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			awb.Profile = "CUKAIR98LHRBAC";  //shed
			awb.OutTurns.RemoveAndDeleteAll();
			awb.NumberOfPiecesReceived = 0;
			awb.NumberOfPiecesExpected = 5;
			Factory.Save();
			var controller = new NonPersistentSplitLineOrchestrator(awb);
			controller.SplitsAndFlightData.SplitLines = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory).SplitLines;
			var guiMessageInitiator = new SendsMessagesToCustomsGUI();
			AssertEquals("Pre-req - no splits on local awb", 0, awb.Splits.Count);
			controller.PerformSplit_FCS(guiMessageInitiator);
			AssertEquals("Two, fake plus new FCS", 2, awb.Messages.Count);
			var fcs = awb.Messages[1];
			AssertEquals("ApplicationReference = CommDB", "CUKSYS98COMMDB", fcs.EM_ApplicationReference);
			AssertEquals("ApplicationCode", "CUK", fcs.EM_ApplicationCode);
			AssertContains("Message Text is edifact", "BGM+:::FCS+", fcs.EM_MessageText);
			AssertEquals("Split count", awb.Splits.Count, controller.SplitsAndFlightData.SplitLines.Count);
			AssertEquals("Split 2 reference", "02", awb.Splits[1].SplitReference);
			AssertEquals("Split 2 pieces", 2, (int)awb.Splits[1].NumberOfPiecesExpected);
			AssertEquals("Split 2 weight", 12.67m, awb.Splits[1].Weight);
			AssertEquals("Split 2 presense is ASS", PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection, awb.Splits[1].PresenceOnNetworkStatus);
			AssertEquals("Changes saved to database", true, awb.Splits[1].IsInDatabase);
			AssertEquals("SDC", ShipmentDescriptionCodes.Codes.TotalConsignmentManifested, awb.ShipmentDescriptionCode);
			AssertEquals(3, awb.OutTurns.Count);
			AssertEquals("", awb.OutTurns[0].C5_MarksAndNumbers);
			AssertEquals("Handle with care", awb.OutTurns[1].C5_MarksAndNumbers);
			AssertEquals("Daniel", awb.OutTurns[2].C5_MarksAndNumbers);
			AssertEquals(0, awb.OutTurns[0].C5_PackagesOutturned);
			AssertEquals(0, awb.OutTurns[1].C5_PackagesOutturned);
			AssertEquals(0, awb.OutTurns[2].C5_PackagesOutturned);
		}

		public void TestMakeUsingFcs_NoPiecesReceived_CheckingInAllPiecesToSimultaneouslySetStatus1()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var awb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			awb.Messages.AddNew().EM_ReceiveTransmit = "RCV"; // Existing message to make it look like need FRC
			awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			awb.Profile = "CUKAIR98LHRBAC";  //shed
			awb.OutTurns.RemoveAndDeleteAll();
			awb.NumberOfPiecesReceived = 0;
			awb.NumberOfPiecesExpected = 5;
			Factory.Save();
			var controller = new NonPersistentSplitLineOrchestrator(awb);
			var npSplitsToRequest = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory).SplitLines;
			foreach (NonPersistentSplitLine npSplit in npSplitsToRequest)
			{
				npSplit.NumberOfPiecesReceived = npSplit.NumberOfPiecesExpected; // want to simultaneously check in all pieces
				npSplit.WarehouseLocationID = locationBac1.PK;  // shut the validation up
				npSplit.HandlingDetail = "Shhh";
			}
			controller.SplitsAndFlightData.SplitLines = npSplitsToRequest;
			var guiMessageInitiator = new SendsMessagesToCustomsGUI();
			AssertEquals("Pre-req - no splits on local awb", 0, awb.Splits.Count);
			controller.PerformSplit_FCS(guiMessageInitiator);
			AssertEquals("Sox messages - existing FRI, plus FCS, FRC for parent, 3xFRCs for splits", 6, awb.Messages.Count);
			var fcs = awb.Messages[1];
			var frcMaster = awb.Messages[2];
			var frcSplit01 = awb.Messages[3];
			var frcSplit02 = awb.Messages[4];
			var frcSplit03 = awb.Messages[5];
			AssertContains("BGM+:::FCS+", fcs.EM_MessageText);
			AssertContains("BGM+:::FRC+", frcMaster.EM_MessageText);
			AssertContains("BGM+:::FRC+", frcSplit01.EM_MessageText);
			AssertContains("BGM+:::FRC+", frcSplit02.EM_MessageText);
			AssertContains("BGM+:::FRC+", frcSplit03.EM_MessageText);
			AssertContains("ACD::01", frcSplit01.EM_MessageText);
			AssertContains("ACD::02", frcSplit02.EM_MessageText);
			AssertContains("ACD::03", frcSplit03.EM_MessageText);
			AssertEquals(3, awb.OutTurns.Count);
			AssertEquals(2, awb.OutTurns[0].C5_PackagesOutturned);
			AssertEquals(2, awb.OutTurns[1].C5_PackagesOutturned);
			AssertEquals(1, awb.OutTurns[2].C5_PackagesOutturned);
		}
	}
}
