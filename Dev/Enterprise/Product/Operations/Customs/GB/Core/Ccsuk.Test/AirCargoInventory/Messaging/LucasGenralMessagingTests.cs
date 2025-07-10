using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	public class LucasGenralMessagingTests : CcsukNonChiefResponseBaseMessageProcessorTest
	{
		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_Hawb()
		{
			CreateMawbAndCreateInboundQueuedMessage(true, "HAWB=87654321", false);
			var hawb1 = mawb.ChildBills[0];
			hawb1.LatestCustomsActionText = "OK TO POOP";
			hawb1.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			Factory.Save();

			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(1, outboundMessages.Length);
			AssertContains("'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: HAWB=87654321:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::REF   ?:87654321                    ORIG?:USLAX  NPX?:0       NPR?:0  :MASTER?:80112345678                 DEST?:LHR    GWT?:0.000   NPD?:'FTX+AAA+++HOUSE ?:87654321                                            CT ?:       :SHPMT ?:BA112                       DATE?:11/12/1987:DESCR ?:                                                               :ENTRY ?:                  ROE?:        SOE?:        ICS?:            :STATUS?:CC-OK TO POOP               DATE?:18/09/1971 00?:00'FTX+AAA+++MUCR  ?:A?:80112345678::SHPR  ?:                                                               :ADDR  ?:                                                               'FTX+AAA+++CNSEE ?:                                                               :ADDR  ?:                                                               ::ARRVD ?::DEP   ?:                            DEST?:LHRKLM'UNT+8+1'", outboundMessages[0].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
			AssertEquals(EDIInterchange.Status.Received, inboundMessage.Interchange.EI_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_HawbSplitExactMatch()
		{
			CombineAssertions(() =>
			{
				CreateMawbAndCreateInboundQueuedMessage(true, "HAWB=87654321");
				var hawb1 = mawb.ChildBills[0];
				var split1 = hawb1.Splits.AddNew();
				var split2 = hawb1.Splits.AddNew();
				split1.SplitReference = "01";
				split2.SplitReference = "02";
				split1.NumberOfPiecesExpected = 1;
				split2.NumberOfPiecesExpected = 2;
				hawb1.CS_PiecesManifested = 3;
				hawb1.LatestCustomsActionText = "OK TO POOP";
				hawb1.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
				Factory.Save();

				RunProcessors();
				var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
				AssertEquals(1, outboundMessages.Length);
				AssertContains("Message body, ordered by split ref", "UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: HAWB=87654321:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::START RESPONSE TO RS INTERROGATION HAWB=87654321 FROM LHRBAC:HAWB     SRF NPX NPR NPD DESC                GWT    AGT CAC DATE     E'FTX+AAA+++87654321     3   0   0                              LXA CC  19710918  :87654321 01  1   0   0                              LXA               :87654321 02  2   0   0                              LXA               :PAGE 01/END'UNT+6+1'", outboundMessages[0].EM_MessageText);
				inboundMessage.Reload();
				AssertEquals("RCV", inboundMessage.EM_Status);
			});
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_HawbWithDeclaration()
		{
			CreateOrganisations();
			CreateMawbAndCreateInboundQueuedMessage(true, "HAWB=87654321");
			var hawb = mawb.ChildBills[0];
			hawb.LatestCustomsActionText = "OK TO POOP";
			hawb.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			var dec = Factory.New<JobDeclaration>();
			dec.ImporterDocumentaryAddress.E2_OA_Address = org3.MainAddress.PK;
			dec.SupplierDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_MessageType = "EXP";
			dec.JE_DeclarationType = "EFD";
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "120-123456A";
			dec.JE_TotalNoOfPieces = 70;
			dec.SingleEntry.CH_StyleOfEntrySOE = "J";
			dec.SingleEntry.CH_ImportClearanceStatusICS = "77";
			dec.JE_GoodsDescription = "STUFF TWO";
			dec.JE_EntryStatus = "RT6";
			dec.ZG_CTStatusID = "X";
			hawb.CS_PiecesLanded = 88;
			hawb.CS_PiecesManifested = 89;
			hawb.CS_Weight = 666;
			hawb.CS_JE_CustomsFormalEntry = dec.PK;
			Factory.Save();

			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(1, outboundMessages.Length);
			AssertContains("UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: HAWB=87654321:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::REF   ?:87654321                    ORIG?:USLAX  NPX?:89      NPR?:88 :MASTER?:80112345678                 DEST?:LHR    GWT?:666.000 NPD?:'FTX+AAA+++HOUSE ?:87654321                                            CT ?:X      :SHPMT ?:BA112                       DATE?:11/12/1987:DESCR ?:                                                               :ENTRY ?:EFD 120-123456A   ROE?: RT6    SOE?: J      ICS?: 77         :STATUS?:CC-OK TO POOP               DATE?:18/09/1971 00?:00'FTX+AAA+++MUCR  ?:A?:80112345678::SHPR  ?:DANIEL CLARKE                                                  :ADDR  ?:STREET2 BOROUGH MILKY BEANS MK14 6LY                           'FTX+AAA+++CNSEE ?:DANIEL CLARKE?'S COMPANY LTD                                    :ADDR  ?:STREET3 BOROUGH MILKY BEANS MK14 6LY                           ::ARRVD ?::DEP   ?:                            DEST?:LHRKLM'UNT+8+1'", outboundMessages[0].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_HawbManyRecordsPartialQuery()
		{
			CombineAssertions(() =>
			{
				CreateMawbAndCreateInboundQueuedMessage(true, "HAWB=8765432");
				var hawb1 = mawb.ChildBills[0];
				hawb1.Split(MakeSplitRequests());
				hawb1.CS_PiecesManifested = 338;
				hawb1.CS_PiecesLanded = 336;
				hawb1.Splits["01"].NumberOfPiecesReceived = 69;
				hawb1.Splits["02"].NumberOfPiecesReceived = 269;
				var hawb2 = mawb.ChildBills.AddNew();
				hawb2.CS_HAWB = "87654329";
				hawb2.CS_GoodsDescription = "Stuff h 2";
				hawb2.CS_PiecesLanded = 777;
				hawb2.CS_PiecesManifested = 888;
				hawb2.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
				Factory.Save();

				RunProcessors();
				var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
				AssertEquals(1, outboundMessages.Length);
				AssertContains("Response to LUCAS, ordered by house", "UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: HAWB=8765432:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::START RESPONSE TO RS INTERROGATION HAWB=8765432 FROM LHRBAC:HAWB     SRF NPX NPR NPD DESC                GWT    AGT CAC DATE     E'FTX+AAA+++87654321     338 336 0                              LXA               :87654329     888 777 0   STUFF H 2                  LXA CC  19710918  :87654321 01  69  69  0                       123.4  LXA               :87654321 02  269 269 0                       12345  LXA               :PAGE 01/END'UNT+6+1'", outboundMessages[0].EM_MessageText);
				inboundMessage.Reload();
				AssertEquals("RCV", inboundMessage.EM_Status);
			});
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_HawbManyRecordsWholeQuery()
		{
			CombineAssertions(() =>
			{
				LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
				CreateMawbAndCreateInboundQueuedMessage(true, "HAWB=87654321");
				mawb.Profile = "CUKAIR98LHRBAC";
				var hawb1 = mawb.ChildBills[0];
				hawb1.Profile = mawb.Profile;
				hawb1.Split(MakeSplitRequests());
				hawb1.CS_PiecesManifested = 338;
				var otS1 = hawb1.OutTurns.AddNew();
				otS1.SplitReferenceToWhichThisPertains = "01";
				otS1.C5_PackagesOutturned = 68;
				var otS2 = hawb1.OutTurns.AddNew();
				otS2.SplitReferenceToWhichThisPertains = "02";
				otS2.C5_PackagesOutturned = 268;
				var mawb2 = Factory.New<CusMAWB>();
				mawb2.CM_MAWB = "66655544333";
				mawb2.Profile = mawb.Profile;
				var hawb2 = mawb2.ChildBills.AddNew();
				hawb2.CS_HAWB = hawb1.CS_HAWB;
				hawb2.CS_GoodsDescription = "Stuff h 2";
				hawb2.OutTurns.AddNew().C5_PackagesOutturned = 777;
				hawb2.CS_PiecesManifested = 888;
				hawb2.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
				Factory.Save();
				RunProcessors();
				var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
				AssertEquals(1, outboundMessages.Length);
				AssertContains("Response to LUCAS, ordered by house", "UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: HAWB=87654321:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::START RESPONSE TO RS INTERROGATION HAWB=87654321 FROM LHRBAC:HAWB     SRF NPX NPR NPD DESC                GWT    AGT CAC DATE     E'FTX+AAA+++87654321     338 336 0                              LXA               :87654321     888 777 0   STUFF H 2                      CC  19710918  :87654321 01  69  68  0                       123.4  LXA               :87654321 02  269 268 0                       12345  LXA               :PAGE 01/END'UNT+6+1'", outboundMessages[0].EM_MessageText);
				inboundMessage.Reload();
				AssertEquals("RCV", inboundMessage.EM_Status);
			});
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_Mawb_ImportCusMAWBWithCusHAWBs()
		{
			CombineAssertions(() =>
			{
				CreateMawbAndCreateInboundQueuedMessage(true, "MAWB=801-12345678");
				var hawb1 = mawb.ChildBills[0];
				hawb1.CS_HAWB = "22222222";
				hawb1.Split(MakeSplitRequests(true));
				hawb1.CS_PiecesManifested = 338;
				hawb1.CS_PiecesLanded = 336;
				hawb1.Splits["01"].NumberOfPiecesReceived = 69;
				hawb1.Splits["02"].NumberOfPiecesReceived = 269;
				var hawb2 = mawb.ChildBills.AddNew();
				hawb2.CS_HAWB = "11111111";
				hawb2.CS_GoodsDescription = "Stuff h 2";
				hawb2.CS_PiecesLanded = 777;
				hawb2.CS_PiecesManifested = 888;
				hawb2.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
				var hawb2Delivery = hawb2.OutTurns.AddNew();
				hawb2Delivery.C5_CargoReceiptDate = ZDateTime.Today;
				hawb2Delivery.C5_PackagesOutturned = 2;
				hawb2Delivery.IsDelivered = true;
				Factory.Save();

				RunProcessors();
				var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
				AssertEquals(2, outboundMessages.Length);
				AssertContains("UNH+1+CIMFSA:2:0:IA+", outboundMessages[0].EM_MessageText);
				AssertContains("FSA/2:801-12345678LAXLHR/P68T15:OSI/SEE PRINTER SPOOLER", outboundMessages[0].EM_MessageText);
				AssertContains("Response to LUCAS, ordered by split reference", "UNH+2+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: MAWB=801-12345678:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::START RESPONSE TO RS INTERROGATION MAWB=801-12345678 FROM LHRBAC:HAWB     SRF NPX NPR NPD DESC                GWT    AGT CAC DATE     E'FTX+AAA+++11111111     888 777 2   STUFF H 2                  LXA CC  19710918  :22222222     338 336 0                              LXA               :22222222 01  69  69  0                       123.4  LXA               :22222222 02  269 269 0                       12345  LXA               :PAGE 01/END'UNT+6+2'", outboundMessages[1].EM_MessageText);
				inboundMessage.Reload();
				AssertEquals("RCV", inboundMessage.EM_Status);
			});
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_BasicNotCleared()
		{
			GBCustomsDataRegistry.Instance.LucasFakePimaForCwTesting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DANIEL");
			CreateMawbAndCreateInboundQueuedMessage(true, "MAWB=801-12345678");
			inboundMessage.Interchange.EI_From = "DANIEL";
			mawb.ChildBills[0].Delete();
			Factory.Save();

			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals("A reply is sent, meaning that we do indeed treat this as a LUCAS enquiry", 1, outboundMessages.Length);
			AssertContains("UNH+1+CIMFSA:2:0:IA+COMMONACCESSREFERENCE", outboundMessages[0].EM_MessageText);
			AssertContains("OSI/SDC T NPR=68", outboundMessages[0].EM_MessageText);
			AssertEquals("Delivered back to pima 'Daniel'", "DANIEL", outboundMessages[0].EM_ApplicationReference);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_BasicCleared()
		{
			CreateMawbAndCreateInboundQueuedMessage(true, "MAWB=801-12345678");
			mawb.ChildBills[0].Delete();
			mawb.SetCustomsActionCode("CA", ZDateTime.Now);
			mawb.SetCustomsActionCode("CC", ZDateTime.Now.AddDays(1));
			Factory.Save();

			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(1, outboundMessages.Length);
			AssertContains("UNH+1+CIMFSA:2:0:IA+COMMONACCESSREFERENCE", outboundMessages[0].EM_MessageText);
			AssertContains("OSI/SDC T CC 12 DEC 0102 NPR=68", outboundMessages[0].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_Shipper_OrgNotFound()
		{
			CreateMawbAndCreateInboundQueuedMessage(true, "SHPR=Anything Here");
			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(1, outboundMessages.Length);
			AssertContains("UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: SHPR=ANYTHING HERE:FROM?:LHRBAC                                   SENT?: 11/12/1987 1?:02?:03::REQUEST REJECTED - NO MATCHING RECORDS FOUND:PAGE 01/END'UNT+5+1", outboundMessages[0].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_Shipper_MatchOrgOneDec()
		{
			CreateOrganisations();
			CreateMawbAndCreateInboundQueuedMessage(true, "SHPR=DANIEL CLARKE's COMPANY LTD "); // Match 
			var helper = new DeclarationTestHelper(Factory);
			var declaration = helper.CreateExportAirDeclaration();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_DeclarationType = "EFD";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.JE_LocationOfGoods = "LHR";
			declaration.SubLocation = "BAC";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = org3.MainAddress.PK;
			declaration.ZG_CTStatusID = "X";

			if (!declaration.CustomsEntryInstructions.Any())
			{
				declaration.CustomsEntryInstructions.AddNew();
			}

			declaration.ActiveEntryHeaders[0].CH_CEI_Instruction = declaration.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault().PK;
			Factory.Save();

			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(1, outboundMessages.Length);
			AssertContains("UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: SHPR=DANIEL CLARKE:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::REF   ?:8GB123456789000-B00001216   ORIG?:LHR    NPX?:10      NPR?:0  :MASTER?:08108051202                 DEST?:LHR    GWT?:100     NPD?:0  'FTX+AAA+++HOUSE ?:HOME0003                                            CT ?:X      :SHPMT ?:QF253                       DATE?::DESCR ?:3 HOLE                                                         :ENTRY ?:EFD               ROE?:        SOE?:        ICS?:            :STATUS?:                            DATE?:'FTX+AAA+++MUCR  ?:::SHPR  ?:DANIEL CLARKE?'S COMPANY LTD                                    :ADDR  ?:STREET3 BOROUGH MILKY BEANS MK14 6LY                           'FTX+AAA+++CNSEE ?:EMIRATES AIRLINES                                              :ADDR  ?:EMIRATES ENGINEERING BUILDING AIRPORT ROAD SYDNEY STATE POSTCOD::ARRVD ?:11/12/1987 00?:00:DEP   ?:                            DEST?:GBLHRBAC'UNT+8+1'", outboundMessages[0].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_Shipper_MatchOrgManyDecs()
		{
			CreateOrganisations();
			CreateMawbAndCreateInboundQueuedMessage(true, "SHPR=DANIEL CLARKE"); // Match 
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.SupplierDocumentaryAddress.E2_OA_Address = org3.MainAddress.PK;
			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.JE_TotalNoOfPieces = 69;
			declaration1.SingleEntry.CH_StyleOfEntrySOE = "H";
			declaration1.JE_GoodsDescription = "DUCR 1";
			declaration1.JE_RL_NKFinalDestination = "USNYC";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.SupplierDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			declaration2.CustomsEntryHeaders.AddNew();
			declaration2.JE_TotalNoOfPieces = 70;
			declaration2.SingleEntry.CH_StyleOfEntrySOE = "J";
			declaration2.JE_GoodsDescription = "STUFF TWO";
			declaration2.JE_RL_NKFinalDestination = "USBOS";
			Factory.Save();
			declaration2.JE_UCR += "/6";
			Factory.Save();
			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(1, outboundMessages.Length);
			AssertContains($"UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: SHPR=DANIEL CLARKE:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::DUCR                                PART  NPX DESCRIPTION     AOD S:----------------------------------- ---- ---- --------------- --- -'FTX+AAA+++7-{declaration1.JE_DeclarationReference}                              69   DUCR 1          NYC H:7-{declaration2.JE_DeclarationReference}                         /6   70   STUFF TWO       BOS J:PAGE 01/END'UNT+6+1'", outboundMessages[0].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_Shipper_MatchOrgButNoDeclarations()
		{
			CreateOrganisations();
			CreateMawbAndCreateInboundQueuedMessage(true, "SHPR=DANIEL CLARKE's COMPANY LTD "); // Match 
			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(1, outboundMessages.Length);
			AssertContains("UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: SHPR=DANIEL CLARKE:FROM?:LHRBAC                                   SENT?: 11/12/1987 1?:02?:03::REQUEST REJECTED - CLIENT FOUND BUT NO CONSIGNMENTS MATCH:PAGE 01/END'UNT+5+1'", outboundMessages[0].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_BasicWithSplits()
		{
			CreateMawbAndCreateInboundQueuedMessage(true, "MAWB=801-12345678");
			mawb.ChildBills[0].Delete();
			var basic = mawb;
			basic.Split(MakeSplitRequests(true));
			basic.Splits[0].SetCustomsActionCode("AD", ZDateTime.BrettsBirthday.AddYears(-50));
			basic.Splits[1].SetCustomsActionCode("DA", ZDateTime.BrettsBirthday.AddYears(50));
			Factory.Save();

			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(2, outboundMessages.Length);
			AssertContains("FSA/2:801-12345678LAXLHR/P68T15:OSI/SEE PRINTER SPOOLER", outboundMessages[0].EM_MessageText);
			AssertContains("UNH+2+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: MAWB=801-12345678:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::START RESPONSE TO RS INTERROGATION MAWB=801-12345678 FROM LHRBAC:HAWB     SRF NPX NPR NPD DESC                GWT    AGT CAC DATE     E'FTX+AAA+++(BASIC)  01  69  0   0   COLUMBIAN FLOUR     123.4  LXA DA  20210918  :(BASIC)  02  269 0   0   COLUMBIAN FLOUR     12345  LXA AD  19210918  :PAGE 01/END'UNT+6+2'", outboundMessages[1].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_Ducr_OneHit_Export()
		{
			var helper = new DeclarationTestHelper(Factory);
			var declaration = helper.CreateExportAirDeclaration();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			shipment.OuterPackLines.AddNew().JL_Outturn = 50;
			shipment.OuterPackLines.AddNew().JL_Outturn = 25;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MasterUCR = "A:12365498732";
			declaration.JE_ExportDate = ZDateTime.BrettsBirthday.AddYears(8);
			declaration.JE_LocationOfGoods = "LHR";
			declaration.SubLocation = "BAC";
			if (!declaration.CustomsEntryInstructions.Any())
			{
				var cei = declaration.CustomsEntryInstructions.AddNew();
				cei.CEI_Style = EU.Business.EntryStyleListExport.Codes.ExportNormal;
				cei.CEI_SubStyle = Business.CodeDescriptionPairLists.EntrySubStyleListExport.Codes.FullDeclarationGoodsNotArrived_IEFD;
			}

			declaration.DoMerge();
			var entry = declaration.CustomsEntryHeaders[0];

			entry.EntryNumber = "120-123456A";
			entry.CH_EntrySubmittedDate = ZDateTime.BrettsBirthday;
			entry.CH_CEI_Instruction = declaration.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault().PK;
			declaration.SingleEntry.CH_ImportClearanceStatusICS = "69";
			declaration.SingleEntry.CH_StyleOfEntrySOE = "70";
			declaration.SingleEntry.CH_RouteOfEntry = "6";
			declaration.JE_TotalNoOfPacks = 777;
			entry.CH_EntryStatus = EntryStatusList.Codes.Route1;
			declaration.ZG_LCPDepart = ZDateTime.Now.AddMonths(6);
			declaration.JE_LocationOfGoods = "LHR";
			declaration.SubLocation = "BAC";
			declaration.ZG_CTStatusID = "X";
			CreateMawbAndCreateInboundQueuedMessage(false, "DUCR=" + declaration.JE_UCR);
			Factory.Save();
			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(1, outboundMessages.Length);
			AssertContains("UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: DUCR=8GB123456789000-B00001216:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::REF   ?:8GB123456789000-B00001216   ORIG?:LHR    NPX?:777     NPR?:75 :MASTER?:08108051202                 DEST?:LHR    GWT?:100     NPD?:0  'FTX+AAA+++HOUSE ?:HOME0003                                            CT ?:X      :SHPMT ?:QF253                       DATE?:18/09/1979:DESCR ?:3 HOLE                                                         :ENTRY ?:EFD 120-123456A 18/09/1971  ROE?: 6      SOE?: 70     ICS?: 69    :STATUS?:                            DATE?:'FTX+AAA+++MUCR  ?:A?:12365498732::SHPR  ?:PREMIUM AIRCRAFT INTERIORS UK LTD                              :ADDR  ?:HEATH TECNA WATCHMOOR POINT CAMBERLEY GU15 3AQ                 'FTX+AAA+++CNSEE ?:EMIRATES AIRLINES                                              :ADDR  ?:EMIRATES ENGINEERING BUILDING AIRPORT ROAD SYDNEY STATE POSTCOD::ARRVD ?:11/12/1987 00?:00:DEP   ?:11/06/1988 01?:02            DEST?:GBLHRBAC'UNT+8+1'", outboundMessages[0].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_Ducr_OneHit_Import()
		{
			var helper = new DeclarationTestHelper(Factory);
			var declaration = helper.CreateImportAirDeclaration();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MasterUCR = "HBAC12365498732";
			declaration.JE_ExportDate = ZDateTime.BrettsBirthday.AddYears(8);
			declaration.JE_LocationOfGoods = "LHR";
			declaration.SubLocation = "BAC";
			if (!declaration.CustomsEntryInstructions.Any())
			{
				var cei = declaration.CustomsEntryInstructions.AddNew();
				cei.CEI_Style = EU.Business.EntryStyleListImport.Codes.ImportNormal;
				cei.CEI_SubStyle = Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.C21GoodsArrived;
			}

			declaration.DoMerge();
			var entry = declaration.CustomsEntryHeaders[0];

			entry.EntryNumber = "120-123456A";
			entry.CH_EntrySubmittedDate = ZDateTime.BrettsBirthday;
			entry.CH_CEI_Instruction = declaration.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault().PK;
			declaration.SingleEntry.CH_ImportClearanceStatusICS = "69";
			declaration.SingleEntry.CH_RouteOfEntry = "6";
			entry.CH_EntryStatus = EntryStatusList.Codes.Route1;
			declaration.SingleEntry.CH_StyleOfEntrySOE = "70";
			entry.CH_EntryStatus = EntryStatusList.Codes.Route1;
			declaration.ZG_LCPDepart = ZDateTime.Now.AddMonths(6);
			declaration.ZG_CTStatusID = "X";
			CreateMawbAndCreateInboundQueuedMessage(true, "DUCR=" + declaration.JE_UCR);
			var hawb = mawb.ChildBills[0];
			hawb.CS_JE_CustomsFormalEntry = declaration.PK;
			hawb.SetCustomsActionCode("XO", ZDateTime.BrettsBirthday.AddYears(77));
			hawb.CS_PiecesLanded = 666;
			hawb.CS_PiecesManifested = 777;
			var delivery1 = hawb.OutTurns.AddNew();
			var delivery2 = hawb.OutTurns.AddNew();
			delivery1.C5_ReceiptOnlyIndicator = true;
			delivery2.C5_ReceiptOnlyIndicator = true;
			delivery1.C5_PackagesOutturned = 50;
			delivery2.C5_PackagesOutturned = 25;

			hawb.LatestCustomsActionText = "OK TO FORNICATE";
			Factory.Save();
			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(1, outboundMessages.Length);
			AssertContains("UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: DUCR=8GB123456789000-B00001216:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::REF   ?:8GB123456789000-B00001216   ORIG?:SYD    NPX?:777     NPR?:666:MASTER?:08108051202                 DEST?:LHR    GWT?:100     NPD?:75 'FTX+AAA+++HOUSE ?:HOME0003                                            CT ?:X      :SHPMT ?:QF253                       DATE?:18/09/1979:DESCR ?:3 HOLE                                                         :ENTRY ?:IFD 120-123456A 18/09/1971  ROE?: 6      SOE?: 70     ICS?: 69    :STATUS?:XO-OK TO FORNICATE          DATE?:18/09/2048 00?:00'FTX+AAA+++MUCR  ?:HBAC12365498732::SHPR  ?:EMIRATES AIRLINES                                              :ADDR  ?:EMIRATES ENGINEERING BUILDING AIRPORT ROAD SYDNEY STATE POSTCOD'FTX+AAA+++CNSEE ?:PREMIUM AIRCRAFT INTERIORS UK LTD                              :ADDR  ?:HEATH TECNA WATCHMOOR POINT CAMBERLEY GU15 3AQ                 ::ARRVD ?:11/12/1987 00?:00:DEP   ?:11/06/1988 01?:02            DEST?:GBLHRBAC'UNT+8+1'", outboundMessages[0].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_Ducr_ManyHits()
		{
			var helper = new DeclarationTestHelper(Factory);
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.JE_TotalNoOfPieces = 68;
			declaration1.SingleEntry.CH_StyleOfEntrySOE = "H";
			declaration1.JE_GoodsDescription = "DUCR 1";
			declaration1.JE_RL_NKFinalDestination = "USNYC";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.CustomsEntryHeaders.AddNew();
			declaration2.JE_TotalNoOfPieces = 70;
			declaration2.SingleEntry.CH_StyleOfEntrySOE = "J";
			declaration2.JE_GoodsDescription = "STUFF TWO";
			declaration2.JE_RL_NKFinalDestination = "USBOS";
			Factory.Save();
			declaration2.JE_UCR = declaration1.JE_UCR + "/69";
			CreateMawbAndCreateInboundQueuedMessage(true, "DUCR=" + declaration1.JE_UCR);
			Factory.Save();
			RunProcessors();
			var outboundMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals(1, outboundMessages.Length);
			AssertContains($"UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: DUCR=7-{declaration1.JE_DeclarationReference}:FROM?:LHRBAC                                     SENT?: 11/12/1987 01?:02::DUCR                                PART  NPX DESCRIPTION     AOD S:----------------------------------- ---- ---- --------------- --- -'FTX+AAA+++7-{declaration1.JE_DeclarationReference}                              68   DUCR 1          NYC H:7-{declaration1.JE_DeclarationReference}                         /69  70   STUFF TWO       BOS J:PAGE 01/END'UNT+6+1'", outboundMessages[0].EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_NoMatchesFound()
		{
			CreateMawbAndCreateInboundQueuedMessage(true, "MAWB=XXX-12345678");
			RunProcessors();
			var outboundMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals("GEN", outboundMessage.EM_MessageType);
			AssertEquals("TXT", outboundMessage.EM_MessageSubType);
			AssertEquals("CUKCTM98AAABBB/LHR69", outboundMessage.EM_ApplicationReference);
			AssertEquals("CUKFFW98LHRBAC", outboundMessage.EM_MessageOwner);
			AssertEquals("UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: MAWB=XXX-12345678:FROM?:LHRBAC                                   SENT?: 11/12/1987 1?:02?:03::REQUEST REJECTED - NO MATCHING RECORDS FOUND:PAGE 01/END'UNT+5+1'", outboundMessage.EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_InvalidRequest()
		{
			CreateMawbAndCreateInboundQueuedMessage(true, "00000000000");
			RunProcessors();
			var outboundMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals("GEN", outboundMessage.EM_MessageType);
			AssertEquals("TXT", outboundMessage.EM_MessageSubType);
			AssertEquals("CUKCTM98AAABBB/LHR69", outboundMessage.EM_ApplicationReference);
			AssertContains("UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: 00000000000:FROM?:LHRBAC                                   SENT?: 11/12/1987 1?:02?:03::REQUEST REJECTED - INVALID DATA IN REQUEST:PAGE 01/END'UNT+5+1'", outboundMessage.EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("RCV", inboundMessage.EM_Status);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestRespondToLucasEnquiry_UnexpectedSystemError()
		{
			CreateMawbAndCreateInboundQueuedMessage(true, "MAWB=801-12345678");
			var mawb = Factory.LoadTop1<CusMAWB>(new ZQuery());
			mawb.CargoTerminalOperatorAirport = "XXX";
			Factory.Save();
			RunProcessors();
			var outboundMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, "TRX"));
			AssertEquals("GEN", outboundMessage.EM_MessageType);
			AssertEquals("TXT", outboundMessage.EM_MessageSubType);
			AssertEquals("CUKCTM98AAABBB/LHR69", outboundMessage.EM_ApplicationReference);
			AssertEquals("CUKFFW98LHRBAC", outboundMessage.EM_MessageOwner);
			AssertContains("UNH+1+GENRAL:0:912:UN+COMMONACCESSREFERENCE'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++DEP RESPONSE FOR?: MAWB=801-12345678:FROM?:LHRBAC                                   SENT?: 11/12/1987 1?:02?:03::REQUEST REJECTED - SYSTEM FAILURE. CONTACT DEP OPERATOR:PAGE 01/END'UNT+5+1'", outboundMessage.EM_MessageText);
			inboundMessage.Reload();
			AssertEquals("ERR", inboundMessage.EM_Status);
		}

		void CreateMawbAndCreateInboundQueuedMessage(bool createAwbToo, string enquiryQueryString, bool putInCar = true)
		{
			var inboundInterchangeText = "UNB+UNOA:2+CUKCTM98AAABBB/LHR69:IATA+CUKFFW98LHRBAC:IATA+110324:1407+72'UNH+94+GENRAL:0:912:UN'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++"
											+ enquiryQueryString + "'UNT+5+94'UNZ+1+72'";

			if (putInCar)
			{
				inboundInterchangeText = inboundInterchangeText.Replace("UN'BGM", "UN+COMMONACCESSREFERENCE'BGM");
			}
			if (createAwbToo)
			{
				mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			}
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, inboundInterchangeText, "CUK");
			inboundMessage = interchange.ContainedMessages[0];
			Factory.Save();
		}

		void CreateOrganisations()
		{
			org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "DAN1";
			org1.OH_FullName = "DANIEL";
			MakeNewAddress(org1, "Street1");

			org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "DAN2";
			org2.OH_FullName = "DANIEL CLARKE";
			MakeNewAddress(org2, "Street2");

			org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "DAN3";
			org3.OH_FullName = "DANIEL CLARKE'S COMPANY LTD";
			MakeNewAddress(org3, "Street3");
		}

		void MakeNewAddress(OrgHeader org, string street)
		{
			var address = org.MainAddress;
			address.OA_Address1 = street;
			address.OA_Address2 = "Borough";
			address.OA_State = "Bucks";
			address.OA_City = "Milky Beans";
			address.OA_PostCode = "MK14 6LY";
			address.OA_RL_NKRelatedPortCode = "GBMIK";
		}

		List<ICuscarLine> MakeSplitRequests(bool reorder = false)
		{
			var split1 = new CuscarLineWithFlagsToShowWhatsSet();
			split1.DescriptionOfGoods = "Very long description goes here yo";
			split1.LineOrSplitNumber = "01";
			split1.NumberOfPiecesExpected = 69;
			split1.NumberOfPiecesReceived = 68;
			split1.Weight = 123.4m;

			var split2 = new CuscarLineWithFlagsToShowWhatsSet();
			split2.DescriptionOfGoods = "Line two";
			split2.LineOrSplitNumber = "02";
			split2.NumberOfPiecesExpected = 269;
			split2.NumberOfPiecesReceived = 268;
			split2.Weight = 12345m;
			if (reorder)
			{
				return new List<ICuscarLine> { split2, split1 };
			}
			else
			{
				return new List<ICuscarLine> { split1, split2 };
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			branchEnvironment = DisposableEnvironment.ForBranch(DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDown()
		{
			base.TearDown();
			branchEnvironment.Dispose();
		}

		OrgHeader org1;
		OrgHeader org2;
		OrgHeader org3;
		CusMAWB mawb;
		EDIMessage inboundMessage;
	}
}
