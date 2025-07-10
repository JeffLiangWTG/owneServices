using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRSEIMessage))]
	sealed class CMRSEIMessageTest : CMRCUSRESMessageTest
	{
		public void TestGetWrappedObject()
		{
			AssertEquals("Precondition", 0, header.Messages.Count);
			message.SetEM_LinkedObject();
			AssertEquals("Message added to header", 1, header.Messages.Count);
			AssertEquals(message.PK, header.Messages[0].PK);
		}

		public void TestGetStatus()
		{
			AssertEquals("status", Enterprise.Customs.AU.Declaration.Business.CMRMessage.CMRMessageStatusDescription.ACCEPTED, message.GetStatus());
		}

		public void TestICMRDepotMessageMembers()
		{
			AssertEquals("LloydsNumber", "7631456", message.LloydsNumber);
			AssertEquals("VoyageNumber", "6990", message.VoyageNumber);
			AssertEquals("OriginPremiseID", "", ((ICMRDepotMessage)message).OriginPremiseID);
			AssertEquals("DestinationPremiseID", "", ((ICMRDepotMessage)message).DestinationPremiseID);
			AssertEquals("OurPremiseID", "9914N", ((ICMRDepotMessage)message).OurPremiseID);
			AssertEquals("MessageType", CMRDepotMessageType.Status, ((ICMRDepotMessage)message).MessageType);
			AssertEquals("Lines", 3, ((ICMRDepotMessage)message).Lines.Length);

			AssertEquals("Lines", 30, ((ICMRDepotMessage)message).Lines[2].NumberOfPackages);
			AssertEquals("Lines", "BX", ((ICMRDepotMessage)message).Lines[2].PackageType);
			AssertEquals("Lines", "OCLU8911239", ((ICMRDepotMessage)message).Lines[2].ContainerNumber);
			AssertEquals("Lines", "HBL002", ((ICMRDepotMessage)message).Lines[2].HouseBillNumber);
			AssertEquals("Lines", "OBLDPT001", ((ICMRDepotMessage)message).Lines[2].OceanBillNumber);
			AssertEquals("Lines", "LCL", ((ICMRDepotMessage)message).Lines[2].ContainerMode);
			AssertEquals("Lines", ExpectedMarks, ((ICMRDepotMessage)message).Lines[2].MarksAndNumbers);
			AssertEquals("Lines", "STUFF TYPE 2", ((ICMRDepotMessage)message).Lines[2].GoodsDescription);
		}

		public void TestGetReport()
		{
			AssertEquals("GetReport", ExpectedReport, message.GetReport().ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<CMRSEIMessage>();
			message.EM_MessageText = SampleSEIMessage;
			header = Factory.New<CusOutturnHeader>();
			header.C6_VoyageNum = "6990";
			header.C6_LloydsIMO = "7631456";
			header.C6_OutturningPremiseID = "9914N";
			header.C6_SendersMessageReference = "O00000007";
		}

		internal const string ExpectedMarks = @"MARKS1
MARKS2
MARKS3
MARKS4
MARKS5
MARKS6
MARKS7
MARKS8
MARKS9";

		const string ExpectedReport = @"Sea Cargo Establishment Information - (SEI)
Processing Date: 17/03/2009 15:46:45
Vessel: ANRO ASIA (7631456)
Voyage: 6990
Inland Movement Mode: ROA
Recipient Site: AAA374M
UBM Responsible Party: 41065894724

Processing Date: 17/03/2009 15:46:45
Container: OCLU8911239
Container Mode: FCL
Packages: 100 BX
Freight Forwarder Indicator ON (Master or Sub-Consol)
Net Weight: 20000 KG
Gross Weight: 23000 KG
Volume: 20 CM
Consignee: LOCAL FORWARDER
Goods Description: CONSOLIDATED CARGO

Processing Date: 17/03/2009 15:46:45
House Bill: HBL001
Ocean Bill: OBLDPT001
Container: OCLU8911239
Container Mode: LCL
Packages: 20 BX
Net Weight: 5000 KG
Gross Weight: 5000 KG
Volume: 5 CM
Consignee: CONSIGNEE
Goods Description: STUFF TYPE 1

Processing Date: 17/03/2009 15:46:45
House Bill: HBL002
Ocean Bill: OBLDPT001
Container: OCLU8911239
Container Mode: LCL
Packages: 30 BX
Net Weight: 7001 KG
Gross Weight: 7000 KG
Volume: 7 CM
Consignee: CONSIGNEE 2
Goods Description: STUFF TYPE 2
Marks: MARKS1
MARKS2
MARKS3
MARKS4
MARKS5
MARKS6
MARKS7
MARKS8
MARKS9
";

		internal const string SampleSEIMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEI+1G79 7IAF 71F9:1++11'DTM+9:20090317154645464587:ZZZ'TDT+20+6990++11++++7631456::11'" +
"TDT+1++ROA'NAD+MR+AAA374M::95'NAD+VW+41065894724::95'RFF+ABO:O00000007/DAT8::8'DOC+1'PAC+++FCL:67:95'PAC+100++BX:185:95'RFF+MB:OBLDPT001'RFF+AAQ:OCLU8911239'" +
"FTX+AAA+++CONSOLIDATED CARGO'GIS+FFO:109:95'MEA+AAE+G+KG:0000000023000.00'MEA+AAE+AAL+KG:0000000020000.00'MEA+AAE+ABJ+CM:0000000000020.00'NAD+CN++LOCAL FORWARDER'" +
"DOC+1'PAC+++LCL:67:95'PAC+20++BX:185:95'RFF+MB:OBLDPT001'RFF+BH:HBL001'RFF+AAQ:OCLU8911239'FTX+AAA+++STUFF TYPE 1'MEA+AAE+G+KG:0000000005000.00'" +
"MEA+AAE+AAL+KG:0000000005000.00'MEA+AAE+ABJ+CM:0000000000005.00'NAD+CN++CONSIGNEE'DOC+1'PAC+++LCL:67:95'PAC+30++BX:185:95'RFF+MB:OBLDPT001'RFF+BH:HBL002'" +
"RFF+AAQ:OCLU8911239'PCI+28+MARKS1:MARKS2:MARKS3:MARKS4:MARKS5:MARKS6:MARKS7:MARKS8:MARKS9'FTX+AAA+++STUFF TYPE 2'MEA+AAE+G+KG:0000000007000.00'" +
"MEA+AAE+AAL+KG:0000000007001.00'MEA+AAE+ABJ+CM:0000000000007.00'NAD+CN++CONSIGNEE 2'UNT+43+000001'";

		CMRSEIMessage message;
		CusOutturnHeader header;
	}
}
