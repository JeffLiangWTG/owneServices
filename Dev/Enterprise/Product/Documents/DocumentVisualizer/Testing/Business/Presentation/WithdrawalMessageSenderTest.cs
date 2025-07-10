using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Macros;
using CargoWise.Types;
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
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class WithdrawalMessageSenderTest : TestCaseWithFactory
	{
		#region TestResendMessageAfterRejection

		[ExpectNoExceptions]
		public void TestResendMessageAfterRejection()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "ABC123";

			var recipient = Factory.NewWithValidTestData<OrgHeader>();
			recipient.OH_Code = "TESTORG";

			consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;
			SetComms(recipient);

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			Factory.Save();

			storage = consol.LoadOrCreateDocumentData("xxx");

			const string documentName = "Shipping Instruction";

			AddEvents(storage,
				documentName,
				new[]
				{
					Events.DataExport,
					Events.MessageSent
				});

			AddEvents(storage,
				documentName,
				new[]
				{
					Events.MessageAccepted
				});

			AddEvents(storage,
				documentName,
				new[]
				{
					Events.MessageWithdrawCancelRequest
				});

			AddEvents(storage,
				documentName,
				new[]
				{
					Events.InterchangeRejected
				});

			var dex = storage
				.Logs.GetAllLogs()
				.Cast<StmALog>()
				.FirstOrDefault(log => log.SL_SE_NKEvent == Events.DataExportCode);

			var edimessage = Factory.New<IXmlEDIMessage>();
			edimessage.EM_MessageText =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">DEM</DataProvider>
        <Key>ABC123</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Shipping Instruction</DocumentName>
        <IsSystemDefined>false</IsSystemDefined>
        <Purpose></Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <Company>
          <Code>DEM</Code>
          <Country Name=""United States"">US</Country>
          <Name>Demo Company</Name>
        </Company>
        <EventBranch Name=""DEMHQ"">DEM</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate></TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
        <RecipientRoleCollection>
          <RecipientRole Description=""Carrier"">CAR</RecipientRole>
        </RecipientRoleCollection>
      </Workflow>
    </DataContext>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
  </Shipment>
</UniversalShipment>";

			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = dex.PK;
			pivot.XX_Relation1TableCode = dex.TablePrefix;

			pivot.XX_Relation2ID = edimessage.PK;
			pivot.XX_Relation2TableCode = ((BusinessObject)edimessage).TablePrefix;

			pivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var worksheet = DummyWorksheet.Parse(
$@"#Config:Name=""{documentName}"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
					new MacroScope(),
					Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

				var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);

				dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
				dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

				var documentData = dataObject.MakeDynamic();

				var document = new DummyDocument();
				document.Data = documentData;

				var notifications = new Moq.Mock<IUserNotificationService>();
				Expression<Func<IUserNotificationService, string>> queryUserResponseExpression =
					s => s.QueryUserResponse(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>(), Moq.It.IsAny<int>());
				Expression<Action<IUserNotificationService>> showMessageExpression = s => s.ShowMessage("Message withdrawal has been sent.", "Sending Withdraw/Cancel Request");
				notifications.Setup(queryUserResponseExpression).Returns("withdrawal reason");
				notifications.Setup(showMessageExpression);

				var messageWithdrawalEventFired = false;

				var broker = new EventBroker();
				broker
					.GetEvent<MessageWithdrawalSentEvent>()
					.Subscribe(e => messageWithdrawalEventFired = true);

				var services = new ServiceContainer();
				services.Register<IUserNotificationService>(notifications.Object);
				services.Register<IEventBroker>(broker);

				var sender = new WithdrawalMessageSender(messageInstructions, document, storage, services);
				var parameters = WithdrawalMessageSender.Parameters.New(null);
				sender.Send(parameters);

				notifications.Verify(showMessageExpression, Moq.Times.Once);
				AssertEquals("MessageWithdrawalSentEvent fired", true, messageWithdrawalEventFired);
			}
		}

		void AddEvents(IStmALogParent logParent, string documentName, IEnumerable<ZArchitecture.Business.Event> events)
		{
			foreach (var @event in events)
			{
				logParent.Logs.AddNew(@event,
					ZDateTimeOffset.Now,
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));
			}

			Factory.Save();
			Thread.Sleep(10);
		}

		#endregion

		#region TestSendMessage_AddActionPurpose

		public void TestSendMessage_AddActionPurpose()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "ABC123";

			PrepareSentUXml(consol);

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var notifications = new Moq.Mock<IUserNotificationService>();

			notifications.Setup(s => s.QueryUserResponse(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>(), Moq.It.IsAny<int>())).Returns("response");

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var sender = new WithdrawalMessageSender(messageInstructions, document, storage, services);
			var parameters = WithdrawalMessageSender.Parameters.New(null);
			sender.Send(parameters);

			var log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageWithdrawCancelRequestCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			AssertNotNull("MWR event sent found", log);
			AssertEquals("MWR log reference", "|DEP=Carrier|MST=Shipping Instruction|RES=response", log.SL_Reference);

			log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.DataExportCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>ABC123</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Shipping Instruction</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>WTH</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose>WTH</ActionPurpose>
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
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>ReasonForMessageCancellation</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteText>response</NoteText>
      </Note>
    </NoteCollection>
  </Shipment>
</UniversalShipment>";

			var message = Factory.Load<IXmlEDIMessage>(new ZQuery()).Where(m => m.Interchange != null).First();
			var interchange = message.Interchange;

			var xml = XMLMessageTestHelper.GetXml(log);

			AssertMultilineASCIIEquals("sent xml", string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), xml.ToString());
			AssertEquals(string.Empty, XMLMessageTestHelper.ValidateXMLAgainstSchema((IDataObject)document.Data.Value, xml));
		}

		#endregion

		#region TestSendMessage_AddNoteCollectionAndReason

		public void TestSendMessage_AddNoteCollectionAndReason()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "ABC123";

			PrepareSentUXml(consol);

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var notifications = new Moq.Mock<IUserNotificationService>();

			notifications.Setup(s => s.QueryUserResponse(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>(), Moq.It.IsAny<int>())).Returns("Withdrawal reason");

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var sender = new WithdrawalMessageSender(messageInstructions, document, storage, services);
			var parameters = WithdrawalMessageSender.Parameters.New(null);
			sender.Send(parameters);

			var log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageWithdrawCancelRequestCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			AssertNotNull("MWR event sent found", log);
			AssertEquals("MWR log reference", "|DEP=Carrier|MST=Shipping Instruction|RES=Withdrawal reason", log.SL_Reference);

			log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.DataExportCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>ABC123</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Shipping Instruction</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>WTH</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose>WTH</ActionPurpose>
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
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>ReasonForMessageCancellation</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteText>Withdrawal reason</NoteText>
      </Note>
    </NoteCollection>
  </Shipment>
</UniversalShipment>";

			var message = Factory.Load<IXmlEDIMessage>(new ZQuery()).Where(m => m.Interchange != null).First();
			var interchange = message.Interchange;

			var xml = XMLMessageTestHelper.GetXml(log);

			AssertMultilineASCIIEquals("sent xml", string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), xml.ToString());
			AssertEquals(string.Empty, XMLMessageTestHelper.ValidateXMLAgainstSchema((IDataObject)document.Data.Value, xml));
		}

		#endregion

		#region TestSendMessage_WithUserSelectedCodeAndDescriptionWithdrawalReason

		public void TestSendMessage_WithUserSelectedCodeAndDescriptionWithdrawalReason()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "ABC123";

			PrepareSentUXml(consol, "Test Document");

			var worksheet = DummyWorksheet.Parse(
@"#Config
Name=""Test Document""
EHubClientID=""Zoom Zoom""
MessageRecipient=""Carrier""
RequireMessageAmendmentReason=true
MessageWithdrawalOptions=[[""C1"", ""Vessel Not acted upon""], [""C2"", ""Dual Clearance""], [""C3"", ""Cancelling Order""], [""C4"", ""Goods Short Shipped""]]
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var notifications = new Moq.Mock<IUserNotificationService>();

			ICodeDescription userSelectedCodeDescription = new DummyCodeDescription
			{
				Code = "C3",
				Description = "Cancelling Order"
			};

			notifications.Setup(s => s.QueryUserResponse(Moq.It.IsNotNull<string>(), Moq.It.IsNotNull<string>(), Moq.It.IsNotNull<ICodeDescriptionPairList>())).Returns(userSelectedCodeDescription);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(),
				Array.Empty<IMacroLibrary>().CreateContext(), "Test Document");

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var sender = new WithdrawalMessageSender(messageInstructions, document, storage, services);
			var parameters = WithdrawalMessageSender.Parameters.New(null);
			sender.Send(parameters);

			var log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageWithdrawCancelRequestCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			AssertNotNull("MWR event sent found", log);
			AssertEquals("MWR log reference", "|DEP=Carrier|MST=Test Document|RES=C3 - Cancelling Order", log.SL_Reference);

			log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.DataExportCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>ABC123</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Test Document</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>WTH</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose>WTH</ActionPurpose>
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
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>ReasonForMessageCancellation</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteText>C3 - Cancelling Order</NoteText>
      </Note>
    </NoteCollection>
  </Shipment>
</UniversalShipment>";

			var message = Factory.Load<IXmlEDIMessage>(new ZQuery()).Where(m => m.Interchange != null).First();
			var interchange = message.Interchange;

			var xml = XMLMessageTestHelper.GetXml(log);

			AssertMultilineASCIIEquals("sent xml", string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), xml.ToString());
			AssertEquals(string.Empty, XMLMessageTestHelper.ValidateXMLAgainstSchema((IDataObject)document.Data.Value, xml));
		}

		#endregion

		#region TestSendMessage_WithCustomWithdrawalReason

		public void TestSendMessage_WithCustomWithdrawalReason()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "ABC123";

			PrepareSentUXml(consol, "Test Document");

			var worksheet = DummyWorksheet.Parse(
@"#Config
Name=""Test Document""
EHubClientID=""Zoom Zoom""
MessageRecipient=""Carrier""
RequireMessageAmendmentReason=true
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var notifications = new Moq.Mock<IUserNotificationService>();

			ICodeDescription userSelectedCodeDescription = new DummyCodeDescription
			{
				Code = "C3",
				Description = "Cancelling Order"
			};

			notifications.Setup(s => s.QueryUserResponse(Moq.It.IsNotNull<string>(), Moq.It.IsNotNull<string>(), Moq.It.IsNotNull<ICodeDescriptionPairList>())).Returns(userSelectedCodeDescription);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(),
				Array.Empty<IMacroLibrary>().CreateContext(), "Test Document");

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var messageWithdrawalReason = new
			{
				Reason1 = "reason1",
				Reason2 = "reason2"
			};

			Func<IDataObject, object, bool> populateMessageWithdrawalReason = (dataObjectToSend, reason) =>
			{
				AssertEquals("Custom ammendment reason has been passed to PopulateMessageWithdrawalReason", messageWithdrawalReason, reason);

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
					Fail("Incorrect DataObject has been passed to PopulateMessageWithdrawalReason");
				}

				return true;
			};

			var messagingExtensions = new Mock<IMessagingExtensions>();
			var customWithdrawalSupporter = messagingExtensions.As<ICustomMessageWithdrawalSupporter>();

			messagingExtensions.Setup(m => m.ContinueWithSendingMessageWithdrawal(It.IsAny<IUserNotifications>())).Returns(true);
			customWithdrawalSupporter.Setup(m => m.GetMessageWithdrawalReason()).Returns(messageWithdrawalReason);
			customWithdrawalSupporter.Setup(m => m.PopulateMessageWithdrawalReason(It.IsNotNull<IDataObject>(), It.IsNotNull<object>())).Returns(populateMessageWithdrawalReason);

			var dummySupporter = new DummyConsolVisualizableDocumentSupporter();

			using (dummySupporter.Activate(messagingExtensions.Object))
			{
				var sender = new WithdrawalMessageSender(messageInstructions, document, storage, services);
				var parameters = WithdrawalMessageSender.Parameters.New(null);

				Assert("message withdrawal has been sent", sender.Send(parameters));
			}

			var log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageWithdrawCancelRequestCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			AssertNotNull("MWR event sent found", log);
			AssertEquals("MWR log reference", "|DEP=Carrier|MST=Test Document|RES={ Reason1 = reason1, Reason2 = reason2 }", log.SL_Reference);

			log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.DataExportCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>ABC123</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Test Document</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>WTH</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose>WTH</ActionPurpose>
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
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
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

			var message = Factory.Load<IXmlEDIMessage>(new ZQuery()).Where(m => m.Interchange != null).First();
			var interchange = message.Interchange;

			var xml = XMLMessageTestHelper.GetXml(log);

			AssertMultilineASCIIEquals("sent xml", string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), xml.ToString());
			AssertEquals(string.Empty, XMLMessageTestHelper.ValidateXMLAgainstSchema((IDataObject)document.Data.Value, xml));
		}

		public void TestSendMessage_WithCustomWithdrawalReason_Cancel()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "ABC123";

			PrepareSentUXml(consol, "Test Document");

			var worksheet = DummyWorksheet.Parse(
@"#Config
Name=""Test Document""
EHubClientID=""Zoom Zoom""
MessageRecipient=""Carrier""
RequireMessageAmendmentReason=true
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var notifications = new Moq.Mock<IUserNotificationService>();

			ICodeDescription userSelectedCodeDescription = new DummyCodeDescription
			{
				Code = "C3",
				Description = "Cancelling Order"
			};

			notifications.Setup(s => s.QueryUserResponse(Moq.It.IsNotNull<string>(), Moq.It.IsNotNull<string>(), Moq.It.IsNotNull<ICodeDescriptionPairList>())).Returns(userSelectedCodeDescription);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(),
				Array.Empty<IMacroLibrary>().CreateContext(), "Test Document");

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			object messageWithdrawalReason = null;

			Func<IDataObject, object, bool> populateMessageWithdrawalReason = (dataObjectToSend, reason) =>
			{
				Fail("should not PopulateMessageWithdrawalReason as no reason was returned");
				return false;
			};

			var messagingExtensions = new Mock<IMessagingExtensions>();
			var customWithdrawalSupporter = messagingExtensions.As<ICustomMessageWithdrawalSupporter>();

			messagingExtensions.Setup(m => m.ContinueWithSendingMessageWithdrawal(It.IsAny<IUserNotifications>())).Returns(true);
			customWithdrawalSupporter.Setup(m => m.GetMessageWithdrawalReason()).Returns(messageWithdrawalReason);
			customWithdrawalSupporter.Setup(m => m.PopulateMessageWithdrawalReason(It.IsNotNull<IDataObject>(), It.IsNotNull<object>())).Returns(populateMessageWithdrawalReason);

			var dummySupporter = new DummyConsolVisualizableDocumentSupporter();

			using (dummySupporter.Activate(messagingExtensions.Object))
			{
				var sender = new WithdrawalMessageSender(messageInstructions, document, storage, services);
				var parameters = WithdrawalMessageSender.Parameters.New(null);

				Assert("message withdrawal has not been sent", !sender.Send(parameters));
			}
		}

		#endregion

		#region TestSendMessage_WithoutResponse

		[ExpectNoExceptions]
		public void TestSendMessage_WithoutResponse()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "ABC123";

			var recipient = Factory.NewWithValidTestData<OrgHeader>();
			recipient.OH_Code = "TESTORG";

			consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;
			SetComms(recipient);

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			Factory.Save();

			storage = consol.LoadOrCreateDocumentData("xxx");

			const string documentName = "Shipping Instruction";

			AddEvents(storage,
				documentName,
				new[]
				{
					Events.DataExport,
					Events.MessageSent
				});

			AddEvents(storage,
				documentName,
				new[]
				{
					Events.MessageAccepted
				});

			AddEvents(storage,
				documentName,
				new[]
				{
					Events.MessageWithdrawCancelRequest
				});

			var dex = storage
				.Logs.GetAllLogs()
				.Cast<StmALog>()
				.FirstOrDefault(log => log.SL_SE_NKEvent == Events.DataExportCode);

			var edimessage = Factory.New<IXmlEDIMessage>();
			edimessage.EM_MessageText =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">DEM</DataProvider>
        <Key>ABC123</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Shipping Instruction</DocumentName>
        <IsSystemDefined>false</IsSystemDefined>
        <Purpose></Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <Company>
          <Code>DEM</Code>
          <Country Name=""United States"">US</Country>
          <Name>Demo Company</Name>
        </Company>
        <EventBranch Name=""DEMHQ"">DEM</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate></TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
        <RecipientRoleCollection>
          <RecipientRole Description=""Carrier"">CAR</RecipientRole>
        </RecipientRoleCollection>
      </Workflow>
    </DataContext>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
  </Shipment>
</UniversalShipment>";

			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = dex.PK;
			pivot.XX_Relation1TableCode = dex.TablePrefix;

			pivot.XX_Relation2ID = edimessage.PK;
			pivot.XX_Relation2TableCode = ((BusinessObject)edimessage).TablePrefix;

			pivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var worksheet = DummyWorksheet.Parse(
$@"#Config:Name=""{documentName}"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
					new MacroScope(),
					Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

				var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2011_11);

				dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
				dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

				var documentData = dataObject.MakeDynamic();

				var document = new DummyDocument();
				document.Data = documentData;

				var notifications = new Moq.Mock<IUserNotificationService>();

				notifications.Setup(s => s.ShowMessage("Message can be withdrawn only after you have received a response to the previous message.", "Sending Withdraw/Cancel Request"));
				notifications.Setup(s => s.ShowMessage("Message withdrawal has been sent.", "Sending Withdraw/Cancel Request"));

				var services = new ServiceContainer();
				services.Register<IUserNotificationService>(notifications.Object);
				services.Register<IEventBroker>(new EventBroker());

				var sender = new WithdrawalMessageSender(messageInstructions, document, storage, services);
				var parameters = WithdrawalMessageSender.Parameters.New(null);
				sender.Send(parameters);

				notifications.Verify(s => s.ShowMessage("Message can be withdrawn only after you have received a response to the previous message.", "Sending Withdraw/Cancel Request"), Moq.Times.Once);
				notifications.Verify(s => s.ShowMessage("Message withdrawal has been sent.", "Sending Withdraw/Cancel Request"), Moq.Times.Never);
			}
		}

		#endregion

		#region TestSendMessageAndThenWithdrawal

		public void TestSendMessageAndThenWithdrawal()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "ABC123";

			PrepareSentUXml(consol);

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":XmlNamespace=""http://www.google.com/ShippingInstruction/1"":
# End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var notifications = new Moq.Mock<IUserNotificationService>();

			notifications.Setup(s => s.QueryUserResponse(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>(), Moq.It.IsAny<int>())).Returns("withdrawal reason");

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(new EventBroker());

			var sender = new WithdrawalMessageSender(messageInstructions, document, storage, services);

			var map = new MacroMap(new Dictionary<string, object>
			{
				[WithdrawalMessageSender.Parameters.SentCurrentDocumentDataName] = true
			});

			var parameters = WithdrawalMessageSender.Parameters.New(map);

			sender.Send(parameters);

			var log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.MessageWithdrawCancelRequestCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			AssertNotNull("MWR event sent found", log);
			AssertEquals("MWR log reference", "|DEP=Carrier|MST=Shipping Instruction|RES=withdrawal reason", log.SL_Reference);

			log = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.DataExportCode)
				.OrderByDescending(l => l.SL_EventTime)
				.FirstOrDefault();

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.google.com/ShippingInstruction/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>ABC123</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Shipping Instruction</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>WTH</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose>WTH</ActionPurpose>
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
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>ReasonForMessageCancellation</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteText>withdrawal reason</NoteText>
      </Note>
    </NoteCollection>
  </Shipment>
</UniversalShipment>";

			var message = Factory.Load<IXmlEDIMessage>(new ZQuery()).Where(m => m.Interchange != null).First();
			var interchange = message.Interchange;

			var xml = XMLMessageTestHelper.GetXml(log);

			AssertMultilineASCIIEquals("sent xml", string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), xml.ToString());
			AssertEquals(string.Empty, XMLMessageTestHelper.ValidateXMLAgainstSchema((IDataObject)document.Data.Value, xml));
		}

		#endregion

		#region Implementation

		DummyDocument document;
		VisualizerDocumentData storage;

		void PrepareSentUXml(BusinessObject consol, string documentName = "Shipping Instruction")
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);

			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var documentData = dataObject.MakeDynamic();

			document = new DummyDocument();
			document.Data = documentData;

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			storage = consol.LoadOrCreateDocumentData("xxx");

			var recipient = Factory.NewWithValidTestData<OrgHeader>();
			recipient.OH_Code = "TESTORG";

			consol[JobConsolSchema.JK_OA_ShippingLineAddress] = recipient.MainAddress.PK;

			SetComms(recipient);

			Factory.Save();

			var logParent = (IStmALogParent)storage;

			logParent.Logs.AddNew(Events.MessageSent, $"|DEP=Carrier|MST={documentName}", ZDateTimeOffset.Now, false);
			var dex = logParent.Logs.AddNew(Events.DataExport, ZDateTimeOffset.Now, false);

			var edimessage = Factory.New<IXmlEDIMessage>();
			edimessage.EM_MessageText = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">DEM</DataProvider>
        <Key>ABC123</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Test Document</DocumentName>
        <IsSystemDefined>false</IsSystemDefined>
        <Purpose></Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>DEM</Code>
          <Country Name=""United States"">US</Country>
          <Name>Demo Company</Name>
        </Company>
        <EventBranch Name=""DEMHQ"">DEM</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate></TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
        <RecipientRoleCollection>
          <RecipientRole Description=""Carrier"">CAR</RecipientRole>
        </RecipientRoleCollection>
      </Workflow>
    </DataContext>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
  </Shipment>
</UniversalShipment>";

			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = dex.PK;
			pivot.XX_Relation1TableCode = dex.TablePrefix;

			pivot.XX_Relation2ID = edimessage.PK;
			pivot.XX_Relation2TableCode = ((BusinessObject)edimessage).TablePrefix;

			pivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();
			Thread.Sleep(10);

			storage.Logs.AddNew(Events.MessageAccepted,
				ZDateTimeOffset.Now,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			Factory.Save();
		}

		void SetComms(OrgHeader recipient)
		{
			var ediComs = recipient.EDICommunicationsModes.AddNew();
			ediComs.EK_Module = "DOC";
			ediComs.EK_CommsDirection = "TRX";
			ediComs.EK_FileFormat = "XUS";
			ediComs.EK_CommunicationsTransport = "HUB";
			ediComs.EK_Destination = "ABC";
		}

		#endregion
	}
}
