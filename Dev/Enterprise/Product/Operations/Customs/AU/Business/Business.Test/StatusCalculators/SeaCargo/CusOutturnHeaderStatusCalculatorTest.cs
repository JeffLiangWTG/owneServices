using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusOutturnHeaderStatusCalculator))]
	sealed class CusOutturnHeaderStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeriveStatusIfEmptyWithMessages()
		{
			var message1 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			message1.EM_Status = EDIMessage.Status.Sent;
			var splitMessage2 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			splitMessage2.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Rejected;
			splitMessage2.EM_Status = EDIMessage.Status.Received;
			var splitMessage3 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			splitMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage3.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment; // unexpected sub type - this is important
			splitMessage3.EM_Status = EDIMessage.Status.Sent;

			var cycleCount = 0;
			OutturnHeader.Calculator.OnDerivingStatus += (sender, eventArgs) => AssertLessThan("Does not enter a recursion and blow up", cycleCount++, 1);

			AssertEquals(CMRBaseStatuses.Codes.NotSent, OutturnHeader.OutturnStatus.Code);
		}

		public void TestReceiveCargoAtDepot()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "MASTER1";
			oceanBill.CB_Voyage = "V123";
			oceanBill.CB_LloydsIMO = "123456";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CONT1";
			var house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "H1";
			var pack1 = house1.Pivot.AddNew();
			pack1.CV_CN = container.PK;
			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "H2";
			var pack2 = house2.Pivot.AddNew();
			pack2.CV_CN = container.PK;
			pack2.CV_IsHeldAtOutturn = true;
			pack2.CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;

			var header = Factory.New<CusOutturnHeader>();
			header.C6_LloydsIMO = "123456";
			header.C6_VoyageNum = "V123";
			header.C6_OutturningPremiseID = "139K";
			header.C6_SendersMessageReference = "O00000123";
			var line1 = header.Outturns.AddNew();
			line1.C5_ContainerNumber = "CONT1";
			line1.C5_HouseBill = "H1";
			line1.C5_MasterBill = "MASTER1";
			line1.C5_CargoType = "LCL";
			line1.C5_PackagesOutturned = 2;
			line1.C5_CargoReceiptDate = new ZDate(2014, 07, 07);
			line1.C5_CargoUnpackDate = new ZDate(2014, 07, 07);
			var line2 = header.Outturns.AddNew();
			line2.C5_ContainerNumber = "CONT1";
			line2.C5_HouseBill = "H2";
			line2.C5_MasterBill = "MASTER1";
			line2.C5_CargoType = "LCL";
			line2.C5_PackagesOutturned = 2;
			line2.C5_CargoReceiptDate = new ZDate(2014, 07, 07);
			line2.C5_CargoUnpackDate = new ZDate(2014, 07, 07);

			var sentMessage = Factory.New<CMRSEAOUTMessage>();
			sentMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			sentMessage.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+" + EDIMessage.MessageNumberPlaceHolder + @":::SEAOUT+O00000123/SYD3:3+4'
NAD+VW+41065894724::95'
TDT+20+6993++11++++7631456::11'
LOC+4+9914N::95'
CNI++:::I'
RFF+AAQ:CONT1'
GID+1'
RFF+ACU:NIL'
GID+1'
RFF+BH:H1'
GID+1'
RFF+MB:MASTER1'
GIS+N:62:95'
GIS+U:63:95'
GIS+U:71:95'
GIS+N:186:95'
GIS+N:188:95'
TDT+1'
DTM+420:20140706:102'
DTM+420:1400:401'
DTM+570:20140706:102'
DTM+570:1400:401'
GID+1'
PAC+2'
PAC+++LCL:67:95'
CNI++:::I'
RFF+AAQ:CONT1'
GID+1'
RFF+ACU:NIL'
GID+1'
RFF+BH:H2'
GID+1'
RFF+MB:MASTER1'
GIS+N:62:95'
GIS+U:63:95'
GIS+U:71:95'
GIS+N:186:95'
GIS+N:188:95'
TDT+1'
DTM+420:20140706:102'
DTM+420:1400:401'
DTM+570:20140706:102'
DTM+570:1400:401'
GID+1'
PAC+2'
PAC+++LCL:67:95'
UNT+94+1'".Replace("\r\n", "");
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			header.Messages.Add(sentMessage);
			Factory.Save();

			CMRSEAOUTRMessage inMessage = Factory.New<CMRSEAOUTRMessage>();
			inMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEAOUTR+30DC HA06 6GB5:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEAOUT'
RFF+AFM:9'
RFF+ABO:O00000123/SYD3::003'
DTM+310:20050921010113:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			inMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inMessage.EM_Status = EDIMessage.Status.Received;
			inMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Clear;
			inMessage.SetEM_LinkedObject();
			CMRStatusRecalculationSuspender.ResumeStatusRecalculation(Factory);
			Factory.Save();

			AssertEquals(1, pack1.CargoReceivedAtDepotLogs.Count);
			Assert(pack1.CV_IsHeldAtOutturn);
			AssertEquals(1, pack2.ReadyForLocalDeliveryLogs.Count);
			Assert(!pack2.CV_IsHeldAtOutturn);
		}

		public void TestInterestedMessageTypes()
		{
			var calculator = new CusOutturnHeaderStatusCalculatorForTest(Factory.New<CusOutturnHeader>());
			AssertEquals("length", 1, calculator.InterestedMessageTypesExposed.Length);
			AssertEquals("only one's type", CMRMessage.CMRMessageTypes.SEAOUT, calculator.InterestedMessageTypesExposed[0]);
		}

		public void TestStatusInfo()
		{
			var calculator = new CusOutturnHeaderStatusCalculatorForTest(Factory.New<CusOutturnHeader>());
			AssertEquals("name", "C6_MessageStatus", calculator.StatusInfoExposed.Name);
		}

		public void TestClearAllSeaOutturnLogs()
		{
			var message1 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var splitMessage2 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			splitMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			var splitMessage3 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			splitMessage3.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;

			message1.EM_Status = EDIMessage.Status.Pending;
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;

			OutturnHeader.CusUnderbondOutturnLogManager.AddANewSplitMessageFailedLog("Split Message Failure");
			AssertEquals("HasSplitMessageFailedLog", true, OutturnHeader.CusUnderbondOutturnLogManager.HasSplitMessageFailedLog);

			var message4 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			message4.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			OutturnHeader.Calculator.DeriveStatusNow();
			AssertEquals("HasSplitMessageFailedLog should be cleared now", false, OutturnHeader.CusUnderbondOutturnLogManager.HasSplitMessageFailedLog);
		}

		public void TestOutturnLogsForSea()
		{
			var message1 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var splitMessage2 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			splitMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			var splitMessage3 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			splitMessage3.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;

			message1.EM_Status = EDIMessage.Status.Pending;
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;

			OutturnHeader.CusUnderbondOutturnLogManager.AddANewSplitMessageOriginalRejectedLog("Split Message Original Rejection");
			AssertEquals("HasSplitMessageOriginalRejectedLog", true, OutturnHeader.CusUnderbondOutturnLogManager.HasSplitMessageOriginalRejectedLog);

			var message4 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			message4.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			OutturnHeader.Calculator.DeriveStatusNow();
			AssertEquals("Message sent again, HasSplitMessageOriginalRejectedLog should be cleared now", false, OutturnHeader.CusUnderbondOutturnLogManager.HasSplitMessageOriginalRejectedLog);
		}

		public void TestResponseStatusUpdatesOutturnLastMessageDate()
		{
			var outturn1 = OutturnHeader.Outturns.AddNew();
			var outturn2 = OutturnHeader.Outturns.AddNew();
			var outturn3 = OutturnHeader.Outturns.AddNew();
			OutturnHeader.Factory.Save();

			AssertEquals("Pre-condition expected", ZDateTime.Empty, outturn1.C5_LastMessageDate);
			AssertEquals("Pre-condition expected", ZDateTime.Empty, outturn2.C5_LastMessageDate);
			AssertEquals("Pre-condition expected", ZDateTime.Empty, outturn3.C5_LastMessageDate);

			var outgoingMessage = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			AssertEquals("Pre-condition expected", "SUT", outgoingMessage.EM_MessageType);
			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_MessageText = "UNH+1+CUSCAR:D:99B:UN'BGM+263:::SEAOUT+O00000555/CMT1:2+4'NAD+VW+41065894724::95'TDT+20+9870++11++++9044748::11'LOC+4+9914N::95'CNI++:::D'RFF+AAQ:ANLU7766175'GID+1'RFF+ACU:SC'GID+1'RFF+BH:16031'GID+1'RFF+MB:MBLWC1602'GIS+N:62:95'GIS+U:63:95'GIS+U:71:95'GIS+N:186:95'GIS+N:188:95'TDT+1'DTM+420:20160216:102'DTM+420:0215:401'DTM+570:20160216:102'DTM+570:0215:401'GID+1'PAC+1'PAC+++PK:185:95'PAC+++LCL:67:95'FTX+AAA+++SURPLUS GOODS'PCI+28+SURPLUS GOODS'CNI++:::I'RFF+AAQ:ANLU7766175'GID+1'RFF+ACU:SC'GID+1'RFF+BH:16031'GID+1'RFF+MB:MBLWC1602'GIS+N:62:95'GIS+U:63:95'GIS+U:71:95'GIS+N:186:95'GIS+N:188:95'TDT+1'DTM+420:20160216:102'DTM+420:0215:401'DTM+570:20160216:102'DTM+570:0215:401'GID+1'PAC+1'PAC+++PK:185:95'PAC+++LCL:67:95'FTX+AAA+++BOOK'PCI+28+BOOK'UNT+54+1'";

			var responseMessage = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTRMessage));
			AssertEquals("Pre-condition expected", "SUT", responseMessage.EM_MessageType);
			responseMessage.EM_MessageSubType = "CLR";
			responseMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_Status = "RCV";
			responseMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEAOUTR+1872 5G2I H3A6:001+11'NAD+MR+AAA374M::95'RFF+ACW:SEAOUT'RFF+AFM:4'RFF+ABO:O00000555/CMT1::002'DTM+310:20160216004252:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

			outturn3.C5_MessageStatus = CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected;

			var calculator = new CusOutturnHeaderStatusCalculatorForTest(OutturnHeader);
			calculator.DeriveStatusNow();
			AssertEquals("Outturn1 should have been updated", ZDateTime.Today, outturn1.C5_LastMessageDate);
			AssertEquals("Outturn2 should have been updated", ZDateTime.Today, outturn2.C5_LastMessageDate);
			AssertEquals("Outturn3 should not have been updated", ZDateTime.Empty, outturn3.C5_LastMessageDate);

			var withdrawalMessage = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			withdrawalMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			withdrawalMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			withdrawalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			withdrawalMessage.EM_Status = "SNT";
			withdrawalMessage.EM_MessageText = "UNH+1+CUSCAR:D:99B:UN'BGM+263:::SEAOUT+O00000555/CMT1:12+50'DTM+570:20120503:102'DTM+570:0812:401'NAD+VW+41065894724::95'TDT+20+442++6+QF::3'LOC+59+9914N::95'DTM+132:20120503:102'UNT+9+1'";

			var responseMessage2 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTRMessage));
			AssertEquals("Pre-condition expected", "SUT", responseMessage2.EM_MessageType);
			responseMessage2.EM_MessageSubType = "CLR";
			responseMessage2.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_Status = "RCV";
			responseMessage2.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEAOUTR+I6E7 6HB2 3A6:001+11'NAD+MR+AAA374M::95'RFF+ACW:SEAOUT'RFF+AFM:4'RFF+ABO:O00000555/CMT1::003'DTM+310:20160216060930:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

			outturn3.C5_LastMessageDate = ZDateTime.Today.AddDays(-3);
			calculator.DeriveStatusNow();
			AssertEquals("Outturn last message date should have been reset when withdrawn", ZDateTime.Empty, outturn1.C5_LastMessageDate);
			AssertEquals("Outturn last message date should have been reset when withdrawn", ZDateTime.Empty, outturn2.C5_LastMessageDate);
			AssertEquals("Outturn last message date should have been reset when withdrawn", ZDateTime.Empty, outturn3.C5_LastMessageDate);
		}

		public void TestClearAllOutturnLogs()
		{
			var message1 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var splitMessage2 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			splitMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			var splitMessage3 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			splitMessage3.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;

			message1.EM_Status = EDIMessage.Status.Pending;
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;

			OutturnHeader.CusUnderbondOutturnLogManager.AddANewSplitMessageFailedLog("Split Message Failure");
			AssertEquals("HasSplitMessageFailedLog", true, OutturnHeader.CusUnderbondOutturnLogManager.HasSplitMessageFailedLog);

			var message4 = OutturnHeader.Messages.AddNew(typeof(CMRSEAOUTMessage));
			message4.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			OutturnHeader.Calculator.DeriveStatusNow();
			AssertEquals("HasSplitMessageFailedLog should be cleared now", false, OutturnHeader.CusUnderbondOutturnLogManager.HasSplitMessageFailedLog);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusOutturnHeaderStatusCalculator(CusOutturnHeader.New(Factory));

		CusOutturnHeader outturnHeader;
		CusOutturnHeader OutturnHeader => outturnHeader ?? (outturnHeader = CusOutturnHeader.New(Factory));

		sealed class CusOutturnHeaderStatusCalculatorForTest : CusOutturnHeaderStatusCalculator
		{
			public CusOutturnHeaderStatusCalculatorForTest(CusOutturnHeader header)
				: base(header)
			{ }

			internal ZString[] InterestedMessageTypesExposed => InterestedMessageTypes;

			internal ZPropertyInfo StatusInfoExposed => StatusInfo;
		}
	}
}
