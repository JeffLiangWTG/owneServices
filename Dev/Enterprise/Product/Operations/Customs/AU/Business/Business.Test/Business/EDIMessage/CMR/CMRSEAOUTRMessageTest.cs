using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRSEAOUTRMessage))]
	sealed class CMRSEAOUTRMessageTest : CMRCUSRESMessageTest
	{
		public void TestGetWrappedObjectForOutturnHeader()
		{
			CusOutturnHeader header = CusOutturnHeader.New(Factory);
			header.C6_SendersMessageReference = "O00000123";
			CMRSEAOUTRMessage message = Factory.New<CMRSEAOUTRMessage>();
			message.EM_MessageText = outturnHeaderSEAOUTRMessageText;
			message.SetEM_LinkedObject();
			AssertEquals("LinkedObject", header, message.EM_LinkedObject);
		}

		readonly string outturnHeaderSEAOUTRMessageText = @"UNH+000001+CUSRES:D:99B:UN'
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

		public void TestCloningSEAOUTMessages()
		{
			CusOutturnHeader header = CusOutturnHeader.New(Factory);
			header.C6_SendersMessageReference = "O00000123";

			var seiMessage = (CMRSEIMessage)header.Messages.AddNew(typeof(CMRSEIMessage));
			seiMessage.EM_MessageText = InternalCUSRESMessageTest.SEIMessage;
			seiMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			seiMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			seiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var outMessage = (CMRSEAOUTMessage)header.Messages.AddNew(typeof(CMRSEAOUTMessage));
			outMessage.EM_MessageText = InternalCUSRESMessageTest.OutGoingMessage;
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var sutMessage = (CMRSEAOUTRMessage)header.Messages.AddNew(typeof(CMRSEAOUTRMessage));
			sutMessage.EM_MessageText = InternalCUSRESMessageTest.SUTMessageText;
			sutMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			sutMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEAOUT;
			sutMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			sutMessage.SetEM_LinkedObject();

			AssertEquals(3, header.Messages.Count);

			CMRCUSRESMessage linkedMessage = sutMessage.LinkOrCloneMessage(header);
			header.Messages.Load();
			AssertEquals(3, header.Messages.Count);
			AssertEquals(sutMessage, linkedMessage);
		}

		public void TestCloningOutturnResponseToLinesAndSetOutturnMessageStatus()
		{
			var header = CusOutturnHeader.New(Factory);
			header.C6_SendersMessageReference = "O00000123";
			var line1 = header.Outturns.AddNew();
			line1.C5_ContainerNumber = "CONT1";
			line1.C5_CargoType = "FCL";
			var line2 = header.Outturns.AddNew();
			line2.C5_ContainerNumber = "CONT1";
			line2.C5_HouseBill = "H1";
			line2.C5_MasterBill = "MASTER1";
			line2.C5_CargoType = "LCL";
			var line3 = header.Outturns.AddNew();
			line3.C5_ContainerNumber = "CONT1";
			line3.C5_HouseBill = "H2";
			line3.C5_MasterBill = "MASTER1";
			line3.C5_CargoType = "LCL";
			var line4 = header.Outturns.AddNew();
			line4.C5_ContainerNumber = "CONT1";
			line4.C5_HouseBill = "H3";
			line4.C5_MasterBill = "MASTER1";
			line4.C5_CargoType = "LCL";
			var line5 = header.Outturns.AddNew();
			line5.C5_ContainerNumber = "CONT1";
			line5.C5_HouseBill = "H1";
			line5.C5_MasterBill = "MASTER2";
			line5.C5_CargoType = "LCL";
			var line6 = header.Outturns.AddNew();
			line6.C5_ContainerNumber = "CONT2";
			line6.C5_HouseBill = "H1";
			line6.C5_MasterBill = "MASTER1";
			line6.C5_CargoType = "LCL";
			var line7 = header.Outturns.AddNew();
			line7.C5_ContainerNumber = "CONT2";
			line7.C5_HouseBill = "H2";
			line7.C5_MasterBill = "MASTER2";
			line7.C5_CargoType = "LCL";

			var sentMessage = Factory.New<CMRSEAOUTMessage>();
			sentMessage.EM_MessageText = outturnSEAOUTMessageText;
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			header.Messages.Add(sentMessage);

			AssertEquals("pre-condition", 1, header.Messages.Count);
			AssertEquals("pre-condition", 0, line1.Messages.Count);
			AssertEquals("pre-condition", 0, line2.Messages.Count);
			AssertEquals("pre-condition", 0, line3.Messages.Count);
			AssertEquals("pre-condition", 0, line4.Messages.Count);
			AssertEquals("pre-condition", 0, line5.Messages.Count);
			AssertEquals("pre-condition", 0, line6.Messages.Count);
			AssertEquals("pre-condition", 0, line7.Messages.Count);

			var inMessage = Factory.New<CMRSEAOUTRMessage>();
			inMessage.EM_MessageText = outturnHeaderSEAOUTRMessageText;
			inMessage.EM_MessageSubType = EDIMessage.Status.Rejected;
			inMessage.SetEM_LinkedObject();
			AssertEquals("LinkedObject", header, inMessage.EM_LinkedObject);
			AssertEquals("added to header messages", 2, header.Messages.Count);
			AssertEquals("no change", 0, line1.Messages.Count);
			AssertEquals("cloned", 1, line2.Messages.Count);
			AssertEquals("cloned", 1, line3.Messages.Count);
			AssertEquals("no change", 0, line4.Messages.Count);
			AssertEquals("no change", 0, line5.Messages.Count);
			AssertEquals("no change", 0, line6.Messages.Count);
			AssertEquals("cloned", 1, line7.Messages.Count);
			AssertEquals("type", typeof(CMRSEAOUTRMessage), line2.Messages[0].GetType());
			AssertEquals("type", typeof(CMRSEAOUTRMessage), line3.Messages[0].GetType());
			AssertEquals("type", typeof(CMRSEAOUTRMessage), line7.Messages[0].GetType());
			AssertEquals("LinkedObject", line2, line2.Messages[0].EM_LinkedObject);
			AssertEquals("LinkedObject", line3, line3.Messages[0].EM_LinkedObject);
			AssertEquals("LinkedObject", line7, line7.Messages[0].EM_LinkedObject);
			AssertEquals("received", EDIMessage.Status.Received, line2.Messages[0].EM_Status);
			AssertEquals("received", EDIMessage.Status.Received, line3.Messages[0].EM_Status);
			AssertEquals("received", EDIMessage.Status.Received, line7.Messages[0].EM_Status);

			AssertEquals("Outturn message status defaulted", "NOT", line2.C5_MessageStatus);
			AssertEquals("Outturn message status defaulted", "NOT", line3.C5_MessageStatus);
			AssertEquals("Outturn message status defaulted", "NOT", line7.C5_MessageStatus);
		}

		readonly string outturnSEAOUTMessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+" + EDIMessage.MessageNumberPlaceHolder + @":::SEAOUT+O00000123/SYD3:3+4'
NAD+VW+41065894724::95'
TDT+20+6993++11++++7631456::11'
LOC+4+9914N::95'
CNI++:::D'
RFF+AAQ:CONT1'
GID+1'
RFF+ACU:NIL'
GID+1'
RFF+BH:H1'
GID+1'
RFF+MB:MASTER1'
GIS+N:62:95'
GIS+U:63:95'
GIS+R:71:95'
GIS+N:186:95'
GIS+N:188:95'
TDT+1'
DTM+420:20090505:102'
DTM+420:0508:401'
GID+1'
PAC+50'
PAC+++PK:185:95'
PAC+++LCL:67:95'
FTX+AAA+++H5 STUFF'
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
DTM+420:20090507:102'
DTM+420:0527:401'
DTM+570:20090508:102'
DTM+570:0008:401'
GID+1'
PAC+50'
PAC+++PK:185:95'
PAC+++LCL:67:95'
FTX+AAA+++H3 STUFF'
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
DTM+420:20090505:102'
DTM+420:0508:401'
DTM+570:20090508:102'
DTM+570:0008:401'
GID+1'
PAC+50'
PAC+++PK:185:95'
PAC+++LCL:67:95'
FTX+AAA+++STUFF'
CNI++:::I'
RFF+AAQ:CONT2'
GID+1'
RFF+ACU:NIL'
GID+1'
RFF+BH:H2'
GID+1'
RFF+MB:MASTER2'
GIS+N:62:95'
GIS+U:63:95'
GIS+U:71:95'
GIS+N:186:95'
GIS+N:188:95'
TDT+1'
DTM+420:20090505:102'
DTM+420:0508:401'
DTM+570:20090508:102'
DTM+570:0008:401'
GID+1'
PAC+50'
PAC+++PK:185:95'
PAC+++LCL:67:95'
FTX+AAA+++STUFF'
CNI++:::I'
RFF+AAQ:CONT3'
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
DTM+420:20090505:102'
DTM+420:0508:401'
DTM+570:20090508:102'
DTM+570:0008:401'
GID+1'
PAC+50'
PAC+++PK:185:95'
PAC+++LCL:67:95'
FTX+AAA+++STUFF'
UNT+94+1'".Replace("\r\n", "");

		public void TestIsFullyAccepted()
		{
			CMRSEAOUTRMessage message = Factory.New<CMRSEAOUTRMessage>();
			message.EM_MessageText = SEAOUTRMessageWithOutErrorsText;
			Assert("IsFullyAccepted", message.IsFullyAccepted);
			CMRSEAOUTRMessage message2 = Factory.New<CMRSEAOUTRMessage>();
			message2.EM_MessageText = SEAOUTRMessageRejectedText;
			Assert("IsFullyAccepted", !message2.IsFullyAccepted);
			CMRSEAOUTRMessage message3 = Factory.New<CMRSEAOUTRMessage>();
			message3.EM_MessageText = SEAOUTRMessageCCFErrorText;
			Assert("IsFullyAccepted", !message3.IsFullyAccepted);
			CMRSEAOUTRMessage message4 = Factory.New<CMRSEAOUTRMessage>();
			message4.EM_MessageText = SEAOUTRMessageWithErrorsText;
			Assert("IsFullyAccepted", !message4.IsFullyAccepted);
		}

		public void TestIsFullyRejected()
		{
			CMRSEAOUTRMessage message = Factory.New<CMRSEAOUTRMessage>();
			message.EM_MessageText = SEAOUTRMessageWithOutErrorsText;
			Assert("IsFullyRejected", !message.IsFullyRejected);
			CMRSEAOUTRMessage message2 = Factory.New<CMRSEAOUTRMessage>();
			message2.EM_MessageText = SEAOUTRMessageRejectedText;
			Assert("IsFullyRejected", message2.IsFullyRejected);
			CMRSEAOUTRMessage message3 = Factory.New<CMRSEAOUTRMessage>();
			message3.EM_MessageText = SEAOUTRMessageCCFErrorText;
			Assert("IsFullyRejected", message3.IsFullyRejected);
			CMRSEAOUTRMessage message4 = Factory.New<CMRSEAOUTRMessage>();
			message4.EM_MessageText = SEAOUTRMessageWithErrorsText;
			Assert("IsFullyRejected", !message4.IsFullyRejected);
		}
		public void TestMatchingLine()
		{
			CMRSEAOUTRMessage message = Factory.New<CMRSEAOUTRMessage>();
			message.EM_MessageText = SEAOUTRMessageWithErrorsText;
			AssertNull(message.MatchingLine("OCLU6990054", "LBOBL6993", "H1"));
			AssertEquals("OCLU6990055/LBOBL6994/H2", message.MatchingLine("OCLU6990055", "LBOBL6994", "H2").ToString());
		}

		public static string SEAOUTRMessageWithOutErrorsText = @"UNH+000001+CUSRES:D:99B:UN'
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

		public static string SEAOUTRMessageRejectedText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEAOUTR+41A5 IDCB C0AJ:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEAOUT'
RFF+AFM:4'
RFF+ABO:O00000011/DAT1::002'
DTM+310:20090506052216:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5201:6:95'
FTX+AAO+++THIS TRANSACTION WAS REJECTED'
ERP+1'
ERC+ERROR:80:95'
ERC+CG1952:6:95'
FTX+AAO+++(CT=LCL,RDT=06/05/2009,RTM=151300,CNT=OCLU6990054) NUMBER OF PACKAGES IS NOT SUPPLIED CT=LCL,RDT=06/05/2009,RTM=151300,CNT=OCLU6990054,OBL=LBOBL6993,HBL=H4'
ERP+1'
ERC+ERROR:80:95'
ERC+CG1954:6:95'
FTX+AAO+++(CT=LCL,RDT=06/05/2009,RTM=151300,CNT=OCLU6990054) PACKAGE TYPE IS NOT SUPPLIED CT=LCL,RDT=06/05/2009,RTM=151300,CNT=OCLU6990054,OBL=LBOBL6993,HBL=H4'
ERP+1'
ERC+ERROR:80:95'
ERC+CG2005:6:95'
FTX+AAO+++AT LEAST ONE LINE DETAIL IS MANDATORY'
CNT+55:003'
UNT+25+000001'".Replace("\r\n", "");

		public static string SEAOUTRMessageCCFErrorText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEAOUTR+CCF_AAA374M_182720_SCO_1:1+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEAOUT'
RFF+AFM:4'
RFF+ABO:O00000011/DAT1::7'
DTM+310:20090506131556:204'
ERP+1'
ERC+CCFERROR:80:95'
ERC+15:6:95'
FTX+AAO+++The mandatory field OUTTURNRESULTTYPE is missing from LINE'
CNT+55:1'
UNT+13+000001'".Replace("\r\n", "");

		public static string SEAOUTRMessageWithErrorsText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEAOUTR+2646 EGJ1 9AAJ:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEAOUT'
RFF+AFM:4'
RFF+ABO:O00000011/DAT1::005'
DTM+310:20090506130506:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5202:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'
ERP+1'
ERC+ERROR:80:95'
ERC+CG1978:6:95'
FTX+AAO+++CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054 REPORT NOT FOUND FOR CHANGE/DELETE SCO LINE CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054,OBL=LBOBL6993,HBL=H1'
ERP+1'
ERC+ERROR:80:95'
ERC+CG1978:6:95'
FTX+AAO+++(CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990055) NUMBER OF PACKAGES IS NOT SUPPLIED CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990055,OBL=LBOBL6994,HBL=H2'
ERP+1'
ERC+ERROR:80:95'
ERC+CG1978:6:95'
FTX+AAO+++(CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054) NUMBER OF PACKAGES IS NOT SUPPLIED CT=LCL,RDT=06/05/2009,RTM=225600,CNT=,OBL=,HBL='
CNT+55:001'
UNT+17+000001'".Replace("\r\n", "");
	}
}
