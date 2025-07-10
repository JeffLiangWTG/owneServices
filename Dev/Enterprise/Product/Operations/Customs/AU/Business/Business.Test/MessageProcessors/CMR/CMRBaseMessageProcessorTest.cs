using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using Moq.Protected;
using static Enterprise.Customs.AU.Declaration.Business.CMRBaseMessageProcessor;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class CMRBaseMessageProcessorTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestSuccessfullyPreProcessCONTRL()
		{
			messageToProcess = Factory.New<CMRCONTRLMessage>();
			messageToProcess.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			messageToProcess.EM_Status = EDIMessage.Status.Queued;

			var mockControlProcessor = new Mock<CONTRLMessageProcessor>(new LoggingInformation()) { CallBase = true };
			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCONTROLProcessor()).Returns(mockControlProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = "UNH+000001+CONTRL:D:3:UN'UCI+104715+AAA374M::AAA374M+AAA336C+4+14'UNT+3+000001'";
			processor.PreProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.PreProcessedOK, messageToProcess.EM_Status);
		}

		public void TestSuccessfullyProcessCONTRL()
		{
			messageToProcess = Factory.New<CMRCONTRLMessage>();
			messageToProcess.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			messageToProcess.EM_Status = EDIMessage.Status.Queued;

			var mockControlProcessor = new Mock<CONTRLMessageProcessor>(new LoggingInformation());
			mockControlProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Received);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCONTROLProcessor()).Returns(mockControlProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = "UNH+000001+CONTRL:D:3:UN'UCI+104715+AAA374M::AAA374M+AAA336C+4+14'UNT+3+000001'";
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Received, messageToProcess.EM_Status);
		}

		public void TestUnsuccessfullyProcessCONTRL()
		{
			var mockControlProcessor = new Mock<CONTRLMessageProcessor>(new LoggingInformation());
			mockControlProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Error);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCONTROLProcessor()).Returns(mockControlProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = "UNH+000001+CONTRL:D:3:UN'UCI+104715+AAA374M::AAA374M+AAA336C+4+14'UNT+3+000001'";
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestSuccessfullyPreProcessEXDR()
		{
			var mockProcessor = new Mock<EXDRMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoPreProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.PreProcessedOK);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = eXRMessageText;
			processor.PreProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.PreProcessedOK, messageToProcess.EM_Status);
		}

		public void TestSuccessfullyProcessEXDR()
		{
			var mockProcessor = new Mock<EXDRMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Received);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = eXRMessageText;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Received, messageToProcess.EM_Status);
		}

		public void TestUnsuccessfullyProcessEXDR()
		{
			var mockProcessor = new Mock<EXDRMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Error);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = eXRMessageText;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestPreProcessGarbage()
		{
			var mockProcessor = new Mock<EXDRMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Received);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = "sdfsdfsdf";
			processor.PreProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestProcessGarbage()
		{
			var mockProcessor = new Mock<EXDRMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Received);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = "sdfsdfsdf";
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestPreProcessInvalidCUSRES()
		{
			var processor = new CMRTestMessageProcessor(new LoggingInformation());
			messageToProcess.EM_Status = EDIMessage.Status.Queued;
			messageToProcess.EM_MessageText = eXRMessageText.Replace("EXDR", "LALA");
			processor.PreProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestProcessInvalidCUSRES()
		{
			CMRTestMessageProcessor processor = new CMRTestMessageProcessor(new LoggingInformation());
			messageToProcess.EM_Status = EDIMessage.Status.Queued;
			messageToProcess.EM_MessageText = eXRMessageText.Replace("EXDR", "LALA");
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestProcessCARSTFromCTOOutOfSequence()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "D225A";
			messageToProcess.EM_Status = EDIMessage.Status.Queued;
			messageToProcess.EM_MessageText = cARSTMessageText;
			messageToProcess.EM_MessageType = "CRS";
			messageToProcess.Factory.Save();

			var processor = new AUCCRSMessageProcessor();
			processor.ExecuteBatch();

			messageToProcess.Reload();
			AssertEquals("Status", EDIMessage.Status.Received, messageToProcess.EM_Status);
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "";
		}

		public void TestProcessCARMOV()
		{
			messageToProcess.EM_MessageText = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.AU.Declaration.Business.Testing.MessageProcessors.CMR.TestFiles.CARMOVDoNotLoadMessage.txt").Replace("\r\n", "");
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Received, messageToProcess.EM_Status);
		}

		public void TestSuccessfullyProcessIMDR()
		{
			var mockProcessor = new Mock<IMDRMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Received);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = CMRImportDeclarationTestData.IMRHeld;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Received, messageToProcess.EM_Status);
		}

		public void TestUnsuccessfullyProcessIMDR()
		{
			var mockProcessor = new Mock<IMDRMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Error);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = CMRImportDeclarationTestData.IMDRClear;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestSuccessfullyProcessATD()
		{
			var mockProcessor = new Mock<ATDMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Received);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = aTDMessageText;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Received, messageToProcess.EM_Status);
		}

		public void TestUnsuccessfullyProcessATD()
		{
			var mockProcessor = new Mock<ATDMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Error);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = aTDMessageText;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestSuccessfullyProcessSAM()
		{
			var mockProcessor = new Mock<SAMMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Received);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = sAMMessageText;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Received, messageToProcess.EM_Status);
		}

		public void TestUnsuccessfullyProcessSAM()
		{
			var mockProcessor = new Mock<SAMMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Error);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = sAMMessageText;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestSuccessfullyProcessDSA()
		{
			var mockProcessor = new Mock<DSAMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Received);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = DSAMessageText;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Received, messageToProcess.EM_Status);
		}

		public void TestUnsuccessfullyProcessDSA()
		{
			var mockProcessor = new Mock<DSAMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Error);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = DSAMessageText;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestSuccessfullyProcessDOCS()
		{
			var mockProcessor = new Mock<DOCSMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Received);
			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = dOCSMessageText;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Received, messageToProcess.EM_Status);
		}

		public void TestUnsuccessfullyProcessDOCS()
		{
			var mockProcessor = new Mock<DOCSMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Error);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = dOCSMessageText;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestSuccessfullyProcessPAYREC()
		{
			var mockProcessor = new Mock<PAYRECMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Received);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Received, messageToProcess.EM_Status);
		}

		public void TestUnsuccessfullyProcessPAYREC()
		{
			var mockProcessor = new Mock<PAYRECMessageProcessor>(new LoggingInformation());
			mockProcessor.Protected().Setup<string>("DoProcessingReturningStatus", messageToProcess).Returns(EDIMessage.Status.Error);

			var mockDirector = new Mock<CMRTestMessageProcessor>(new LoggingInformation()) { CallBase = true };
			mockDirector.Setup(m => m.GetCUSRESProcessor(It.IsAny<string>())).Returns(mockProcessor.Object);
			processor = mockDirector.Object;

			messageToProcess.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			processor.ProcessMessage(messageToProcess);
			AssertEquals("Status", EDIMessage.Status.Error, messageToProcess.EM_Status);
		}

		public void TestGetCUSRESProcessor()
		{
			processor = new CMRTestMessageProcessor(new LoggingInformation());
			AssertProcessorType("QWERTY", null);

			AssertProcessorType("EXDR", typeof(EXDRMessageProcessor));
			AssertProcessorType("EXREL", typeof(EXRELMessageProcessor));
			AssertProcessorType("IMDR", typeof(IMDRMessageProcessor));
			AssertProcessorType("EMMR", typeof(EMMRMessageProcessor));
			AssertProcessorType("DEPARTR", typeof(DEPARTRMessageProcessor));
			AssertProcessorType("STREQR", typeof(STREQRMessageProcessor));
			AssertProcessorType("DEPRELR", typeof(DEPRELRMessageProcessor));
			AssertProcessorType("DEPRECR", typeof(DEPRECRMessageProcessor));
			AssertProcessorType("WARRELR", typeof(WARRELRMessageProcessor));
			AssertProcessorType("WARRETR", typeof(WARRETRMessageProcessor));
			AssertProcessorType("CTORECR", typeof(CTORECRMessageProcessor));
			AssertProcessorType("CTOREMR", typeof(CTOREMRMessageProcessor));
			AssertProcessorType("IDL", typeof(IDLMessageProcessor));
			AssertProcessorType("CARMOV", typeof(CARMOVMessageProcessor));
			AssertProcessorType("AIRCRR", typeof(AIRCRRMessageProcessor));
			AssertProcessorType("PAYREC", typeof(PAYRECMessageProcessor));
			AssertProcessorType("SACR", typeof(SACRMessageProcessor));
			AssertProcessorType("SEACRR", typeof(SEACRRMessageProcessor));
			AssertProcessorType("UBMREQE", typeof(UBMREQEMessageProcessor));
			AssertProcessorType("UBMREQR", typeof(UBMREQRMessageProcessor));
			AssertProcessorType("AIRIARR", typeof(AIRIARRMessageProcessor));
			AssertProcessorType("SEAIARR", typeof(SEAIARRMessageProcessor));
			AssertProcessorType("AIRAARR", typeof(AIRAARRMessageProcessor));
			AssertProcessorType("SEAAARR", typeof(SEAAARRMessageProcessor));
			AssertProcessorType("AIROUTR", typeof(AIROUTRMessageProcessor));
			AssertProcessorType("SEAOUTR", typeof(SEAOUTRMessageProcessor));
			AssertProcessorType("CARST", typeof(CARSTMessageProcessor));
			AssertProcessorType("PAYINV", typeof(PAYINVMessageProcessor));
			AssertProcessorType("CARLSTR", typeof(CARLSTRMessageProcessor));
			AssertProcessorType("SAM", typeof(SAMMessageProcessor));
			AssertProcessorType("DSA", typeof(DSAMessageProcessor));
			AssertProcessorType("IMDR", typeof(IMDRMessageProcessor));
			AssertProcessorType("ATD", typeof(ATDMessageProcessor));
			AssertProcessorType("DOCS", typeof(DOCSMessageProcessor));
			AssertProcessorType("REFACC", typeof(REFACCMessageProcessor));
			AssertProcessorType("REFREJ", typeof(REFREJMessageProcessor));
			AssertProcessorType("PAYEXC", typeof(PAYEXCMessageProcessor));
			AssertProcessorType("DRWBCKR", typeof(DRWBCKRMessageProcessor));
			AssertProcessorType("CLREGR", typeof(CLREGRMessageProcessor));
			AssertProcessorType("CLNTDUP", typeof(CLNTDUPMessageProcessor));
			AssertProcessorType("ERM", typeof(ERMMessageProcessor));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var factory = new BusinessObjectFactory();
			var logger = new LoggingInformation();
			processor = new CMRTestMessageProcessor(logger);
			messageToProcess = factory.New<CMRCARMOVMessage>();
			messageToProcess.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			messageToProcess.EM_Status = EDIMessage.Status.Queued;
		}

		void AssertProcessorType(string documentName, Type expectedType)
		{
			object cUSRESProcessor = processor.GetCUSRESProcessor(documentName);
			Type processorType = cUSRESProcessor == null ? null : cUSRESProcessor.GetType();

			AssertEquals("Processor return for document name: " + documentName, expectedType, processorType);
		}

		readonly string eXRMessageText = "UNH+000129+CUSRES:D:99B:UN'BGM+961:::EXDR+3474 IC7I EA1D:001+11'FTX+AHN+++DOCUMENT ERROR - EMBARGOED VALIDATION??:INFORMATIONA:THE GOODS COVERED BY THE DOCUMENT CANNOT BE DEALT WITH. A COUNTRY CODE SUBJECT TO UN SANCTIONS HAS BEEN DETECTED BY CUSTOMS. PLEASE CONTACT CUSTOMS. THE LODGED OR AMENDED INFORMATION HAS NOT PASSED THE REQUIRED EDITS.'NAD+MR+41065894724::95'RFF+ABO:B00097994::001'RFF+ACW:EXD'RFF+AFM:9'RFF+ED:AAAAC9XT6'ERP+::0001'ERC+XD0091::95'FTX+AAO+++WARNING - PERMIT NUMBER HAS NOT BEEN PROVIDED FOR AHECC AHECC CODE=000000090309000.'CNT+5:0001'CNT+55:01'UNT+14+000129'";
		readonly string cARSTMessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+82C5 9EF4 B65:1+8'DTM+9:20150913032322852510:ZZZ'DTM+132:20150913:102'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'FTX+AHN+++CARGO REPORT SAC:YES'TDT+20+0121++6+KE::3'LOC+12+AUSYD::6'LOC+4+D225A::95'NAD+MR+FGJ476F::95'NAD+UD+48068763333::95'RFF+ABO:A00028942/HST1::1'RFF+MWB:18082339390'RFF+HWB:132343838'DOC+1'PAC+0000001'UNT+17+000001'";
		readonly string sAMMessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SAM+1754 6J61 39F5:1+11'FTX+AHN+++CLEAR:CLEAR'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'" +
			"NAD+IM++MR CHRISTOPHER JOHN REED'RFF+ABO:S00039750/1MESSAGEERRORS/1::1'RFF+ABT:AAAATW49W::1'RFF+ABQ:1'RFF+ADU:S00040559'DOC+1'ERP+::2'ERC+4::95'FTX+ABS+++IFIS (IMPORTED FOOD INSPECTION SCHEME) CLEARANCE REQUIRED'" +
			"DOC+1'ERP+::3'ERC+4::95'FTX+ABS+++IMPORTED FOOD CONTROL ACT 1992. IFIS (IMPORTED FOOD INSPECTION SCHEME) PERMISSION TO DELIVER TO THE IMPORTER REQUIRED'UNT+20+000001'";

		public const string DSAMessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+138G JCIF 0D18:1+11'DTM+9:20080812100800000000:ZZZ'TDT+20++S'" +
			"NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+CARGOWISE EDI PTY LTD'NAD+IM++AUSTRALIAN CUSTOMS SERVICE'RFF+ABO:S00001178/1/DAT1::2'" +
			"RFF+ACW:FID'RFF+AAE:N10'RFF+ABT:AAACG4CNN::2'RFF+ABQ:1TR2562652'RFF+ADU:DSA TESTER'RFF+AMI:FINALISED'DOC+S+1'DTM+192:20080710:102'" +
			"DTM+192:0316:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:HELD'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'CST+1'" +
			"FTX+AHN+++LCL UNDERBOND SATISFIED:NO'CST+1'FTX+AHN+++DECONSOL UNDERBOND SATISFIED:NO'DOC+S+2'DTM+192:20080710:102'DTM+192:0316:401'" +
			"GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:HELD'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'CST+1'FTX+AHN+++LCL UNDERBOND SATISFIED:NO'" +
			"CST+1'FTX+AHN+++DECONSOL UNDERBOND SATISFIED:NO'UNT+40+000001'";

		readonly string aTDMessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::ATD+13B9 DAI3 I355:1+11'DTM+58:20050204:102'DTM+138:20050204:102'FTX+AHN+++FINALISED:FINALISED'TDT+20++6'" +
			"LOC+12+AUSYD::6'EQD+AH+N123::95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LTD'NAD+IM++EAGLE DATAMATION NB'RFF+ABO:S00039750/1MESSAGEERRORS/1::1'RFF+ABT:AAAANNPR6::1'" +
			"RFF+ABQ:SIMPLE MAIL'RFF+AIA:AAAANNPTY'RFF+AAE:N10'DOC+1+1'PAC+150+1'CST+1'TAX+1'MOA+68:0.0000'MOA+40:15.0000'MEA+AAA+::WAR+NO:100.00000'RFF+ABD:96092000'RFF+AED:17'CNT+5:1'CNT+2:1'CNT+3:0'UNT+30+000001'UNZ+1+00000000273283'";

		readonly string dOCSMessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DOCS+338A 7494 19A5:1+11'DTM+184:1545:401'DTM+184:20050322:102'" +
			"FTX+ACD+++TEST 2::IN ACCORDANCE WITH SECTION 71DA OF THE CUSTOMS ACT 1901 YOU ARE REQUIRED TO DELIVER THE FOLLOWING INFORMATION AND COMMERCIAL DOCUMENTS'" +
			"NAD+MR+AAA374M::95'NAD+IM++EAGLE DATAMATION NB'NAD+CB++EAGLE DATAMATION INTERNATIONAL'NAD+CM++SEAN BLACKMORE:ICSBUSSUPP'CTA+IC'COM+02 6229 3571:TE'CTA+IC'COM+02 6229 3571:FX'" +
			"CTA+IC'COM+SEAN.BLACKMORE@CUSTOMS.GOV.AU:EM'RFF+ABO:S00039750/1MESSAGEERRORS/1::1'RFF+ABT:AAAAPALGE::1'RFF+ABQ:EXW FIF CHARGE'UNT+19+000001'UNZ+1+00000000274969'";

		EDIMessage messageToProcess;
		CMRTestMessageProcessor processor;
	}
}
