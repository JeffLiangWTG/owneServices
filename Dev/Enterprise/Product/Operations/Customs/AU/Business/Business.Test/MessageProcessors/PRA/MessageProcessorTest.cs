using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessRejectedMessage()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000000002";
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "POCU2819799";
			Factory.Save();

			AssertEquals("Status Email Count", 0, messageProcessor.StatusEmailSendCount);

			const string expectedRejectedMessageBody = "A 'Message Rejected' response has been received from the PRA";
			EDIMessage rejectedMessage = Factory.New<EDIMessage>();
			rejectedMessage.EM_MessageText = MessagePRARejected.Replace("\r", "").Replace("\n", "");
			rejectedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			rejectedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rejectedMessage.EM_LinkUniqueID = container.PK;

			messageProcessor.ProcessMessage(rejectedMessage);

			AssertEquals("Status Email Count", 1, messageProcessor.StatusEmailSendCount);
			AssertEquals("Received", EDIMessage.Status.Received, rejectedMessage.EM_Status);
			AssertEquals("Message Sub Type", PRAConstants.MessageRejected, rejectedMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", container.PK, rejectedMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedRejectedMessageBody, rejectedMessage.EM_MessageInterpretation);
			AssertEquals("Result from ProcessMessage", "RCV", rejectedMessage.EM_Status);

			StmALog dataRejectLog = FindMostRecentDataLog(Events.MessageRejected, container.PK);
			AssertNotNull(dataRejectLog);
			AssertEquals("Event parameter", "|DEP=1-stop|MST=PRA", dataRejectLog.SL_Reference);
		}

		StmALog FindMostRecentDataLog(Event eventName, ZGuid parentPK)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, parentPK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventName.Code);
			query.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + " DESC";
			var result = Factory.LoadTop1<StmALog>(query);

			return result;
		}

		public void TestProcessCancellationAcceptedMessage()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000000002";
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "POCU2819799";

			Factory.Save();

			AssertEquals("Precondition: LastPRAMessageSentWasCancellation", false, container.LastPRAMessageSentWasCancellation);

			EDIMessage message = container.PRAMessages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageSubType = "SCN"; // Cancel Message
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);
			Factory.Save();

			AssertEquals("LastPRAMessageSentWasCancellation", true, container.LastPRAMessageSentWasCancellation);

			const string expectedAcceptMessageBody = "A 'Message Accepted' response has been received from the PRA";
			EDIMessage message1 = Factory.New<EDIMessage>();
			message1.EM_MessageText = MessagePRAAccepted.Replace("\r", "").Replace("\n", "");
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_LinkUniqueID = container.PK;
			messageProcessor.ProcessMessage(message1);

			AssertEquals("Received", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("Message Sub Type", PRAConstants.MessageAcknowledged, message1.EM_MessageSubType);
			AssertEquals("Linked to entry", container.PK, message1.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedAcceptMessageBody, message1.EM_MessageInterpretation);

			var cancellationAcceptedLog = FindMostRecentDataLog(Events.MessageWithdrawCancelAccepted, container.PK);
			AssertNotNull(cancellationAcceptedLog);
			AssertEquals("Event parameter", "|DEP=1-stop|MST=PRA", cancellationAcceptedLog.SL_Reference);
		}

		public void TestProcessAcceptedMessage()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000000002";
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "POCU2819799";
			Factory.Save();

			AssertEquals("Acknowledgement Email Count", 0, messageProcessor.AcknowledgementEmailSendCount);

			const string expectedAcceptMessageBody = "A 'Message Accepted' response has been received from the PRA";
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = MessagePRAAccepted.Replace("\r", "").Replace("\n", "");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_LinkUniqueID = container.PK;

			messageProcessor.ProcessMessage(message);

			AssertEquals("Acknowledgement Email Count", 1, messageProcessor.AcknowledgementEmailSendCount);
			AssertEquals("Received", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("Message Sub Type", PRAConstants.MessageAcknowledged, message.EM_MessageSubType);
			AssertEquals("Linked to entry", container.PK, message.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedAcceptMessageBody, message.EM_MessageInterpretation);

			var dataAcceptedLog = FindMostRecentDataLog(Events.MessageAccepted, container.PK);
			AssertNotNull(dataAcceptedLog);
			AssertEquals("Event parameter", "|DEP=1-stop|MST=PRA", dataAcceptedLog.SL_Reference);
		}

		public const string MessagePRAAccepted = @"UNH+10151+APERAK:D:00A:UN:ANZ23'
BGM+7+10151+9+AP'
DTM+137:20150326110617:204'
DOC+ERA+EDISYD'
DTM+137:20040715105226:204'
RFF+ERN:CON-C000000002-POCU2819799'
NAD+MS+1STOP'
NAD+MR+EDISRW'
ERC+ERA0100'
FTX+AAO+++A 'Message Accepted' response has been received from the PRA'
RFF+EQD:POCU2819798'
UNT+12+10151'
";

		public const string MessagePRARejected = @"UNH+10151+APERAK:D:00A:UN:ANZ23'
BGM+7+10151+9+AP'
DTM+137:20150326110617:204'
DOC+ERA+EDISYD'
DTM+137:20040715105226:204'
RFF+ERN:CON-C000000002-POCU2819799'
NAD+MS+1STOP'
NAD+MR+EDISRW'
ERC+ERA0104'
FTX+AAO+++A 'Message Rejected' response has been received from the PRA'
RFF+EQD:POCU2819798'
UNT+12+10151'
";

		public void TestGetUserToNotify_OnlyChecksPRAMessages()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001095";
			var container = consol.Containers.AddNew();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";

			var message = Factory.New<PRAMessage>();
			message.EM_MessageText = "Message text";
			message.EM_ApplicationCode = "XXX";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_LinkedObject = container;
			message.EM_SystemCreateUser = "ABC";

			Factory.Save();

			var processor = new DummyMessageProcessor(container);
			var userToNotify = processor.GetUserToNotify(container);
			AssertEquals(null, userToNotify);

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;

			Factory.Save();

			userToNotify = processor.GetUserToNotify(container);
			AssertEquals(staff.PK, userToNotify.PK);
		}

		public void TestProcessResponseCancellationNoErrorFreight()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001095";
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "PONU7537954";
			Factory.Save();

			AssertEquals("Acknowledgement Email Count", 0, messageProcessor.AcknowledgementEmailSendCount);

			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = NoErrorCancellationAcceptance.Replace("\r", "").Replace("\n", "");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);

			AssertEquals("Acknowledgement Email Count", 1, messageProcessor.AcknowledgementEmailSendCount);
			AssertEquals(PRAConstants.MessageAcknowledged, message.EM_MessageSubType);
		}

		public const string NoErrorCancellationAcceptance = @"UNH+4129507+APERAK:D:00A:UN:ANZ23'
BGM+7+4129330+9+AP'
DTM+137:20050705134201:204'
DOC+ERA+AGSSYD'
DTM+137:20050705133300:204'
RFF+ERN:CON-C00001095-PONU7537954'
NAD+MS+CONFI'
NAD+MR+AGSSYD'
ERC+ERA0101'
FTX+AAO+++CONTAINER CANCELLED AS REQUESTED'
RFF+EQD:PONU7537954'
UNT+12+4129507'
";

		public void TestProcessResponseNoErrorFreight()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000000001";
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "POCU2819798";
			Factory.Save();

			AssertEquals("Acknowledgement Email Count", 0, messageProcessor.AcknowledgementEmailSendCount);
			ProcessMessageAndCompareAgainstExpectedResult(NoErrorEDIFACT, NoErrorUserMessage, true);
			AssertEquals("Acknowledgement Email Count", 1, messageProcessor.AcknowledgementEmailSendCount);
		}

		public const string NoErrorEDIFACT = @"UNH+10151+APERAK:D:00A:UN:ANZ23'
BGM+7+10151+9+AP'
DTM+137:20040715110617:204'
DOC+ERA+EDISYD'
DTM+137:20040715105226:204'
RFF+ERN:CON-C000000001-POCU2819798'
NAD+MS+1STOP'
NAD+MR+EDISRW'
ERC+ERA0100'
FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'
RFF+EQD:POCU2819798'
UNT+12+10151'
";

		public const string NoErrorUserMessage = @"-------------- Response Status: PRA Submission Accepted --------------
Consol No         : C000000001
Container No      : POCU2819798
Response From     : 1STOP - 1-Stop Message Processing Centre

Response Status   - ERA0100 - Message received without error
Response Message  - MESSAGE RECEIVED WITHOUT ERROR
----------------------------------------------------------------------

------------------------- Container Details --------------------------
Container No      : POCU2819798
ISO Container Type: 
Container Seal No : 
Weight            : 0 Kg
Commodity         : -
-------------------------- Shipping Details --------------------------
Vessel            : 
Voyage            : 
Departure Date    : 
Port of Discharge :  - 
Receival Wharf    :  - 
------------------------- Clearance Numbers --------------------------
CAN               : 
----------------------------------------------------------------------
";

		public void TestProcessResponseNoErrorCustoms()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DeclarationReference = "B000000001";
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "POCU2819798";
			Factory.Save();

			AssertEquals("Acknowledgement Email Count", 0, messageProcessor.AcknowledgementEmailSendCount);
			ProcessMessageAndCompareAgainstExpectedResult(NoErrorEDIFACTCustoms, NoErrorUserMessageCustoms, true);
			AssertEquals("Acknowledgement Email Count", 1, messageProcessor.AcknowledgementEmailSendCount);
		}

		public const string NoErrorEDIFACTCustoms = @"UNH+10151+APERAK:D:00A:UN:ANZ23'
BGM+7+10151+9+AP'
DTM+137:20040715110617:204'
DOC+ERA+EDISYD'
DTM+137:20040715105226:204'
RFF+ERN:CUS-B000000001-POCU2819798'
NAD+MS+1STOP'
NAD+MR+EDISRW'
ERC+ERA0100'
FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'
RFF+EQD:POCU2819798'
UNT+12+10151'
";

		public const string NoErrorUserMessageCustoms = @"-------------- Response Status: PRA Submission Accepted --------------
Declaration No    : B000000001
Container No      : POCU2819798
Response From     : 1STOP - 1-Stop Message Processing Centre

Response Status   - ERA0100 - Message received without error
Response Message  - MESSAGE RECEIVED WITHOUT ERROR
----------------------------------------------------------------------

------------------------- Container Details --------------------------
Container No      : POCU2819798
ISO Container Type: 
Container Seal No : 
Weight            : 0 Kg
Commodity         : -
-------------------------- Shipping Details --------------------------
Vessel            : 
Voyage            : 
Departure Date    : 
Port of Discharge :  - 
Receival Wharf    :  - 
------------------------- Clearance Numbers --------------------------
CAN               : 
----------------------------------------------------------------------
";

		public void TestProcessResponseNoErrorOneWarningFreight()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000000001";
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "POCU2819798";
			Factory.Save();

			AssertEquals("Acknowledgement Email Count", 0, messageProcessor.AcknowledgementEmailSendCount);
			ProcessMessageAndCompareAgainstExpectedResult(NoErrorOneWarningEDIFACT, NoErrorOneWarningUserMessage, true);
			AssertEquals("Acknowledgement Email Count", 1, messageProcessor.AcknowledgementEmailSendCount);
		}

		public const string NoErrorOneWarningEDIFACT = @"UNH+10152+APERAK:D:00A:UN:ANZ23'
BGM+7+166799+9+AP'
DTM+137:20040510093709:204'
DOC+ERA+EDISYD'
DTM+137:20040510093316:204'
RFF+ERN:CON-C000000001-POCU2819798'
NAD+MS+ASES1'
ERC+ERA0100'
FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'
RFF+EQD:TORU9108833'
FTX+AAI+++WARNING VOYAGE  0410  LLOYDS  8300145  INVALID  ASSUME  410'
UNT+11+10152'";

		public const string NoErrorOneWarningUserMessage = @"-------------- Response Status: PRA Submission Accepted --------------
Consol No         : C000000001
Container No      : POCU2819798
Response From     : ASES1 - Patrick Melbourne - East Swanson

Response Status   - ERA0100 - Message received without error
Response Message  - MESSAGE RECEIVED WITHOUT ERROR

Additional Info   - WARNING VOYAGE  0410  LLOYDS  8300145  INVALID  ASSUME  410
----------------------------------------------------------------------

------------------------- Container Details --------------------------
Container No      : POCU2819798
ISO Container Type: 
Container Seal No : 
Weight            : 0 Kg
Commodity         : -
-------------------------- Shipping Details --------------------------
Vessel            : 
Voyage            : 
Departure Date    : 
Port of Discharge :  - 
Receival Wharf    :  - 
------------------------- Clearance Numbers --------------------------
CAN               : 
----------------------------------------------------------------------
";

		public void TestProcessResponseNoErrorOneWarningCustoms()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DeclarationReference = "B000000001";
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "POCU2819798";
			Factory.Save();

			AssertEquals("Acknowledgement Email Count", 0, messageProcessor.AcknowledgementEmailSendCount);
			ProcessMessageAndCompareAgainstExpectedResult(NoErrorOneWarningEDIFACTCustoms, NoErrorOneWarningUserMessageCustoms, true);
			AssertEquals("Acknowledgement Email Count", 1, messageProcessor.AcknowledgementEmailSendCount);
		}

		public const string NoErrorOneWarningEDIFACTCustoms = @"UNH+10152+APERAK:D:00A:UN:ANZ23'
BGM+7+166799+9+AP'
DTM+137:20040510093709:204'
DOC+ERA+EDISYD'
DTM+137:20040510093316:204'
RFF+ERN:CUS-B000000001-POCU2819798'
NAD+MS+ASES1'
ERC+ERA0100'
FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'
RFF+EQD:TORU9108833'
FTX+AAI+++WARNING VOYAGE  0410  LLOYDS  8300145  INVALID  ASSUME  410'
UNT+11+10152'";

		public const string NoErrorOneWarningUserMessageCustoms = @"-------------- Response Status: PRA Submission Accepted --------------
Declaration No    : B000000001
Container No      : POCU2819798
Response From     : ASES1 - Patrick Melbourne - East Swanson

Response Status   - ERA0100 - Message received without error
Response Message  - MESSAGE RECEIVED WITHOUT ERROR

Additional Info   - WARNING VOYAGE  0410  LLOYDS  8300145  INVALID  ASSUME  410
----------------------------------------------------------------------

------------------------- Container Details --------------------------
Container No      : POCU2819798
ISO Container Type: 
Container Seal No : 
Weight            : 0 Kg
Commodity         : -
-------------------------- Shipping Details --------------------------
Vessel            : 
Voyage            : 
Departure Date    : 
Port of Discharge :  - 
Receival Wharf    :  - 
------------------------- Clearance Numbers --------------------------
CAN               : 
----------------------------------------------------------------------
";

		public void TestProcessResponseWithContainerNotFound()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000000001";
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "POCU1234567";
			Factory.Save();

			AssertEquals("Error Email Count", 0, messageProcessor.ErrorEmailSendCount);
			ProcessMessageAndCompareAgainstExpectedResult(NoErrorOneWarningEDIFACT, ContainerNotFound, false);
			AssertEquals("Error Email Count", 1, messageProcessor.ErrorEmailSendCount);
		}

		public const string ContainerNotFound = @"FATAL PROCESSING ERROR: Could Not Create Container.

------------------- Response Status: PRA Accepted --------------------
Consol No         : Unknown
Container No      : Unknown
Response From     : 
----------------------------------------------------------------------

UNH+10152+APERAK:D:00A:UN:ANZ23'
BGM+7+166799+9+AP'
DTM+137:20040510093709:204'
DOC+ERA+EDISYD'
DTM+137:20040510093316:204'
RFF+ERN:CON-C000000001-POCU2819798'
NAD+MS+ASES1'
ERC+ERA0100'
FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'
RFF+EQD:TORU9108833'
FTX+AAI+++WARNING VOYAGE  0410  LLOYDS  8300145  INVALID  ASSUME  410'
UNT+11+10152'";

		public void TestProcessMissingGroup1()
		{
			ProcessMessageAndCompareAgainstExpectedResult(MissingGroup1EDIFACT, MissingGroup1UserMessage, false);
		}

		public const string MissingGroup1EDIFACT = @"UNH+10151+APERAK:D:00A:UN:ANZ23'
BGM+7+10151+9+AP'
DTM+137:20040715110617:204'
UNT+4+10151'
";

		public const string MissingGroup1UserMessage = @"FATAL PROCESSING ERROR: Could Not Create Container.

------------------- Response Status: PRA Rejected --------------------
Consol No         : Unknown
Container No      : Unknown
Response From     : 
----------------------------------------------------------------------

UNH+10151+APERAK:D:00A:UN:ANZ23'
BGM+7+10151+9+AP'
DTM+137:20040715110617:204'
DOC'
DTM'
UNT+4+10151'";

		public void TestPRAMessageProcessFindingDeclarationWithMessageReference()
		{
			var anotherCompany = Factory.New<GlbCompany>();
			anotherCompany.GC_Code = "CO1";
			anotherCompany.GC_Name = "Another Company";
			anotherCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			var anotherCompanyBranch = anotherCompany.Branches.AddNew();
			anotherCompanyBranch.GB_Code = "BR1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DeclarationReference = "B000000001";
			declaration.JE_GC = anotherCompany.PK;
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "POCU2819798";
			Factory.Save();

			ProcessMessageAndCompareAgainstExpectedResult(PRACustomsResponseMessageText, ExpectedUserPRAMessage, true);
		}

		public void TestPRAMessageProcessCouldNotFindingDeclarationWithMismatchMessageReference()
		{
			var anotherCompany = Factory.New<GlbCompany>();
			anotherCompany.GC_Code = "CO1";
			anotherCompany.GC_Name = "Another Company";
			anotherCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			var anotherCompanyBranch = anotherCompany.Branches.AddNew();
			anotherCompanyBranch.GB_Code = "BR1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DeclarationReference = "B000000001";
			declaration.JE_GC = anotherCompany.PK;
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "POCU28199999";
			Factory.Save();

			ProcessMessageAndCompareAgainstExpectedResult(PRACustomsResponseMessageText, CouldNotIndentifyConatainer, false);
		}

		const string PRACustomsResponseMessageText = @"UNH+10151+APERAK:D:00A:UN:ANZ23'
BGM+7+10151+9+AP'
DTM+137:20040715110617:204'
DOC+ERA+EDISYD'
DTM+137:20040715105226:204'
RFF+ERN:CUS-B000000001-POCU2819798'
NAD+MS+1STOP'
NAD+MR+EDISRW'
ERC+ERA0100'
FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'
RFF+EQD:POCU2819798'
FTX+AAI+++WARNING VOYAGE  0410  LLOYDS  8300145  INVALID  ASSUME  410'
UNT+12+10151'";

		const string ExpectedUserPRAMessage = @"-------------- Response Status: PRA Submission Accepted --------------
Declaration No    : B000000001
Container No      : POCU2819798
Response From     : 1STOP - 1-Stop Message Processing Centre

Response Status   - ERA0100 - Message received without error
Response Message  - MESSAGE RECEIVED WITHOUT ERROR

Additional Info   - WARNING VOYAGE  0410  LLOYDS  8300145  INVALID  ASSUME  410
----------------------------------------------------------------------

------------------------- Container Details --------------------------
Container No      : POCU2819798
ISO Container Type: 
Container Seal No : 
Weight            : 0 Kg
Commodity         : -
-------------------------- Shipping Details --------------------------
Vessel            : 
Voyage            : 
Departure Date    : 
Port of Discharge :  - 
Receival Wharf    :  - 
------------------------- Clearance Numbers --------------------------
CAN               : 
----------------------------------------------------------------------
";
		const string CouldNotIndentifyConatainer = @"FATAL PROCESSING ERROR: Could Not Create Container.

------------------- Response Status: PRA Accepted --------------------
Consol No         : Unknown
Container No      : Unknown
Response From     : 
----------------------------------------------------------------------

UNH+10151+APERAK:D:00A:UN:ANZ23'
BGM+7+10151+9+AP'
DTM+137:20040715110617:204'
DOC+ERA+EDISYD'
DTM+137:20040715105226:204'
RFF+ERN:CUS-B000000001-POCU2819798'
NAD+MS+1STOP'
NAD+MR+EDISRW'
ERC+ERA0100'
FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'
RFF+EQD:POCU2819798'
FTX+AAI+++WARNING VOYAGE  0410  LLOYDS  8300145  INVALID  ASSUME  410'
UNT+12+10151'";
		#region Implementation

		MessageProcessor messageProcessor;

		protected override void SetUp()
		{
			base.SetUp();
			messageProcessor = new MessageProcessor(new LoggingInformation());
		}

		void ProcessMessageAndCompareAgainstExpectedResult(string messageText, string expectedUserMessage, bool expectedResult)
		{
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			messageProcessor.ProcessMessage(message);
			string actualUserMessage = messageProcessor.responseBody;
			AssertMultilineASCIIEquals("Returned Human Readable Message Generated from EDIFACT Message", expectedUserMessage, actualUserMessage);

			AssertEquals("Result from ProcessMessage", expectedResult ? "RCV" : "ERR", message.EM_Status);
		}

		class DummyMessageProcessor : MessageProcessor
		{
			public DummyMessageProcessor(CommonContainer container)
				: base(new LoggingInformation())
			{
			}

			public new GlbStaff GetUserToNotify(IBusiness parent)
			{
				return base.GetUserToNotify(parent);
			}

			public new ZGuid AcknowledgementEmailGroup
			{
				get { return base.AcknowledgementEmailGroup; }
			}

			public new ZString AcknowledgementEmailMode
			{
				get { return base.AcknowledgementEmailMode; }
			}

			public new ZGuid ImpedimentEmailGroup
			{
				get { return base.ImpedimentEmailGroup; }
			}

			public new ZString ImpedimentEmailMode
			{
				get { return base.ImpedimentEmailMode; }
			}

			public new ZGuid ErrorEmailGroup
			{
				get { return base.ErrorEmailGroup; }
			}

			public new ZString ErrorEmailMode
			{
				get { return base.ErrorEmailMode; }
			}
		}

		#endregion
	}
}
