using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Macros;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Constants = CargoWise.EventReference.Constants;
using Languages = Enterprise.Core.SharedConstants.Languages;
using MessageEvent = Enterprise.ZArchitecture.Business.Event;
using Res = Enterprise.DocumentVisualizer.Business.Res;
using SystemKeyValuePair = System.Collections.Generic.KeyValuePair<string, string>;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class MessageSenderTest : TestCaseWithFactory
	{
		IEDIMessage GetLastSentMessage()
		{
			return Factory.Load<IEDIMessage>(new ZQuery())
				.OrderByDescending(m => m.EM_SystemCreateTimeUtc)
				.FirstOrDefault();
		}

		#region TestSendMessage

		public void TestSendMessage()
		{
			AssertSendMessage();
		}

		public void TestSendingMessage_WhenCurrentLanguageIsSetToSpanish()
		{
			using (Res.TemporarilySwitchLanguage(Languages.Spanish))
			using (var mockCache = Res.GetLanguageInstance(Languages.Spanish).UseMockData())
			{
				const string carrierInSpanish = "Transportador";
				var resStringKeyCarrier = "Carrier".GetResStringKey();
				mockCache.Put(resStringKeyCarrier,
					new ResourceStringData(resStringKeyCarrier, carrierInSpanish));

				const string documentNameInSpanish = "Instrucciones de envio";
				var resStringKeyDocumentName = "Shipping Instruction".GetResStringKey();
				mockCache.Put(resStringKeyDocumentName,
					new ResourceStringData(resStringKeyDocumentName, documentNameInSpanish));

				AssertSendMessage(messageRecipientInPopupMessage: carrierInSpanish, expectedTranslatedDocumentName: documentNameInSpanish);
			}
		}

		public void TestSendingMessage_WhenCurrentLanguageIsSetToSpanish_ButNoTranslations()
		{
			using (Res.TemporarilySwitchLanguage(Languages.Spanish))
			using (var mockCache = Res.GetLanguageInstance(Languages.Spanish).UseMockData())
			{
				AssertSendMessage();
			}
		}

		public void TestSendMessage_AbsoluteNamespace()
		{
			AssertSendMessage("http://www.google.com/ShippingInstruction/1");
		}

		public void TestSendMessage_RelativeNamespace()
		{
			AssertSendMessage("/1", "http://www.cargowise.com/Schemas/Universal/2012/11/1");
		}

		void AssertSendMessage(string ns = "", string expectedNs = "", string messageRecipientInPopupMessage = "Carrier", string expectedTranslatedDocumentName = "Shipping Instruction")
		{
			using (Factory.AddDisposableService())
			{
				var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

				var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);

				var documentData = dataObject.MakeDynamic();

				var namespaceParameter = !string.IsNullOrEmpty(ns)
					? $":XmlNamespace=\"{ns}\""
					: string.Empty;

				var worksheet = DummyWorksheet.Parse(
	$@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier""{namespaceParameter}:
#End");

				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
					new MacroScope(documentData),
					Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

				var document = new DummyDocument();
				document.Data = documentData;

				var menu = Factory.New<VisualizerMenuItem>();
				menu.SU_MenuName = "test";

				var template = Factory.New<StmTemplate>();

				var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
				documentPivot.SI_SU = menu.PK;
				documentPivot.SI_SO = template.PK;

				var storage = consol.LoadOrCreateDocumentData("xxx");
				var recipient = Factory.NewWithValidTestData<OrgHeader>();
				recipient.OH_Code = "TESTORG";

				consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;

				Factory.Save();

				var notifications = new Moq.Mock<IUserNotificationService>();

				notifications.Setup(s => s.ShowConfirmation(null, null));

				var messageSentEventFired = false;

				var broker = new EventBroker();
				broker
					.GetEvent<MessageSentEvent>()
					.Subscribe(e => messageSentEventFired = true);

				var services = new ServiceContainer();
				services.Register<IUserNotificationService>(notifications.Object);
				services.Register<IEventBroker>(broker);

				var sender = new MessageSender(messageInstructions, document, storage, services);

				sender.Send(DefaultSenderParameters());
				notifications.Verify(s => s.ShowConfirmation(null, null), Moq.Times.Never);
				notifications.Reset();

				var logs = storage.Logs
					.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode);

				AssertMultilineASCIIEquals("MSN log reference", "|DEP=Carrier|MST=Shipping Instruction", string.Join("\r\n", logs.Select(l => l.SL_Reference)));

				var confirmationMessage = $@"A message has previously been sent, and there is no reply received from the {messageRecipientInPopupMessage}. Resending the message may cause errors and possibly revert to a manual process.

Are you sure you want to send the message?";

				const string confirmationCaption = "Confirmation";

				notifications.Setup(s => s.ShowConfirmation(confirmationMessage, confirmationCaption)).Returns(false);
				sender.Send(DefaultSenderParameters());
				notifications.Verify(s => s.ShowConfirmation(confirmationMessage, confirmationCaption), Moq.Times.Once);
				notifications.Reset();

				logs = storage.Logs
					.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode);

				AssertMultilineASCIIEquals("MSN log reference", "|DEP=Carrier|MST=Shipping Instruction", string.Join("\r\n", logs.Select(l => l.SL_Reference)));

				notifications.Setup(s => s.ShowConfirmation(confirmationMessage, confirmationCaption)).Returns(true);
				sender.Send(DefaultSenderParameters());
				notifications.Verify(s => s.ShowConfirmation(confirmationMessage, confirmationCaption), Moq.Times.Once);
				notifications.Reset();

				logs = storage.Logs
					.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode);

				AssertMultilineASCIIEquals("MSN log references",
				@"|DEP=Carrier|MST=Shipping Instruction
|DEP=Carrier|MST=Shipping Instruction",
				string.Join("\r\n", logs.Select(l => l.SL_Reference)));

				if (!string.IsNullOrEmpty(ns))
				{
					var xml = Factory.Load<Enterprise.Messaging.Integration.IEDIMessage>(new ZQuery())
						.OrderByDescending(m => ((BusinessObject)m)[EDIMessageSchema.EM_SystemCreateTimeUtc])
						.FirstOrDefault().EM_MessageText;

					if (string.IsNullOrEmpty(expectedNs))
					{
						expectedNs = ns;
					}
					AssertContains("sent xml contains correct namespace", $"xmlns=\"{expectedNs}\"", xml);
					AssertContains("sent xml contains correct documentname", "<DocumentName>Shipping Instruction</DocumentName>", xml);
				}

				AssertEquals(expectedTranslatedDocumentName, messageInstructions.TranslatedDocumentName);

				AssertEquals("MessageSentEvent fired", true, messageSentEventFired);
			}
		}

		#endregion

		#region TestSendMessageWithUserSelectedCodeAndDescriptionAmendmentReason

		public void TestSendMessageWithUserSelectedCodeAndDescriptionAmendmentReason()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);

			var documentData = dataObject.MakeDynamic();

			var worksheet = DummyWorksheet.Parse(
				@"#Config
Name=""Test Document""
EHubClientID=""Zoom Zoom""
MessageRecipient=""Carrier""
RequireMessageAmendmentReason=true
MessageAmendmentOptions=[[""01"", ""Attaching Supporting Customs Release documents""], [""02"", ""Updating the contract number only""]]
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var amendmentOptions = new CodeDescriptionPairList();
			amendmentOptions.AddPair("01", "Attaching Supporting Customs Release documents");
			amendmentOptions.AddPair("02", "Updating the contract number only");

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Test Document");

			var document = new DummyDocument();
			document.Data = documentData;

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var storage = consol.LoadOrCreateDocumentData("xxx");
			var recipient = Factory.NewWithValidTestData<OrgHeader>();
			recipient.OH_Code = "TESTORG";

			consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;

			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var sender = new MessageSender(messageInstructions, document, storage, services);

			notifications.Setup(s => s.ShowConfirmation(null, null));
			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.ShowConfirmation(null, null), Times.Never);
			notifications.Reset();

			var logs = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode);

			AssertMultilineASCIIEquals("MSN log reference", "|DEP=Carrier|MST=Test Document", string.Join("\r\n", logs.Select(l => l.SL_Reference)));

			storage.Logs.AddNew(Events.MessageAccepted,
				new KeyValuePair<string, string>(
					Constants.EventReferenceParameters.Codes.MessageType,
					"Test Document"));

			storage.Factory.Save();

			ICodeDescription userSelectedCodeDescription = new DummyCodeDescription
			{
				Code = "02",
				Description = "Updating the contract number only"
			};

			notifications.Setup(s => s.QueryUserResponse(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<ICodeDescriptionPairList>())).Returns(userSelectedCodeDescription);
			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.QueryUserResponse(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<ICodeDescriptionPairList>()), Times.Once);

			var xml = Factory.Load<Enterprise.Messaging.Integration.IEDIMessage>(new ZQuery())
				.OrderByDescending(m => ((BusinessObject)m)[EDIMessageSchema.EM_SystemCreateTimeUtc])
				.FirstOrDefault().EM_MessageText;

			AssertContains("sent xml contains user selected code and description",
				"02 - Updating the contract number only", xml);
		}

		#endregion

		#region TestAllowSendMessageAmendment_ShowsConfirmation

		public void TestAllowSendMessageAmendment_ShowsConfirmation()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);
			var documentData = dataObject.MakeDynamic();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":AllowSendMessageAmendment=false:
#End");
			var worksheetTemplate = new StandardTemplate(worksheet);
			var messageInstruction = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var document = new DummyDocument();
			document.Data = documentData;

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var storage = consol.LoadOrCreateDocumentData("xxx");
			storage.Logs.AddNew(Events.MessageSent, ZDateTimeOffset.Now, false,
				new SystemKeyValuePair(Constants.EventReferenceParameters.Codes.MessageType, "Shipping Instruction"));

			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var sender = new MessageSender(messageInstruction, document, storage, services);

			const string confirmationCaption = "Confirmation";
			const string confirmationMessage = "A message has previously been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";

			notifications.Setup(s => s.ShowMessage(confirmationMessage, confirmationCaption));
			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.ShowMessage(confirmationMessage, confirmationCaption), Times.Once);

			AssertEquals("no new log has been added.", 1, storage.Logs.GetAllLogs().Count);
		}

		#endregion

		#region TestSendMessageAmendmentWithReason_HaveExistingNote

		public void TestSendMessageAmendmentWithReason_HaveExistingNote()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);
			dataObject.SetNoteCollection(() => new DataObjectList<Note>());
			dataObject.NoteCollection.Add(new Note { Description = "Test Note", IsCustomDescription = false, NoteText = "Content" });
			var documentData = dataObject.MakeDynamic();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":RequireMessageAmendmentReason=true:
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var document = new DummyDocument();
			document.Data = documentData;

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var storage = consol.LoadOrCreateDocumentData("xxx");
			var recipient = Factory.NewWithValidTestData<OrgHeader>();
			recipient.OH_Code = "TESTORG";

			consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;

			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var sender = new MessageSender(messageInstructions, document, storage, services);

			notifications.Setup(s => s.ShowConfirmation(null, null));
			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.ShowConfirmation(null, null), Times.Never);
			notifications.Reset();

			var message = GetLastSentMessage();
			var interchange = message.Interchange;

			const string confirmationCaption = "Confirmation";

			const string confirmationMessage = @"A message has previously been sent, and there is no reply received from the Carrier. Resending the message may cause errors and possibly revert to a manual process.

Are you sure you want to send the message?";

			notifications.Setup(s => s.ShowConfirmation(confirmationMessage, confirmationCaption)).Returns(true);
			notifications.Setup(s => s.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns("Response");
			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.ShowConfirmation(confirmationMessage, confirmationCaption), Times.Once);
			notifications.Verify(s => s.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
			notifications.Reset();

			var log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.DataExportCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			var universalXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Shipping Instruction</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>AMD</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate></TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>Test Note</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>Content</NoteText>
      </Note>
      <Note>
        <Description>ReasonForMessageAmendment</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteText>Response</NoteText>
      </Note>
    </NoteCollection>
  </Shipment>
</UniversalShipment>";

			var xml = XMLMessageTestHelper.GetXml(log);
			AssertMultilineASCIIEquals("UXml", string.Format(universalXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), xml.ToString());
			AssertMultilineASCIIEquals("Xml passed schema validation", string.Empty, XMLMessageTestHelper.ValidateXMLAgainstSchema(dataObject, xml));
		}

		#endregion

		#region TestSendMessageAmendmentWithReason_NoExistingNote

		public void TestSendMessageAmendmentWithReason_NoExistingNote()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);

			var documentData = dataObject.MakeDynamic();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":RequireMessageAmendmentReason=true:
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var document = new DummyDocument();
			document.Data = documentData;

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var storage = consol.LoadOrCreateDocumentData("xxx");
			var recipient = Factory.NewWithValidTestData<OrgHeader>();
			recipient.OH_Code = "TESTORG";

			consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;

			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var sender = new MessageSender(messageInstructions, document, storage, services);

			notifications.Setup(s => s.ShowConfirmation(null, null));
			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.ShowConfirmation(null, null), Times.Never);
			notifications.Reset();

			var message = GetLastSentMessage();
			var interchange = message.Interchange;

			const string confirmationCaption = "Confirmation";

			const string confirmationMessage = @"A message has previously been sent, and there is no reply received from the Carrier. Resending the message may cause errors and possibly revert to a manual process.

Are you sure you want to send the message?";

			notifications.Setup(s => s.ShowConfirmation(confirmationMessage, confirmationCaption)).Returns(true);
			notifications.Setup(s => s.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns("Response");
			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.ShowConfirmation(confirmationMessage, confirmationCaption), Times.Once);
			notifications.Verify(s => s.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
			notifications.Reset();

			var log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.DataExportCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			var universalXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Shipping Instruction</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>AMD</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate></TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>ReasonForMessageAmendment</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteText>Response</NoteText>
      </Note>
    </NoteCollection>
  </Shipment>
</UniversalShipment>";

			var xml = XMLMessageTestHelper.GetXml(log);
			AssertMultilineASCIIEquals("expected amendment UXml", string.Format(universalXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), xml.ToString());

			notifications.Setup(s => s.ShowConfirmation(confirmationMessage, confirmationCaption)).Returns(true);
			notifications.Setup(s => s.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns("new");
			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.ShowConfirmation(confirmationMessage, confirmationCaption), Times.Once);
			notifications.Verify(s => s.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

			log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.DataExportCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			universalXml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>3</DataVersion>
        <DocumentName>Shipping Instruction</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>AMD</Purpose>
        <SubmissionVersion>3</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate></TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>ReasonForMessageAmendment</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteText>new</NoteText>
      </Note>
    </NoteCollection>
  </Shipment>
</UniversalShipment>";
			xml = XMLMessageTestHelper.GetXml(log);

			AssertMultilineASCIIEquals("expected 2nd amendment UXml", string.Format(universalXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), xml.ToString());
			AssertMultilineASCIIEquals("Xml passed schema validation", string.Empty, XMLMessageTestHelper.ValidateXMLAgainstSchema(dataObject, xml));
		}

		#endregion

		#region TestSendMessage_WithCustomAmendmentReason

		public void TestSendMessage_WithCustomAmendmentReason()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);

			var documentData = dataObject.MakeDynamic();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":RequireMessageAmendmentReason=true:
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var document = new DummyDocument();
			document.Data = documentData;

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var storage = consol.LoadOrCreateDocumentData("xxx");
			var recipient = Factory.NewWithValidTestData<OrgHeader>();
			recipient.OH_Code = "TESTORG";

			consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;

			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var messageAmendmentReason = new
			{
				Reason1 = "reason1",
				Reason2 = "reason2"
			};

			Func<IDataObject, object, bool> populateMessageAmendmentReason = (dataObjectToSend, reason) =>
			{
				AssertEquals("Custom ammendment reason has been passed to PopulateMessageAmendmentReason", messageAmendmentReason, reason);

				if (dataObjectToSend is Shipment shipment)
				{
					shipment.SetNoteCollection(() => new DataObjectList<Note>
					{
						new Note
						{
							Description = "Custom Reason 1",
							IsCustomDescription = true,
							NoteText = "custom reason 1"
						},
						new Note
						{
							Description = "Custom Reason 2",
							IsCustomDescription = true,
							NoteText = "custom reason 2"
						},
					});
				}
				else
				{
					Fail("Incorrect DataObject has been passed to PopulateMessageAmendmentReason");
				}

				return true;
			};

			var messagingExtensions = new Mock<IMessagingExtensions>();
			var customAmendmentSupporter = messagingExtensions.As<ICustomMessageAmendmentSupporter>();

			messagingExtensions.Setup(m => m.ContinueWithSendingMessageAmendment(It.IsAny<IUserNotifications>())).Returns(true);
			customAmendmentSupporter.Setup(m => m.GetMessageAmendmentReason()).Returns(messageAmendmentReason);
			customAmendmentSupporter.Setup(m => m.PopulateMessageAmendmentReason(It.IsNotNull<IDataObject>(), It.IsNotNull<object>())).Returns(populateMessageAmendmentReason);

			var dummySupporter = new DummyConsolVisualizableDocumentSupporter();

			IEDIMessage message = null;
			IEDIInterchange interchange = null;

			using (dummySupporter.Activate(messagingExtensions.Object))
			{
				var sender = new MessageSender(messageInstructions, document, storage, services);
				notifications.Setup(s => s.ShowConfirmation(null, null));

				Assert("original message has been sent", sender.Send(DefaultSenderParameters()));

				message = GetLastSentMessage();
				interchange = message.Interchange;

				Assert("amendment message has been sent", sender.Send(DefaultSenderParameters()));
			}

			const string expectedUniversalXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Shipping Instruction</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>AMD</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate></TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>Custom Reason 1</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteText>custom reason 1</NoteText>
      </Note>
      <Note>
        <Description>Custom Reason 2</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteText>custom reason 2</NoteText>
      </Note>
    </NoteCollection>
  </Shipment>
</UniversalShipment>";

			var amendmentLog = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.DataExportCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			var xml = XMLMessageTestHelper.GetXml(amendmentLog);

			AssertMultilineASCIIEquals("expected amendment UXml", string.Format(expectedUniversalXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), xml.ToString());
			AssertMultilineASCIIEquals("Xml passed schema validation", string.Empty, XMLMessageTestHelper.ValidateXMLAgainstSchema(dataObject, xml));
		}

		public void TestSendMessage_WithCustomAmendmentReason_Cancel()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);

			var documentData = dataObject.MakeDynamic();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":RequireMessageAmendmentReason=true:
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var document = new DummyDocument();
			document.Data = documentData;

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var storage = consol.LoadOrCreateDocumentData("xxx");
			var recipient = Factory.NewWithValidTestData<OrgHeader>();
			recipient.OH_Code = "TESTORG";

			consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;

			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			object messageAmendmentReason = null;

			Func<IDataObject, object, bool> populateMessageAmendmentReason = (dataObjectToSend, reason) =>
			{
				Fail("should not PopulateMessageAmendmentReason as no reason was returned");
				return false;
			};

			var messagingExtensions = new Mock<IMessagingExtensions>();
			var customAmendmentSupporter = messagingExtensions.As<ICustomMessageAmendmentSupporter>();

			messagingExtensions.Setup(m => m.ContinueWithSendingMessageAmendment(It.IsAny<IUserNotifications>())).Returns(true);
			customAmendmentSupporter.Setup(m => m.GetMessageAmendmentReason()).Returns(messageAmendmentReason);
			customAmendmentSupporter.Setup(m => m.PopulateMessageAmendmentReason(It.IsNotNull<IDataObject>(), It.IsNotNull<object>())).Returns(populateMessageAmendmentReason);

			var dummySupporter = new DummyConsolVisualizableDocumentSupporter();

			using (dummySupporter.Activate(messagingExtensions.Object))
			{
				var sender = new MessageSender(messageInstructions, document, storage, services);
				notifications.Setup(s => s.ShowConfirmation(null, null));

				Assert("original message has been sent", sender.Send(DefaultSenderParameters()));

				Assert("amendment message has not been sent", !sender.Send(DefaultSenderParameters()));
			}
		}

		#endregion

		#region TestNoMessageSentWithNoAmendmentReason

		public void TestNoMessageSentWithNoAmendmentReason()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);

			var documentData = dataObject.MakeDynamic();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":RequireMessageAmendmentReason=true:
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var document = new DummyDocument();
			document.Data = documentData;

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var storage = consol.LoadOrCreateDocumentData("xxx");
			var recipient = Factory.NewWithValidTestData<OrgHeader>();
			recipient.OH_Code = "TESTORG";

			consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;

			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();
			notifications.Setup(s => s.ShowConfirmation(null, null));

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var sender = new MessageSender(messageInstructions, document, storage, services);

			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.ShowConfirmation(null, null), Times.Never);
			notifications.Reset();

			var logs = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode)
				.ToArray();

			AssertMultilineASCIIEquals("Message has been sent",
				"|DEP=Carrier|MST=Shipping Instruction",
				string.Join("\r\n", logs.Select(l => l.SL_Reference)));

			const string confirmationMessage = @"A message has previously been sent, and there is no reply received from the Carrier. Resending the message may cause errors and possibly revert to a manual process.

Are you sure you want to send the message?";

			const string confirmationCaption = "Confirmation";

			notifications.Setup(s => s.ShowConfirmation(confirmationMessage, confirmationCaption)).Returns(false);
			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.ShowConfirmation(confirmationMessage, confirmationCaption), Times.Once);
			notifications.Reset();

			logs = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode)
				.ToArray();

			AssertMultilineASCIIEquals("Amendment message hasn't been sent",
				"|DEP=Carrier|MST=Shipping Instruction",
				string.Join("\r\n", logs.Select(l => l.SL_Reference)));

			notifications.Setup(s => s.ShowConfirmation(confirmationMessage, confirmationCaption)).Returns(true);
			sender.Send(DefaultSenderParameters());
			notifications.Verify(s => s.ShowConfirmation(confirmationMessage, confirmationCaption), Times.Once);
			notifications.Reset();

			logs = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode)
				.ToArray();

			AssertMultilineASCIIEquals("Amendment message hasn't been sent",
@"|DEP=Carrier|MST=Shipping Instruction",
				string.Join("\r\n", logs.Select(l => l.SL_Reference)));
		}

		#endregion

		#region TestNLImportNotificationUsesCorrectConfirmation

		public void TestNLImportNotificationUsesCorrectConfirmationIRJ()
		{
			TestNLImportNotificationUsesCorrectConfirmation(Events.InterchangeRejected, true);
		}

		public void TestNLImportNotificationUsesCorrectConfirmationMJR()
		{
			TestNLImportNotificationUsesCorrectConfirmation(Events.MessageRejected, true);
		}

		public void TestNLImportNotificationUsesCorrectConfirmationIRA()
		{
			TestNLImportNotificationUsesCorrectConfirmation(Events.InterchangeReceiptAcknowledged, false);
		}

		public void TestNLImportNotificationUsesCorrectConfirmationMAA()
		{
			TestNLImportNotificationUsesCorrectConfirmation(Events.MessageAccepted, false);
		}

		#region Implementation

		void TestNLImportNotificationUsesCorrectConfirmation(MessageEvent messageEvent, bool expectMessageSent)
		{
			// Create consol
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);

			var documentData = dataObject.MakeDynamic();

			// create import notification form
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Import Notification"":DataContext=""UXML"":EHubClientID=""PORTBASE"":MessageRecipient=""Portbase"":AllowSendMessageWithdrawal=false:
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Import Notification");

			var document = new DummyDocument();
			document.Data = documentData;

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var storage = consol.LoadOrCreateDocumentData("xxx");
			var recipient = Factory.NewWithValidTestData<OrgHeader>();
			recipient.OH_Code = "TESTORG";

			consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;

			Factory.Save();

			var notifications = new Mock<IUserNotificationService>();

			const string warningMessage = @"Using this option without checking with Portbase first might result in duplicate Import Notification being reported to Portbase.
Re-Sending should only be required when there is a serious messaging failure at the Portbase end.
Before using this option, you should always check with Portbase to make sure they have not already registered your job.";

			const string confirmationCaption = "Warning";

			const string prompt = "If you have done so, please type the following to confirm:";

			const string confirmationMessage = "I have confirmed with the Portbase that they did not process the Import Notification message already sent.";

			var map = new MacroMap(new Dictionary<string, object>
			{
				[nameof(MessageSender.Parameters.AmendmentWarning)] = warningMessage,
				[nameof(MessageSender.Parameters.AmendmentConfirmation)] = confirmationMessage
			});

			// send expect no confirmation
			notifications.Setup(s => s.ShowConfirmation(null, null));

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var sender = new MessageSender(messageInstructions, document, storage, services);

			sender.Send(MessageSender.Parameters.New(map));

			notifications.Verify(s => s.ShowConfirmation(null, null), Times.Never);
			notifications.Reset();

			var logs = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode);

			AssertMultilineASCIIEquals("MSN log reference", "|DEP=Portbase|MST=Import Notification", string.Join("\r\n", logs.Select(l => l.SL_Reference)));

			// get message response and send again expect correct confirmation response
			storage.Logs.AddNew(messageEvent, ZDateTimeOffset.Now, false,
				new SystemKeyValuePair(Constants.EventReferenceParameters.Codes.Department, "Portbase"),
				new SystemKeyValuePair(Constants.EventReferenceParameters.Codes.MessageType, "Import Notification"));
			Factory.Save();

			if (expectMessageSent)
			{
				notifications.Setup(s => s.ShowConfirmation(null, null));

				sender.Send(MessageSender.Parameters.New(map));

				notifications.Verify(s => s.ShowConfirmation(null, null), Times.Never);
				notifications.Reset();

				logs = storage.Logs
					.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode);

				AssertMultilineASCIIEquals("MSN log reference", "|DEP=Portbase|MST=Import Notification\r\n|DEP=Portbase|MST=Import Notification", string.Join("\r\n", logs.Select(l => l.SL_Reference)));
			}
			else
			{
				notifications.Setup(s => s.ShowConfirmation(warningMessage, confirmationCaption, prompt, confirmationMessage)).Returns(false);

				sender.Send(MessageSender.Parameters.New(map));

				notifications.Verify(s => s.ShowConfirmation(warningMessage, confirmationCaption, prompt, confirmationMessage), Times.Once);
				notifications.Reset();

				logs = storage.Logs
					.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode);

				AssertMultilineASCIIEquals("MSN log reference", "|DEP=Portbase|MST=Import Notification", string.Join("\r\n", logs.Select(l => l.SL_Reference)));
			}

			// check confirmation works
			notifications.Setup(s => s.ShowConfirmation(warningMessage, confirmationCaption, prompt, confirmationMessage)).Returns(true);

			sender.Send(MessageSender.Parameters.New(map));

			notifications.Verify(s => s.ShowConfirmation(warningMessage, confirmationCaption, prompt, confirmationMessage), Times.Once);
			notifications.Reset();

			logs = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageSentCode);

			if (expectMessageSent)
			{
				AssertMultilineASCIIEquals("MSN log reference", "|DEP=Portbase|MST=Import Notification\r\n|DEP=Portbase|MST=Import Notification\r\n|DEP=Portbase|MST=Import Notification", string.Join("\r\n", logs.Select(l => l.SL_Reference)));
			}
			else
			{
				AssertMultilineASCIIEquals("MSN log reference", "|DEP=Portbase|MST=Import Notification\r\n|DEP=Portbase|MST=Import Notification", string.Join("\r\n", logs.Select(l => l.SL_Reference)));
			}
		}

		#endregion Implementation

		#endregion TestNLImportNotificationUsesCorrectConfirmation

		MessageSender.Parameters DefaultSenderParameters()
		{
			return MessageSender.Parameters.New(new MacroMap(new Dictionary<string, object>
			{
				[nameof(MessageSender.Parameters.AmendmentWarning)] = @"A message has previously been sent, and there is no reply received from the Carrier. Resending the message may cause errors and possibly revert to a manual process.

Are you sure you want to send the message?",
				[nameof(MessageSender.Parameters.AmendmentConfirmation)] = ""
			}));
		}
	}
}
