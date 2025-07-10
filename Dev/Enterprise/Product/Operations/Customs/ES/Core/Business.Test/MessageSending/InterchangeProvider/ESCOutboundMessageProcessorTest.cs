using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.ES.Business.MessageSending.Testing
{
	class ESCOutboundMessageProcessorTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_GS_NKCusAgent = staff.GS_Code;
				declaration.JE_DeclarationReference = "B00183715";

				var entryHeaderCorrectXML = declaration.CustomsEntryHeaders.AddNew();
				var correctMessageXML = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK,
					applicationCode: ApplicationCodeList.Codes.ESCustomsMessage,
					direction: EDIMessage.Direction.Transmit,
					status: EDIMessage.Status.Queued);
				correctMessageXML.EM_LinkedObject = entryHeaderCorrectXML;

				var entryHeaderCorrectEdifact = declaration.CustomsEntryHeaders.AddNew();
				var correctMessageEdifact = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ArrivalAtExit, TestingData.EdifactMessageText, certificate.CertificateName, certificate.CertificatePK,
					applicationCode: ApplicationCodeList.Codes.ESCustomsMessage,
					direction: EDIMessage.Direction.Transmit,
					status: EDIMessage.Status.Queued);
				correctMessageEdifact.EM_LinkedObject = entryHeaderCorrectEdifact;

				var incorrectApplicationCodeMessage = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ArrivalAtExit, TestingData.EdifactMessageText, certificate.CertificateName, certificate.CertificatePK,
					applicationCode: "AAA",
					direction: EDIMessage.Direction.Transmit,
					status: EDIMessage.Status.Queued);
				var incorrectDirectionMessage = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ArrivalAtExit, TestingData.EdifactMessageText, certificate.CertificateName, certificate.CertificatePK,
					applicationCode: ApplicationCodeList.Codes.ESCustomsMessage,
					direction: EDIMessage.Direction.Receive,
					status: EDIMessage.Status.Queued);
				var incorrectStatusMessage = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ArrivalAtExit, TestingData.EdifactMessageText, certificate.CertificateName, certificate.CertificatePK,
					applicationCode: ApplicationCodeList.Codes.ESCustomsMessage,
					direction: EDIMessage.Direction.Transmit,
					status: EDIMessage.Status.Withdrawn);

				var nullLinkedObjectMessage = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK,
					applicationCode: ApplicationCodeList.Codes.ESCustomsMessage,
					direction: EDIMessage.Direction.Transmit,
					status: EDIMessage.Status.Queued);

				var entryHeaderEmptyApplicationReference = declaration.CustomsEntryHeaders.AddNew();
				var emptyApplicationReferenceMessage = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK,
					applicationCode: ApplicationCodeList.Codes.ESCustomsMessage,
					direction: EDIMessage.Direction.Transmit,
					status: EDIMessage.Status.Queued);
				emptyApplicationReferenceMessage.EM_LinkedObject = entryHeaderEmptyApplicationReference;
				emptyApplicationReferenceMessage.EM_ApplicationReference = ZString.Empty;

				var entryHeaderemptyText = declaration.CustomsEntryHeaders.AddNew();
				var emptyTextMessage = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, ZString.Empty, certificate.CertificateName, certificate.CertificatePK,
					applicationCode: ApplicationCodeList.Codes.ESCustomsMessage,
					direction: EDIMessage.Direction.Transmit,
					status: EDIMessage.Status.Queued);
				emptyTextMessage.EM_LinkedObject = entryHeaderemptyText;

				Factory.Save();

				entryHeaderCorrectXML.CH_BGMReference = "ES000001";
				entryHeaderCorrectEdifact.CH_BGMReference = "ES000002";
				entryHeaderEmptyApplicationReference.CH_BGMReference = "ES000003";
				entryHeaderemptyText.CH_BGMReference = "ES000004";
				Factory.Save();

				CombineAssertions("PRE-CONDITION", () =>
				{
					AssertNull("correctMessageXML Message Interchange", correctMessageXML.Interchange);
					AssertNull("correctMessageEdifact Message Interchange", correctMessageEdifact.Interchange);
					AssertNull("incorrectApplicationCodeMessage Message Interchange", incorrectApplicationCodeMessage.Interchange);
					AssertNull("incorrectDirectionMessage Message Interchange", incorrectDirectionMessage.Interchange);
					AssertNull("incorrectStatusMessage Message Interchange", incorrectStatusMessage.Interchange);
					AssertNull("nullLinkedObjectMessage Message Interchange", nullLinkedObjectMessage.Interchange);
					AssertNull("emptyApplicationReferenceMessage Message Interchange", emptyApplicationReferenceMessage.Interchange);
					AssertNull("emptyTextMessage Message Interchange", emptyTextMessage.Interchange);
				});

				var logger = new LoggingInformation();
				var processor = new ESCOutboundMessageProcessor(logger);
				processor.ProcessMessage(CancellationToken.None);

				correctMessageXML.Reload();
				correctMessageEdifact.Reload();
				incorrectApplicationCodeMessage.Reload();
				incorrectDirectionMessage.Reload();
				incorrectStatusMessage.Reload();
				nullLinkedObjectMessage.Reload();
				emptyApplicationReferenceMessage.Reload();
				emptyTextMessage.Reload();

				CombineAssertions("Status", () =>
				{
					AssertEquals("correctMessageXML Status", EDIMessage.Status.Sent, correctMessageXML.EM_Status);
					AssertEquals("correctMessageEdifact Status", EDIMessage.Status.Sent, correctMessageEdifact.EM_Status);
					AssertEquals("incorrectApplicationCodeMessage Status", EDIMessage.Status.Queued, incorrectApplicationCodeMessage.EM_Status);
					AssertEquals("incorrectDirectionMessage Status", EDIMessage.Status.Queued, incorrectDirectionMessage.EM_Status);
					AssertEquals("incorrectStatusMessage Status", EDIMessage.Status.Withdrawn, incorrectStatusMessage.EM_Status);
					AssertEquals("nullLinkedObjectMessage Status", EDIMessage.Status.Failed, nullLinkedObjectMessage.EM_Status);
					AssertEquals("emptyApplicationReferenceMessage Status", EDIMessage.Status.Failed, emptyApplicationReferenceMessage.EM_Status);
					AssertEquals("emptyTextMessage Status", EDIMessage.Status.Failed, emptyTextMessage.EM_Status);

					AssertNotNull("correctMessageXML Interchange", correctMessageXML.Interchange);
					AssertNotNull("correctMessageEdifact Interchange", correctMessageEdifact.Interchange);
					AssertNull("incorrectApplicationCodeMessage Interchange", incorrectApplicationCodeMessage.Interchange);
					AssertNull("incorrectDirectionMessage Interchange", incorrectDirectionMessage.Interchange);
					AssertNull("incorrectStatusMessage Interchange", incorrectStatusMessage.Interchange);
					AssertNull("nullLinkedObjectMessage Interchange", nullLinkedObjectMessage.Interchange);
					AssertNull("emptyApplicationReferenceMessage Interchange", emptyApplicationReferenceMessage.Interchange);
					AssertNull("emptyTextMessage Interchange", emptyTextMessage.Interchange);

					AssertNotEquals("correctMessageXML.Interchange and correctMessageEdifact.Interchange must be different (no collation)", correctMessageXML.EM_EI, correctMessageEdifact.EM_EI);
				});

				TestingData.AssertOutgoingInterchange("XML", correctMessageXML.Interchange, correctMessageXML.EM_MessageType, SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, correctMessageXML.EM_GP,
					TestingData.GetHeaderTextXML(entryHeaderCorrectXML.CH_BGMReference, correctMessageXML.EM_MessageNum), TestingData.XMLMessageText);
				TestingData.AssertOutgoingInterchange("Edifact", correctMessageEdifact.Interchange, correctMessageEdifact.EM_MessageType, SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, correctMessageEdifact.EM_GP,
					TestingData.GetHeaderTextEdifactAndDOC(entryHeaderCorrectEdifact.CH_BGMReference, correctMessageEdifact.EM_MessageNum), TestingData.EdifactInterchangeText);
			}
		}

		public void TestCreateInterchangeForDirectxT()
		{
			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_GS_NKCusAgent = staff.GS_Code;
				declaration.JE_DeclarationReference = "B00183715";

				var entryHeaderCorrectXML = declaration.CustomsEntryHeaders.AddNew();
				var correctMessageXML = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);
				correctMessageXML.EM_LinkedObject = entryHeaderCorrectXML;

				var entryHeaderCorrectEdifact = declaration.CustomsEntryHeaders.AddNew();
				var correctMessageEdifact = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ArrivalAtExit, TestingData.EdifactMessageText, certificate.CertificateName, certificate.CertificatePK);
				correctMessageEdifact.EM_LinkedObject = entryHeaderCorrectEdifact;
				Factory.Save();

				entryHeaderCorrectXML.CH_BGMReference = "ES000001";
				entryHeaderCorrectEdifact.CH_BGMReference = "ES000002";
				Factory.Save();

				var logger = new LoggingInformation();
				var processor = new ESCOutboundMessageProcessor(logger);
				processor.ProcessMessage(CancellationToken.None);

				correctMessageXML.Reload();
				correctMessageEdifact.Reload();

				TestingData.AssertOutgoingInterchange("XML", correctMessageXML.Interchange,
				 expectedMessageType: DeclarationMessageTypeList.Codes.ImportQuery,
				 expectedDestination: SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt,
				 expectedTransportType: EDIInterchange.TransportType.xT,
				 expectedStatus: EDIInterchange.Status.Queued,
				 expectedMessageGP: correctMessageXML.EM_GP,
				 expectedHeader: ZString.Empty,
				 expectedBody: TestingData.XMLMessageText);

				TestingData.AssertOutgoingInterchange("EDIFACT", correctMessageEdifact.Interchange,
				 expectedMessageType: DeclarationMessageTypeList.Codes.ArrivalAtExit,
				 expectedDestination: SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms,
				 expectedTransportType: EDIInterchange.TransportType.xT,
				 expectedStatus: EDIInterchange.Status.Queued,
				 expectedMessageGP: correctMessageEdifact.EM_GP,
				 expectedHeader: ZString.Empty,
				 expectedBody: TestingData.EdifactInterchangeText);
			}
		}

		public void TestCreateInterchangeForEHub()
		{
			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_GS_NKCusAgent = staff.GS_Code;
				declaration.JE_DeclarationReference = "B00183715";

				var entryHeaderCorrectXML = declaration.CustomsEntryHeaders.AddNew();
				var correctMessageXML = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);
				correctMessageXML.EM_LinkedObject = entryHeaderCorrectXML;

				var entryHeaderCorrectEdifact = declaration.CustomsEntryHeaders.AddNew();
				var correctMessageEdifact = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ArrivalAtExit, TestingData.EdifactMessageText, certificate.CertificateName, certificate.CertificatePK);
				correctMessageEdifact.EM_LinkedObject = entryHeaderCorrectEdifact;
				Factory.Save();

				entryHeaderCorrectXML.CH_BGMReference = "ES000001";
				entryHeaderCorrectEdifact.CH_BGMReference = "ES000002";
				Factory.Save();

				var logger = new LoggingInformation();
				var processor = new ESCOutboundMessageProcessor(logger);
				processor.ProcessMessage(CancellationToken.None);

				correctMessageXML.Reload();
				correctMessageEdifact.Reload();

				TestingData.AssertOutgoingInterchange("XML", correctMessageXML.Interchange,
				 expectedMessageType: DeclarationMessageTypeList.Codes.ImportQuery,
				 expectedDestination: SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub,
				 expectedTransportType: EDIInterchange.TransportType.eHub,
				 expectedStatus: EDIInterchange.Status.eHubQueued,
				 expectedMessageGP: correctMessageXML.EM_GP,
				 expectedHeader: TestingData.GetHeaderTextXML(entryHeaderCorrectXML.CH_BGMReference, correctMessageXML.EM_MessageNum),
				 expectedBody: TestingData.XMLMessageText);

				TestingData.AssertOutgoingInterchange("EDIFACT", correctMessageEdifact.Interchange,
				 expectedMessageType: DeclarationMessageTypeList.Codes.ArrivalAtExit,
				 expectedDestination: SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms,
				 expectedTransportType: EDIInterchange.TransportType.eHub,
				 expectedStatus: EDIInterchange.Status.eHubQueued,
				 expectedMessageGP: correctMessageEdifact.EM_GP,
				 expectedHeader: TestingData.GetHeaderTextEdifactAndDOC(entryHeaderCorrectEdifact.CH_BGMReference, correctMessageEdifact.EM_MessageNum),
				 expectedBody: TestingData.EdifactInterchangeText);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			staff = staffWithCertificateHelperTest.Staff;
			certificate = staffWithCertificateHelperTest.Certificate;
		}

		CertificateProviderTestClass certificate;
		GlbStaff staff;
	}
}
