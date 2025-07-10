using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	// The example number pertain to the example files in the message spec doc, chapter 22.5.

	class FsaTests : CcsukNonChiefResponseBaseMessageProcessorTest
	{
		public void TestFsaResponseForUnknownSplitUpdatesTheSplitAndNotTheParentToPresenceNo_SplitHouse()
		{
			// Send FSR for split, response says no record found. Update (currently: set presence to NO but maybe one day delete the split) the correct record - i.e. the split and not the parent house
			//01666552264+7:1310250401:201++HWB:09187256:01
			CusMAWB mawb;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out mawb, out outboundMessage, false);
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "09187256";
			hawb.Messages.Add(outboundMessage);
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			var splitHouse = hawb.Splits.AddNew();
			splitHouse.SplitReference = "01";
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[31], true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			var hawbReloaded = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			var splitReloaded = new BusinessObjectFactory().Load<SplitHouse>(splitHouse.PK);
			AssertEquals("House's status not updated by FSA", PresenceOnNetworkList.Codes.OnCommDb, hawbReloaded.PresenceOnNetworkStatus);
			AssertEquals("Split's status is updated by FSA", PresenceOnNetworkList.Codes.NotOnCommDb, splitReloaded.PresenceOnNetworkStatus);
		}

		public void TestFsaResponseForUnknownSplitUpdatesTheSplitAndNotTheParentToPresenceNo_SplitBasic()
		{
			CusMAWB basic;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out basic, out outboundMessage, true);
			basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			var splitBasic = basic.Splits.AddNew();
			splitBasic.SplitReference = "01";
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[31].Replace("HWB:09187256:01", "ACD::01"), true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			var basicReloaded = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			var splitReloaded = new BusinessObjectFactory().Load<SplitBasic>(splitBasic.PK);
			AssertEquals("Basic's status not updated by FSA", PresenceOnNetworkList.Codes.OnCommDb, basicReloaded.PresenceOnNetworkStatus);
			AssertEquals("Split's status is updated by FSA", PresenceOnNetworkList.Codes.NotOnCommDb, splitReloaded.PresenceOnNetworkStatus);
		}

		public void TestFsaResponseForUnknownSplitUpdatesRecordToPresenceNo_Basic()
		{
			CusMAWB basic;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out basic, out outboundMessage, true);
			basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[31].Replace("HWB:09187256:01", ""), true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			var basicReloaded = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("Basic's status is not updated by FSA", PresenceOnNetworkList.Codes.NotOnCommDb, basicReloaded.PresenceOnNetworkStatus);
		}

		public void TestFsaResponseForUnknownSplitUpdatesRecordToPresenceNo_House()
		{
			CusMAWB mawb;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out mawb, out outboundMessage, false);
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "09187256";
			hawb.Messages.Add(outboundMessage);
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[31], true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			var hawbReloaded = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertEquals("House's status updated by FSA", PresenceOnNetworkList.Codes.NotOnCommDb, hawbReloaded.PresenceOnNetworkStatus);
		}

		void CreateBasicForFsrFsaUpdateTest(out CusMAWB basic, out EDIMessage outboundMessage, bool addMessageToBasic = true)
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();

			basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "12510000888";
			basic.Profile = "CUKFFW98000LXA"; // to make an agent job

			var mockEdiMessageOutbound = Factory.New<DummyEDIMessage_FsaTests>();
			mockEdiMessageOutbound.GetMessageReferenceNumberReturns = "716";
			outboundMessage = mockEdiMessageOutbound;
			outboundMessage.EM_MessageText = "Outbound message <<MSGNO PLACEHOLDER>>";
			outboundMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;
			outboundMessage.EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.Code;
			outboundMessage.EM_MessageSubType = CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate.Subcode;
			if (addMessageToBasic)
			{
				basic.Messages.Add(outboundMessage);
			}
		}

		public void TestDeleteBogusLocalSplits()
		{
			CusMAWB basic;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out basic, out outboundMessage);
			var split = basic.Splits.AddNew();
			basic.Splits.AddNew();
			split.SetCustomsActionCode("CX", ZDateTime.Now);
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[5].Replace("DEI", "LXA"), true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			var basicReloadedCachingIsSoAnnoying = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("Bogus splits removed", 0, basicReloadedCachingIsSoAnnoying.Splits.Count);
			AssertEquals("Basic's pseudo-CAC is wiped, no longer '--'", "", basicReloadedCachingIsSoAnnoying.CustomsActionCode);
		}

		public void TestDeleteBogusLocalSplitsStatus3()
		{
			CusMAWB basic;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out basic, out outboundMessage);
			var s1 = basic.Splits.AddNew();
			var s2 = basic.Splits.AddNew();
			s2.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval, ZDateTime.BrettsBirthday);
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[5].Replace("DEI", "LXA"), true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			var basicReloadedCachingIsSoAnnoying = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("Bogus splits are NOT removed when one or more has St3 locally ", 2, basicReloadedCachingIsSoAnnoying.Splits.Count);
		}

		public void TestDeleteBogusLocalSplits_KeepWhenFsaIsForSplitItself()
		{
			// When we query a split the response will not state that splits exist, so we should not delete the splits from the parent
			CusMAWB basic;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out basic, out outboundMessage);
			var s1 = basic.Splits.AddNew();
			s1.SplitReference = "01";
			var s2 = basic.Splits.AddNew();
			s2.SplitReference = "02";
			Factory.Save();
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[4].Replace("DEI", "LXA"), true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			var basicReloadedCachingIsSoAnnoying = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("Bogus splits are NOT removed when the FSA pertains to the split itself", 2, basicReloadedCachingIsSoAnnoying.Splits.Count);
		}

		public void TestStandaloneFsrEnquiryParsing()
		{
			var npbo = NonPersistentStandAloneFsrEnquiryForNewTest.GetNonPersistentStandAloneFsrEnquiryForNew(Factory);
			var persistent = StandAloneFsrEnquiry.MakeNewOutboundFromPayload(npbo);
			Factory.Save();
			PrepareExampleAndParse("UNH+12708+CUKFSA:1:912:BT+<<SYSCAR>>'BGM++21120717001+7:1207181028:201'FTX+AAA+++BASIC CONSIGNMENT RECORD RETRIEVED WITH ENTRY DETAILS'DOC+740+21120717001+97:1207170922:201++++OLD'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+123++++BA:172:3++178:120717:101'LOC+84:SYD:145:3+85:LHR:145:3+27:AU+11:LHR:145:3::CWE:129:ZZZ:ERT'TDT+12++40'LOC+85:LHR:145:3::CAX:129:ZZZ'NAD+CB+WIS+CARGOWISE'GDS+2'QTY+118:1'QTY+48:1'MEA+WT++KGM:1.0'DTM+7:1207170922:201'CST++CB:117:ZZZ+66:120:109+S:141:109'FTX+CAT+++OK TFR SHD CAX'DTM+176:1207170927:201'UNT+20+12708'", true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(persistent));
			persistent.Reload();
			var inbound = persistent.LinkedMessage;
			inbound.Reload();
			AssertNotNull(inbound);
			AssertEquals("RCV", inbound.EM_Status);
			AssertEquals("BASIC CONSIGNMENT RECORD RETRIEVED WITH ENTRY DETAILS", persistent.ResponseText);
			AssertContains("<td>OK TFR SHD CAX", inbound.EM_MessageInterpretation);
			AssertEquals("FSR", inbound.EM_MessageType);
			AssertEquals("ENQ", inbound.EM_MessageSubType);
		}

		public void TestStandaloneFsrEnquiryParsingNoRecordFound()
		{
			var npbo = NonPersistentStandAloneFsrEnquiryForNewTest.GetNonPersistentStandAloneFsrEnquiryForNew(Factory);
			var persistent = StandAloneFsrEnquiry.MakeNewOutboundFromPayload(npbo);
			Factory.Save();
			PrepareExampleAndParse("UNH+12711+CUKFSA:1:912:BT+<<SYSCAR>>'BGM++24324324234+7:1207181128:201'DOC+740+24324324234'GIS+2'FTX+AAA+++REJECTED - NO MATCHING CONSIGNMENT RECORD FOUND'UNT+6+12711'", true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(persistent));
			persistent.Reload();
			var inbound = persistent.LinkedMessage;
			inbound.Reload();
			AssertNotNull(inbound);
			AssertEquals("RCV", inbound.EM_Status);
			AssertEquals("REJECTED - NO MATCHING CONSIGNMENT RECORD FOUND", persistent.ResponseText);
			AssertContains("<td>REJECTED - NO MATCHING CONSIGNMENT RECORD FOUND", inbound.EM_MessageInterpretation);
		}

		public void TestResponseText()
		{
			var npbo = NonPersistentStandAloneFsrEnquiryForNewTest.GetNonPersistentStandAloneFsrEnquiryForNew(Factory);
			var request = StandAloneFsrEnquiry.MakeNewOutboundFromPayload(npbo);
			Factory.Save();
			AssertEquals(false, request.HasLinkedMessage);
			AssertEquals("No reply has been received yet", request.ResponseText);
			PrepareExampleAndParse("UNH+JRI1CZQL9QBNA0+CUKFSA:2:912:BT+<<SYSCAR>>'BGM+:::U+12516011354+7:1601141434:201'DOC+740+12516011354'TDT+20++40'LOC+84:ATL:145:3+11:LHR:145:3::CAX:129:ZZZ'GDS+2'QTY+118:10'DOC+740+12516011354'TDT+20++40'LOC+11:LHR:145:3::CWE:129:ZZZ'GDS+2'QTY+118:10'UNT+13+JRI1CZQL9QBNA0'", true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(request));
			request.Reload();
			AssertEquals(" ** Please open the record to see the full response ** ", request.ResponseText);
			PrepareExampleAndParse("UNH+12711+CUKFSA:1:912:BT+<<SYSCAR>>'BGM++24324324234+7:1207181128:201'DOC+740+24324324234'GIS+2'FTX+AAA+++Error Occurred'UNT+6+12711'", true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(request));
			request.Reload();
			AssertEquals("ERROR OCCURRED", request.ResponseText);
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestParseFsaCannotFindJobEver()
		{
			var emailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukErrors, Guid.Empty, Factory);
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[1], true);
			receivedMessage.Reload();
			receivedMessage.Interchange.Reload();
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 1, 0), receivedMessage.EM_HeldUntilDate);  // one minute into future
			AssertEquals(1, receivedMessage.Interchange.EI_RetryCount);
			AssertEquals("QUE", receivedMessage.EM_Status);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			receivedMessage.EM_HeldUntilDate = new ZDateTime(1986, 3, 12, 1, 0, 0);  // rewind to past to simulate real time moving forward
			Factory.Save();
			RunProcessors();
			receivedMessage.Reload();
			receivedMessage.Interchange.Reload();
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 1, 0), receivedMessage.EM_HeldUntilDate);
			AssertEquals(2, receivedMessage.Interchange.EI_RetryCount);
			AssertEquals("QUE", receivedMessage.EM_Status);

			receivedMessage.EM_HeldUntilDate = new ZDateTime(1986, 3, 12, 5, 0, 0);  // fast forward to future - should not run again
			Factory.Save();
			RunProcessors();
			receivedMessage.Reload();
			receivedMessage.Interchange.Reload();
			AssertEquals(new ZDateTime(1986, 3, 12, 5, 0, 0), receivedMessage.EM_HeldUntilDate);  // not touched
			AssertEquals(2, receivedMessage.Interchange.EI_RetryCount);  // not increased
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			receivedMessage.EM_HeldUntilDate = new ZDateTime(1986, 3, 12, 1, 0, 0);  // rewind
			receivedMessage.Interchange.EI_RetryCount = 11;
			Factory.Save();
			RunProcessors();
			receivedMessage.Reload();
			receivedMessage.Interchange.Reload();
			AssertEquals("FAL", receivedMessage.EM_Status);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			AssertContains(emailAddress, Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestParseFsaCannotFindJobOnFirstGoButSuccessLater()
		{
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[1], true);
			receivedMessage.Reload();
			receivedMessage.Interchange.Reload();
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 1, 0), receivedMessage.EM_HeldUntilDate);
			AssertEquals(1, receivedMessage.Interchange.EI_RetryCount);
			AssertEquals("QUE", receivedMessage.EM_Status);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			receivedMessage.EM_HeldUntilDate = new ZDateTime(1986, 3, 12, 1, 0, 0);  // rewind to past to simulate real time moving forward
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "88893080900";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";
			Factory.Save();
			RunProcessors();
			basic.Reload();
			receivedMessage.Reload();
			receivedMessage.Interchange.Reload();
			AssertEquals("RCV", receivedMessage.EM_Status);
			AssertEquals(1, basic.Messages.Count);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
		}

		public void TestParseFsaWithUpdateJobFlagSetBasic()
		{
			CusMAWB basic;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out basic, out outboundMessage);

			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[5].Replace("DEI", "LXA"), true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));

			var basicReloadedCachingIsSoAnnoying = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("Date of customs action applied to job", new ZDateTime(1993, 08, 09, 14, 24, 0), basicReloadedCachingIsSoAnnoying.CustomsActionDate);
			AssertEquals("CA", basicReloadedCachingIsSoAnnoying.CustomsActionCode);
			AssertEquals("ACCEPTED", basicReloadedCachingIsSoAnnoying.LatestCustomsActionText);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, basicReloadedCachingIsSoAnnoying.PresenceOnNetworkStatus);
			AssertNotContains("No update of the local record has been performed", basicReloadedCachingIsSoAnnoying.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertEquals(new ZDateTime(1993, 08, 09, 10, 00, 00), basicReloadedCachingIsSoAnnoying.Status1Date);
		}

		public void TestParseFsaWithUpdateJobFlagSetBasicWithSplits_SomeAlreadyExist()
		{
			GBCustomsDataRegistry.Instance.CcsukQueryChildObjectsWhenMentionedInFsa.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CusMAWB basic;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out basic, out outboundMessage);
			var existingSplit1 = basic.Splits.AddNew();
			existingSplit1.SplitReference = "01";
			existingSplit1.HandlingInformation = "One";
			existingSplit1.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			existingSplit1.LatestCustomsActionText = "POOP";
			existingSplit1.NumberOfPiecesExpected = 10;
			existingSplit1.NumberOfPiecesReceived = 5;
			var basicFsaWithSplits = "UNH+10301+CUKFSA:1:912:BT+<<SYSCAR>>'BGM++12510000888+7:1112151537:201'FTX+AAA+++BASIC CONSIGNMENT RECORD RETRIEVED WHICH HAS SPLITS'DOC+740+12510000888+97:1112151525:201++++OLD'GIS+T:121:ZZZ'TDT+20+001++++BA:172:3++178:111215:101'LOC+84:JFK:145:3+85:LHR:145:3+27:US+11:LHR:145:3::HMC:129:ZZZ:HMC'NAD+CB+LXA+HMC TRAINING'NAD+CM+LHRHMC'GDS+2'QTY+118:10'MEA+WT++KGM:100.0'DOC+SRF:ZZZ+01'TDT+20'LOC+11:LHR:145:3::HMC:129:ZZZ'DOC+SRF:ZZZ+02'TDT+20'LOC+11:LHR:145:3::HMC:129:ZZZ'UNT+19+10301'";
			var result = PrepareExampleAndParse(basicFsaWithSplits, true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			var basicInNewFactory = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("Basic now has TWO splits - one more created, none deleted", 2, basicInNewFactory.Splits.Count);
			var split1Reloaded = basicInNewFactory.Splits["01"];
			AssertEquals("Split 1 not clobbered", "One", split1Reloaded.HandlingInformation);
			AssertEquals(2, basicInNewFactory.Messages.Count);
			AssertEquals("The basic-level FSA mentioned a split, but we do not updates its properties because too little is returned. Value unchanged.", "CC", split1Reloaded.CustomsActionCode);
			AssertEquals("The basic-level FSA mentioned a split, but we do not updates its properties because too little is returned. Value unchanged.", "POOP", split1Reloaded.LatestCustomsActionText);
			AssertEquals("The basic-level FSA mentioned a split, but we do not updates its properties because too little is returned. Value unchanged.", (ZShort)5, split1Reloaded.NumberOfPiecesReceived);
			AssertEquals("The basic-level FSA mentioned a split, but we do not updates its properties because too little is returned. Value unchanged.", (ZShort)10, split1Reloaded.NumberOfPiecesExpected);
		}

		public void TestParseFsaWithUpdateJobFlagSetBasicWithSplits_NoneAlreadyExist()
		{
			CusMAWB basic;
			EDIMessage outboundMessage;
			GBCustomsDataRegistry.Instance.CcsukQueryChildObjectsWhenMentionedInFsa.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CreateBasicForFsrFsaUpdateTest(out basic, out outboundMessage);
			var basicFsaWithSplits = "UNH+10301+CUKFSA:1:912:BT+<<SYSCAR>>'BGM++12510000888+7:1112151537:201'FTX+AAA+++BASIC CONSIGNMENT RECORD RETRIEVED WHICH HAS SPLITS'DOC+740+12510000888+97:1112151525:201++++OLD'GIS+T:121:ZZZ'TDT+20+001++++BA:172:3++178:111215:101'LOC+84:JFK:145:3+85:LHR:145:3+27:US+11:LHR:145:3::HMC:129:ZZZ:HMC'NAD+CB+LXA+HMC TRAINING'NAD+CM+LHRHMC'GDS+2'QTY+118:10'MEA+WT++KGM:100.0'DOC+SRF:ZZZ+01'TDT+20'LOC+11:LHR:145:3::HMC:129:ZZZ'DOC+SRF:ZZZ+02'TDT+20'LOC+11:LHR:145:3::HMC:129:ZZZ'UNT+19+10301'";
			var result = PrepareExampleAndParse(basicFsaWithSplits, true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			var basicInNewFactory = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("Basic now has TWO splits", 2, basicInNewFactory.Splits.Count);
			AssertEquals("Original FSR, FSA, plus two new outbound FSRs", 4, basicInNewFactory.Messages.Count);
			var queuedOutboundMessages = (from EDIMessage m in basicInNewFactory.Messages where m.EM_Status == EDIMessage.Status.Queued && m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit orderby m.EM_MessageNum select m);
			foreach (EDIMessage fsr in queuedOutboundMessages)
			{
				AssertContains("Message is an FSR", "CUKFSR:1:912:BT", fsr.EM_MessageText);
				AssertContains("Message pertains to the correct split", "BGM++12510000888+++ACD::0" + fsr.EM_MessageNum, fsr.EM_MessageText);
				AssertContains("Message requests update (FAU)", "FSRFAU", fsr.EM_MessageType + fsr.EM_MessageSubType);
			}
		}

		public void TestParseFsaWithUpdateJobFlagSetSplitOnBasic()
		{
			CusMAWB basic;
			EDIMessage outboundMessage;
			GBCustomsDataRegistry.Instance.CcsukQueryChildObjectsWhenMentionedInFsa.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CreateBasicForFsrFsaUpdateTest(out basic, out outboundMessage);
			var splitNumberOne = basic.Splits.AddNew();
			var splitNumberTwoWillBeUpdated = basic.Splits.AddNew();
			splitNumberOne.SplitReference = "01";
			splitNumberTwoWillBeUpdated.SplitReference = "02";
			splitNumberTwoWillBeUpdated.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday);
			splitNumberTwoWillBeUpdated.LatestCustomsActionText = "REQUEST ACCEPTED";
			splitNumberTwoWillBeUpdated.NumberOfPiecesExpected = 2;
			splitNumberTwoWillBeUpdated.NumberOfPiecesReceived = 1;
			splitNumberTwoWillBeUpdated.Weight = 15;
			splitNumberTwoWillBeUpdated.WeightCode = "XX";
			var fsaforOneSplit = "UNH+10556+CUKFSA:1:912:BT+<<SYSCAR>>'BGM++12510000888+7:1112301309:201++ACD::02'FTX+AAA+++SPLIT CONSIGNMENT RECORD RETRIEVED WITH ENTRY DETAILS'DOC+SRF:ZZZ+02+97:1112301307:201++++OLD'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+009++++BA:172:3++178:111230:101'LOC+84:ATL:145:3+85:LHR:145:3+27:US+11:LHR:145:3::CWE:129:ZZZ:CWE'TDT+12++40'LOC+85:LGW:145:3::BAC:129:ZZZ'NAD+CB+DJC+CARGOWISE'NAD+CM+LHRCWE'GDS+2'QTY+118:6'QTY+48:5'MEA+WT++KGM:60.0'DTM+7:1112301307:201'FTX+AAA+++SPLIT FSA'CST++CW:117:ZZZ+88:120:109+C:141:109'FTX+CAT+++OK TRANSFER LGW'DTM+176:1112301307:201'RFF+ABE:00000510'UNT+23+10556'";
			RunFsrFauFsaForParentWithSplits<CusMAWB, SplitBasic>(basic, outboundMessage, ref splitNumberOne, ref splitNumberTwoWillBeUpdated, fsaforOneSplit);
		}

		void RunFsrFauFsaForParentWithSplits<typeOfParentAwb, typeOfSplitAwb>(ICcsukCusAwb parentAwb, EDIMessage outboundMessage, ref SplitConsignment splitNumberOne, ref SplitConsignment splitNumberTwoWillBeUpdated, string fsaforOneSplit)
			where typeOfParentAwb : BusinessObject, ICcsukCusAwb
			where typeOfSplitAwb : SplitConsignment
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false, false, "", "CWE", "LHR");
			var result = PrepareExampleAndParse(fsaforOneSplit, true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			var newFactoryBecauseCaching = new BusinessObjectFactory();
			var basicInNewFactory = newFactoryBecauseCaching.Load<typeOfParentAwb>(parentAwb.PK);
			AssertEquals("AWB still has two splits", 2, basicInNewFactory.Splits.Count);
			AssertEquals("parent has just TWO messages, original FSR and inbound FSA. We do NOT create further FSR requests.", 2, basicInNewFactory.Messages.Count);
			splitNumberOne = newFactoryBecauseCaching.Load<typeOfSplitAwb>(splitNumberOne.PK);
			splitNumberTwoWillBeUpdated = newFactoryBecauseCaching.Load<typeOfSplitAwb>(splitNumberTwoWillBeUpdated.PK);
			AssertEquals(6, (int)splitNumberTwoWillBeUpdated.NumberOfPiecesExpected);
			AssertEquals(5, (int)splitNumberTwoWillBeUpdated.NumberOfPiecesReceived);
			AssertEquals(PresenceOnNetworkList.Codes.CompletedOnCcsUk, splitNumberTwoWillBeUpdated.PresenceOnNetworkStatus);
			AssertEquals("CW", splitNumberTwoWillBeUpdated.CustomsActionCode);
			AssertEquals("OK TRANSFER LGW", splitNumberTwoWillBeUpdated.LatestCustomsActionText);
			AssertEquals(new ZDateTime(2011, 12, 30, 13, 07, 0), splitNumberTwoWillBeUpdated.CustomsActionDate);
			AssertEquals(60m, splitNumberTwoWillBeUpdated.Weight);
			AssertEquals("KG", splitNumberTwoWillBeUpdated.WeightCode);
			AssertEquals(new ZDateTime(2011, 12, 30, 13, 07, 0), splitNumberTwoWillBeUpdated.Status1Date);
			AssertEquals(0, (int)splitNumberOne.NumberOfPiecesExpected);
			AssertEquals(0, (int)splitNumberOne.NumberOfPiecesReceived);
			AssertEquals(0m, splitNumberOne.Weight);
			AssertEquals("DJC", splitNumberTwoWillBeUpdated.AgentBadge);
		}

		public void TestParseFsaWithUpdateJobFlagSetSplitOnHouse()
		{
			CusMAWB mawb;
			EDIMessage outboundMessage;
			GBCustomsDataRegistry.Instance.CcsukQueryChildObjectsWhenMentionedInFsa.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CreateBasicForFsrFsaUpdateTest(out mawb, out outboundMessage, false);
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HOUSE001";
			hawb.Messages.Add(outboundMessage);
			var splitNumberOne = hawb.Splits.AddNew();
			var splitNumberTwo = hawb.Splits.AddNew();
			splitNumberOne.SplitReference = "01";
			splitNumberTwo.SplitReference = "02";
			var fsaforOneSplit = "UNH+10556+CUKFSA:1:912:BT+<<SYSCAR>>'BGM++12510000888+7:1112301309:201++HWB:HOUSE001:02'FTX+AAA+++SPLIT CONSIGNMENT RECORD RETRIEVED WITH ENTRY DETAILS'DOC+SRF:ZZZ+02+97:1112301307:201++++OLD'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+009++++BA:172:3++178:111230:101'LOC+84:ATL:145:3+85:LHR:145:3+27:US+11:LHR:145:3::CWE:129:ZZZ:CWE'TDT+12++40'LOC+85:LGW:145:3::BAC:129:ZZZ'NAD+CB+DJC+CARGOWISE'NAD+CM+LHRCWE'GDS+2'QTY+118:6'QTY+48:5'MEA+WT++KGM:60.0'DTM+7:1112301307:201'FTX+AAA+++SPLIT FSA'CST++CW:117:ZZZ+88:120:109+C:141:109'FTX+CAT+++OK TRANSFER LGW'DTM+176:1112301307:201'RFF+ABE:00000510'UNT+23+10556'";
			RunFsrFauFsaForParentWithSplits<CusHAWB, SplitHouse>(hawb, outboundMessage, ref splitNumberOne, ref splitNumberTwo, fsaforOneSplit);
		}

		public void TestParseFsaNoRecordFoundBasic_FsrWithoutUpdate()
		{
			// We still update the presence to NO even when the request was FSA (not FsaWithUpdate)
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "No matter";
			FsrFsaCycleTestRunner(CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode, basic);
		}

		public void TestParseFsaNoRecordFoundBasic_FsrWithUpdate()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "No matter";
			FsrFsaCycleTestRunner(CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate.Subcode, basic);
		}

		public void TestParseFsaNoRecordFoundLastHouseInConsol_OneHouse()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "12387654321";
			var oneAndOnlyHawb = mawb.ChildBills.AddNew();
			FsrFsaCycleTestRunner(CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode, oneAndOnlyHawb);
			mawb.Reload();
			AssertEquals("The last house has been deleted so we query the consol", 1, mawb.Messages.Count);
			AssertContains("BGM++12387654321+++++FSA'", mawb.Messages[0].EM_MessageText);
		}

		public void TestParseFsaNoRecordFoundLastHouseInConsol_ManyHouses()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "12387654321";
			var oneHouse = mawb.ChildBills.AddNew();
			var anotherHouse = mawb.ChildBills.AddNew();
			anotherHouse.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			FsrFsaCycleTestRunner(CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode, oneHouse);
			mawb.Reload();
			AssertEquals("No FSR for mawb as another house exists", 0, mawb.Messages.Count);
		}

		public void TestParseFsaNoRecordFoundLastHouseInConsol_ManyHousesButAllOthersDeleted()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "12387654321";
			var oneHouse = mawb.ChildBills.AddNew();
			var anotherHouse = mawb.ChildBills.AddNew();
			anotherHouse.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
			FsrFsaCycleTestRunner(CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode, oneHouse);
			mawb.Reload();
			AssertEquals("Mawb has FSR as the last hawb has been deleted", 1, mawb.Messages.Count);
			AssertContains("BGM++12387654321+++++FSA'", mawb.Messages[0].EM_MessageText);
		}

		void FsrFsaCycleTestRunner(string fsrRequestSubType_UpdateOrNoUpdate, ICcsukCusAwb awb)
		{
			var mockEdiMessageOutbound = Factory.New<DummyEDIMessage_FsaTests>();
			mockEdiMessageOutbound.GetMessageReferenceNumberReturns = "716";
			mockEdiMessageOutbound.EM_MessageText = "Outbound message <<MSGNO PLACEHOLDER>>";
			mockEdiMessageOutbound.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			mockEdiMessageOutbound.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			mockEdiMessageOutbound.EM_Status = EDIMessage.Status.Sent;
			mockEdiMessageOutbound.EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.Code;
			mockEdiMessageOutbound.EM_MessageSubType = fsrRequestSubType_UpdateOrNoUpdate;
			awb.Messages.Add(mockEdiMessageOutbound);

			_ = PrepareExampleAndParse(exampleFromSpecificationDocument[10], true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(mockEdiMessageOutbound));
			((BusinessObject)awb).Reload();
			AssertEquals("Awb is updated to NOT ON NETWORK", PresenceOnNetworkList.Codes.NotOnCommDb, awb.PresenceOnNetworkStatus);
		}

		public void TestParseFsaWithUpdateJobFlagSetMawb()
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "88893080901";
			mawb.Profile = "CUKFFW98000LXA";

			var mockEdiMessageOutbound = Factory.New<DummyEDIMessage_FsaTests>();
			mockEdiMessageOutbound.GetMessageReferenceNumberReturns = "716";
			mockEdiMessageOutbound.EM_MessageText = "Outbound message <<MSGNO PLACEHOLDER>>";
			mockEdiMessageOutbound.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			mockEdiMessageOutbound.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			mockEdiMessageOutbound.EM_Status = EDIMessage.Status.Sent;
			mockEdiMessageOutbound.EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.Code;
			mockEdiMessageOutbound.EM_MessageSubType = CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate.Subcode;
			mawb.Messages.Add(mockEdiMessageOutbound);

			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[7], true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(mockEdiMessageOutbound));
			mawb.Reload();

			AssertEquals("AR456", mawb.CM_FlightNo);
			AssertEquals(new ZDate(1993, 08, 09), mawb.CM_ArrivalDate);
			AssertEquals("FRANT", mawb.AirportOfOrigin);
			AssertEquals("STN", mawb.AirportOfDestination);
			AssertEquals("MAN", mawb.AirportOfArrival);
			AssertEquals("KLM", mawb.CargoTerminalOperator);
			AssertEquals("MAN", mawb.CargoTerminalOperatorAirport);
			AssertEquals("T", mawb.ShipmentDescriptionCode);
			AssertEquals("LXA", mawb.AgentBadge);
			AssertEquals(300, (int)mawb.NumberOfPiecesExpected);
			AssertEquals(0, (int)mawb.NumberOfPiecesReceived);
			AssertEquals(300m, mawb.Weight);
			AssertEquals("KG", mawb.WeightCode);
			AssertEquals(EDIInterchange.Status.Received, new BusinessObjectFactory().Load<CusMAWB>(mawb.PK).Messages.LastIncomingMessage.Interchange.EI_Status);
		}

		public void TestParseFsaWithUpdateJobFlagSetMawbWithCommunityHandlingCodes()
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "88893080901";
			mawb.Profile = "CUKFFW98000LXA";

			var mockEdiMessageOutbound = Factory.New<DummyEDIMessage_FsaTests>();
			mockEdiMessageOutbound.GetMessageReferenceNumberReturns = "716";
			mockEdiMessageOutbound.EM_MessageText = "Outbound message <<MSGNO PLACEHOLDER>>";
			mockEdiMessageOutbound.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			mockEdiMessageOutbound.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			mockEdiMessageOutbound.EM_Status = EDIMessage.Status.Sent;
			mockEdiMessageOutbound.EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.Code;
			mockEdiMessageOutbound.EM_MessageSubType = CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate.Subcode;
			mawb.Messages.Add(mockEdiMessageOutbound);

			var messageText = exampleFromSpecificationDocument[7].Replace("GIS+T:121:ZZZ'", "GIS+T:121:ZZZ'GIS+AAA:131'GIS+BBB:131'GIS+CCC:131'");
			var result = PrepareExampleAndParse(messageText, true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(mockEdiMessageOutbound));
			mawb.Reload();

			AssertEquals(3, mawb.CommunityHandlingCodes.Count);
			AssertEquals("AAA", mawb.CommunityHandlingCodes[0].Data.C4_CommunityHandlingCode);
		}

		public void TestParseFsaWithUpdateJobFlagSetMawbButNotOurJob()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "88893080901";
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();

			var mockEdiMessageOutbound = Factory.New<DummyEDIMessage_FsaTests>();
			mockEdiMessageOutbound.GetMessageReferenceNumberReturns = "716";
			mockEdiMessageOutbound.EM_MessageText = "Outbound message <<MSGNO PLACEHOLDER>>";
			mockEdiMessageOutbound.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			mockEdiMessageOutbound.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			mockEdiMessageOutbound.EM_Status = EDIMessage.Status.Sent;
			mockEdiMessageOutbound.EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.Code;
			mockEdiMessageOutbound.EM_MessageSubType = CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate.Subcode;
			mawb.Messages.Add(mockEdiMessageOutbound);

			_ = PrepareExampleAndParse(exampleFromSpecificationDocument[7].Replace("LXA", "FOO"), true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(mockEdiMessageOutbound));
			mawb.Reload();

			AssertEquals(String.Empty, mawb.CM_FlightNo);
			AssertEquals(ZDate.Empty, mawb.CM_ArrivalDate);
			AssertEquals(string.Empty, mawb.AirportOfOrigin);
			AssertEquals(string.Empty, mawb.AirportOfDestination);
			AssertEquals(string.Empty, mawb.AirportOfArrival);
			AssertEquals(string.Empty, mawb.CargoTerminalOperator);
			AssertEquals(string.Empty, mawb.CargoTerminalOperatorAirport);
			AssertEquals(string.Empty, mawb.ShipmentDescriptionCode);
			AssertEquals(string.Empty, mawb.AgentBadge);
			AssertEquals(0, (int)mawb.NumberOfPiecesExpected);
			AssertEquals(0, (int)mawb.NumberOfPiecesReceived);
			AssertEquals(0m, mawb.Weight);

			var mawbReloadedCachingIsSoAnnoying = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			AssertContains("No update of the local record has been performed", mawbReloadedCachingIsSoAnnoying.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertContains("Interpretation still shows data", "<td>Agent Code</td><td>FOO</td>", mawbReloadedCachingIsSoAnnoying.Messages.LastIncomingMessage.EM_MessageInterpretation);
		}

		public void TestParseFsaWithUpdateJobFlagSetMawbButNotOurJobButAirlineAgentInFallback()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow");
			Factory.Save();

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "88893080901";
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);

			var mockEdiMessageOutbound = Factory.New<DummyEDIMessage_FsaTests>();
			mockEdiMessageOutbound.GetMessageReferenceNumberReturns = "716";
			mockEdiMessageOutbound.EM_MessageText = "Outbound message <<MSGNO PLACEHOLDER>>";
			mockEdiMessageOutbound.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			mockEdiMessageOutbound.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			mockEdiMessageOutbound.EM_Status = EDIMessage.Status.Sent;
			mockEdiMessageOutbound.EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.Code;
			mockEdiMessageOutbound.EM_MessageSubType = CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate.Subcode;
			mawb.Messages.Add(mockEdiMessageOutbound);
			interchangeReceipientPima = "CUKAIR98LHRLXA";

			_ = PrepareExampleAndParse(exampleFromSpecificationDocument[7].Replace("LXA", "FOO"), true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(mockEdiMessageOutbound));

			var mawbReloadedCachingIsSoAnnoying = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			AssertNotContains("No update of the local record has been performed", mawbReloadedCachingIsSoAnnoying.Messages.LastIncomingMessage.EM_MessageInterpretation);
			receivedMessage.Reload();
			AssertEquals("RCV", receivedMessage.EM_Status);
			AssertNotContains("No update of the local record has been performed", mawbReloadedCachingIsSoAnnoying.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertEquals("Local job has been updated even though it does not belong to us - this is because the message was addressed to an airline/agent/fallback party", "T", mawbReloadedCachingIsSoAnnoying.ShipmentDescriptionCode);
		}

		public void TestParseFsaWithUpdateJobFlagSetHawb()
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "88893080901";
			mawb.AirportOfOrigin = "OOO";
			mawb.AirportOfDestination = "DDD";
			mawb.AirportOfArrival = "AAA";

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "93080903";
			hawb.Profile = "CUKFFW98000LXA";

			var mockEdiMessageOutbound = Factory.New<DummyEDIMessage_FsaTests>();
			mockEdiMessageOutbound.GetMessageReferenceNumberReturns = "716";
			mockEdiMessageOutbound.EM_MessageText = "Outbound message <<MSGNO PLACEHOLDER>>";
			mockEdiMessageOutbound.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			mockEdiMessageOutbound.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			mockEdiMessageOutbound.EM_Status = EDIMessage.Status.Sent;
			mockEdiMessageOutbound.EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.Code;
			mockEdiMessageOutbound.EM_MessageSubType = CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate.Subcode;
			hawb.Messages.Add(mockEdiMessageOutbound);

			_ = PrepareExampleAndParse(exampleFromSpecificationDocument[8].Replace("KLM", "CAX"), true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(mockEdiMessageOutbound));
			mawb.Reload();
			hawb.Reload();

			AssertEquals("AR456", mawb.CM_FlightNo);
			AssertEquals(new ZDate(1993, 08, 09), mawb.CM_ArrivalDate);
			AssertEquals("OOO", mawb.AirportOfOrigin);
			AssertEquals("DDD", mawb.AirportOfDestination);
			AssertEquals("AAA", mawb.AirportOfArrival);
			AssertEquals("USJFK", hawb.AirportOfOrigin);
			AssertEquals("LHR", hawb.AirportOfDestination);
			AssertEquals("LHR", hawb.AirportOfArrival);
			AssertEquals("CAX", hawb.CargoTerminalOperator);
			AssertEquals("LHR", hawb.CargoTerminalOperatorAirport);
			AssertEquals("T", hawb.ShipmentDescriptionCode);
			AssertEquals("ABC", hawb.AgentBadge);
			AssertEquals(100, (int)hawb.CS_PiecesManifested);
			AssertEquals(100, (int)hawb.CS_PiecesLanded);
			AssertEquals(100m, hawb.CS_Weight);
			AssertEquals("KG", hawb.CS_WeightUQ);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, hawb.PresenceOnNetworkStatus);
		}

		public void TestParseFSA_Example01_BasicAwbNoEntry()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "88893080900";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[1], true);
			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			AssertEquals(1, mawb.Messages.Count);
			AssertEquals("88893080900", result.Header_AirwaybillPrefixAndAirwaybillNumber);
			AssertEquals("", result.Header_HouseWaybillNumber);
			AssertEquals("BASIC CONSIGNMENT RECORD RETRIEVED", result.Header_ReportText);
			AssertEquals("", result.Header_ReportType);
			AssertEquals("", result.Header_SplitReference);
			AssertEquals(1, result.ChildConsignments.Count);
			AssertEquals("DEI", result.ChildConsignments[0].AgentCode);
			AssertEquals("DEI AGENT", result.ChildConsignments[0].AgentName);
			AssertEquals("88893080900", result.ChildConsignments[0].ConsignmentReferenceNumber);
			AssertEquals("740", result.ChildConsignments[0].ConsignmentReferenceNumberType);
			AssertEquals(true, result.ChildConsignments[0].PreArrivalIndicator);
			AssertEquals("T", result.ChildConsignments[0].ShipmentDescriptionCode);
			AssertEquals("PLASTIC DOLLS", result.ChildConsignments[0].DescriptionOfGoods);
			AssertEquals(100m, result.ChildConsignments[0].Weight);
			AssertEquals(100, result.ChildConsignments[0].NPX);
			AssertEquals("FSA:  BASIC CONSIGNMENT RECORD RETRIEVED for CCS-UK Basic Air Waybill 888-93080900", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains(">CCS-UK Basic Air Waybill 888-93080900</a>", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		public void TestParseFSA_Example03()
		{
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[3]);
			AssertEquals(new ZDateTime(1993, 08, 09, 10, 55, 00), result.ChildConsignments[0].Status1Date);
			AssertEquals("CC", result.ChildConsignments[0].CustomsActionCode);
			AssertEquals("01", result.ChildConsignments[0].CustomsClearanceStatus);
			AssertEquals("3", result.ChildConsignments[0].Route);
			AssertEquals("000", result.ChildConsignments[0].InventoryReturnCodeIRC);
			AssertEquals("CUSTOMS CLEARED", result.ChildConsignments[0].CustomsActionText);
			AssertEquals("INVENTORY RETURN CODE TEXT", result.ChildConsignments[0].IRCText.ToString());
			AssertEquals(new ZDateTime(1993, 08, 09, 13, 20, 00), result.ChildConsignments[0].DateOfCustomsAction);
			AssertEquals("131", result.ChildConsignments[0].EntryProcessingUnit);
			AssertEquals("123456J", result.ChildConsignments[0].EntryNumber);
			AssertEquals(new ZDate(1993, 08, 09), result.ChildConsignments[0].EntryDate);
			AssertEquals("12345678", result.ChildConsignments[0].AgentsReferenceNumber);
			AssertEquals(25, result.ChildConsignments[0].NumPackagesEntered);
		}

		public void TestParseFSA_Example05_Transhipment()
		{
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[5]);

			AssertEquals(1, result.ChildConsignments.Count);
			AssertEquals(new ZDateTime(1993, 08, 09, 14, 24, 0), result.ChildConsignments[0].DateOfCustomsAction);
			var onwardLeg = result.ChildConsignments[0].OnwardLeg;
			var inwardLeg = result.ChildConsignments[0].InwardLeg;

			AssertEquals("", inwardLeg.Mode);
			AssertEquals("802", inwardLeg.FlightNumber);
			AssertEquals("BA", inwardLeg.Carrier);
			AssertEquals(new ZDateTime(1993, 07, 23), inwardLeg.Date);
			AssertEquals("MIA", inwardLeg.Locations.AirportOfOrigin.LocationCode);
			AssertEquals("GLW", inwardLeg.Locations.AirportOfDestination.LocationCode);
			AssertEquals("US", inwardLeg.Locations.CountryOfOrigin.LocationCode);
			AssertEquals("UK", inwardLeg.Locations.CounrtyOfDestination.LocationCode);
			AssertEquals("GLW", inwardLeg.Locations.AirportOfArrival.LocationCode);
			AssertEquals("BAS", inwardLeg.Locations.AirportOfArrival.ShedOperator);
			AssertEquals("AAA", inwardLeg.Locations.AirportOfArrival.ShedPhysicalIdentity);

			AssertEquals("", onwardLeg.FlightNumber);
			AssertEquals("40", onwardLeg.Mode);
			AssertEquals("BA", onwardLeg.Carrier);
			AssertEquals("GLW", onwardLeg.Locations.Find(x => x.Function == LocationTypes.Codes.AOO).LocationCode);
			AssertEquals("JFK", onwardLeg.Locations.Find(x => x.Function == LocationTypes.Codes.AOD).LocationCode);
			AssertEquals("12510000889", onwardLeg.Awb);
		}

		public void TestParseFSA_Example06_ManySplits()
		{
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[6]);

			AssertEquals("88893080900", result.Header_AirwaybillPrefixAndAirwaybillNumber);
			AssertEquals(4, result.ChildConsignments.Count);
			AssertEquals("88893080900", result.ChildConsignments[0].ConsignmentReferenceNumber);
			AssertEquals("BASIC CONSIGNMENT RECORD RETRIEVED WHICH HAS SPLITS", result.Header_ReportText);
			AssertEquals("01", result.ChildConsignments[1].ConsignmentReferenceNumber);
			AssertEquals("02", result.ChildConsignments[2].ConsignmentReferenceNumber);
			AssertEquals("03", result.ChildConsignments[3].ConsignmentReferenceNumber);
			AssertEquals("SRF", result.ChildConsignments[1].ConsignmentReferenceNumberType);
			AssertEquals("SRF", result.ChildConsignments[2].ConsignmentReferenceNumberType);
			AssertEquals("SRF", result.ChildConsignments[3].ConsignmentReferenceNumberType);
		}

		public void TestParseFSA_Example07_ManySplitHouses()
		{
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[7]);

			AssertEquals("88893080901", result.Header_AirwaybillPrefixAndAirwaybillNumber);
			AssertEquals(4, result.ChildConsignments.Count);
			AssertEquals("88893080901", result.ChildConsignments[0].ConsignmentReferenceNumber);
			AssertEquals("MASTER CONSIGNMENT RECORD RETRIEVED WHICH HAS 	HOUSES", result.Header_ReportText);
			AssertEquals("93080902", result.ChildConsignments[1].ConsignmentReferenceNumber);
			AssertEquals("93080903", result.ChildConsignments[2].ConsignmentReferenceNumber);
			AssertEquals("93080904", result.ChildConsignments[3].ConsignmentReferenceNumber);
			AssertEquals("703", result.ChildConsignments[1].ConsignmentReferenceNumberType);
			AssertEquals("703", result.ChildConsignments[2].ConsignmentReferenceNumberType);
			AssertEquals("703", result.ChildConsignments[3].ConsignmentReferenceNumberType);
		}

		public void TestParseFSA_Example08_HouseWithSplits()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CargoTerminalOperatorAirport = "LHR";
			mawb.CargoTerminalOperator = "KLM";
			var hawb = mawb.ChildBills.AddNew();
			mawb.CM_MAWB = "88893080901";
			hawb.CS_HAWB = "93080903";
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[8].Replace("+<<SYSCAR>>", ""), true);
			var secondFactory = new BusinessObjectFactory();
			hawb = secondFactory.Load<CusHAWB>(hawb.PK);
			mawb = secondFactory.Load<CusMAWB>(mawb.PK);
			AssertEquals(1, hawb.Messages.Count);
			AssertEquals(0, mawb.Messages.Count);
			AssertEquals("88893080901", result.Header_AirwaybillPrefixAndAirwaybillNumber);
			AssertEquals("93080903", result.Header_HouseWaybillNumber);
			AssertEquals("", result.Header_SplitReference);
			AssertEquals(3, result.ChildConsignments.Count);
			AssertEquals("FSA:  HOUSE CONSIGNMENT RECORD RETRIEVED WHICH HAS 	SPLITS for CCS-UK House Bill 888-93080901-93080903", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains(">CCS-UK House Bill 888-93080901-93080903</a>", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		public void TestParseFSA_Example09_HouseInSeveralSheds()
		{
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[9]);
			AssertEquals("88893080925", result.Header_AirwaybillPrefixAndAirwaybillNumber);
			AssertEquals("", result.Header_HouseWaybillNumber);
			AssertEquals(2, result.ChildConsignments.Count);
			AssertEquals("88893080925", result.ChildConsignments[0].ConsignmentReferenceNumber);
			AssertEquals("88893080925", result.ChildConsignments[1].ConsignmentReferenceNumber);
			AssertEquals("KLM", result.ChildConsignments[0].InwardLeg.Locations.AirportOfArrival.ShedOperator);
			AssertEquals("LHS", result.ChildConsignments[1].InwardLeg.Locations.AirportOfArrival.ShedOperator);
		}

		[TestDate(2011, 1, 1)]
		public void TestParseFSA_Example11_InventoryFailureReportE0()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_GS_NKCusAgent = "DJC";
			cusEntryHeader.EntryNumber = "131-123456J";
			cusEntryHeader.CusEntryNumber.CE_IssueDate = new ZDate(1992, 06, 01);
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[11], true);
			cusEntryHeader = new BusinessObjectFactory().Load<CusEntryHeader>(cusEntryHeader.PK);
			AssertEquals(1, cusEntryHeader.Messages.Count);
			AssertEquals("11110001234", result.Header_AirwaybillPrefixAndAirwaybillNumber);
			AssertEquals("E0", result.Header_ReportType);
			AssertEquals("INVENTORY FAILURE REPORT", result.Header_ReportText);
			AssertEquals("", result.Header_HouseWaybillNumber);
			AssertEquals(1, result.ChildConsignments.Count);
			AssertEquals("12345678", result.ChildConsignments[0].AgentsReferenceNumber);
			AssertEquals("11110001234", result.ChildConsignments[0].ConsignmentReferenceNumber);
			AssertEquals("123456J", result.ChildConsignments[0].EntryNumber);
			AssertEquals(new ZDateTime(1992, 6, 1, 0, 0, 0), result.ChildConsignments[0].EntryDate);
			AssertEquals("131", result.ChildConsignments[0].EntryProcessingUnit);
			AssertEquals("000", result.ChildConsignments[0].InventoryReturnCodeIRC);
			AssertEquals(0, result.ChildConsignments[0].InventoryVersionNo);
			AssertEquals("NPX DOES NOT EQUAL NOP", result.ChildConsignments[0].IRCText);
			AssertEquals(23, result.ChildConsignments[0].NPR);
			AssertEquals(0, result.ChildConsignments[0].NPX);
			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
			AssertContainsExactElementsInAnyOrder(new string[] { "daniel@wisetechglobal.com", "yawn@soPointless.com" }, relevantEMail.Recipients.RecipientsAsDelimitedString(";").Split(';'));
			AssertContains("FSA: Inventory Failure Report E0 INVENTORY FAILURE REPORT", relevantEMail.Subject);
			AssertContains($">Customs Entry 1-{declaration.JE_DeclarationReference}</a>", relevantEMail.Body);
		}

		[TestDate(2011, 1, 1)]
		public void TestParseFSA_InventoryFailureReportE0_MultipleLocationsForHouse()
		{
			var mawbAtLhrBAC = Factory.New<CusMAWB>();
			mawbAtLhrBAC.CM_MAWB = "08612820323";
			mawbAtLhrBAC.CargoTerminalOperator = "BAC";
			mawbAtLhrBAC.CargoTerminalOperatorAirport = "LHR";
			var hawbAtLhrBac = mawbAtLhrBAC.ChildBills.AddNew();
			hawbAtLhrBac.CS_HAWB = "21190411";
			var mawbAtLhrMhx = Factory.New<CusMAWB>();
			mawbAtLhrMhx.CM_MAWB = "08612820323";
			mawbAtLhrMhx.CargoTerminalOperator = "MHX";
			mawbAtLhrMhx.CargoTerminalOperatorAirport = "LHR";
			var hawbAtLhrMhx = mawbAtLhrMhx.ChildBills.AddNew();
			hawbAtLhrMhx.CS_HAWB = "21190411";
			Factory.Save();
			PrepareExampleAndParse("UNH+NUM5CX8ATKEKS0+CUKFSA:1:912:BT'BGM+:::E0+08612820323+7:1510241809:201++HWB:21190411'DOC+703+21190411'GIS+23:117:ZZZ'TDT+20++40'LOC+11:LHR:145:3::MHX:129:ZZZ'CST+0+038:110:ZZZ'FTX+IRT+++NO AGENT NOMINATED'RFF+ACF:120'RFF+TN:115078V+141:20151024:102'RFF+ABE:AI20701'QTY+66:77'UNT+13+NUM5CX8ATKEKS0", true);
			hawbAtLhrBac = new BusinessObjectFactory().Load<CusHAWB>(hawbAtLhrBac.PK); // stupid cache
			hawbAtLhrMhx = new BusinessObjectFactory().Load<CusHAWB>(hawbAtLhrMhx.PK); // stupid cache
			AssertEquals(1, hawbAtLhrMhx.Messages.Count);
			AssertEquals(0, hawbAtLhrBac.Messages.Count);
		}

		[TestDate(1992, 06, 01)]  //in message
		public void TestParseFSA_Example11_InventoryFailureReportE0_NonUniqueEntryNumber()
		{
			var declarationMay = Factory.New<JobDeclaration>();
			var cusEntryHeaderMay = declarationMay.CustomsEntryHeaders.AddNew();
			cusEntryHeaderMay.EntryNumber = "131-123456J";  // in message
			cusEntryHeaderMay.CusEntryNumber.CE_IssueDate = new ZDate(1992, 05, 01);  // NOT in message
			var declarationJune = Factory.New<JobDeclaration>();
			var cusEntryHeaderJune = declarationJune.CustomsEntryHeaders.AddNew();
			cusEntryHeaderJune.EntryNumber = cusEntryHeaderMay.EntryNumber;
			cusEntryHeaderJune.CusEntryNumber.CE_IssueDate = new ZDate(1992, 06, 01);  // in message
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[11], true);
			cusEntryHeaderMay = new BusinessObjectFactory().Load<CusEntryHeader>(cusEntryHeaderMay.PK);
			cusEntryHeaderJune = new BusinessObjectFactory().Load<CusEntryHeader>(cusEntryHeaderJune.PK);
			AssertEquals(0, cusEntryHeaderMay.Messages.Count);
			AssertEquals(1, cusEntryHeaderJune.Messages.Count);
		}

		public void TestParseFSA_Example12_MismatchReportG5()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "11112345677";

			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[12], true);
			AssertEquals("G5", result.Header_ReportType);
			AssertContains("PRE ARRIVAL AGENT MISMATCH REPORT", result.Header_ReportText);
			AssertEquals("OLD", result.ChildConsignments[0].OldOrNewDataIndicator);
			AssertEquals("NEW", result.ChildConsignments[1].OldOrNewDataIndicator);

			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
			AssertContainsExactElementsInAnyOrder(new string[] { "daniel@wisetechglobal.com", "yawn@soPointless.com" }, relevantEMail.Recipients.RecipientsAsDelimitedString(";").Split(';'));
			AssertContains("Pre-arrival Agent Mismatch Report", relevantEMail.Subject);
			AssertContains("<td>Old Or New Data Indicator</td><td>NEW</td>", relevantEMail.Body);
			AssertContains("<td>Old Or New Data Indicator</td><td>OLD</td>", relevantEMail.Body);
		}

		public void TestParseFSA_Example14_InterShedRemovalAdviceReport_Basic_P5()
		{
			// No local AWB already exists but we make one from P5 and use NEW data to update it
			BadgeCodeSetting badgeHeathrow;
			CredentialsSetting credentialHeathrow;
			BadgeCodeSetting badgeGatwick;
			CredentialsSetting credentialGatwick;
			BadgeCodeSetting badgeGatwickIrreelevant;
			CredentialsSetting credentialGatwickIrrelevant;
			GlbCompany company;
			GlbBranch heathrowBranch;
			GlbBranch gatwickBranch;
			GlbBranch crappyBranchForServiceTask;
			CuscarInboundParserTests.CreateThreeBranches(out company, out heathrowBranch, out gatwickBranch, out crappyBranchForServiceTask, Factory);

			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			AssertEquals("Precondition: there should be no print jobs in database", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			this.interchangeReceipientPima = CuscarInboundParserTests.heathrowPima;
			using (DisposableEnvironment.ForBranch(crappyBranchForServiceTask.PK.ToGuid()))
			{
				var printerForCcsuk = Factory.New<StmPrintQueue>();
				GB.Registry.GBCustomsDataRegistry.Instance.PrinterCcsuk_P5.SetValue(Guid.Empty, heathrowBranch.PK.ToGuid(), Guid.Empty, printerForCcsuk.PK.ToGuid());

				CuscarInboundParserTests.CreateCredentials(out badgeHeathrow, out credentialHeathrow, out badgeGatwick, out credentialGatwick, out badgeGatwickIrreelevant, out credentialGatwickIrrelevant, company, heathrowBranch, gatwickBranch, Factory);
				var result = PrepareExampleAndParse(exampleFromSpecificationDocument[14].Replace("LHS", "HHH"), true);
				AssertEquals("P5", result.Header_ReportType);

				var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
				AssertContainsExactElementsInAnyOrder(new string[] { "daniel@wisetechglobal.com", "yawn@soPointless.com" }, relevantEMail.Recipients.RecipientsAsDelimitedString(";").Split(';'));
				AssertContains("Advice of Inter-Shed Removal Report", relevantEMail.Subject);
				AssertContains("<td>00746340000</td>", relevantEMail.Body);
				AssertContains("<td>Description Of Goods</td><td>PENS</td>", relevantEMail.Body);
				AssertNotContains("has not been inserted", relevantEMail.Body);

				var basic = Factory.LoadTop1<CusMAWB>(new ZQuery());
				AssertEquals("00746340000", basic.CM_MAWB);
				AssertEquals("PENS", basic.DescriptionOfGoods);
				AssertEquals("Job is now in new/target shed", "HHH", basic.CargoTerminalOperator);
				receivedMessage.Reload();
				AssertEquals(basic.MasterLevelHouseHelper.PK, receivedMessage.EM_LinkUniqueID);
				AssertEquals("AWB is listed as not yet on network for new shed, FRI still needed", PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc, basic.PresenceOnNetworkStatus);
				AssertEquals(CuscarInboundParserTests.heathrowPima, basic.Profile);
				AssertEquals(heathrowBranch.PK, basic.CM_GB);
				AssertEquals("Do not set NPR on job even if the messages contains it", (short)0, basic.NumberOfPiecesReceived);
				AssertEquals((short)10, basic.NumberOfPiecesExpected);

				var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
				AssertNotNull("P5 print job not printed automatically", printJob);

				AssertContains("P5 Inter Airport Removal 007-46340000", printJob.SP_EmailSubjectLine);

				AssertEquals("Print Job not attached to correct parent", basic.PK, printJob.SP_ParentGuid);
			}
		}

		public void TestP5ForHouse()
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			interchangeReceipientPima = "CUKAIR98LHRCAX";
			var p5RealExampleFromTorqueForHouse = "UNH+ISR2DB8FW9ZE10+CUKFSA:1:912:BT'BGM+:::P5+15791966722+7:1701232312:201++HWB:00031093'DOC+703+00031093+++++OLD'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+015+40+++QR:172:3++178:170123:101'LOC+84:DAC:145:3+85:LHR:145:3+11:LHR:145:3::NBS:129:ZZZ'TDT+12++40'NAD+CB+DSK'GDS+2'QTY+118:46'MEA+WT++KGM:375'FTX+AAA+++SHIRT'DOC+703+00031093+++++NEW'GIS+29:117:ZZZ'GIS+T:121:ZZZ'TDT+20+015+40+++QR:172:3++178:170123:101'LOC+84:DAC:145:3+85:LHR:145:3+11:LHR:145:3::ELX:129:ZZZ'TDT+12++40'NAD+CB+DSK'GDS+2'QTY+118:46'MEA+WT++KGM:375'FTX+AAA+++SHIRT'UNT+25+ISR2DB8FW9ZE10'";
			p5RealExampleFromTorqueForHouse = p5RealExampleFromTorqueForHouse.Replace("ELX", "CAX").Replace("DSK", "LXA");
			var result = PrepareExampleAndParse(p5RealExampleFromTorqueForHouse, true);
			var hawbCreated = new BusinessObjectFactory().LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "00031093"));
			AssertEquals(ZDateTime.Empty, hawbCreated.Status1Date);
			AssertEquals(ZDateTime.Empty, hawbCreated.MAWB.Status1Date);
		}

		public void TestFsaP5DeleteWithExistingOnwardRecord()
		{
			// Special kind of P5 that advises deletion/cancellation of the ISR request.  No OLD or NEW sections of the report. 

			interchangeReceipientPima = "CUKAIR98LHRCAX";
			var originalAwbAlreadyExistsWithCbStatus = Factory.New<CusMAWB>(); // original AWB, for agent
			originalAwbAlreadyExistsWithCbStatus.CM_MAWB = "16530803080";
			originalAwbAlreadyExistsWithCbStatus.Profile = "CUKFFW98000GRN";
			originalAwbAlreadyExistsWithCbStatus.AgentBadge = "GRN";
			originalAwbAlreadyExistsWithCbStatus.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRORN";  // old/original shed
			originalAwbAlreadyExistsWithCbStatus.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterShedRemoval, ZDateTime.BrettsBirthday);
			originalAwbAlreadyExistsWithCbStatus.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;

			// Simulate making ISR-only record in new shed, which shares a database with agent GRN. This would have arrived by processing a P5 report sent to LHRORN. 
			var onwardAwbAlreadyExistsAfterPreviousP5Report = Factory.New<CusMAWB>();
			onwardAwbAlreadyExistsAfterPreviousP5Report.CM_MAWB = "16530803080";
			onwardAwbAlreadyExistsAfterPreviousP5Report.Profile = "CUKAIR98LHRCAX";
			onwardAwbAlreadyExistsAfterPreviousP5Report.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRCAX";  // new shed
			onwardAwbAlreadyExistsAfterPreviousP5Report.AgentBadge = "GRN";
			onwardAwbAlreadyExistsAfterPreviousP5Report.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;

			Factory.Save();

			// Now cancel the movement into LHRORN... sent to new shed (CAX)
			var p5ReportDeletesRecordInOnwardShed = "UNH+FSN1ARQF32HVC0+CUKFSA:1:912:BT'BGM+:::P5+16530803080+7:0811190727:201'FTX+AAA+++ADVICE OF INTER-SHED REMOVAL REPORT MESSAGE'DOC+740+16530803080'GIS+23:117:ZZZ'GIS+T:121:ZZZ'TDT+20+1234+40+++BA:172:3++178:080126:101'LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::ORN:129:ZZZ'NAD+CB+GRN'GDS+2'QTY+118:100'MEA+WT++KGM:700'FTX+AAA+++2054 2 3'CST++CX:117:ZZZ+94:120:109'FTX+CAT+++REQUEST CANX'DTM+176:0811190422:201'RFF+CKN:822'UNT+18+FSN1ARQF32HVC0'";
			var result = PrepareExampleAndParse(p5ReportDeletesRecordInOnwardShed, true);

			originalAwbAlreadyExistsWithCbStatus.Reload();
			originalAwbAlreadyExistsWithCbStatus = new BusinessObjectFactory().Load<CusMAWB>(originalAwbAlreadyExistsWithCbStatus.PK);
			AssertEquals("Original record should be untouched", "LHRORN", originalAwbAlreadyExistsWithCbStatus.MasterLevelHouseHelper.CS_WarehouseLocation);
			AssertEquals("Original record should be untouched", "CB", originalAwbAlreadyExistsWithCbStatus.CustomsActionCode);
			AssertEquals("Original record should be untouched", PresenceOnNetworkList.Codes.OnCommDb, originalAwbAlreadyExistsWithCbStatus.PresenceOnNetworkStatus);

			onwardAwbAlreadyExistsAfterPreviousP5Report = new BusinessObjectFactory().Load<CusMAWB>(onwardAwbAlreadyExistsAfterPreviousP5Report.PK);
			AssertEquals("Onward record should be untouched", "LHRCAX", onwardAwbAlreadyExistsAfterPreviousP5Report.MasterLevelHouseHelper.CS_WarehouseLocation);
			AssertEquals("New P5 added to onward record", 1, onwardAwbAlreadyExistsAfterPreviousP5Report.Messages.Count);
			AssertEquals("Onward record should be updated to ISX", PresenceOnNetworkList.Codes.IsrRequestCancelled, onwardAwbAlreadyExistsAfterPreviousP5Report.PresenceOnNetworkStatus);

			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
			AssertContains("Advice of Inter-Shed Removal Report", relevantEMail.Subject);
		}

		public void TestFsaP5DeleteWithNoOnwardRecord_Basic()
		{
			interchangeReceipientPima = "CUKAIR98LHRCAX";
			var p5ReportDeletesRecordInOnwardShed = "UNH+FSN1ARQF32HVC0+CUKFSA:1:912:BT'BGM+:::P5+16530803080+7:0811190727:201'FTX+AAA+++ADVICE OF INTER-SHED REMOVAL REPORT MESSAGE'DOC+740+16530803080'GIS+23:117:ZZZ'GIS+T:121:ZZZ'TDT+20+1234+40+++BA:172:3++178:080126:101'LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::ORN:129:ZZZ'NAD+CB+GRN'GDS+2'QTY+118:100'MEA+WT++KGM:700'FTX+AAA+++2054 2 3'CST++CX:117:ZZZ+94:120:109'FTX+CAT+++REQUEST CANX'DTM+176:0811190422:201'RFF+CKN:822'UNT+18+FSN1ARQF32HVC0'";
			var result = PrepareExampleAndParse(p5ReportDeletesRecordInOnwardShed, true);
			AssertEquals("No record created", 0, Factory.Load<CusMAWB>(new ZQuery()).Length);
			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
			AssertContains("Advice of Inter-Shed Removal Report", relevantEMail.Subject);
		}

		public void TestFsaP5DeleteWithNoOnwardRecord_House()
		{
			interchangeReceipientPima = "CUKAIR98LHRCAX";
			var p5ReportDeletesRecordInOnwardShed = "UNH+FSN1ARQF32HVC0+CUKFSA:1:912:BT'BGM+:::P5+16530803080+7:0811190727:201++HWB:HOUSEISR'FTX+AAA+++ADVICE OF INTER-SHED REMOVAL REPORT MESSAGE'DOC+703+HOUSEISR'GIS+23:117:ZZZ'GIS+T:121:ZZZ'TDT+20+1234+40+++BA:172:3++178:080126:101'LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::ORN:129:ZZZ'NAD+CB+GRN'GDS+2'QTY+118:100'MEA+WT++KGM:700'FTX+AAA+++2054 2 3'CST++CX:117:ZZZ+94:120:109'FTX+CAT+++REQUEST CANX'DTM+176:0811190422:201'RFF+CKN:822'UNT+18+FSN1ARQF32HVC0'";
			var result = PrepareExampleAndParse(p5ReportDeletesRecordInOnwardShed, true);
			AssertEquals("No house record created", 0, Factory.Load<CusHAWB>(new ZQuery()).Length);
			AssertEquals("No mawb record created", 0, Factory.Load<CusMAWB>(new ZQuery()).Length);
			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
			AssertContains("Advice of Inter-Shed Removal Report", relevantEMail.Subject);
		}

		public void TestFsaP5DeleteWithNoOnwardRecord_SplitHouse()
		{
			interchangeReceipientPima = "CUKAIR98LHRCAX";
			var p5ReportDeletesRecordInOnwardShed = "UNH+FSN1ARQF32HVC0+CUKFSA:1:912:BT'BGM+:::P5+16530803080+7:0811190727:201++HWB:HOUSEISR:02'FTX+AAA+++ADVICE OF INTER-SHED REMOVAL REPORT MESSAGE'DOC+SRF+02'GIS+23:117:ZZZ'GIS+T:121:ZZZ'TDT+20+1234+40+++BA:172:3++178:080126:101'LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::ORN:129:ZZZ'NAD+CB+GRN'GDS+2'QTY+118:100'MEA+WT++KGM:700'FTX+AAA+++2054 2 3'CST++CX:117:ZZZ+94:120:109'FTX+CAT+++REQUEST CANX'DTM+176:0811190422:201'RFF+CKN:822'UNT+18+FSN1ARQF32HVC0'";
			var result = PrepareExampleAndParse(p5ReportDeletesRecordInOnwardShed, true);
			AssertEquals("No house record created", 0, Factory.Load<CusHAWB>(new ZQuery()).Length);
			AssertEquals("No mawb record created", 0, Factory.Load<CusMAWB>(new ZQuery()).Length);
			AssertEquals("No split record created", 0, Factory.Load<SplitConsignment>(new ZQuery()).Length);
			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
			AssertContains("Advice of Inter-Shed Removal Report", relevantEMail.Subject);
		}

		public void TestFsaP5DeleteWithNoOnwardRecord_SplitBasic()
		{
			interchangeReceipientPima = "CUKAIR98LHRCAX";
			var p5ReportDeletesRecordInOnwardShed = "UNH+FSN1ARQF32HVC0+CUKFSA:1:912:BT'BGM+:::P5+16530803080+7:0811190727:201++ACD::02'FTX+AAA+++ADVICE OF INTER-SHED REMOVAL REPORT MESSAGE'DOC+SRF+02'GIS+23:117:ZZZ'GIS+T:121:ZZZ'TDT+20+1234+40+++BA:172:3++178:080126:101'LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::ORN:129:ZZZ'NAD+CB+GRN'GDS+2'QTY+118:100'MEA+WT++KGM:700'FTX+AAA+++2054 2 3'CST++CX:117:ZZZ+94:120:109'FTX+CAT+++REQUEST CANX'DTM+176:0811190422:201'RFF+CKN:822'UNT+18+FSN1ARQF32HVC0'";
			var result = PrepareExampleAndParse(p5ReportDeletesRecordInOnwardShed, true);
			AssertEquals("No house record created", 0, Factory.Load<CusHAWB>(new ZQuery()).Length);
			AssertEquals("No mawb record created", 0, Factory.Load<CusMAWB>(new ZQuery()).Length);
			AssertEquals("No split record created", 0, Factory.Load<SplitConsignment>(new ZQuery()).Length);
			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
			AssertContains("Advice of Inter-Shed Removal Report", relevantEMail.Subject);
		}

		public void TestFsaP5DeleteWithNoOnwardRecord_HouseButMawbExists()
		{
			interchangeReceipientPima = "CUKAIR98LHRCAX";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "16530803080";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRCAX";
			var p5ReportDeletesRecordInOnwardShed = "UNH+FSN1ARQF32HVC0+CUKFSA:1:912:BT'BGM+:::P5+16530803080+7:0811190727:201++HWB:HOUSEISR'FTX+AAA+++ADVICE OF INTER-SHED REMOVAL REPORT MESSAGE'DOC+703+HOUSEISR'GIS+23:117:ZZZ'GIS+T:121:ZZZ'TDT+20+1234+40+++BA:172:3++178:080126:101'LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::ORN:129:ZZZ'NAD+CB+GRN'GDS+2'QTY+118:100'MEA+WT++KGM:700'FTX+AAA+++2054 2 3'CST++CX:117:ZZZ+94:120:109'FTX+CAT+++REQUEST CANX'DTM+176:0811190422:201'RFF+CKN:822'UNT+18+FSN1ARQF32HVC0'";
			var result = PrepareExampleAndParse(p5ReportDeletesRecordInOnwardShed, true);
			AssertEquals("No house record created", 0, Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false)).Length);
			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
			AssertContains("Advice of Inter-Shed Removal Report", relevantEMail.Subject);
		}

		public void TestParseFSA_Example14_InterShedRemovalAdviceReport_Basic_P5_CommunityHandlingCodes()
		{
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("BAR", "CUKAIR98LHRBAR");
			interchangeReceipientPima = "CUKAIR98LHRBAR";
			var messageText = exampleFromSpecificationDocument[14].Replace("GIS+T:121:ZZZ'", "GIS+T:121:ZZZ'GIS+AAA:131'GIS+BBB:131'GIS+CCC:131'");
			var result = PrepareExampleAndParse(messageText.Replace("LHS", "BAR"), true);
			AssertEquals("P5", result.Header_ReportType);
			var awb = Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertEquals(3, awb.CommunityHandlingCodes.Count);
		}

		public void TestParseFSA_Example14_InterShedRemovalAdviceReport_Basic_P5_CommunityHandlingCodes_SuppressInsertOfCHCs()
		{
			GBCustomsDataRegistry.Instance.CcsukPutChcsFromInboundP5ReportOntoNewJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("BAR", "CUKAIR98LHRBAR");
			interchangeReceipientPima = "CUKAIR98LHRBAR";
			var messageText = exampleFromSpecificationDocument[14].Replace("GIS+T:121:ZZZ'", "GIS+T:121:ZZZ'GIS+AAA:131'GIS+BBB:131'GIS+CCC:131'");
			var result = PrepareExampleAndParse(messageText.Replace("LHS", "BAR"), true);
			AssertEquals("P5", result.Header_ReportType);
			var awb = Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertEquals(0, awb.CommunityHandlingCodes.Count);
		}

		public void TestParseFSA_Example14_InterShedRemovalAdviceReport_Basic_P5_AutoInsertIsDisabled()
		{
			GBCustomsDataRegistry.Instance.CcsukP5ProcessorShouldInsertNewRecords.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[14].Replace("LHS", "HHH"), true);
			AssertEquals("P5", result.Header_ReportType);
			var relevantEMail = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject != "ediEnterprise Communication");
			AssertContains("Advice of Inter-Shed Removal Report", relevantEMail.Subject);
			AssertContains("<td>00746340000</td>", relevantEMail.Body);
			AssertContains("<td>Description Of Goods</td><td>PENS</td>", relevantEMail.Body);
			AssertContains("has not been inserted", relevantEMail.Body);
			AssertNull("No AWB inserted", Factory.LoadTop1<CusMAWB>(new ZQuery()));
		}

		public void TestParseFSA_Example14_InterShedRemovalAdviceReport_Basic_P5_AlreadyExistsInAnotherShed()
		{
			var existingBasic = Factory.New<CusMAWB>();
			existingBasic.CM_MAWB = "00746340000";
			existingBasic.MasterLevelHouseHelper.CS_WarehouseLocation = "WHATEV";  // doesn't matter 
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("BAR", "CUKAIR98LHRBAR");
			Factory.Save();
			PrepareExampleAndParse(exampleFromSpecificationDocument[14], true);
			var newMawb = Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.PK, SQLComparisonOperator.NotEqual, existingBasic.PK));
			AssertNotNull(newMawb);
			AssertEquals("LHRBAR", newMawb.MasterLevelHouseHelper.CS_WarehouseLocation);
			AssertEquals("Existing Awb is updated from P5", "PENS", newMawb.DescriptionOfGoods);
			AssertEquals((short)0, newMawb.NumberOfPiecesReceived);
		}

		public void TestParseFSA_Example14_InterShedRemovalAdviceReport_Basic_P5_AlreadyExistsDuplicateP5()
		{
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("BAR", "CUKAIR98LHRBAR");
			var existingBasic = Factory.New<CusMAWB>();
			existingBasic.Profile = "CUKAIR98LHRBAR";
			existingBasic.CM_MAWB = "00746340000";
			existingBasic.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRBAR";  // same as in NEW group
			existingBasic.OutTurns.AddNew().C5_PackagesOutturned = 20;
			Factory.Save();
			PrepareExampleAndParse(exampleFromSpecificationDocument[14], true);
			var newMawb = Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.PK, SQLComparisonOperator.NotEqual, existingBasic.PK));
			AssertNull("No additional record created", newMawb);
			existingBasic.Reload();
			AssertEquals("Existing Awb is updated from P5", "PENS", existingBasic.DescriptionOfGoods);
			AssertEquals("NPR unaffected", (short)20, existingBasic.NumberOfPiecesReceived);
		}

		public void TestParseFSA_Example28_InterShedRemovalAdviceReport_House_P5_AlreadyExistsInAnotherShed()
		{
			// A record exists, but different shed, so make a new one from P5
			var existingMawb = Factory.New<CusMAWB>();
			existingMawb.CM_MAWB = "17022012005";
			existingMawb.MasterLevelHouseHelper.CS_WarehouseLocation = "WHATEV";
			var existingHawb = existingMawb.ChildBills.AddNew();
			existingHawb.CS_HAWB = "WHOLEHSE";
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");
			Factory.Save();
			PrepareExampleAndParse(exampleFromSpecificationDocument[28], true);
			var newMawb = Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.PK, SQLComparisonOperator.NotEqual, existingMawb.PK));
			var newHawb = newMawb.ChildBills[0];
			AssertNotNull(newMawb);
			AssertNotNull(newHawb);
			AssertEquals("LHRCWE", newMawb.MasterLevelHouseHelper.CS_WarehouseLocation);
			AssertEquals("LHRCWE", newHawb.CS_WarehouseLocation);
			AssertEquals("WHOLE", newHawb.CS_GoodsDescription);
		}

		public void TestParseFSA_Example28_InterShedRemovalAdviceReport_House_P5_AlreadyExistsInAOurShed()
		{
			// Record already exists, and for this shed, so UPDATE (don't duplicate) it
			var existingMawb = Factory.New<CusMAWB>();
			existingMawb.CM_MAWB = "17022012005";
			existingMawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRCWE";
			var existingHawb = existingMawb.ChildBills.AddNew();
			existingHawb.CS_HAWB = "WHOLEHSE";
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");
			Factory.Save();
			PrepareExampleAndParse(exampleFromSpecificationDocument[28], true);
			AssertEquals("No new mawb", 1, Factory.GetDatabaseCount(typeof(CusMAWB), new ZQuery()));
			AssertEquals("No new hawb", 1, Factory.GetDatabaseCount(typeof(CusHAWB), new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false)));
			existingMawb.Reload();
			existingHawb.Reload();
			AssertEquals(PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc, existingHawb.PresenceOnNetworkStatus);
			AssertEquals("WHOLE", existingHawb.CS_GoodsDescription);
			AssertEquals((ZShort)6, existingHawb.CS_PiecesManifested);
		}

		public void TestParseFSA_Example27_InterShedRemovalAdviceReport_SplitHouse_P5_InsertNewSplit()
		{
			var printerForCcsuk = Factory.New<StmPrintQueue>();
			GB.Registry.GBCustomsDataRegistry.Instance.PrinterCcsuk_P5.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printerForCcsuk.PK.ToGuid());

			// No local AWB already exists but we make one from P5 and use NEW data to update it
			GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[27], true);
			AssertEquals("P5", result.Header_ReportType);

			var mawb = Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertEquals("17022012005", mawb.CM_MAWB);
			var hawb = mawb.ChildBills[0];
			AssertEquals("SPLITHSE", hawb.CS_HAWB);
			AssertEquals("TO SPLIT", hawb.DescriptionOfGoods);
			var splitHouse = hawb.Splits["02"];
			AssertEquals("Job is now in new/target shed", "CWE", splitHouse.CargoTerminalOperator);
			AssertEquals((ZShort)3, splitHouse.NumberOfPiecesExpected);
			AssertEquals("USATL", splitHouse.AirportOfOrigin);
			receivedMessage.Reload();
			AssertEquals(hawb.PK, receivedMessage.EM_LinkUniqueID);
			AssertEquals("AWB is listed as not yet on network for new shed, FRI still needed", PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc, splitHouse.PresenceOnNetworkStatus);
			AssertEquals("AWB is listed as not yet on network for new shed, FRI still needed", PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc, hawb.PresenceOnNetworkStatus);
			AssertEquals("AWB is listed as not yet on network for new shed, FRI still needed", PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc, mawb.PresenceOnNetworkStatus);

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertNotNull("P5 print job not printed automatically", printJob);
		}

		public void TestParseFSA_Example27_InterShedRemovalAdviceReport_SplitHouse_P5_WhenSplitAlreadyExists()
		{
			var existingMawb = Factory.New<CusMAWB>();
			existingMawb.CM_MAWB = "17022012005";
			existingMawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRCWE";
			var existingHawb = existingMawb.ChildBills.AddNew();
			existingHawb.CS_HAWB = "SPLITHSE";

			var existingSplitHouse = existingHawb.Splits.AddNew();
			existingSplitHouse.SplitReference = "02";
			existingHawb.CS_GoodsDescription = "ORIGINAL";
			Factory.Save();

			var printerForCcsuk = Factory.New<StmPrintQueue>();
			GB.Registry.GBCustomsDataRegistry.Instance.PrinterCcsuk_P5.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printerForCcsuk.PK.ToGuid());

			// No local AWB already exists but we make one from P5 and use NEW data to update it
			GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[27], true);
			AssertEquals("P5", result.Header_ReportType);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			existingHawb = newFactory.Load<CusHAWB>(existingHawb.PK);
			AssertEquals("No new Split added", 1, existingHawb.Splits.Count);
			receivedMessage.Reload();
			var expectedRedHeading = "<h3 style='color:red'>A split 02 was advised via a P5 report for bill LHRCWE-170-22012005, but it already exists. No extra split was added. The existing split will be updated using data from the P5 report.</h3>";
			AssertContains("Red coloured text in HTML heading.", expectedRedHeading, receivedMessage.EM_MessageInterpretation);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestParseFSA_Example27_InterShedRemovalAdviceReport_SplitHouse_P5_MakeDummyBasic()
		{
			GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[27], true);
			AssertEquals("P5", result.Header_ReportType);
			var dummyBasic = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, SQLComparisonOperator.DoesNotStartWith, "170"));
			AssertEquals("CWE03120427", dummyBasic.CM_MAWB);  // Now
			AssertEquals(true, dummyBasic.IsBasic);
			AssertEquals(false, dummyBasic.HasSplits);
			AssertEquals("SPLITHSE/02", dummyBasic.DescriptionOfGoods);
			AssertEquals((ZShort)3, dummyBasic.NumberOfPiecesExpected);
			AssertEquals("CAR", dummyBasic.AgentBadge);
			AssertEquals("AWB is listed as not yet on network for new shed, FRI still needed", PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc, dummyBasic.PresenceOnNetworkStatus);
			AssertContains("This is a skeleton record created for LHRCAX-17022012005-SPLITHSE/02", dummyBasic.Messages[0].EM_MessageInterpretation);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestParseFSA_Example27_InterShedRemovalAdviceReport_SplitHouse_P5_MakeDummyBasicEvenIfHouseAlreadyExists()
		{
			var existingMawb = Factory.New<CusMAWB>();
			existingMawb.CM_MAWB = "17022012005";
			existingMawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRCWE";
			var existingHawb = existingMawb.ChildBills.AddNew();
			existingHawb.CS_HAWB = "WHOLEHSE";
			existingMawb.DescriptionOfGoods = "ORIGINAL";
			existingHawb.CS_GoodsDescription = "ORIGINALH";
			TestParseFSA_Example27_InterShedRemovalAdviceReport_SplitHouse_P5_MakeDummyBasic();
			existingMawb.Reload();
			AssertEquals(0, existingMawb.Messages.Count);
			existingHawb.Reload();
			AssertEquals(0, existingHawb.Messages.Count);
			AssertEquals("ORIGINAL", existingMawb.DescriptionOfGoods);
			AssertEquals("ORIGINALH", existingHawb.DescriptionOfGoods);
		}

		public void TestParseFSA_Example28_InterShedRemovalAdviceReport_HouseAlreadyExists_P5()
		{
			// A local HAWB already exists so update it with new data
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[28], true);
			AssertEquals("P5", result.Header_ReportType);

			var mawb = Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertEquals("17022012005", mawb.CM_MAWB);
			var hawb = mawb.ChildBills[0];
			AssertEquals("WHOLEHSE", hawb.CS_HAWB);
			AssertEquals("WHOLE", hawb.DescriptionOfGoods);
			AssertEquals(false, hawb.HasSplits);
			AssertEquals("Job is now in new/target shed", "CWE", hawb.CargoTerminalOperator);
			AssertEquals((ZShort)6, hawb.CS_PiecesManifested);
			AssertEquals("USLAX", hawb.AirportOfOrigin);
			receivedMessage.Reload();
			AssertEquals(hawb.PK, receivedMessage.EM_LinkUniqueID);
			AssertEquals("AWB is listed as not yet on network for new shed, FRI still needed", PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc, hawb.PresenceOnNetworkStatus);
			AssertEquals("AWB is listed as not yet on network for new shed, FRI still needed", PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc, mawb.PresenceOnNetworkStatus);
		}

		public void TestParseFSA_Example29_InterShedRemovalAdviceReport_SplitBasic_P5()
		{
			// No local AWB already exists but we make one from P5 and use NEW data to update it
			GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[29], true);
			AssertEquals("P5", result.Header_ReportType);

			var basic = Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertEquals("17022012006", basic.CM_MAWB);
			AssertEquals(true, basic.IsBasic);
			AssertEquals("BASIC TO SPLIT", basic.DescriptionOfGoods);
			var splitBasic = basic.Splits["02"];
			AssertEquals("Job is now in new/target shed", "CWE", splitBasic.CargoTerminalOperator);
			AssertEquals((ZShort)8, splitBasic.NumberOfPiecesExpected);
			AssertEquals("USATL", splitBasic.AirportOfOrigin);
			receivedMessage.Reload();
			AssertEquals(basic.MasterLevelHouseHelper.PK, receivedMessage.EM_LinkUniqueID);
			AssertEquals("AWB is listed as not yet on network for new shed, FRI still needed", PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc, splitBasic.PresenceOnNetworkStatus);
			AssertEquals("AWB is listed as not yet on network for new shed, FRI still needed", PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc, basic.PresenceOnNetworkStatus);
		}

		public void TestParseFSA_Example29_InterShedRemovalAdviceReport_SplitBasic_P5_WhenSplitAlreadyExists()
		{
			var existingMawb = Factory.New<CusMAWB>();
			existingMawb.CM_MAWB = "17022012006";
			existingMawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRCWE";
			var existingSplitBasic = existingMawb.Splits.AddNew();
			existingSplitBasic.SplitReference = "02";
			existingMawb.DescriptionOfGoods = "ORIGINAL";
			Factory.Save();

			GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[29], true);
			AssertEquals("P5", result.Header_ReportType);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			existingMawb = newFactory.Load<CusMAWB>(existingMawb.PK);
			AssertEquals("No new Split added", 1, existingMawb.Splits.Count);
			receivedMessage.Reload();
			var expectedRedHeading = "<h3 style='color:red'>A split 02 was advised via a P5 report for basic LHRCWE-170-22012006, but it already exists. No extra split was added. The existing split will be updated using data from the P5 report.</h3>";
			AssertContains("Red coloured text in HTML heading.", expectedRedHeading, receivedMessage.EM_MessageInterpretation);
		}

		public void TestParseFSA_Example29_InterShedRemovalAdviceReport_SplitBasic_P5_WhereBasicIs_A_MasterWithRealHouses()
		{
			#region Setup pre-existing data as prerequisite:

			const string PortFrom = "USATL";
			const string PortTo = "GBLHR";

			var master = Factory.New<CusMAWB>();
			master.CM_MAWB = "17022012006";
			master.CM_FlightNo = "BA40";
			master.DescriptionOfGoods = "GOODS DESC.";
			master.CargoTerminalOperatorAirportAndShed = "LHRCWE";
			master.AirportOfOrigin = PortFrom;
			master.AirportOfDestination = PortTo;
			master.AirportOfArrival = PortTo;
			master.CM_RL_NKLoadPort = PortFrom;
			master.CM_RL_NKDischargePort = PortTo;

			var house_1 = master.ChildBills.AddNew();
			house_1.CS_HAWB = "Hawb-1";
			house_1.CS_RL_NKLoadPort = PortFrom;
			house_1.CS_RL_NKDischargePort = PortTo;
			house_1.CS_RL_NKOrigin = PortFrom;
			house_1.CS_RL_NKDestination = PortTo;

			var house_2 = master.ChildBills.AddNew();
			house_2.CS_HAWB = "Hawb-2";
			house_2.CS_RL_NKLoadPort = PortFrom;
			house_2.CS_RL_NKDischargePort = PortTo;
			house_2.CS_RL_NKOrigin = PortFrom;
			house_2.CS_RL_NKDestination = PortTo;

			GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");

			Factory.Save();

			#endregion

			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[29], true);
			AssertEquals("P5", result.Header_ReportType);

			var query = new ZQuery(CusMAWBSchema.CM_MAWB, "17022012006");
			var basic = Factory.LoadTop1<CusMAWB>(query);

			AssertNotNull("Retrieve MAWB by MAWB number.", basic);
			AssertEquals("GOODS DESC.", basic.DescriptionOfGoods);
			AssertEquals("BA40", basic.CM_FlightNo);
			AssertEquals("In this case, there must be no splits.", 0, basic.Splits.Count);

			receivedMessage.Reload();
			Assert("EDIMessage.EM_MessageInterpretation must contain text.", !receivedMessage.EM_MessageInterpretation.IsEmpty);

			var expectedRedHeading = "<h3 style='color:red'>An additional split 02 was advised via a P5 report for basic LHRCWE-170-22012006, but you have already added houses to this. No extra split was added. You are recommended to adjust the piece count against the houses for the extra 8 pieces that have been advised.</h3>";
			AssertContains("Red coloured text in HTML heading.", expectedRedHeading, receivedMessage.EM_MessageInterpretation);
		}

		public void TestParseFSA_Example30Or26WithCommunityHandlingCodes()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "88893080900";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[30], true);
			AssertEquals("88893080900", result.Header_AirwaybillPrefixAndAirwaybillNumber);
			AssertEquals("T", result.ChildConsignments[0].ShipmentDescriptionCode);
			AssertEquals("AAA", result.ChildConsignments[0].CommunityHandlingCodes[0]);
			AssertEquals("BBB", result.ChildConsignments[0].CommunityHandlingCodes[1]);
			AssertEquals("CCC", result.ChildConsignments[0].CommunityHandlingCodes[2]);
			Factory.Save();
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertContains("AAA; BBB; CCC", basic.Messages[0].EM_MessageInterpretation);
		}

		public void TestParseInboundFsaForExportConsol()
		{
			// response to an FSR that was send from an export consol - the SYS-CAR will point to a ForwardingConsool, not an AWB
			// Example number 5 has a sys-car placehold
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "11122222222";
			consol.JK_RL_NKLoadPort = "GBLHR";
			var shutup = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, shutup);
			var helper = wrapper.MawbExportHelper;
			helper.ME_ExportShed = "BAC";
			helper.ME_ExportLocation = "LHR";

			var mockEdiMessageOutbound = Factory.New<DummyEDIMessage_FsaTests>();
			mockEdiMessageOutbound.GetMessageReferenceNumberReturns = "716";
			mockEdiMessageOutbound.EM_MessageText = "Outbound message <<MSGNO PLACEHOLDER>>";
			mockEdiMessageOutbound.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			mockEdiMessageOutbound.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			mockEdiMessageOutbound.EM_Status = EDIMessage.Status.Sent;
			mockEdiMessageOutbound.EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.Code;
			mockEdiMessageOutbound.EM_MessageSubType = CcsukTransmissionMessageFunction.CUKFSR.FsaForExport.Subcode;
			mockEdiMessageOutbound.EM_LinkedObject = consol;

			_ = PrepareExampleAndParse(exampleFromSpecificationDocument[5], true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(mockEdiMessageOutbound));
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals(2, consol.Messages.Count);
			var receivedMessage = consol.Messages.LastIncomingMessage;
			AssertContains("BASIC CONSIGNMENT RECORD RETRIEVED", receivedMessage.EM_MessageInterpretation);
			AssertEquals("FSA", receivedMessage.EM_MessageType);
			AssertEquals("EXP", receivedMessage.EM_MessageSubType);
		}

		public void TestParseFSA_Example21_ReportJ1WithPrinters()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "00747410000";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRLHS";
			var split = basic.Splits.AddNew();
			split.SplitReference = "01";
			//BGM+:::J1+00747410000+7:9309061546:201++ACD::1'

			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[21], true);
			AssertEquals("J1", result.Header_ReportType);
			var basicReloaded = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertContains("<th> Split Reference</th><th>1</th></tr></thead><tr><td>Consignment Reference Number</td><td>1</td>", basicReloaded.Messages[0].EM_MessageInterpretation);
			AssertContains("<td>Print Location 1</td><td>001A</td></tr><tr><td>Print Location 2</td><td>POOP</td>", basicReloaded.Messages[0].EM_MessageInterpretation);
			AssertContains("<td>Special Action Indicator</td><td>Y</td>", basicReloaded.Messages[0].EM_MessageInterpretation);
		}

		[ExpectNoExceptions]
		public void TestParseFSA_Example32()
		{
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");
			Factory.Save();
			PrepareExampleAndParse(exampleFromSpecificationDocument[32], true);
		}

		public void TestParseFSA_Example33_WithTemporaryStorageEndDate()
		{
			CusMAWB mawb;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out mawb, out outboundMessage, false);
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[33], true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			receivedMessage.Reload();
			AssertEquals("Status", EDIMessage.Direction.Receive, receivedMessage.EM_Status);
		}

		public void TestParseFSA_Example34_WithTemporaryStorageEndDate()
		{
			CusMAWB mawb;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out mawb, out outboundMessage, false);
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "09187256";
			hawb.CS_IsMasterHouse = true;
			hawb.Messages.Add(outboundMessage);
			GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("DEU", "CUKAIR98LHRBAR");
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[34], true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			AssertEquals("TemporaryStorageEndDate", new ZDate(2017, 11, 20), hawb.TemporaryStorageEndDate.Date);
		}

		public void TestParseFSA_Example35_WithTemporaryStorageEndDate()
		{
			GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("DEU", "CUKAIR98LHRBAR");
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[35], true);
			var basic = Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertEquals("00746340000", basic.CM_MAWB);
			AssertEquals("TemporaryStorageEndDate", new ZDate(2016, 4, 11), basic.TemporaryStorageEndDate.Date);
		}

		public void TestParseFSA_Example36_WithTemporaryStorageEndDateOnlyInOldHalfOfMessage()
		{
			GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("DEU", "CUKAIR98LHRBAR");
			var result = PrepareExampleAndParse(exampleFromSpecificationDocument[36], true);
			var basic = Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertEquals("00746340000", basic.CM_MAWB);
			AssertEquals("TemporaryStorageEndDate", new ZDate(2016, 4, 11), basic.TemporaryStorageEndDate.Date);
		}

		public void TestParseFSA_Example37_LOCSegmentHasVeryLittleData()
		{
			CusMAWB mawb;
			EDIMessage outboundMessage;
			CreateBasicForFsrFsaUpdateTest(out mawb, out outboundMessage, true);
			mawb.Profile = "CUKAIR98LHRCOX";
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("AAA", "CUKAIR98LHRCOX");
			PrepareExampleAndParse(exampleFromSpecificationDocument[37], true, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(outboundMessage));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestP5_AttachmentToConsolAndShipment()
		{
			RunAttachmentToConsolAndShipmentTest(true);
		}

		public void TestP5_NoAttachmentToConsolAndShipmentWhenDisabledInRegistry()
		{
			RunAttachmentToConsolAndShipmentTest(false);
		}

		void RunAttachmentToConsolAndShipmentTest(bool linkEnabled)
		{
			using (GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GBCustomsDataRegistry.Instance.CcsukP5ProcessorLinkToConsole.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, linkEnabled))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKDischargePort = "GBLON";
				consol.JK_MasterBillNum = "17022012038";
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_HouseBill = "87654321";
				shipment.JS_RL_NKDestination = "GBLHR";
				shipment.JS_TransportMode = "AIR";
				consol.Shipments.Add(shipment);

				CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CWE", "CUKAIR98LHRCWE");
				var result = PrepareExampleAndParse(exampleFromSpecificationDocument[38], true);
				AssertEquals("P5", result.Header_ReportType);

				var mawb = Factory.LoadTop1<CusMAWB>(new ZQuery());
				AssertEquals("17022012038", mawb.CM_MAWB);
				var hawb = mawb.ChildBills[0];
				AssertEquals("87654321", hawb.CS_HAWB);

				if (linkEnabled)
				{
					AssertEquals("Consol linked to MAWB", consol.PK, mawb.CM_JK);
					AssertEquals("Shipment linked to HAWB", shipment.PK, hawb.CS_JS);
				}
				else
				{
					AssertEquals("Consol not linked to MAWB", ZGuid.Empty, mawb.CM_JK);
					AssertEquals("Shipment not linked to HAWB", ZGuid.Empty, hawb.CS_JS);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var anotherStaff = Factory.New<GlbStaff>();
			anotherStaff.GS_EmailAddress = "daniel@wisetechglobal.com";
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			var staffGroup = Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup);
			staffGroup.Staff.Add(currentUserInCurrentFactory);
			staffGroup.Staff.Add(anotherStaff);
			interchangeReceipientPima = "CUKFFW98000LXA";
			branchEnvironment = DisposableEnvironment.ForBranch(Enterprise.Customs.GB.Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDown()
		{
			base.TearDown();
			branchEnvironment.Dispose();
		}

		public void TestParseDoesntFailForAllRemainingExamples()
		{
			for (int index = 1; index < exampleFromSpecificationDocument.Length; index++)
			{
				try
				{
					PrepareExampleAndParse(exampleFromSpecificationDocument[index]);
				}
				catch (Exception ex)
				{
					Assert("Example number (0-based) " + index.ToString() + " threw on parsing. Error: " + ex.Message, false);
				}
			}
			Assert(true);
		}

		FsaResponseMessage PrepareExampleAndParse(string input, bool runServiceTaskToo = false, string outboundMessagePkForSysCar = null)
		{
			var mockEdiMessage = Factory.NewMoq<EDIMessage>();
			mockEdiMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("716");
			receivedMessage = mockEdiMessage.Object;
			receivedMessage.EM_MessageText = input.Replace(System.Environment.NewLine, "");
			receivedMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var receivedInterchange = Factory.New<EDIInterchange>();
			receivedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			receivedInterchange.EI_InterchangeNum = DateTime.Now.Ticks.ToString();
			receivedInterchange.EI_From = "Her Maj";
			receivedInterchange.EI_BodyText = "whatever";
			receivedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			receivedInterchange.EI_To = interchangeReceipientPima;
			receivedMessage.EM_EI = receivedInterchange.PK;

			if (outboundMessagePkForSysCar != null)
			{
				receivedMessage.EM_MessageText = receivedMessage.EM_MessageText.Replace("<<SYSCAR>>", outboundMessagePkForSysCar);
			}

			if (runServiceTaskToo)
			{
				Factory.Save();
				RunProcessors();
			}

			var parser = new FsaParser(receivedMessage);
			return parser.Parse();
		}

		string interchangeReceipientPima;

		EDIMessage receivedMessage;

		readonly string[] exampleFromSpecificationDocument = new string[]
		{
// Example 0 doesn't exist
"",

//1.	Status of pre-arrival basic AWB record for which an entry has not been made.
@"UNH+716+CUKFSA:1:912:BT'
BGM++88893080900'
FTX+AAA+++BASIC CONSIGNMENT RECORD RETRIEVED'
DOC+740+88893080900+97:9308091200:201'
GIS+T:121:ZZZ'
GIS+PAI:109:109'
TDT+20'
LOC+84:LAX:145:3+85:LHR:145:3+11:LHR:145:3::KLM:129:ZZZ:AAA'
NAD+CB+DEI+DEI AGENT'
GDS+2'
QTY+118:100'
MEA+WT++KGM:100.0'
FTX+AAA+++PLASTIC DOLLS'
UNT+14+716'",

//2.	Status of post-arrival basic AWB record.
@"UNH+MSGREF+CUKFSA:1:912:BT'
BGM++88893080900'
FTX+AAA+++BASIC CONSIGNMENT RECORD RETRIEVED'
DOC+740+88893080900+97:9308091124:201'
GIS+T:121:ZZZ'
TDT+20+567++++IB:172:3++178:930809:101'
LOC+84:LAX:145:3+85:LHR:145:3+27:US+11:LHR:145:3::KLM:129:ZZZ:AAA'
NAD+CB+DEI'
GDS+2'
QTY+118:100'
QTY+48:100'
MEA+WT++KGM:100.0'
DTM+7:9308091341:201'
FTX+AAA+++PLASTIC DOLLS'
UNT+17+MSGREF'",

//3.	Status of basic AWB record for which entry has been made and cleared.
@"UNH+MSGREF+CUKFSA:1:912:BT'
BGM++11177777777'
DOC+740+11177777777+97:9308091055:201'
GIS+23:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+AZ123++++AZ:172:3++178:930809:101'
LOC+84:LAX:145:3+85:LHR:145:3+27:US+11:LHR:145:3::KLM:129:ZZZ:AAA'
NAD+CB+FRF+FREDS FORWARDING'
GDS+2'
QTY+118:25'
QTY+48:25'
MEA+WT++KGM:100'
DTM+7:9308091055:201'
FTX+AAA+++SMALL WIDGETS'
CST++CC:117:ZZZ+01:120:109+3:141:109+000:110:ZZZ'
FTX+CAT+++CUSTOMS CLEARED'
FTX+IRT+++INVENTORY RETURN CODE TEXT'
DTM+176:9308091320:201'
RFF+ACF:131'
RFF+TN:123456J+141:19930809:102'
RFF+ABE:12345678'
QTY+66:25'
UNT+23+MSGREF'",

//4.	Status of split basic AWB record for which an entry has not been made. Enquiry made on split number.
@"UNH+MSGREF+CUKFSA:1:912:BT+<<SYSCAR>>'
BGM++88893080900+++ACD::01'
FTX+AAA+++SPLIT CONSIGNMENT RECORD RETRIEVED'
DOC+SRF:ZZZ+01+97:9308091351:201'
GIS+T:121:ZZZ'
TDT+20+567++++IB:172:3++178:930809:101'
LOC+84:LAX:145:3+85:LHR:145:3+27:US+11:LHR:145:3::KLM:129:ZZZ:AAA'
NAD+CB+DEI'
GDS+2'
QTY+118:30'
QTY+48:30'
MEA+WT++KGM:30.0'
DTM+7:9308091351:201'
FTX+AAA+++PLASTIC DOLLS'
UNT+17+MSGREF'
",

//5.	Status of 'through basic' consignment; authority to tranship has been granted
@"UNH+MSGREF+CUKFSA:1:912:BT+<<SYSCAR>>'
BGM++12510000888'
FTX+AAA+++BASIC CONSIGNMENT RECORD RETRIEVED WITH ENTRY 	DETAILS'
DOC+740+12510000888+97:9308091000:201'
GIS+27:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+802++++BA:172:3++178:930723:101'
LOC+84:MIA:145:3+85:GLW:145:3+27:US+28:UK+11:GLW:145:3::BAS:129:ZZZ:AAA'
TDT+12++40+++BA:172:3'
LOC+84:GLW:145:3+85:JFK:145:3'
RFF+AWB:12510000889'
NAD+CB+DEI'
GDS+2'
QTY+118:90'
QTY+48:90'
MEA+WT++KGM:900.0'
DTM+7:9308091000:201'
CST++CA:117:ZZZ+00:120:109+3:141:109+000:110:ZZZ'
FTX+CAT+++ACCEPTED'
DTM+176:9308091424:201'
RFF+ABE:87654321'
UNT+23+MSGREF'
",

//6.	Status returned on basic that has 3 splits (no split number supplied in the interrogation).
@"UNH+MSGREF+CUKFSA:1:912:BT'
BGM++88893080900'
FTX+AAA+++BASIC CONSIGNMENT RECORD RETRIEVED WHICH HAS SPLITS'
DOC+740+88893080900+97:9308091124:201'
GIS+23:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+567++++IB:172:3++178:930809:101'
LOC+84:LAX:145:3+85:LHR:145:3+27:US+11:LHR:145:3::KLM:129:ZZZ:AAA'
NAD+CB+DEI'
GDS+2'
QTY+118:100'
QTY+48:100'
MEA+WT++KGM:100.0'
DTM+7:9308091341:201'
DOC+SRF:ZZZ+01'
DOC+SRF:ZZZ+02'
DOC+SRF:ZZZ+03'
UNT+29+MSGREF'",

//7.	Status returned on AWB that contains 4 house AWBs (no house details supplied in the interrogation).
@"UNH+MSGREF+CUKFSA:1:912:BT+<<SYSCAR>>'
BGM++88893080901+7:9308091348:201'
FTX+AAA+++MASTER CONSIGNMENT RECORD RETRIEVED WHICH HAS 	HOUSES'
DOC+741+88893080901+97:9308091346:201'
GIS+T:121:ZZZ'
TDT+20+456++++AR:172:3++178:930809:101'
LOC+84:XAT:145:3+85:STN:145:3+27:XX+11:MAN:145:3::KLM:129:ZZZ:AAA'
NAD+CB+LXA'
GDS+2'
QTY+118:300'
MEA+WT++KGM:300.0'
DTM+7:9308091347:201'
DOC+703+93080902'
DOC+703+93080903'
DOC+703+93080904'
UNT+23+MSGREF'"

,//8.	Status of House AWB with splits. House Waybill number supplied.

@"UNH+MSGREF+CUKFSA:1:912:BT+<<SYSCAR>>'
BGM++88893080901+++HWB:93080903'
FTX+AAA+++HOUSE CONSIGNMENT RECORD RETRIEVED WHICH HAS 	SPLITS'
DOC+703+93080903+97:9308091346:201'
GIS+T:121:ZZZ'
TDT+20+456++++AR:172:3++178:930809:101'
LOC+84:JFK:145:3+85:LHR:145:3+27:US+11:LHR:145:3::KLM:129:ZZZ:AAA'
NAD+CB+ABC'
GDS+2'
QTY+118:100'
QTY+48:100'
MEA+WT++KGM:100.0'
DTM+7:9308091347:201'
DOC+SRF:ZZZ+01'
DOC+SRF:ZZZ+02'
UNT+23+MSGREF'
",

//9.	Status of basic AWB that exists in more than 1 shed (Customs interrogations). Shed details not supplied in interrogation.
//Note that details are nearly identical except for the date and time of record creation and the shed associated with the airport of receipt.

@"UNH+MSGREF+CUKFSA:1:912:BT'
BGM++88893080925'
DOC+740+88893080925+97:9308091600:201'
TDT+20'
LOC+84:ALC:145:3+85:LHR:145:3+11:LHR:145:3::KLM:129:ZZZ'
DOC+740+88893080925+97:9308091645:201'
TDT+20'
LOC+84:ALC:145:3+85:LHR:145:3+11:LHR:145:3::LHS:129:ZZZ'
UNT+11+MSGREF'
",

//10.	Record requested not found.

@"UNH+MSGREF+CUKFSA:1:912:BT+<<SYSCAR>>'
BGM++88893080900'
DOC+740+88893080900'
GIS+2'
FTX+AAA+++REJECTED   NO MATCHING CONSIGNMENT RECORD FOUND'
UNT+6+MSGREF'

",

//11.	Inventory Failure Report (E0).

@"UNH+MSGREF+CUKFSA:1:912:BT'
BGM+:::E0+11110001234+7:9206011300:201'
FTX+AAA+++INVENTORY FAILURE REPORT'
DOC+740+11110001234+97:9206011246:201'
GIS+23:117:ZZZ'
TDT+20'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::AZS:129:ZZZ'
GDS+2'
QTY+48:23'
CST+0+23:120:109+000:110:ZZZ'
FTX+IRT+++NPX DOES NOT EQUAL NOP'
RFF+ACF:131'
RFF+TN:123456J+141:19920601:102'
RFF+ABE:12345678'
UNT+15+MSGREF'",

//12.	Pre-arrival Agent Mismatch Report (G5)

@"UNH+MSGREF+CUKFSA:1:912:BT'
BGM+:::G5+11112345677+7:9206011300:201'
FTX+AAA+++PRE ARRIVAL AGENT MISMATCH REPORT'
DOC+740+11112345677+97:9206011246:201++++OLD'
TDT+20'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::AZS:129:ZZZ'
NAD+CB+YOG+ZEN MOVERS'
DOC+740+11112345677+++++NEW'
NAD+CB+WOW+WOW SHIFTERS'
UNT+10+MSGREF'",

//13.	Goods Arrival Reprocessing Error Report (H3)

@"UNH+MSGREF+CUKFSA:1:912:BT'
BGM+:::H3+11112345677+7:9206011900:201'
FTX+AAA+++GOOD ARRIVAL REPROCESSING ERROR'
DOC+740+11123456778+97:9206011800:201'
GIS+23:117:ZZZ'
TDT+20'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::AZS:129:ZZZ'
GDS+2'
QTY+48:20'
CST+0+23:120:109'
RFF+ACF:131'
RFF+TN:1234567+141:19920601:102'
RFF+ABE:12345678'
RFF+CKN:1'
UNT+15+MSGREF'
",

//14.	Advice of Inter-Shed Removal Report (P5)

@"UNH+ISR1FVBG3DKYO0+CUKFSA:1:912:BT'
BGM+:::P5+00746340000+7:9308101452:201'
DOC+740+00746340000+++++OLD'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+002+40+++BA:172:3++178:931027:101'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::LHS:129:ZZZ'
TDT+12'
NAD+CB+DEU'
GDS+2'
QTY+118:10'
QTY+48:10'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
DOC+740+00746340000+++++NEW'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+002+40+++BA:172:3++178:931027:101'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::BAR:129:ZZZ'
TDT+12'
NAD+CB+DEU'
GDS+2'
QTY+118:10'
QTY+48:10'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
UNT+27+ISR1FVBG3DKYO0'",

//15.	Report Duplication Report (U).

@"UNH+FRI1FUB0QF1HT0+CUKFSA:1:912:BT'
BGM+:::U+32132132132+7:9307081357:201'
DOC+740+32132132132'
TDT+20'
LOC+84:MIA:145:3+11:LHR:145:3::TWA:129:ZZZ'
GDS+2'
QTY+118:1'
DOC+740+32132132132'
TDT+20'
LOC+11:LHR:145:3::LHS:129:ZZZ'
GDS+2'
QTY+118:100'
QTY+48:11'
UNT+14+FRI1FUB0QF1HT0'",

//16.	Report JA
//This report is identical to report EO except for the data element 'report type' in the BGM section which is equal to JA.

@"UNH+IUM1FW0RQDDM60+CUKFSA:1:912:BT'
BGM+:::JA+42042710000+7:9309021407:201++HWB:42710101'
DOC+703+42710101'
GIS+23:117:ZZZ'
TDT+20'
LOC+11:MAN:145:3::LHS:129:ZZZ'
CST+0'
RFF+ACF:130'
RFF+TN:4271427+141:19921231:102'
UNT+10+IUM1FW0RQDDM60'",

//17.	Report JC
@"

UNH+FCS1FTMMR708N0+CUKFSA:1:912:BT'
BGM+:::JC+00340300000+++HWB:40304030'
DOC+703+40304030+++++OLD'
TDT+20'
LOC+11:LHR:145:3::BUR:129:ZZZ'
NAD+CB+DEU+DUDDLEYS'
CST+0'
RFF+ACF:034'
RFF+TN:4303000+141:19930328:102'
UNT+10+FCS1FTMMR708N0'",

//18.	Report JD
@"UNH+FSN1FW6C1TKZ00+CUKFSA:1:912:BT'
BGM+:::JD+00645060000+7:9309071514:201'
FTX+AAA+++CUSTOMS ADVICE FOR CCS 05 07 OR O9'
DOC+740+00645060000+++++OLD'
GIS+23:117:ZZZ'
TDT+20'
LOC+84:JFK:145:3+85:LHR:145:3+27:US+28:GB+11:LHR:145:3::LHS:129:ZZZ'
GDS+2'
QTY+118:10'
QTY+48:0000'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
DOC+740+00645060000+++++NEW'
UNT+14+FSN1FW6C1TKZ00'",

//19.	Report JE
@"UNH+FSN1FV5R8JVBS0+CUKFSA:1:912:BT'
BGM+:::JE+00645860000+7:9308051101:201'
FTX+AAA+++FALLBACK CCS 95 ADVICE'
DOC+740+00645860000+++++OLD'
GIS+23:117:ZZZ'
TDT+20'
LOC+84:JFK:145:3+85:STA:145:3+11:STA:145:3::LHS:129:ZZZ'
NAD+CB+DOD+DODGY'
GDS+2'
QTY+118:10'
QTY+48:0000'
CST++CA:117:ZZZ'
FTX+CAT+++ENTRY ACCEPTED'
DTM+176:9210261130:201'
RFF+ACF:045'
RFF+TN:0003402+141:19921010:102'
RFF+ABE:12345678'
UNT+18+FSN1FV5R8JVBS0'",

// 20.  report J4
@"
UNH+MSGRFF+CUKFSA:1:912:BT'
BGM+J4+1112345677+7:9205200856:201'
FTX+AAA+++report text'
DOC+740+1112345677'
GIS+C:121:ZZZ'
TDT+20+BA123+40+++BAA:172:3:+178:920520:101'
LOC+84:LAX:145:3+85:LHR:145:3:+11:LHR:145:3::LHS:129:ZZZ'
NAD+CB+FRF'
GDS+2'
QTY+118:100'
QTY+48:100'
MEA+WT++KGM:200'
FTX+AAA+++SMALL BOLTS'
RFF+HS+BL6687879'
RFF+HS+577587788'
UNT+16+MSGREF'",

//21.	Report J1

@"UNH+Y965FW598TM160+CUKFSA:1:912:BT'
BGM+:::J1+00747410000+7:9309061546:201++ACD::01'
FTX+AAA+++RECORD NO 533 NO MATCH ON AOD'
DOC+SRF:ZZZ+1'
GIS+28:117:ZZZ'
GIS+SAI:109:ZZZ'
TDT+20'
LOC+84:MAN:145:3+27:GB+28:FR+11:LHR:145:3::LHS:129:ZZZ'
LOC+5:LHV:139'
RFF+AWB:00700041074'
NAD+CB+DAD+TEST'
NAD+CM'
COM+001A:CA'
COM+POOP:CA'
GDS+2'
QTY+118:10'
QTY+48:10'
MEA+WT++KGM:121.5'
FTX+AAA+++TEST AAAAAAA'
CST+0'
RFF+ABE:4321'
RFF+CKN:822'
UNT+23+Y965FW598TM160'",

//22.	Report J2

@"UNH+Z083FVJDKZBX60+CUKFSA:1:912:BT'
BGM+:::J2+32132132132+7:9308171930:201'
DOC+740+32132132132'
GIS+23:117:ZZZ'
TDT+20'
LOC+84:MAN:145:3+85:LHR:145:3+11:LHR:145:3::LHS:129:ZZZ'
NAD+CB+DAD+TEST'
GDS+2'
QTY+118:11'
QTY+48:211'
MEA+WT++KGM:34567.5'
FTX+AAA+++TEST AAAAAAA'
CST+0'
FTX+CAT+++ENTRY ACCEPT'
DTM+176:1993080111:201'
RFF+ACF:001'
UNT+17+Z083FVJDKZBX60'",

//23.	Report J3

@"UNH+X488FW7JHKBN40+CUKFSA:1:912:BT'
BGM+:::J3+00747410000+7:9309081730:201'
FTX+AAA+++AOD NOT CUSTOMS APPROVED'
DOC+740+00747410000'
GIS+24:117:ZZZ'
TDT+20'
LOC+84:MAN:145:3+85:MAN:145:3+11:LHR:145:3::LHS:129:ZZZ'
GDS+2'
QTY+118:10'
QTY+48:10'
MEA+WT++KGM:121.5'
FTX+AAA+++MTT  TES'
UNT+13+X488FW7JHKBN40'",

//24.	Report J6

@"UNH+X495FW7JTYZ740+CUKFSA:1:912:BT'
BGM+:::J6A+88847410000+7:9309081742:201'
DOC+740+88847410000+++++OLD'
GIS+24:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20'
LOC+84:MAN:145:3+85:MAN:145:3+11:LHR:145:3::LHS:129:ZZZ'
GDS+2'
QTY+118:10'
FTX+AAA+++MTT TES'
CST+0'
RFF+ACF:001'
DOC+740+88847410000+++++NEW'
GIS+E:121:ZZZ'
TDT+20'
LOC+84:MAN:145:3+85:MAN:145:3'
GDS+2'
QTY+118:10'
FTX+AAA+++MTT TES'
UNT+20+X495FW7JTYZ740'

UNH+X235FW6BV5U1C0+CUKFSA:1:912:BT'
BGM+:::J6B+32132132132+7:9309071508:201'
DOC+740+32132132132+++++OLD'
GIS+28:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20'
LOC+84:MAN:145:3+85:JFK:145:3+11:LHR:145:3::LHS:129:ZZZ'
NAD+CB+DAD+TEST'
GDS+2'
QTY+118:10'
QTY+48:10'
FTX+AAA+++NPRREDUCED TES'
CST+0+00:120:109'
RFF+ACF:001'
DOC+740+32132132132+++++NEW'
GIS+T:121:ZZZ'
TDT+20'
LOC+84:MAN:145:3+85:JFK:145:3'
GDS+2'
QTY+118:10'
QTY+48:21'
FTX+AAA+++NPR REDUCED TES'
UNT+23+X235FW6BV5U1C0'",

//25.	Report J7

@"UNH+MSGRFF+CUKFSA:1:912:BT'
BGM+J7+1112345677+7:9205261530:201'
FTX+AAA+++report text'
DOC+740+1112345677'
GIS+LIC:109:ZZZ'
GIS+SAI:109:ZZZ'
TDT+12+++++AF:172:3'
LOC+85:ZRH:145:3+27:US+28:GDR+11:LHR:145:3::AZS:129:ZZZ'
RFF+740:1112345677'
NAD+CB+OJH'
GDS+2'
QTY+118:30'
QTY+48:30'
MEA+WT++KGM:35'
FTX+AAA+++PAINT BRUSHES'
CST++CT:117:ZZZ'
FTX+CAT+++REMOVE OVERSEAS'
RFF+ABE:HONESTRFF1'
UNT+19+MSGREF'",

//26.	Report J9

@"UNH+MSGRFF+CUKFSA:1:912:BT'
BGM+J9+1112345677+7:9205261400:201'
DOC+740+1112345677'
GIS+LIC:109:ZZZ'
GIS+SAI:109:ZZZ'
TDT+12+++++AF:172:3'
LOC+85:LHR:145:3+27:US+28:UK+11:LHR:145:3::AZS:129:ZZZ'
RFF+740:1117654321'
NAD+CB++FRED BLOGS'
GDS+2'
QTY+118:10'
QTY+48:10'
MEA+WT++KGM:200'
FTX+AAA+++COLUMBIAN FLOUR'
CST+CW:117:ZZZ'
FTX+CAT+++REMOVE UK'
RFF+ACF:101'
RFF+TN:1234567Y+141:19920526'
UNT+19+MSGREF'
"

// 27 Real (not example) message for P5 for split house
,
@"UNH+JISRBS14SW6W90+CUKFSA:1:912:BT'
BGM+:::P5+17022012005+7:1202171150:201++HWB:SPLITHSE:02'
DOC+SRF:ZZZ+02+++++OLD'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+005+40+++BA:172:3++178:120217:101'
LOC+84:ATL:145:3+85:LHR:145:3+11:LHR:145:3::CAX:129:ZZZ'
TDT+12++40'
NAD+CB+CAR'
GDS+2'
QTY+118:3'
MEA+WT++KGM:3'
FTX+AAA+++TO SPLIT'
DOC+SRF:ZZZ+02+++++NEW'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+005+40+++BA:172:3++178:120217:101'
LOC+84:ATL:145:3+85:LHR:145:3+11:LHR:145:3::CWE:129:ZZZ'
TDT+12++40'
NAD+CB+CAR'
GDS+2'
QTY+118:3'
MEA+WT++KGM:3'
FTX+AAA+++TO SPLIT'
UNT+25+JISRBS14SW6W90'",

// 28 Real (not example) message for P5 for whole house

@"UNH+JISRBS14SPCS00+CUKFSA:1:912:BT'
BGM+:::P5+17022012005+7:1202171150:201++HWB:WHOLEHSE'
DOC+703+WHOLEHSE+++++OLD'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+005+40+++BA:172:3++178:120217:101'
LOC+84:LAX:145:3+85:LHR:145:3+11:LHR:145:3::CAX:129:ZZZ'
TDT+12++40'
NAD+CB+CAR'
GDS+2'
QTY+118:6'
MEA+WT++KGM:6'
FTX+AAA+++WHOLE'
DOC+703+WHOLEHSE+++++NEW'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+005+40+++BA:172:3++178:120217:101'
LOC+84:LAX:145:3+85:LHR:145:3+11:LHR:145:3::CWE:129:ZZZ'
TDT+12++40'
NAD+CB+CAR'
GDS+2'
QTY+118:6'
MEA+WT++KGM:6'
FTX+AAA+++WHOLE'
UNT+25+JISRBS14SPCS00'
",

// 29 P5 split basic
@"UNH+JISRBS14XIOQR0+CUKFSA:1:912:BT'
BGM+:::P5+17022012006+7:1202171154:201++ACD::02'
DOC+SRF:ZZZ+02+++++OLD'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+006+40+++BA:172:3++178:120217:101'
LOC+84:ATL:145:3+85:LHR:145:3+11:LHR:145:3::CAX:129:ZZZ'
TDT+12++40'
NAD+CB+CAR'
GDS+2'
QTY+118:8'
MEA+WT++KGM:8'
FTX+AAA+++BASIC TO SPLIT'
DOC+SRF:ZZZ+02+++++NEW'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+006+40+++BA:172:3++178:120217:101'
LOC+84:ATL:145:3+85:LHR:145:3+11:LHR:145:3::CWE:129:ZZZ'
TDT+12++40'
NAD+CB+CAR'
GDS+2'
QTY+118:8'
MEA+WT++KGM:8'
FTX+AAA+++BASIC TO SPLIT'
UNT+25+JISRBS14XIOQR0'
"
,

// Our number 30, Example number 26 from v4.5 of specs - query response with community handling codes
@"UNH+MSGREF+CUKFSA:1:912:BT'
BGM++88893080900'
FTX+AAA+++BASIC CONSIGNMENT RECORD RETRIEVED'
DOC+740+88893080900+97:9308091124:201'
GIS+T:121:ZZZ'
GIS+AAA:131'
GIS+BBB:131'
GIS+CCC:131'
TDT+20+567++++IB:172:3++178:930809:101'
LOC+84:LAX:145:3+85:LHR:145:3+27:US+11:LHR:145:3::KLM:129:ZZZ:AAA'
NAD+CB+DEI'
GDS+2'
QTY+118:100'
QTY+48:100'
MEA+WT++KGM:100.0'
DTM+7:9308091341:201'
FTX+AAA+++PLASTIC DOLLS'
UNT+18+MSGREF'
"

,
// Our message #31, FSR query on a split house, response says no such split
@"UNH+3091+CUKFSA:1:912:BT+<<SYSCAR>>'
BGM++12510000888+7:1310250401:201++HWB:09187256:01'
DOC+SRF:ZZZ+01'
GIS+2'
FTX+AAA+++REJECTED - NO MATCHING CONSIGNMENT RECORD FOUND'
UNT+6+3091'"
,
// #32
@"UNH+JISRDRCVIJTRH0+CUKFSA:3:912:BT'
BGM+:::P5+04072018003+7:1807041059:201++ACD::01'
DOC+SRF:ZZZ+01+++++OLD'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+003+40+++BA:172:3++178:180704:101'
LOC+84:ATL:145:3+85:LHR:145:3+11:LHR:145:3::CWE:129:ZZZ'
TDT+12++40'
NAD+CB+DJC'
GDS+2'
QTY+118:6'
MEA+WT++KGM:6'
FTX+AAA+++SPLIT BASIC P5'
DTM+164:20181002:102'
DOC+SRF:ZZZ+01+++++NEW'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+003+40+++BA:172:3++178:180704:101'
LOC+84:ATL:145:3+85:LHR:145:3+11:LHR:145:3::CAX:129:ZZZ'
TDT+12++40'
NAD+CB+DJC'
GDS+2'
QTY+118:6'
MEA+WT++KGM:6'
FTX+AAA+++SPLIT BASIC P5'
UNT+26+JISRDRCVIJTRH0'
",

// #33 real world CUKFSA, with Temporary Storage end date
@"UNH+60+CUKFSA:3:912:BT+<<SYSCAR>>'
BGM++00173582666+7:1708291206:201'
DOC+740+00173582666'
GDS+2'
DTM+164:20171120:102'
DOC+740+00173582666'
TDT+20'
LOC+11:LHR:145:3::AAS:129:ZZZ'
DOC+740+00173582666'
TDT+20'
LOC+11:LHR:145:3::WXS:129:ZZZ'
UNT+12+60'
",

// #34 CUKFSA House, with Temporary Storage end date
@"UNH+60+CUKFSA:3:912:BT+<<SYSCAR>>'
BGM++00173582666+7:1708291206:201'
DOC+740+00173582666'
TDT+20'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::BAR:129:ZZZ'
GDS+2'
DTM+164:20171120:102'
UNT+8+60'
",

// #35 Advice of Inter-Shed Removal Report (P5) with Temporary Storage end date
@"UNH+ISR1FVBG3DKYO0+CUKFSA:3:912:BT'
BGM+:::P5+00746340000+7:9308101452:201'
DOC+740+00746340000+++++OLD'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+002+40+++BA:172:3++178:931027:101'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::LHS:129:ZZZ'
TDT+12'
NAD+CB+DEU'
GDS+2'
QTY+118:10'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
DTM+164:20160411:102'
DOC+740+00746340000+++++NEW'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+002+40+++BA:172:3++178:931027:101'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::BAR:129:ZZZ'
TDT+12'
NAD+CB+DEU'
GDS+2'
QTY+118:10'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
DTM+164:20160411:102'
UNT+27+ISR1FVBG3DKYO0'"
,

// #36 Advice of Inter-Shed Removal Report (P5) with Temporary Storage end date  BUT only in the 'old' half of the message
@"UNH+ISR1FVBG3DKYO0+CUKFSA:3:912:BT'
BGM+:::P5+00746340000+7:9308101452:201'
DOC+740+00746340000+++++OLD'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+002+40+++BA:172:3++178:931027:101'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::LHS:129:ZZZ'
TDT+12'
NAD+CB+DEU'
GDS+2'
QTY+118:10'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
DTM+164:20160411:102'
DOC+740+00746340000+++++NEW'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+002+40+++BA:172:3++178:931027:101'
LOC+84:JFK:145:3+85:LHR:145:3+11:LHR:145:3::BAR:129:ZZZ'
TDT+12'
NAD+CB+DEU'
GDS+2'
QTY+118:10'
MEA+WT++KGM:100'
FTX+AAA+++PENS'
UNT+26+ISR1FVBG3DKYO0'
",

// 37 real-world FSA without much data in the LOC segment
@"UNH+221+CUKFSA:3:912:BT+<<SYSCAR>>'
BGM++00173582666+7:1904040913:201'
FTX+AAA+++MASTER CONSIGNMENT RECORD RETRIEVED'
DOC+741+00173582666+97:1904031445:201++++OLD'
TDT+20'
LOC+11:LHR:145:3::COX:129:ZZZ:ERT'
GDS+2'
DTM+164:20190702:102'
UNT+9+221'
",

// 38 P5 for whole house

@"UNH+JISRBS14SPCS00+CUKFSA:1:912:BT'
BGM+:::P5+17022012038+7:1202171150:201++HWB:87654321'
DOC+703+87654321+++++OLD'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+005+40+++BA:172:3++178:120217:101'
LOC+84:LAX:145:3+85:LHR:145:3+11:LHR:145:3::CAX:129:ZZZ'
TDT+12++40'
NAD+CB+CAR'
GDS+2'
QTY+118:6'
MEA+WT++KGM:6'
FTX+AAA+++WHOLE'
DOC+703+87654321+++++NEW'
GIS+29:117:ZZZ'
GIS+T:121:ZZZ'
TDT+20+005+40+++BA:172:3++178:120217:101'
LOC+84:LAX:145:3+85:LHR:145:3+11:LHR:145:3::CWE:129:ZZZ'
TDT+12++40'
NAD+CB+CAR'
GDS+2'
QTY+118:6'
MEA+WT++KGM:6'
FTX+AAA+++WHOLE'
UNT+25+JISRBS14SPCS00'
"        };
	}

	sealed class DummyEDIMessage_FsaTests : EDIMessage
	{
		public DummyEDIMessage_FsaTests(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
		{
		}

		public string GetMessageReferenceNumberReturns { get; set; } = string.Empty;

		protected override string GetMessageReferenceNumber()
		{
			return GetMessageReferenceNumberReturns;
		}
	}
}
