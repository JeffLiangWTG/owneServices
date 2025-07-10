using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCUSRESMessage))]
	internal class InternalCUSRESMessageTest : CMRCUSRESMessageTest
	{
		public void TestEntryNumber()
		{
			string validCusresMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+28HJ E7EH 7DB5:1+11'
DTM+138:20050829:102'
NAD+MR+AAA374M::95'
NAD+CM+66015286036::95'
NAD+IM+51006765546::95'
NAD+VT+AA33HF::95'
NAD+COQ+242200::215'
NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'
RFF+ABO:B00148283/1/SYD2::2'
RFF+ABQ:OWNER REF'
RFF+ADU:B00148283'
RFF+ABT:AAAA3YMJT'
RFF+RA:AAAA3YML6'
TAX+3'
MOA+7:0000000000000.00'
TAX+3'
MOA+23:0000000000030.10'
TAX+3'
MOA+9:0000000000050.00'
TAX+3'
MOA+58:0000000000000.00'
TAX+3'
MOA+149:0000000000000.00'
TAX+3'
MOA+369:0000000000155.00'
TAX+3'
MOA+371:0000000000000.00'
TAX+3'
MOA+26:0000000000006.50'
TAX+3'
MOA+206:0000000000000.00'
TAX+3'
MOA+304:0000000000000.00'
TAX+3'
MOA+128:0000000000241.60'
UNT+37+000001'".Replace("\r\n", "");

			CMRCUSRESMessage message = Factory.New<CMRCUSRESMessage>();
			message.EM_MessageText = validCusresMessage;
			AssertEquals("Entry Number in the message", "AAAA3YMJT", message.EntryNumber);
		}

		public void TestDrawbackClaimID()
		{
			string validCusresMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::DRWBCKR+341H 9152 22G7:1+11'
FTX+CUR+++OWNERS REF'
FTX+AHN+++LODGED:LODGED'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF'
NAD+P1+++FRED'
NAD+CB++CARGOWISE EDI PTY LTD'
RFF+ABO:B00001142/DAT20::1'
RFF+RF:AAACFRA9M::1'
RFF+ADU:TESTER DRAWBACK'
ERP+::0'
ERC+DR0058::95'
FTX+AAO+++LINE NBR?:=001-EXP DEC NBR IS QUOTED ON ANOTHER DBK LINE NBR?:=001'
UNT+15+000001'".Replace("\r\n", "");
			CMRCUSRESMessage message = Factory.New<CMRCUSRESMessage>();
			message.EM_MessageText = validCusresMessage;
			AssertEquals("Entry Number in the message", "AAACFRA9M", message.DrawbackClaimID);
		}

		public void TestGetReferenceFromSendersReference()
		{
			CMRCUSRESMessage message = Factory.New<CMRCUSRESMessage>();
			AssertEquals("Reference", "B00001001", message.GetReferenceFromSendersReference("B00001001/SYD1"));
		}

		public void TestERMSendersReference()
		{
			var incomingMessage = Factory.New<CMRAIRCRRMessage>();
			incomingMessage.EM_MessageText = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.AU.Declaration.Business.Testing.MessageProcessors.CMR.TestFiles.ERMCCFErrorMessage.txt").Replace("\r\n", "");
			AssertEquals("ERM message senders reference", "A00008674/DAT2", incomingMessage.SendersReference);
		}

		#region SEI Message, OutGoing Message

		public static string SEIMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEI+19IG 8C9F 37BF:1++11'
DTM+9:20151103122936695784:ZZZ'
TDT+20+PW0311++11++++9044748::11'
TDT+1++ROA'
NAD+MR+AAA374M::95'
NAD+VW+41065894724::95'
RFF+ABO:O00000538/CMT1::1'
DOC+1'
PAC+++FCL:67:95'
PAC+100++CN:185:95'
RFF+MB:GOH MASTER BILL'
RFF+AAQ:GOHU1111118'
PCI+28+NM'
FTX+AAA+++STUFF'
GIS+FFO:109:95'
MEA+AAE+G+KG:0000000020000.00'
MEA+AAE+AAL+KG:0000000020000.00'
MEA+AAE+ABJ+CU:0000000000100.00'
NAD+CN++AS CONS'
DOC+1'
PAC+++LCL:67:95'
PAC+50++PF:185:95'
RFF+MB:GOH MASTER BILL'
RFF+BH:GOH-HOUSE2'
RFF+AAQ:GOHU1111118'
PCI+28+NM'
FTX+AAA+++MY STUFF'
MEA+AAE+G+KG:0000000010000.00'
MEA+AAE+AAL+KG:0000000010000.00'
MEA+AAE+ABJ+CU:0000000000050.00'
NAD+CN++AMY TEST'
DOC+1'
PAC+++LCL:67:95'
PAC+50++PF:185:95'
RFF+MB:GOH MASTER BILL'
RFF+BH:GOH-HOUSE1'
RFF+AAQ:GOHU1111118'
PCI+28+NM'
FTX+AAA+++MY STUFF'
MEA+AAE+G+KG:0000000010000.00'
MEA+AAE+AAL+KG:0000000010000.00'
MEA+AAE+ABJ+CU:0000000000050.00'
NAD+CN++AMY TEST'
UNT+45+000001'
".Replace("\r\n", "");

		public static string OutGoingMessage = @"UNH+1+CUSCAR:D:99B:UN'
BGM+263:::SEAOUT+O00000538/CMT1:1+9'
NAD+VW+41065894724::95'
TDT+20+PW0311++11++++9044748::11'
LOC+4+9914N::95'
CNI++:::I'
RFF+AAQ:GOHU1111118'
GID+1'
RFF+ACU:SH'
GIS+N:62:95'
GIS+U:63:95'
GIS+R:71:95'
GIS+N:186:95'
GIS+N:188:95'
TDT+1'
DTM+420:20151103:102'
DTM+420:0140:401'
GID+1'
PAC+0'
PAC+++FCL:67:95'
FTX+AAA+++STUFF'
PCI+28+NM'
UNT+23+1'
".Replace("\r\n", "");

		public static string SUTMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEAOUTR+30F4 6DC6 87BF:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEAOUT'
RFF+AFM:9'
RFF+ABO:O00000538/CMT1::001'
DTM+310:20151103014119:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'
".Replace("\r\n", "");

		#endregion

		public void TestGetOutgoingMessageByBGMRefAndVersion()
		{
			JobDeclaration header = Factory.New<JobDeclaration>();
			var seiMessage = (CMRSEIMessage)header.Messages.AddNew(typeof(CMRSEIMessage));
			seiMessage.EM_MessageText = SEIMessage;
			seiMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			seiMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			seiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var outMessage = (CMRSEAOUTMessage)header.Messages.AddNew(typeof(CMRSEAOUTMessage));
			outMessage.EM_MessageText = OutGoingMessage;
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var sutMessage = (CMRCUSRESMessage)header.Messages.AddNew(typeof(CMRCUSRESMessage));
			sutMessage.EM_MessageText = SUTMessageText;
			sutMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			sutMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			sutMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var outgoingMsg = sutMessage.GetOutgoingMessageByBGMRefAndVersion(header.Messages, CMRMessage.CMRMessageTypes.SEAOUT);
			AssertNotEquals("GetOutgoingMessageByBGMRefAndVersion with parameter", null, outgoingMsg);
			outgoingMsg = seiMessage.GetOutgoingMessageByBGMRefAndVersion(header.Messages, CMRMessage.CMRMessageTypes.SEI);
			AssertEquals("GetOutgoingMessageByBGMRefAndVersion", null, outgoingMsg);
		}

		public void TestLastSentOrPendingOutgoingCMRMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var message = (CMRCUSRESMessage)declaration.Messages.AddNew(typeof(CMRCUSRESMessage));
			AssertEquals(null, message.LastSentOrPendingOutgoingCMRMessage);

			var outgoingMessage1 = declaration.Messages.AddNew(typeof(CMRMessage));
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals(null, message.LastSentOrPendingOutgoingCMRMessage);
			outgoingMessage1.EM_Status = EDIMessage.Status.Queued;
			AssertEquals(null, message.LastSentOrPendingOutgoingCMRMessage);
			outgoingMessage1.EM_Status = EDIMessage.Status.Sent;
			AssertEquals(outgoingMessage1, message.LastSentOrPendingOutgoingCMRMessage);

			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			AssertEquals(null, message.LastSentOrPendingOutgoingCMRMessage);

			var outgoingMessage2 = declaration.Messages.AddNew(typeof(CMRMessage));
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage2.EM_SystemCreateTimeUtc = outgoingMessage1.EM_SystemCreateTimeUtc.AddSeconds(-1);
			AssertEquals(outgoingMessage2, message.LastSentOrPendingOutgoingCMRMessage);

			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			AssertEquals(outgoingMessage2, message.LastSentOrPendingOutgoingCMRMessage);

			Factory.Save();
			AssertEquals(outgoingMessage1, message.LastSentOrPendingOutgoingCMRMessage);
		}

		public void TestReleasePendingMessages()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CMRCUSRESMessage message = (CMRCUSRESMessage)declaration.Messages.AddNew(typeof(CMRCUSRESMessage));
			EDIMessage outgoingMessage = declaration.Messages.AddNew();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Pending;
			message.ReleasePendingMessages();
			AssertEquals("OutgoingMessage.EM_Status", EDIMessage.Status.Queued, outgoingMessage.EM_Status);
		}

		public void TestReleasePendingMessagesForAIROUTGo1AtATime()
		{
			var underbond = Factory.New<CusUnderbond>();
			var responseMessage1 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));

			var splitMessage1 = underbond.Messages.AddNew();
			splitMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_Status = EDIMessage.Status.Pending;
			splitMessage1.EM_ApplicationReference = "6";

			var splitMessage3 = underbond.Messages.AddNew();
			splitMessage3.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_ApplicationReference = "8";

			var splitMessage2 = underbond.Messages.AddNew();
			splitMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage2.EM_ApplicationReference = "7";

			responseMessage1.ReleasePendingMessages();
			AssertEquals("OutgoingMessage.EM_Status should only set the first split message to Queued", EDIMessage.Status.Queued, splitMessage1.EM_Status);
			AssertEquals("OutgoingMessage.EM_Status should still be Pending for second split message", EDIMessage.Status.Pending, splitMessage2.EM_Status);
			AssertEquals("OutgoingMessage.EM_Status should still be Pending for third split message", EDIMessage.Status.Pending, splitMessage3.EM_Status);

			var responseMessage2 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage2.ReleasePendingMessages();
			AssertEquals("OutgoingMessage.EM_Status should now be set to Queued for second split message", EDIMessage.Status.Queued, splitMessage2.EM_Status);
			AssertEquals("OutgoingMessage.EM_Status should still be Pending for third split message", EDIMessage.Status.Pending, splitMessage3.EM_Status);

			var responseMessage3 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage3.ReleasePendingMessages();
			AssertEquals("OutgoingMessage.EM_Status should still now be Queued for third split message", EDIMessage.Status.Queued, splitMessage3.EM_Status);
		}

		public void TestReleasePendingMessagesForSEAOUTGo1AtATimeAndInSequence()
		{
			var underbond = Factory.New<CusUnderbond>();
			var responseMessage1 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));

			var splitMessage1 = underbond.Messages.AddNew();
			splitMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_Status = EDIMessage.Status.Pending;
			splitMessage1.EM_ApplicationReference = "6";

			var splitMessage3 = underbond.Messages.AddNew();
			splitMessage3.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_ApplicationReference = "8";

			var splitMessage2 = underbond.Messages.AddNew();
			splitMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage2.EM_ApplicationReference = "7";

			var splitMessage4 = underbond.Messages.AddNew();
			splitMessage4.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage4.EM_Status = EDIMessage.Status.Pending;
			splitMessage4.EM_ApplicationReference = "9";

			responseMessage1.ReleasePendingMessages();
			AssertEquals("OutgoingMessage.EM_Status should only set the first split message to Queued", EDIMessage.Status.Queued, splitMessage1.EM_Status);
			AssertEquals("OutgoingMessage.EM_Status should still be Pending for second split message", EDIMessage.Status.Pending, splitMessage2.EM_Status);
			AssertEquals("OutgoingMessage.EM_Status should still be Pending for third split message", EDIMessage.Status.Pending, splitMessage3.EM_Status);
			AssertEquals("OutgoingMessage.EM_Status should still be Pending for fourth split message", EDIMessage.Status.Pending, splitMessage4.EM_Status);

			var responseMessage2 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage2.ReleasePendingMessages();
			AssertEquals("OutgoingMessage.EM_Status should now be set to Queued for second split message", EDIMessage.Status.Queued, splitMessage2.EM_Status);
			AssertEquals("OutgoingMessage.EM_Status should still be Pending for third split message", EDIMessage.Status.Pending, splitMessage3.EM_Status);
			AssertEquals("OutgoingMessage.EM_Status should still be Pending for fourth split message", EDIMessage.Status.Pending, splitMessage4.EM_Status);

			var responseMessage3 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage3.ReleasePendingMessages();
			AssertEquals("OutgoingMessage.EM_Status should still now be Queued for third split message", EDIMessage.Status.Queued, splitMessage3.EM_Status);
			AssertEquals("OutgoingMessage.EM_Status should still be Pending for fourth split message", EDIMessage.Status.Pending, splitMessage4.EM_Status);

			var responseMessage4 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage4.ReleasePendingMessages();
			AssertEquals("OutgoingMessage.EM_Status should still now be Queued for fourth & last split message", EDIMessage.Status.Queued, splitMessage4.EM_Status);
		}

		public void TestCancelPendingMessages()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CMRCUSRESMessage message = (CMRCUSRESMessage)declaration.Messages.AddNew(typeof(CMRCUSRESMessage));
			EDIMessage outgoingMessage = declaration.Messages.AddNew();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Pending;
			message.CancelPendingMessages(message);
			AssertEquals("OutgoingMessage.EM_Status", EDIMessage.Status.Cancelled, outgoingMessage.EM_Status);
		}

		public void TestCancelPendingMessagesForAIROUTSetsEventLogWhenRequired()
		{
			var underbond = Factory.New<CusUnderbond>();
			var responseMessage1 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			responseMessage1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::AIROUTR+CCF_AAA374M_1_AWO_1:1+11'
NAD+MR+AAA374M::95'
RFF+ACW:AIROUT'
RFF+AFM:9'
RFF+ABO:U00003133/CMT1::1'
DTM+310:20140507051019:204'
ERP+1'
ERC+CCFERROR:80:95'
ERC+22:6:95'
FTX+AAO+++The length of MASTAIRWAYBILLNO in LINE[1] is 10 characters which is less than the minimum field length of 11 characters'
ERP+1'
ERC+CCFERROR:80:95'
ERC+22:6:95'
FTX+AAO+++The length of MASTAIRWAYBILLNO in LINE[2] is 10 characters which is less than the minimum field length of 11 characters'
ERP+1'
ERC+CCFERROR:80:95'
ERC+22:6:95'
FTX+AAO+++The length of MASTAIRWAYBILLNO in LINE[999] is 10 characters which is less than the minimum field length of 11 characters'
ERP+1'";

			CMRAIROUTMessage splitMessage1 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage1);
			splitMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_ApplicationReference = "SMI6";
			splitMessage1.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage1.EM_LinkUniqueID = responseMessage1.PK;
			splitMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00003133/CMT1:1+9'DTM+570:20140507:102'DTM+570:0429:401'NAD+VW+41065894724::95'TDT+20+038++6+BI::3'LOC+59+9912J::95'DTM+132:20140507:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406511401'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406537501'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406541301'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406634701'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406679901'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406688401'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406833101'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406833301'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406856701'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140407129501'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140407139701'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140407161901'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:SH1474301434599496'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:SH1481201434005062'GID+1'PAC+1'CN'";
			splitMessage1.EM_Status = EDIMessage.Status.Sent;

			CMRAIROUTMessage splitMessage3 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage3);
			splitMessage3.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_ApplicationReference = "SMI8";

			CMRAIROUTMessage splitMessage2 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage2);
			splitMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage2.EM_ApplicationReference = "SMI7";

			responseMessage1.CancelPendingMessages(responseMessage1);
			AssertEquals("Split message 1 has been sent", EDIMessage.Status.Sent, splitMessage1.EM_Status);
			AssertEquals("All pending split messages should be cancelled", EDIMessage.Status.Cancelled, splitMessage2.EM_Status);
			AssertEquals("All pending split messages should be cancelled", EDIMessage.Status.Cancelled, splitMessage3.EM_Status);

			AssertEquals("Cancelled log should have been created on the Underbond", CMRCUSRESMessage.splitMessageOriginalCancelled, underbond.Logs.MostRecentLog.SL_Reference);
			AssertEquals("Cancelled log SL_SE_NKEvent", AutoEvents.UnderbondSplitOutturnOriginalRejected.ToString(), underbond.Logs.MostRecentLog.SL_SE_NKEvent);
			AssertEquals("Outturn Status", CMRBaseStatuses.Codes.OriginalRejected, underbond.OutturnStatus.Code);
		}

		public void TestCancelPendingMessagesForSEAOUTSetsEventLogWhenRequired()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			var responseMessage1 = (CMRCUSRESMessage)outturnHeader.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			responseMessage1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEAOUTR+20I5 2A91 I8G5:001+11'NAD+MR+AAA374M::95'RFF+ACW:SEAOUT'RFF+AFM:9'RFF+ABO:U00000199/SYD2::002'DTM+310:20050914101927:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5201:6:95'FTX+AAO+++THIS TRANSACTION WAS REJECTED'ERP+1'ERC+ERROR:80:95'ERC+CG1945:6:95'FTX+AAO+++CT=FCL,RDT=15/09/2005,RTM=021500,CNT=OCLU2324297 DATE OF RECEIPT OR UNLOAD MUST BE LESS THAN OR EQUA CT=FCL,RDT=15/09/2005,RTM=021500,CNT=OCLU2324297'ERP+1'ERC+ERROR:80:95'ERC+CG2005:6:95'FTX+AAO+++AT LEAST ONE LINE DETAIL IS MANDATORY'CNT+55:002'UNT+21+000001'";

			var splitMessage1 = Factory.New<CMRSEAOUTMessage>();
			outturnHeader.Messages.Add(splitMessage1);
			splitMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_ApplicationReference = "SMI1";
			splitMessage1.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage1.EM_LinkUniqueID = responseMessage1.PK;
			splitMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::SEAOUT+O00000409/CMT1:1+9'NAD+VW+41065894724::95'TDT+20+901++11++++8811924::11'LOC+4+1399K::95'CNI++:::I'RFF+AAQ:AAAA1111117'GID+1'RFF+ACU:SH'GIS+Y:62:95'GIS+U:63:95'GIS+U:71:95'GIS+N:186:95'GIS+N:188:95'TDT+1'DTM+420:20160218:102'DTM+420:2318:401'DTM+570:20160218:102'DTM+570:2318:401'GID+1'PAC+3'PAC+++YC:185:95'PAC+++FCL:67:95'UNT+24+1'";
			splitMessage1.EM_Status = EDIMessage.Status.Sent;

			var splitMessage3 = Factory.New<CMRSEAOUTMessage>();
			outturnHeader.Messages.Add(splitMessage3);
			splitMessage3.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_ApplicationReference = "SMI3";

			var splitMessage2 = Factory.New<CMRSEAOUTMessage>();
			outturnHeader.Messages.Add(splitMessage2);
			splitMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage2.EM_ApplicationReference = "SMI2";

			responseMessage1.CancelPendingMessages(responseMessage1);
			AssertEquals("Split message 1 has been sent", EDIMessage.Status.Sent, splitMessage1.EM_Status);
			AssertEquals("All pending split messages for this SEA Outturn should be cancelled", EDIMessage.Status.Cancelled, splitMessage2.EM_Status);
			AssertEquals("All pending split messages for this SEA Outturn should be cancelled", EDIMessage.Status.Cancelled, splitMessage3.EM_Status);

			AssertEquals("Cancelled log should have been created on the Underbond", CMRCUSRESMessage.splitMessageOriginalCancelled, outturnHeader.Logs.MostRecentLog.SL_Reference);
			AssertEquals("Cancelled log SL_SE_NKEvent", AutoEvents.UnderbondSplitOutturnOriginalRejected.ToString(), outturnHeader.Logs.MostRecentLog.SL_SE_NKEvent);
			AssertEquals("Outturn Status", CMRBaseStatuses.Codes.OriginalRejected, outturnHeader.OutturnStatus.Code);
		}

		public void TestSplitMessageRejectedStatusAndDates()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();

			var underbond = Factory.New<CusUnderbond>();
			var outturn1 = underbond.Outturns.AddNew();
			outturn1.C5_PackagesOutturned = 20;
			outturn1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			outturn1.C5_ParentID = hawb1.PK;

			var outturn2 = underbond.Outturns.AddNew();
			outturn2.C5_PackagesOutturned = 0;
			outturn2.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			outturn2.C5_ParentID = hawb2.PK;
			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;

			var splitMessage1 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage1);
			splitMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage1.EM_ApplicationReference = "SMI1";
			splitMessage1.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00003307/CMT5:6+9'DTM+570:20150617:102'DTM+570:0617:401'NAD+VW+41065894724::95'TDT+20+318++6+QF::3'LOC+59+9912J::95'DTM+132:20150617:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08100323982'GID+1'RFF+HWB:A93283'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08100323982'GID+1'RFF+HWB:B0032847'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08100323982'GID+1'RFF+HWB:F03238'GID+1'PAC+3'UNT+33+1'";
			splitMessage1.EM_Status = EDIMessage.Status.Sent;

			var splitMessage2 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage2);
			splitMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage2.EM_ApplicationReference = "SMI2";
			splitMessage1.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00003307/CMT5:7+4'DTM+570:20150617:102'DTM+570:0617:401'NAD+VW+41065894724::95'TDT+20+318++6+QF::3'LOC+59+9912J::95'DTM+132:20150617:102'CNI++:::I'RFF+MWB:08100323982'GID+1'RFF+HWB:B9432'GID+1'PAC+1'UNT+15+1'";
			splitMessage2.EM_Status = EDIMessage.Status.Pending;

			var responseMessage1 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			responseMessage1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::AIROUTR+B549 0C61 6FF:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:AIROUT'
RFF+AFM:9'
RFF+ABO:U00003307/CMT5::006'
DTM+310:20150619073946:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001";
			splitMessage1.EM_LinkUniqueID = responseMessage1.PK;

			responseMessage1.ReleasePendingMessages();
			AssertEquals("Second message should now be released", EDIMessage.Status.Queued, splitMessage2.EM_Status);

			var responseMessage2 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			responseMessage2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::AIROUTR+CCF_AAA374M_1_AWO_1:1+11'
NAD+MR+AAA374M::95'
RFF+ACW:AIROUT'
RFF+AFM:4'
RFF+ABO:U00003307/CMT5::7'
DTM+310:20150619074025:204'
ERP+1'
ERC+CCFERROR:80:95'
ERC+15:6:95'
FTX+AAO+++The mandatory field OUTTURNRESULT is missing from LINE'
CNT+55:1'
UNT+13+000001'";
			splitMessage2.EM_LinkUniqueID = responseMessage2.PK;

			responseMessage2.CancelPendingMessages(responseMessage2);
			AssertEquals("Outturn Status", CMRBaseStatuses.Codes.AmendmentRejected, underbond.OutturnStatus.Code);
			var calculator = new CusUnderbondOutturnStatusCalculator(underbond);
			calculator.DeriveStatusNow();
			foreach (CusOutturn outturn in underbond.Outturns)
			{
				AssertEquals("LastMessageDate should not have been updated yet for this rejected outturn", ZDate.Empty, outturn.C5_LastMessageDate);
			}
		}

		public void TestSeaOutturnSplitMessageRejectedStatusAndDates()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			var outturn1 = outturnHeader.Outturns.AddNew();
			outturn1.C5_PackagesOutturned = 20;
			outturn1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			outturn1.C5_ParentID = outturnHeader.PK;

			var outturn2 = outturnHeader.Outturns.AddNew();
			outturn2.C5_PackagesOutturned = 0;
			outturn2.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			outturn2.C5_ParentID = outturnHeader.PK;
			outturnHeader.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;

			var splitMessage1 = Factory.New<CMRSEAOUTMessage>();
			outturnHeader.Messages.Add(splitMessage1);
			splitMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage1.EM_ApplicationReference = "SMI1";
			splitMessage1.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::SEAOUT+U00003307/CMT5:6+9'DTM+570:20150617:102'DTM+570:0617:401'NAD+VW+41065894724::95'TDT+20+318++6+QF::3'LOC+59+9912J::95'DTM+132:20150617:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08100323982'GID+1'RFF+HWB:A93283'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08100323982'GID+1'RFF+HWB:B0032847'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08100323982'GID+1'RFF+HWB:F03238'GID+1'PAC+3'UNT+33+1'";
			splitMessage1.EM_Status = EDIMessage.Status.Sent;

			var splitMessage2 = Factory.New<CMRSEAOUTMessage>();
			outturnHeader.Messages.Add(splitMessage2);
			splitMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage2.EM_ApplicationReference = "SMI2";
			splitMessage2.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::SEAOUT+U00003307/CMT5:7+4'DTM+570:20150617:102'DTM+570:0617:401'NAD+VW+41065894724::95'TDT+20+318++6+QF::3'LOC+59+9912J::95'DTM+132:20150617:102'CNI++:::I'RFF+MWB:08100323982'GID+1'RFF+HWB:B9432'GID+1'PAC+1'UNT+15+1'";
			splitMessage2.EM_Status = EDIMessage.Status.Pending;

			var responseMessage1 = (CMRCUSRESMessage)outturnHeader.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			responseMessage1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEAOUTR+B549 0C61 6FF:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEAOUT'
RFF+AFM:9'
RFF+ABO:U00003307/CMT5::006'
DTM+310:20150619073946:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001";
			splitMessage1.EM_LinkUniqueID = responseMessage1.PK;

			responseMessage1.ReleasePendingMessages();
			AssertEquals("Second message should now be released", EDIMessage.Status.Queued, splitMessage2.EM_Status);

			//simulate message has now been sent
			splitMessage2.EM_Status = EDIMessage.Status.Sent;

			var responseMessage2 = (CMRCUSRESMessage)outturnHeader.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			responseMessage2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEAOUTR+CCF_AAA374M_1_AWO_1:1+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEAOUT'
RFF+AFM:4'
RFF+ABO:U00003307/CMT5::7'
DTM+310:20150619074025:204'
ERP+1'
ERC+CCFERROR:80:95'
ERC+15:6:95'
FTX+AAO+++The mandatory field OUTTURNRESULT is missing from LINE'
CNT+55:1'
UNT+13+000001'";
			splitMessage2.EM_LinkUniqueID = responseMessage2.PK;

			responseMessage2.CancelPendingMessages(responseMessage2);
			AssertEquals("Outturn Status", CMRBaseStatuses.Codes.AmendmentRejected, outturnHeader.OutturnStatus.Code);
			var calculator = new CusOutturnHeaderStatusCalculator(outturnHeader);
			calculator.DeriveStatusNow();
			foreach (CusOutturn outturn in outturnHeader.Outturns)
			{
				AssertEquals("LastMessageDate should not have been updated yet for this rejected outturn", ZDate.Empty, outturn.C5_LastMessageDate);
			}
		}

		public void TestAIROUTSplitMessageEventLogs()
		{
			var underbond = Factory.New<CusUnderbond>();

			var responseMessage1 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			responseMessage1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+1FE4 AFFJ 390F:001+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:9'RFF+ABO:U00003133/CMT5::005'DTM+310:20150616060021:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

			CMRAIROUTMessage splitMessage1 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage1);
			splitMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_ApplicationReference = "SMI6";
			splitMessage1.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage1.EM_LinkUniqueID = responseMessage1.PK;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00003133/CMT1:1+9'DTM+570:20140507:102'DTM+570:0429:401'NAD+VW+41065894724::95'TDT+20+038++6+BI::3'LOC+59+9912J::95'DTM+132:20140507:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406511401'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406537501'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406541301'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406634701'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406679901'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406688401'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406833101'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406833301'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406856701'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140407129501'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140407139701'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140407161901'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:SH1474301434599496'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:SH1481201434005062'GID+1'PAC+1'CN'";
			splitMessage1.EM_Status = EDIMessage.Status.Sent;

			var responseMessage2 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			responseMessage2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+CCF_AAA374M_1_AWO_1:1+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:4'RFF+ABO:U00003133/CMT5::6'DTM+310:20150616060158:204'ERP+1'ERC+CCFERROR:80:95'ERC+15:6:95'FTX+AAO+++The mandatory field OUTTURNRESULT is missing from LINE[866]'CNT+55:1'UNT+13+000001'";

			CMRAIROUTMessage splitMessage2 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage2);
			splitMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00003133/CMT2:6+4'DTM+570:20140507:102'DTM+570:0429:401'NAD+VW+41065894724::95'TDT+20+038++6+BI::3'LOC+59+9912J::95'DTM+132:20140507:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH0581201434624359'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH0584201434733350'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH0586101434909551'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH0587501434893765'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH1768101434603582'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH1769401";
			splitMessage2.EM_Status = EDIMessage.Status.Sent;
			splitMessage2.EM_ApplicationReference = "SMI7";
			splitMessage1.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage1.EM_LinkUniqueID = responseMessage2.PK;

			CMRAIROUTMessage splitMessage3 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage3);
			splitMessage3.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_ApplicationReference = "SMI8";

			CMRAIROUTMessage splitMessage4 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage4);
			splitMessage4.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage4.EM_Status = EDIMessage.Status.Pending;
			splitMessage4.EM_ApplicationReference = "SMI9";

			responseMessage2.CancelPendingMessages(responseMessage2);
			AssertEquals("Split message 1 has been sent", EDIMessage.Status.Sent, splitMessage1.EM_Status);
			AssertEquals("Split message 2 has been sent but was rejected", EDIMessage.Status.Sent, splitMessage2.EM_Status);
			AssertEquals("All remaining pending split messages should be cancelled", EDIMessage.Status.Cancelled, splitMessage3.EM_Status);
			AssertEquals("All remaining pending split messages should be cancelled", EDIMessage.Status.Cancelled, splitMessage4.EM_Status);

			AssertEquals("Cancelled log should have been created on the Underbond", CMRCUSRESMessage.splitMessageCancelledReference, underbond.Logs.MostRecentLog.SL_Reference);
			AssertEquals("Cancelled log SL_SE_NKEvent", AutoEvents.CancelledCode, underbond.Logs.MostRecentLog.SL_SE_NKEvent);
		}

		public void TestSEAOUTSplitMessageEventLogs()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();

			var responseMessage1 = (CMRCUSRESMessage)outturnHeader.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			responseMessage1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEAOUTR+1FE4 AFFJ 390F:001+11'NAD+MR+AAA374M::95'RFF+ACW:SEAOUT'RFF+AFM:9'RFF+ABO:U00003133/CMT5::005'DTM+310:20150616060021:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

			CMRSEAOUTMessage splitMessage1 = Factory.New<CMRSEAOUTMessage>();
			outturnHeader.Messages.Add(splitMessage1);
			splitMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_ApplicationReference = "SMI6";
			splitMessage1.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage1.EM_LinkUniqueID = responseMessage1.PK;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::SEAOUT+U00003133/CMT1:1+9'DTM+570:20140507:102'DTM+570:0429:401'NAD+VW+41065894724::95'TDT+20+038++6+BI::3'LOC+59+9912J::95'DTM+132:20140507:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406511401'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406537501'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406541301'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406634701'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406679901'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406688401'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406833101'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406833301'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140406856701'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140407129501'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140407139701'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:20140407161901'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:SH1474301434599496'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:2671212121'GID+1'RFF+HWB:SH1481201434005062'GID+1'PAC+1'CN'";
			splitMessage1.EM_Status = EDIMessage.Status.Sent;

			var responseMessage2 = (CMRCUSRESMessage)outturnHeader.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			responseMessage2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEAOUTR+CCF_AAA374M_1_AWO_1:1+11'NAD+MR+AAA374M::95'RFF+ACW:SEAOUT'RFF+AFM:4'RFF+ABO:U00003133/CMT5::6'DTM+310:20150616060158:204'ERP+1'ERC+CCFERROR:80:95'ERC+15:6:95'FTX+AAO+++The mandatory field OUTTURNRESULT is missing from LINE[866]'CNT+55:1'UNT+13+000001'";

			CMRSEAOUTMessage splitMessage2 = Factory.New<CMRSEAOUTMessage>();
			outturnHeader.Messages.Add(splitMessage2);
			splitMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::SEAOUT+U00003133/CMT2:6+4'DTM+570:20140507:102'DTM+570:0429:401'NAD+VW+41065894724::95'TDT+20+038++6+BI::3'LOC+59+9912J::95'DTM+132:20140507:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH0581201434624359'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH0584201434733350'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH0586101434909551'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH0587501434893765'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH1768101434603582'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:26712121211'GID+1'RFF+HWB:SH1769401";
			splitMessage2.EM_Status = EDIMessage.Status.Sent;
			splitMessage2.EM_ApplicationReference = "SMI7";
			splitMessage2.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage2.EM_LinkUniqueID = responseMessage2.PK;

			CMRSEAOUTMessage splitMessage3 = Factory.New<CMRSEAOUTMessage>();
			outturnHeader.Messages.Add(splitMessage3);
			splitMessage3.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_ApplicationReference = "SMI8";

			CMRSEAOUTMessage splitMessage4 = Factory.New<CMRSEAOUTMessage>();
			outturnHeader.Messages.Add(splitMessage4);
			splitMessage4.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			splitMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage4.EM_Status = EDIMessage.Status.Pending;
			splitMessage4.EM_ApplicationReference = "SMI9";

			responseMessage2.CancelPendingMessages(responseMessage2);
			AssertEquals("Split message 1 has been sent", EDIMessage.Status.Sent, splitMessage1.EM_Status);
			AssertEquals("Split message 2 has been sent but was rejected", EDIMessage.Status.Sent, splitMessage2.EM_Status);
			AssertEquals("All remaining pending split messages should be cancelled", EDIMessage.Status.Cancelled, splitMessage3.EM_Status);
			AssertEquals("All remaining pending split messages should be cancelled", EDIMessage.Status.Cancelled, splitMessage4.EM_Status);

			AssertEquals("Cancelled log should have been created on the Underbond", CMRCUSRESMessage.splitMessageCancelledReference, outturnHeader.Logs.MostRecentLog.SL_Reference);
			AssertEquals("Cancelled log SL_SE_NKEvent", AutoEvents.CancelledCode, outturnHeader.Logs.MostRecentLog.SL_SE_NKEvent);
		}

		#region TestWrappedObjects

		public void TestGetWrappedObjectForCusEntryHeader()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B09090990";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.PopulateCH_BGMReferenceIfNeeded();

			GetWrappedObjectDeclarationMessageTestHelper message = Factory.New<GetWrappedObjectDeclarationMessageTestHelper>();
			message.Reference = entryHeader.CH_BGMReference;
			AssertEquals("Entry Header Reference number has / in BGM", true, entryHeader.CH_BGMReference.Contains(CusEntryHeader.ReferenceNumberSeparator));
			AssertEquals("Entry Header is retrived", entryHeader, message.GetWrappedObject());
		}

		public void TestGetWrappedObjectForCusSeaManTranHead()
		{
			CusSeaManTranHead bizo = Factory.New<CusSeaManTranHead>();
			bizo.BT_SendersMessageReference = "T12345";
			GetWrappedObjectTestHelper message = Factory.New<GetWrappedObjectTestHelper>();
			message.Reference = "T12345";
			AssertEquals("GetWrappedObject", bizo, message.GetWrappedObject());
			ErrorReporter.Clear();
		}

		public void TestGetWrappedObjectForCusSeaArrivalPort()
		{
			CusSeaManArrivalPort bizo = Factory.New<CusSeaManArrivalPort>();
			bizo.BA_SendersMessageReference = "A12345";
			GetWrappedObjectTestHelper message = Factory.New<GetWrappedObjectTestHelper>();
			message.Reference = "A12345";
			AssertEquals("GetWrappedObject", bizo, message.GetWrappedObject());
			ErrorReporter.Clear();
		}

		public void TestGetWrappedObjectForCusSeaOBLHeader()
		{
			CusSeaManOBLHeader bizo = Factory.New<CusSeaManOBLHeader>();
			bizo.BO_SendersMessageReference = "H12345";
			GetWrappedObjectTestHelper message = Factory.New<GetWrappedObjectTestHelper>();
			message.Reference = "H12345";
			AssertEquals("GetWrappedObject", bizo, message.GetWrappedObject());
			ErrorReporter.Clear();
		}

		public void TestGetWrappedObjectForJobVoyage()
		{
			JobVoyage bizo = Factory.New<JobVoyage>();
			bizo.JV_SendersMessageReference = "J00000002";
			GetWrappedObjectTestHelper message = Factory.New<GetWrappedObjectTestHelper>();
			message.Reference = "J00000002";
			AssertEquals("GetWrappedObject", typeof(CustomsJobVoyageWrapper), message.GetWrappedObject().GetType());
			ErrorReporter.Clear();
		}

		public void TestGetWrappedObjectForVoyageDestination()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageDestination bizo = voyage.Destinations.AddNew();
			bizo.JB_SendersMessageReference = "D00000002";
			GetWrappedObjectTestHelper message = Factory.New<GetWrappedObjectTestHelper>();
			message.Reference = "D00000002";
			AssertEquals("GetWrappedObject", typeof(CustomsVoyageDestinationWrapper), message.GetWrappedObject().GetType());
			ErrorReporter.Clear();
		}

		public void TestGetWrappedObjectForCTOCusHAWB()
		{
			CTOCusMAWB cusMAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB bizo = cusMAWB.ChildBills.AddNew();
			bizo.CS_MessageReference = "M00000002";
			GetWrappedObjectTestHelper message = Factory.New<GetWrappedObjectTestHelper>();
			message.Reference = "M00000002";
			AssertEquals("GetWrappedObject", bizo, message.GetWrappedObject());
			ErrorReporter.Clear();
		}

		public void TestGetWrappedObjectForCusSCAHouse()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse bizo = oceanBill.HouseBills.AddNew();
			bizo.CA_BGMReference = "L00000002";
			//Factory.Save();
			GetWrappedObjectTestHelper message = Factory.New<GetWrappedObjectTestHelper>();
			message.Reference = "L00000002";
			AssertEquals("GetWrappedObject", bizo, message.GetWrappedObject());
			ErrorReporter.Clear();
		}

		public void TestGetWrappedObjectForCusPartShip()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_MessageReference = "M00001000";
			CusPartShip partShip = hAWB.PartShips.AddNew();
			partShip.CG_MessageReference = "000001";

			GetWrappedObjectTestHelper message = Factory.New<GetWrappedObjectTestHelper>();
			message.Reference = "PM00001000/000001";
			AssertEquals("GetWrappedObject", partShip, message.GetWrappedObject());
			ErrorReporter.Clear();
		}

		public void TestGetWrappedObjectForExportCustomsManifestLines()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			ExportCustomsManifestLines line = header.Lines.AddNew();

			line.EL_UserReferenceNum = "KL00001000";
			GetWrappedObjectTestHelper message = Factory.New<GetWrappedObjectTestHelper>();
			message.Reference = "KL00001000";
			AssertEquals(line, message.GetWrappedObject());
		}

		public void TestGetWrappedObjectForExportCustomsManifestHeader()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();

			header.ED_BGMReference = "K00001000";
			GetWrappedObjectTestHelper message = Factory.New<GetWrappedObjectTestHelper>();
			message.Reference = "K00001000";
			AssertEquals(header, message.GetWrappedObject());
		}

		#endregion

		public void TestGetStatusFromMessageFTXSeg()
		{
			ZString validCusresMessage =
			 "UNH+000001+CUSRES:D:99B:UN'" +
			 "BGM+961:::IMDR+AJ3D AI8G 755:1+11'FTX+AHN+++HELDXX:HELDYY'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA447Y::95'" +
			 "NAD+VT+AA33JL::95'NAD+CB+54333::95'NAD+IM++PORTER DATA MANAGEMENT PTY LTD'NAD+CB++SOFTWARE CRAFT PTY LTD'RFF+ABO:B00122382/1/1::1'" +
			 "RFF+ABT:AAAAT33FA::1'RFF+ABQ:433407'RFF+ADU:B00122382'RFF+AAE:N10'ERP+::0'ERC+ID0060::95'FTX+AAO+++INVOICE TOTAL AMOUNT IS LESS THAN FREE ON BOARD AMOUNT'" +
			 "TAX+3'MOA+39:0000000002534.21'TAX+3'MOA+40:0000000002534.21'TAX+3'MOA+369:0000000000266.09'TAX+3'MOA+68:0000000000126.71'TAX+3'" +
			 "MOA+292:0000000009450.00'TAX+3'" +
			 "MOA+128:0000000000297.84'TAX+3'MOA+26:0000000000002.50'TAX+3'MOA+23:0000000000029.25'DOC+1+1'FTX+AHN+++HELD:HELD'CST+1+N10::95'" +
			 "FTX+AAF+++FREE'TAX+1'GIS+LAQ:109:95'TAX+1'MOA+40:0000000002534.21'TAX+1'MOA+369:0000000000266.09'TAX+1'MOA+68:0000000000126.71'" +
			 "ERP+::2'ERC+1::95'FTX+ABS+++IFIS (IMPORTED FOOD INSPECTION SCHEME) CLEARANCE REQUIRED'ERP+::3'ERC+1::95'FTX+ABS+++IMPORTED FOOD " +
			 "CONTROL ACT 1992. IFIS (IMPORTED FOOD INSPECTION SCHEME) PERMISSION TO DELIVER TO THE IMPORTER REQUIRED'CNT+5:1'UNT+55+000001'" +
			 "UNZ+1+00000000003899'";
			CMRCUSRESMessage message = Factory.New<CMRCUSRESMessage>();
			message.EM_MessageText = validCusresMessage;
			AssertEquals("GetCargoStatusForTransportLine", "HELDYY", message.GetCargoStatusForTransportLine(0));
			AssertEquals("GetCustomsStatusFromMessage", "HELDYY", message.GetCustomsStatusFromMessage());
		}

		#region GetWrappedObjectTestHelpers

		class GetWrappedObjectTestHelper : CMRCUSRESMessage
		{
			public GetWrappedObjectTestHelper(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected internal override string GetReferenceFromSendersReference()
			{
				return Reference;
			}

			public string Reference;
		}

		class GetWrappedObjectDeclarationMessageTestHelper : CMRImportDeclarationMessage
		{
			public GetWrappedObjectDeclarationMessageTestHelper(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected internal override string GetReferenceFromSendersReference()
			{
				return Reference;
			}

			public string Reference;
		}

		#endregion

		public void TestAIROUTSplitMessageRejectedEventLogs()
		{
			var underbond = Factory.New<CusUnderbond>();

			var responseMessage1 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			responseMessage1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+305F 06EE DH0F:001+11'NAD+MR+AAL669E::95'RFF+ACW:AIROUT'RFF+AFM:9'RFF+ABO:U00000565/LB29::013'DTM+310:20150629051117:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

			CMRAIROUTMessage splitMessage1 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage1);
			splitMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_ApplicationReference = "SMI1";
			splitMessage1.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage1.EM_LinkUniqueID = responseMessage1.PK;
			splitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00000565/LB29:13+9'DTM+570:20150629:102'DTM+570:0345:401'NAD+VW+41065894724::95'TDT+20+101++6+QF::3'LOC+59+9914N::95'DTM+132:20121109:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08155550386'GID+1'RFF+HWB:H0386A'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08155550386'GID+1'RFF+HWB:H0386E'GID+1'PAC+1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08155550386'GID+1'RFF+HWB:H0386F'GID+1'PAC+3'UNT+33+1'";
			splitMessage1.EM_Status = EDIMessage.Status.Sent;

			var responseMessage2 = (CMRCUSRESMessage)underbond.Messages.AddNew(typeof(CMRCUSRESMessage));
			responseMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			responseMessage2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+CCF_AAL669E_1_AWO_1:1+11'NAD+MR+AAL669E::95'RFF+ACW:AIROUT'RFF+AFM:4'RFF+ABO:U00000565/LB29::14'DTM+310:20150629051257:204'ERP+1'ERC+CCFERROR:80:95'ERC+15:6:95'FTX+AAO+++The mandatory field OUTTURNRESULT is missing from LINE[2]'CNT+55:1'UNT+13+000001'";

			CMRAIROUTMessage splitMessage2 = Factory.New<CMRAIROUTMessage>();
			underbond.Messages.Add(splitMessage2);
			splitMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.AIROUT;
			splitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			splitMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00000565/LB29:14+4'DTM+570:20150629:102'DTM+570:0345:401'NAD+VW+41065894724::95'TDT+20+101++6+QF::3'LOC+59+9914N::95'DTM+132:20121109:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08155550386'GID+1'RFF+HWB:H0386G'GID+1'PAC+4'CNI++:::I'RFF+MWB:08155550386'GID+1'RFF+HWB:H0386H'GID+1'PAC+5'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08155550386'GID+1'RFF+HWB:H0386I'GID+1'PAC+6'UNT+31+1'";
			splitMessage2.EM_Status = EDIMessage.Status.Sent;
			splitMessage2.EM_ApplicationReference = "SMI2";
			splitMessage2.EM_LinkTable = CusUnderbondSchema.Constants.TableName;
			splitMessage2.EM_LinkUniqueID = responseMessage2.PK;

			responseMessage2.CancelPendingMessages(responseMessage2);
			AssertEquals("Split message 1 has been sent", EDIMessage.Status.Sent, splitMessage1.EM_Status);
			AssertEquals("Split message 2 has been sent but was rejected", EDIMessage.Status.Sent, splitMessage2.EM_Status);

			AssertEquals("Cancelled log should have been created on the Underbond", CMRCUSRESMessage.splitMessageCancelledReference, underbond.Logs.MostRecentLog.SL_Reference);
			AssertEquals("Cancelled log SL_SE_NKEvent", AutoEvents.CancelledCode, underbond.Logs.MostRecentLog.SL_SE_NKEvent);
		}
	}
}
