using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.CusRes.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq.Protected;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	class EmrAndErsMessagingTests : CusResAndDtiResponseProcessorTest
	{
		public void TestParseERS_DeclarationOnly()
		{
			// ERS without MUCR in header but a single DUCR in body - will receive this if arrival is done at DUCR level
			CreateConsolAndDeclarations("UNH+10549944000664+UKCINV:D:00A:UN:109001'BGM+ERS:105:109'LOC+14+LHR:156:109:XCW+120::109'DTM+178:201306061341:203'UNS+D'LOC+14'RFF+ABO:BAR:69P'GEI+ICS+07'GEI+ROE+2'GEI+SOE+9'CNT+11:1'MEA+AAR++KGM:1.000'CST++61071100:122'AUT+CUK98000CAR'UNS+S'UNT+16+10549944000664'");
			RunProcessor();
			consol.Reload();
			AssertEquals(0, consol.Messages.Count);
			receivedMessage.Reload();
			entry1.Reload();
			entry2.Reload();
			jobDeclaration1.Reload();
			jobDeclaration2.Reload();
			AssertEquals("", entry1.CH_RouteOfEntry);
			AssertEquals("2", entry2.CH_RouteOfEntry);
			AssertEquals("07", entry2.CH_ImportClearanceStatusICS);
			AssertEquals("9", entry2.CH_StyleOfEntrySOE);
			AssertEquals("ERS", receivedMessage.EM_MessageType);
			AssertContains(">Declaration BAR/69<", receivedMessage.EM_MessageInterpretation);
			AssertEquals(jobDeclaration2.CustomsEntryHeaders[0], receivedMessage.EM_LinkedObject);
		}

		public void TestParseEMR_Success()
		{
			CreateConsolAndDeclarations(emrMessageText);

			RunProcessor();

			consol.Reload();
			receivedMessage.Reload();
			AssertEquals("RCV", receivedMessage.EM_Status);
			AssertEquals("EMR", receivedMessage.EM_MessageType);
			AssertEquals("000", receivedMessage.EM_MessageSubType);

			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("66", wrapper.MawbExportHelper.ME_ChiefMasterRouteOfEntry);
			AssertEquals("7", wrapper.MawbExportHelper.ME_ChiefMasterStyleOfEntry);
			AssertEquals("movementReferenceNumber123456789012", wrapper.MawbExportHelper.ME_ChiefMovementReference);
			AssertEquals("000", wrapper.MawbExportHelper.ME_ChiefCustomsReturnCode);
			AssertEquals("LHR", wrapper.MawbExportHelper.ME_ChiefGoodsLocation);
			AssertEquals("BAC", wrapper.MawbExportHelper.ME_ChiefShed);
			AssertEquals("epu-no", wrapper.MawbExportHelper.ME_ChiefEntryProcessingUnitNumber);
			AssertEquals("eps-id", wrapper.MawbExportHelper.ME_ChiefEntryProcessingUnitID);
			AssertEquals(new ZDateTime(1987, 12, 11, 1, 2, 0), wrapper.MawbExportHelper.ME_ChiefGoodsArrivalDateTime);

			AssertEquals(consol.PK, receivedMessage.EM_LinkUniqueID);
			entry1.Reload();
			entry2.Reload();
			jobDeclaration1.Reload();
			jobDeclaration2.Reload();
			AssertEquals("60", entry1.CH_ImportClearanceStatusICS);
			AssertEquals("01", entry2.CH_ImportClearanceStatusICS);
			AssertEquals("8", entry1.CH_StyleOfEntrySOE);
			AssertEquals("7", entry2.CH_StyleOfEntrySOE);
			AssertEquals("H", entry1.CH_RouteOfEntry);
			AssertEquals("1", entry2.CH_RouteOfEntry);
			AssertContains("<h4>Declaration BAR/69</h4>", receivedMessage.EM_MessageInterpretation);
			AssertContains("<h4>Declaration FOO</h4>", receivedMessage.EM_MessageInterpretation);
			AssertContains("<tr><td>Master UCR</td><td>A:12387654321</td>", receivedMessage.EM_MessageInterpretation);
		}

		public void TestParseERS_Success()
		{
			CreateConsolAndDeclarations(ersMessageText);

			RunProcessor();

			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("", wrapper.MawbExportHelper.ME_ChiefMasterRouteOfEntry);
			AssertEquals("", wrapper.MawbExportHelper.ME_ChiefMasterStyleOfEntry);
			AssertEquals("movt-ref", wrapper.MawbExportHelper.ME_ChiefMovementReference);
			AssertEquals("", wrapper.MawbExportHelper.ME_ChiefCustomsReturnCode);
			AssertEquals("LHR", wrapper.MawbExportHelper.ME_ChiefGoodsLocation);
			AssertEquals("BAC", wrapper.MawbExportHelper.ME_ChiefShed);
			AssertEquals("epu-no", wrapper.MawbExportHelper.ME_ChiefEntryProcessingUnitNumber);
			AssertEquals("eps-id", wrapper.MawbExportHelper.ME_ChiefEntryProcessingUnitID);
			AssertEquals(new ZDateTime(1987, 12, 11, 1, 2, 0), wrapper.MawbExportHelper.ME_ChiefGoodsArrivalDateTime);

			receivedMessage.Reload();
			AssertEquals("RCV", receivedMessage.EM_Status);
			AssertEquals("ERS", receivedMessage.EM_MessageType);
			AssertEquals(consol.PK, receivedMessage.EM_LinkUniqueID);
			entry1.Reload();
			entry2.Reload();
			jobDeclaration1.Reload();
			jobDeclaration2.Reload();
			AssertEquals("60", entry1.CH_ImportClearanceStatusICS);
			AssertEquals("7", entry1.CH_StyleOfEntrySOE);
			AssertEquals("2", entry1.CH_RouteOfEntry);
			AssertContains("<h4>Declaration FOO</h4>", receivedMessage.EM_MessageInterpretation);
		}

		void CreateConsolAndDeclarations(ZString messageText)
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "12387654321";
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_MasterBillIssueDate = ZDateTime.Now.AddMonths(-1);
			var mockOutboundEdiMessage = Factory.NewMoq<EDIMessage>();
			mockOutboundEdiMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("84");
			receivedMessage = mockOutboundEdiMessage.Object;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbEdifactShared;
			receivedMessage.EM_MessageText = messageText.Replace(System.Environment.NewLine, "");
			receivedMessage.EM_Status = EDIMessage.Status.Sent;
			receivedMessage.EM_Status = "QUE";
			jobDeclaration1 = Factory.New<JobDeclaration>();
			entry1 = jobDeclaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "FOO";
			jobDeclaration2 = Factory.New<JobDeclaration>();
			entry2 = jobDeclaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_BGMReference = "BAR/69";
			Factory.Save();
		}

		ForwardingConsol consol;
		EDIMessage receivedMessage;
		JobDeclaration jobDeclaration1;
		Business.Declaration.CusEntryHeader entry1;
		Business.Declaration.CusEntryHeader entry2;
		JobDeclaration jobDeclaration2;

		const string ersMessageText = @"UNH+sys-mrn+UKCINV:D:00A:UN:109001'
BGM+ERS:105:109'
RFF+UCN:A?:12387654321'
RFF+AES:movt-ref'
LOC+14+LHR:156:109:BAC+epu-no::109:eps-id'
DTM+178:198712110102:203'
UNS+D'
LOC+14'
RFF+ABO:FOO:X'
GEI+ICS+60'
GEI+SOE+7'
GEI+ROE+2'
CNT+11:999'
MEA+AAR++KGM:888'
CST++cmdty-code:122'
AUT+submit-role'
UNS+S'
UNT+18+sys-mrn'";

		const string emrMessageText = @"UNH+sys-mrn+UKCINV:D:00A:UN:109001'
BGM+EMR:105:109'
GEI+CRC+000'
GEI+ROE+66'
GEI+SOE+7'
RFF+UCN:A?:12387654321'
RFF+AES:movementReferenceNumber1234567890123456789012345678901234567890123456789'
CNT+10:2'
LOC+14+LHR:156:109:BAC+epu-no::109:eps-id'
DTM+178:198712110102:203'
UNS+D'
LOC+14'
RFF+ABO:FOO:A'
GEI+ICS+60'
GEI+SOE+8'
GEI+ROE+H'
CNT+11:111'
MEA+AAR++KGM:222'
CST++cmdty-code:122'
AUT+submit-role'
LOC+14'
RFF+ABO:BAR:69A'
GEI+ICS+01'
GEI+SOE+7'
GEI+ROE+1'
CNT+11:333'
MEA+AAR++KGM:444'
CST++cmdty-code:122'
AUT+submit-role'
UNS+S'
UNT+31+sys-mrn'";
	}

	class EaaAndEalMessagingTests : CusResAndDtiResponseProcessorTest
	{
		public void TestParseEAA()
		{
			CreateConsolAndDeclarations(eaaResponseText);

			RunProcessor();

			consol.Reload();
			receivedMessage.Reload();
			sentMessage.Reload();
			AssertEquals("RCV", receivedMessage.EM_Status);
			AssertEquals("EAA", receivedMessage.EM_MessageType);
			AssertEquals("101", receivedMessage.EM_MessageSubType);
			AssertEquals("ACK", sentMessage.EM_Status);
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("101", wrapper.MawbExportHelper.ME_ChiefCustomsReturnCode);
			consol.Messages.Load();
			AssertEquals(2, consol.Messages.Count);
			var sentEmail = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<h4>EAA Response</h4><p>Port-Shed-EPU: LHR-CAX-120</p><p>CRC: 101", sentEmail.Body);
			AssertContains("To open the job, click here", sentEmail.Body);
			AssertContains("<p>Date/time: 27/09/2011 10:29 </p><h4>Repeating Details - 1 of 1</h4><p>UCR (& part): A:27092011001 </p><p>Route:  - </p>", sentEmail.Body);
			AssertContains("EAA response from CHIEF for Consol C00001000 (Master Bill='Poop')", sentEmail.Subject);
			AssertContains("<h4>EAA Response</h4><p>Port-Shed-EPU: LHR-CAX-120</p><p>CRC: 101", receivedMessage.EM_MessageInterpretation);
		}

		public void TestParseEAL()
		{
			CreateConsolAndDeclarations(eaaResponseText.Replace("EAA", "EAL"));

			RunProcessor();

			consol.Reload();
			receivedMessage.Reload();
			sentMessage.Reload();
			AssertEquals("RCV", receivedMessage.EM_Status);
			AssertEquals("EAL", receivedMessage.EM_MessageType);
			AssertEquals("101", receivedMessage.EM_MessageSubType);
			AssertEquals("ACK", sentMessage.EM_Status);
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("101", wrapper.MawbExportHelper.ME_ChiefCustomsReturnCode);
			consol.Messages.Load();
			AssertEquals(2, consol.Messages.Count);
			var sentEmail = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<h4>EAL Response</h4><p>Port-Shed-EPU: LHR-CAX-120</p><p>CRC: 101", sentEmail.Body);
			AssertContains("<p>Date/time: 27/09/2011 10:29 </p><h4>Repeating Details - 1 of 1</h4><p>UCR (& part): A:27092011001 </p><p>Route:  - </p>", sentEmail.Body);
			AssertContains("EAL response from CHIEF for Consol C00001000 (Master Bill='Poop')", sentEmail.Subject);
			AssertContains("<h4>EAL Response</h4><p>Port-Shed-EPU: LHR-CAX-120</p><p>CRC: 101", receivedMessage.EM_MessageInterpretation);
		}

		void CreateConsolAndDeclarations(ZString messageText)
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "Poop";
			consol.JK_RL_NKLoadPort = "GBXXX";

			var anotherStaff = Factory.New<GlbStaff>();
			anotherStaff.GS_EmailAddress = "daniel@wisetechglobal.com";
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Environment.Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			var staffGroup = Factory.Load<GlbGroup>(Enterprise.Customs.GB.Registry.GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup);
			staffGroup.Staff.Add(currentUserInCurrentFactory);
			staffGroup.Staff.Add(anotherStaff);
			Factory.Save();

			var mockSentEdiMessage = Factory.NewMoq<EDIMessageDummyForTest_123>();
			mockSentEdiMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
			sentMessage = mockSentEdiMessage.Object;
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			consol.Messages.Add(sentMessage);
			sentMessage.EM_MessageText = "SENT" + EDIMessage.MessageNumberPlaceHolder;
			var mockReceivedEdiMessage = Factory.NewMoq<EDIMessageDummyForTest_123>();
			mockReceivedEdiMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
			receivedMessage = mockReceivedEdiMessage.Object;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbEdifactShared;
			receivedMessage.EM_Status = EDIMessage.Status.Sent;
			receivedMessage.EM_Status = "QUE";
			receivedMessage.EM_MessageText = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(messageText.Replace(System.Environment.NewLine, ""), sentMessage);

			Factory.Save();
		}

		ForwardingConsol consol;
		EDIMessage receivedMessage;
		EDIMessage sentMessage;

		const string eaaResponseText = @"UNH+10015865418577+UKCINV:D:00A:UN:109001+<<SYSCAR>>'
BGM+EAA:105:109'
GEI+CRC+101'
LOC+14+LHR:156:109:CAX+120::109'
DTM+178:201109271029:203'
UNS+D'
LOC+14'
RFF+ABO:A?:27092011001'
GEI+SOE+0'
UNS+S'
UNT+11+10015865418577'";
	}

	public class EDIMessageDummyForTest_123 : EDIMessage
	{
		public EDIMessageDummyForTest_123(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "123";
		}
	}
}
