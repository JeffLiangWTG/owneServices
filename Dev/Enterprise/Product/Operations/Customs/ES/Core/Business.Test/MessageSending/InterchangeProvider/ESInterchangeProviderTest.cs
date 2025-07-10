using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIInterchanges;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Moq;
using MessageTypeCodes = Enterprise.Customs.ES.Messaging.DeclarationMessageTypeList.Codes;

namespace Enterprise.Customs.ES.Business.MessageSending.Testing
{
	class ESInterchangeProviderTest : InterchangeProviderTestCase
	{
		public void TestInterchangeType()
		{
			var message = Factory.CreateOutboundMessage(MessageTypeCodes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);
			Factory.Save();
			var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
			ediMessageCollection.Add(message);

			var interchangeProvider = SetMockProvider("ES000001", ediMessageCollection);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			AssertType(typeof(ESEDIInterchange), interchangeProvider.Interchanges.Single());
		}

		ESInterchangeProvider SetMockProvider(ZString entryRef, NonDependentEDIMessageCollection ediMessageCollection, string entryMRN = "")
		{
			var mockProvider = new Mock<IESMessageInfoProvider>();
			mockProvider.CallBase = true;
			mockProvider.Setup(m => m.Broker).Returns(staff);
			mockProvider.Setup(m => m.EntryReference).Returns(entryRef);
			mockProvider.Setup(m => m.MRN).Returns(entryMRN);

			var logger = new LoggingInformation();
			var interchangeProviderMock = new Mock<ESInterchangeProvider>(new object[] { ediMessageCollection, logger });
			interchangeProviderMock.CallBase = true;
			foreach (EDIMessage message in ediMessageCollection)
			{
				interchangeProviderMock.Setup(c => c.GetMessageProvider(message)).Returns(mockProvider.Object);
			}
			return interchangeProviderMock.Object;
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(false))
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			{
				var expectedEntryReference = "ES000001";

				var correctMessageXML = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageEDI = Factory.CreateOutboundMessage(MessageTypeCodes.ArrivalAtExit, TestingData.EdifactMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageNPE = Factory.CreateOutboundMessage(MessageTypeCodes.InBoxNotificationForExport, TestingData.NPEMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageDOC = Factory.CreateOutboundMessage(MessageTypeCodes.EsDocumentRequest, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);

				Factory.Save();

				CombineAssertions("PRE-CONDITION", () =>
				{
					AssertNull("correctMessageXML Message Interchange", correctMessageXML.Interchange);
				});

				var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
				ediMessageCollection.Add(correctMessageXML);
				ediMessageCollection.Add(correctMessageEDI);
				ediMessageCollection.Add(correctMessageNPE);
				ediMessageCollection.Add(correctMessageDOC);

				var interchangeProvider = SetMockProvider(expectedEntryReference, ediMessageCollection);
				interchangeProvider.PackCollatedMessagesIntoInterchanges();
				var interchanges = interchangeProvider.Interchanges;

				Factory.Save();

				correctMessageXML.Reload();
				correctMessageEDI.Reload();
				correctMessageNPE.Reload();
				correctMessageDOC.Reload();

				CombineAssertions(() =>
				{
					AssertEquals("4 interchanges created", 4, interchanges.Length);
					AssertEquals("correctMessageXML Status", EDIMessage.Status.Sent, correctMessageXML.EM_Status);
					AssertNotNull("correctMessageXML Interchange", correctMessageXML.Interchange);
					AssertEquals("correctMessageEDI Status", EDIMessage.Status.Sent, correctMessageEDI.EM_Status);
					AssertNotNull("correctMessageEDI Interchange", correctMessageEDI.Interchange);
					AssertEquals("correctMessageNPE Status", EDIMessage.Status.Sent, correctMessageNPE.EM_Status);
					AssertNotNull("correctMessageNPE Interchange", correctMessageNPE.Interchange);
					AssertEquals("correctMessageDOC Status", EDIMessage.Status.Sent, correctMessageDOC.EM_Status);
					AssertNotNull("correctMessageDOC Interchange", correctMessageDOC.Interchange);
				});

				var headerTextXML = TestingData.GetHeaderTextXML(expectedEntryReference, correctMessageXML.EM_MessageNum);
				var headerTextEDI = TestingData.GetHeaderTextEdifactAndDOC(expectedEntryReference, correctMessageEDI.EM_MessageNum);
				var npeHeaderText = TestingData.GetHeaderTextNPEForEHub(expectedEntryReference, correctMessageNPE.EM_MessageNum);
				var headerTextDOC = TestingData.GetHeaderTextEdifactAndDOC(expectedEntryReference, correctMessageDOC.EM_MessageNum);
				TestingData.AssertOutgoingInterchange("XML-EHUB", correctMessageXML.Interchange, MessageTypeCodes.ImportQuery, SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, correctMessageXML.EM_GP, headerTextXML, TestingData.XMLMessageText);
				TestingData.AssertOutgoingInterchange("EDI-EHUB", correctMessageEDI.Interchange, MessageTypeCodes.ArrivalAtExit, SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, correctMessageEDI.EM_GP, headerTextEDI, TestingData.EdifactInterchangeText);
				TestingData.AssertOutgoingInterchange("NPE-EHUB", correctMessageNPE.Interchange, MessageTypeCodes.InBoxNotificationForExport, SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, correctMessageNPE.EM_GP, npeHeaderText, TestingData.NPEMessageText);
				TestingData.AssertOutgoingInterchange("DOC-EHUB", correctMessageDOC.Interchange, MessageTypeCodes.EsDocumentRequest, SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, correctMessageXML.EM_GP, headerTextDOC, TestingData.XMLMessageText);
			}
		}

		public void TestMessagesPopulateNewInterchange_EHub_ExceptInbox()
		{
			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(false))
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var expectedEntryReference = "ES000001";
				var expectedEntryMRN = "20ES00999830001277";

				var correctMessageXML = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageEDI = Factory.CreateOutboundMessage(MessageTypeCodes.ArrivalAtExit, TestingData.EdifactMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageNPE = Factory.CreateOutboundMessage(MessageTypeCodes.InBoxNotificationForExport, TestingData.NPEMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageDOC = Factory.CreateOutboundMessage(MessageTypeCodes.EsDocumentRequest, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);

				Factory.Save();

				CombineAssertions("PRE-CONDITION", () =>
				{
					AssertNull("correctMessageXML Message Interchange", correctMessageXML.Interchange);
				});

				var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
				ediMessageCollection.Add(correctMessageXML);
				ediMessageCollection.Add(correctMessageEDI);
				ediMessageCollection.Add(correctMessageNPE);
				ediMessageCollection.Add(correctMessageDOC);

				var interchangeProvider = SetMockProvider(expectedEntryReference, ediMessageCollection, expectedEntryMRN);
				interchangeProvider.PackCollatedMessagesIntoInterchanges();
				var interchanges = interchangeProvider.Interchanges;

				Factory.Save();

				correctMessageXML.Reload();
				correctMessageEDI.Reload();
				correctMessageNPE.Reload();
				correctMessageDOC.Reload();

				CombineAssertions(() =>
				{
					AssertEquals("4 interchanges created", 4, interchanges.Length);
					AssertEquals("correctMessageXML Status", EDIMessage.Status.Sent, correctMessageXML.EM_Status);
					AssertNotNull("correctMessageXML Interchange", correctMessageXML.Interchange);
					AssertEquals("correctMessageEDI Status", EDIMessage.Status.Sent, correctMessageEDI.EM_Status);
					AssertNotNull("correctMessageEDI Interchange", correctMessageEDI.Interchange);
					AssertEquals("correctMessageNPE Status", EDIMessage.Status.Sent, correctMessageNPE.EM_Status);
					AssertNotNull("correctMessageNPE Interchange", correctMessageNPE.Interchange);
					AssertEquals("correctMessageDOC Status", EDIMessage.Status.Sent, correctMessageDOC.EM_Status);
					AssertNotNull("correctMessageDOC Interchange", correctMessageDOC.Interchange);
				});

				var headerTextXML = TestingData.GetHeaderTextXML(expectedEntryReference, correctMessageXML.EM_MessageNum);
				var headerTextEDI = TestingData.GetHeaderTextEdifactAndDOC(expectedEntryReference, correctMessageEDI.EM_MessageNum);
				var npeHeaderText = TestingData.GetHeaderTextNPEForXT(expectedEntryMRN);
				var headerTextDOC = TestingData.GetHeaderTextEdifactAndDOC(expectedEntryReference, correctMessageDOC.EM_MessageNum);
				TestingData.AssertOutgoingInterchange("XML-EHUB", correctMessageXML.Interchange, MessageTypeCodes.ImportQuery, SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, correctMessageXML.EM_GP, headerTextXML, TestingData.XMLMessageText);
				TestingData.AssertOutgoingInterchange("EDI-EHUB", correctMessageEDI.Interchange, MessageTypeCodes.ArrivalAtExit, SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, correctMessageEDI.EM_GP, headerTextEDI, TestingData.EdifactInterchangeText);
				TestingData.AssertOutgoingInterchange("NPE-XTT", correctMessageNPE.Interchange, MessageTypeCodes.InBoxNotificationForExport, SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, EDIInterchange.TransportType.xT, EDIInterchange.Status.Queued, correctMessageNPE.EM_GP, npeHeaderText, TestingData.NPEMessageText);
				TestingData.AssertOutgoingInterchange("DOC-EHUB", correctMessageDOC.Interchange, MessageTypeCodes.EsDocumentRequest, SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, correctMessageXML.EM_GP, headerTextDOC, TestingData.XMLMessageText);
			}
		}

		public void TestMessagesPopulateNewInterchange_DirectXT_ExceptInbox()
		{
			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(true))
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			{
				var expectedEntryReference = "ES000001";

				var correctMessageXML = Factory.CreateOutboundMessage(MessageTypeCodes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageEDI = Factory.CreateOutboundMessage(MessageTypeCodes.ArrivalAtExit, TestingData.EdifactMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageNPE = Factory.CreateOutboundMessage(MessageTypeCodes.InBoxNotificationForExport, TestingData.NPEMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageDOC = Factory.CreateOutboundMessage(MessageTypeCodes.EsDocumentRequest, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);

				Factory.Save();

				var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
				ediMessageCollection.Add(correctMessageXML);
				ediMessageCollection.Add(correctMessageEDI);
				ediMessageCollection.Add(correctMessageNPE);
				ediMessageCollection.Add(correctMessageDOC);

				var interchangeProvider = SetMockProvider(expectedEntryReference, ediMessageCollection);
				interchangeProvider.PackCollatedMessagesIntoInterchanges();
				var interchanges = interchangeProvider.Interchanges;

				Factory.Save();

				correctMessageXML.Reload();
				correctMessageEDI.Reload();
				correctMessageNPE.Reload();
				correctMessageDOC.Reload();

				CombineAssertions(() =>
				{
					AssertEquals("4 interchanges created", 4, interchanges.Length);
					AssertEquals("correctMessageXML Status", EDIMessage.Status.Sent, correctMessageXML.EM_Status);
					AssertNotNull("correctMessageXML Interchange", correctMessageXML.Interchange);
					AssertEquals("correctMessageEDI Status", EDIMessage.Status.Sent, correctMessageEDI.EM_Status);
					AssertNotNull("correctMessageEDI Interchange", correctMessageEDI.Interchange);
					AssertEquals("correctMessageNPE Status", EDIMessage.Status.Sent, correctMessageNPE.EM_Status);
					AssertNotNull("correctMessageNPE Interchange", correctMessageNPE.Interchange);
					AssertEquals("correctMessageDOC Status", EDIMessage.Status.Sent, correctMessageDOC.EM_Status);
					AssertNotNull("correctMessageDOC Interchange", correctMessageDOC.Interchange);
				});

				var npeHeaderText = TestingData.GetHeaderTextNPEForEHub(expectedEntryReference, correctMessageNPE.EM_MessageNum);
				TestingData.AssertOutgoingInterchange("XML-XTT", correctMessageXML.Interchange, MessageTypeCodes.ImportQuery, SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, EDIInterchange.TransportType.xT, EDIInterchange.Status.Queued, correctMessageXML.EM_GP, null, TestingData.XMLMessageText);
				TestingData.AssertOutgoingInterchange("EDI-XTT", correctMessageEDI.Interchange, MessageTypeCodes.ArrivalAtExit, SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, EDIInterchange.TransportType.xT, EDIInterchange.Status.Queued, correctMessageEDI.EM_GP, null, TestingData.EdifactInterchangeText);
				TestingData.AssertOutgoingInterchange("NPE-EHUB", correctMessageNPE.Interchange, MessageTypeCodes.InBoxNotificationForExport, SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, correctMessageNPE.EM_GP, npeHeaderText, TestingData.NPEMessageText);
				TestingData.AssertOutgoingInterchange("DOC-XTT", correctMessageDOC.Interchange, MessageTypeCodes.EsDocumentRequest, SpanishCustomsTypeCodeList.Codes.DocumentTestSpanishCustoms, EDIInterchange.TransportType.xT, EDIInterchange.Status.Queued, correctMessageXML.EM_GP, null, TestingData.XMLMessageText);
			}
		}

		public void TestMessagesPopulateNewInterchange_DirectXT()
		{
			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(true))
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var expectedEntryReference = "ES000001";
				var expectedEntryMRN = "20ES00999830001277";

				var correctMessageXML = Factory.CreateOutboundMessage(MessageTypeCodes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageEDI = Factory.CreateOutboundMessage(MessageTypeCodes.ArrivalAtExit, TestingData.EdifactMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageNPE = Factory.CreateOutboundMessage(MessageTypeCodes.InBoxNotificationForExport, TestingData.NPEMessageText, certificate.CertificateName, certificate.CertificatePK);
				var correctMessageDOC = Factory.CreateOutboundMessage(MessageTypeCodes.EsDocumentRequest, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);

				Factory.Save();

				var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
				ediMessageCollection.Add(correctMessageXML);
				ediMessageCollection.Add(correctMessageEDI);
				ediMessageCollection.Add(correctMessageNPE);
				ediMessageCollection.Add(correctMessageDOC);

				var interchangeProvider = SetMockProvider(expectedEntryReference, ediMessageCollection, expectedEntryMRN);
				interchangeProvider.PackCollatedMessagesIntoInterchanges();
				var interchanges = interchangeProvider.Interchanges;

				Factory.Save();

				correctMessageXML.Reload();
				correctMessageEDI.Reload();
				correctMessageNPE.Reload();
				correctMessageDOC.Reload();

				CombineAssertions(() =>
				{
					AssertEquals("4 interchanges created", 4, interchanges.Length);
					AssertEquals("correctMessageXML Status", EDIMessage.Status.Sent, correctMessageXML.EM_Status);
					AssertNotNull("correctMessageXML Interchange", correctMessageXML.Interchange);
					AssertEquals("correctMessageEDI Status", EDIMessage.Status.Sent, correctMessageEDI.EM_Status);
					AssertNotNull("correctMessageEDI Interchange", correctMessageEDI.Interchange);
					AssertEquals("correctMessageNPE Status", EDIMessage.Status.Sent, correctMessageNPE.EM_Status);
					AssertNotNull("correctMessageNPE Interchange", correctMessageNPE.Interchange);
					AssertEquals("correctMessageDOC Status", EDIMessage.Status.Sent, correctMessageDOC.EM_Status);
					AssertNotNull("correctMessageDOC Interchange", correctMessageDOC.Interchange);
				});

				var npeHeaderText = TestingData.GetHeaderTextNPEForXT(expectedEntryMRN);
				TestingData.AssertOutgoingInterchange("XML-XTT", correctMessageXML.Interchange, MessageTypeCodes.ImportQuery, SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, EDIInterchange.TransportType.xT, EDIInterchange.Status.Queued, correctMessageXML.EM_GP, null, TestingData.XMLMessageText);
				TestingData.AssertOutgoingInterchange("EDI-XTT", correctMessageEDI.Interchange, MessageTypeCodes.ArrivalAtExit, SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, EDIInterchange.TransportType.xT, EDIInterchange.Status.Queued, correctMessageEDI.EM_GP, null, TestingData.EdifactInterchangeText);
				TestingData.AssertOutgoingInterchange("NPE-XTT", correctMessageNPE.Interchange, MessageTypeCodes.InBoxNotificationForExport, SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, EDIInterchange.TransportType.xT, EDIInterchange.Status.Queued, correctMessageNPE.EM_GP, npeHeaderText, TestingData.NPEMessageText);
				TestingData.AssertOutgoingInterchange("DOC-XTT", correctMessageDOC.Interchange, MessageTypeCodes.EsDocumentRequest, SpanishCustomsTypeCodeList.Codes.DocumentTestSpanishCustoms, EDIInterchange.TransportType.xT, EDIInterchange.Status.Queued, correctMessageXML.EM_GP, null, TestingData.XMLMessageText);
			}
		}

		public void TestMessagesPopulateNewInterchange_InboxPendingList()
		{
			var correctMessageXML = Factory.CreateOutboundMessage(MessageTypeCodes.InboxPendingList, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);

			Factory.Save();

			var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
			ediMessageCollection.Add(correctMessageXML);

			var logger = new LoggingInformation();
			var interchangeProvider = new ESInterchangeProvider(ediMessageCollection, logger);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			var interchanges = interchangeProvider.Interchanges;

			Factory.Save();

			correctMessageXML.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("One interchange created", 1, interchanges.Length);
				AssertEquals("Direct XT XML Message Status", EDIMessage.Status.Sent, correctMessageXML.EM_Status);
			});

			TestingData.AssertOutgoingInterchange("XML-XTT", correctMessageXML.Interchange, MessageTypeCodes.InboxPendingList, SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, EDIInterchange.TransportType.xT, EDIInterchange.Status.Queued, correctMessageXML.EM_GP, null, TestingData.XMLMessageText);
		}

		public void TestMessagesPopulateNewInterchange_NoEM_GP()
		{
			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(false))
			{
				var expectedEntryReference = "ES000001";

				var correctMessageXML = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, ZGuid.Empty);

				Factory.Save();

				CombineAssertions("PRE-CONDITION", () =>
				{
					AssertNull("correctMessageXML Message Interchange", correctMessageXML.Interchange);
				});

				var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
				ediMessageCollection.Add(correctMessageXML);

				var interchangeProvider = SetMockProvider(expectedEntryReference, ediMessageCollection);
				interchangeProvider.PackCollatedMessagesIntoInterchanges();
				var interchanges = interchangeProvider.Interchanges;

				Factory.Save();

				correctMessageXML.Reload();

				CombineAssertions(() =>
				{
					AssertEquals("1 interchange created", 1, interchanges.Length);
					AssertEquals("correctMessageXML Status", EDIMessage.Status.Sent, correctMessageXML.EM_Status);
					AssertNotNull("correctMessageXML Interchange", correctMessageXML.Interchange);
				});

				var headerTextXML = TestingData.GetHeaderTextXML(expectedEntryReference, correctMessageXML.EM_MessageNum);
				TestingData.AssertOutgoingInterchange("XML-EHUB", correctMessageXML.Interchange, MessageTypeCodes.ImportQuery, SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, EDIInterchange.TransportType.eHub, EDIInterchange.Status.eHubQueued, certificate.CertificatePK, headerTextXML, TestingData.XMLMessageText);
			}
		}

		public void TestMessagesPopulateNewInterchange_NoLinkedObject()
		{
			var message = Factory.CreateOutboundMessage(MessageTypeCodes.ImportQuery, TestingData.XMLMessageText, certificate.CertificateName, certificate.CertificatePK);
			Factory.Save();

			CombineAssertions("PRE-CONDITION", () =>
			{
				AssertNull("message Message Interchange", message.Interchange);
			});

			var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
			ediMessageCollection.Add(message);

			var logger = new LoggingInformation();
			var interchangeProvider = new ESInterchangeProvider(ediMessageCollection, logger);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			var interchanges = interchangeProvider.Interchanges;

			Factory.Save();

			message.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("No interchanges created", 0, interchanges.Length);
				AssertEquals("message Status", EDIMessage.Status.Failed, message.EM_Status);

				var note = message.Notes.FindByDescription("Processing Log").Single();
				AssertContains("ST_NoteText", "Message's Linked Object is null so can't continue with processing. The message's status has been set to 'Failed'.", note.ST_NoteText);

				AssertContains("logger", "Message's Linked Object is null so can't continue with processing. The message's status has been set to 'Failed'.", logger.UserLogStrings[0]);

				AssertNull("message Interchange", message.Interchange);
			});
		}

		public void TestMessagesPopulateNewInterchange_NoApplicationReference()
		{
			var mockProvider = new Mock<IESMessageInfoProvider>();
			mockProvider.CallBase = true;
			mockProvider.Setup(m => m.Broker).Returns(staff);
			mockProvider.Setup(m => m.EntryReference).Returns("ES000001");
			mockProvider.Setup(m => m.MRN).Returns("20ES00999830001277");

			var message = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, TestingData.XMLMessageText, ZString.Empty, certificate.CertificatePK);
			message.EM_LinkedObject = Factory.NewWithValidTestData<CusEntryHeader>();
			Factory.Save();

			CombineAssertions("PRE-CONDITION", () =>
			{
				AssertNull("message Message Interchange", message.Interchange);
			});

			var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
			ediMessageCollection.Add(message);

			var logger = new LoggingInformation();

			var interchangeProviderMock = new Mock<ESInterchangeProvider>(new object[] { ediMessageCollection, logger });
			interchangeProviderMock.CallBase = true;
			interchangeProviderMock.Setup(c => c.GetMessageProvider(message)).Returns(mockProvider.Object);
			var interchangeProvider = interchangeProviderMock.Object;
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			var interchanges = interchangeProvider.Interchanges;

			Factory.Save();

			message.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("No interchanges created", 0, interchanges.Length);
				AssertEquals("message Status", EDIMessage.Status.Failed, message.EM_Status);

				var note = message.Notes.FindByDescription("Processing Log").Single();
				AssertContains("ST_NoteText", "Message's Application Reference (certificate name) is empty so can't continue with processing. The message's status has been set to 'Failed'.", note.ST_NoteText);

				AssertContains("logger", "Message's Application Reference (certificate name) is empty so can't continue with processing. The message's status has been set to 'Failed'.", logger.UserLogStrings[0]);

				AssertNull("message Interchange", message.Interchange);
			});
		}

		public void TestMessagesPopulateNewInterchange_NoMessageText()
		{
			var mockProvider = new Mock<IESMessageInfoProvider>();
			mockProvider.CallBase = true;
			mockProvider.Setup(m => m.Broker).Returns(staff);
			mockProvider.Setup(m => m.EntryReference).Returns("ES000001");
			mockProvider.Setup(m => m.MRN).Returns("20ES00999830001277");

			var message = Factory.CreateOutboundMessage(DeclarationMessageTypeList.Codes.ImportQuery, ZString.Empty, certificate.CertificateName, certificate.CertificatePK);
			message.EM_LinkedObject = Factory.NewWithValidTestData<CusEntryHeader>();
			Factory.Save();

			CombineAssertions("PRE-CONDITION", () =>
			{
				AssertNull("message Message Interchange", message.Interchange);
			});

			var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
			ediMessageCollection.Add(message);

			var logger = new LoggingInformation();

			var interchangeProviderMock = new Mock<ESInterchangeProvider>(new object[] { ediMessageCollection, logger });
			interchangeProviderMock.CallBase = true;
			interchangeProviderMock.Setup(c => c.GetMessageProvider(message)).Returns(mockProvider.Object);
			var interchangeProvider = interchangeProviderMock.Object;
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			var interchanges = interchangeProvider.Interchanges;

			Factory.Save();

			message.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("No interchanges created", 0, interchanges.Length);
				AssertEquals("message Status", EDIMessage.Status.Failed, message.EM_Status);

				var note = message.Notes.FindByDescription("Processing Log").Single();
				AssertContains("ST_NoteText", "Message's Message Text is empty so can't continue with processing. The message's status has been set to 'Failed'.", note.ST_NoteText);

				AssertContains("logger", "Message's Message Text is empty so can't continue with processing. The message's status has been set to 'Failed'.", logger.UserLogStrings[0]);

				AssertNull("message Interchange", message.Interchange);
			});
		}

		protected override Enterprise.Messaging.InterchangeProviders.InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new ESInterchangeProvider(collection, new LoggingInformation());

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
