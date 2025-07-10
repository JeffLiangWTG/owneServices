using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.DocumentVisualizer.Business.XmlNamespaceReplacer;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class UXmlSenderTest : TestCaseWithFactory
	{
		#region TestSend_EDICommsSetup_SendToEHub

		public void TestSend_EDICommsSetup_SendToEHub()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };

			var notifications = new DummyNotifications();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var data = dataObject.MakeDynamic();

			var document = new Mock<IDocument>();
			var deliveryService = new Mock<IDocumentDeliveryService>();

			document.Setup(d => d.Data).Returns(data);

			deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

			var parameters = new UXmlSender.Parameters
			{
				Instructions = messageInstructions,
				Document = document.Object,
				DocumentData = documentData,
				DeliveryService = deliveryService.Object
			};

			var sender = new UXmlSender(parameters);

			Assert("message has been sent", sender.Send(notifications));

			deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Never);

			AssertContainsExactElementsInAnyOrder("no notifications",
				Array.Empty<string>(),
				notifications.Notifications.Select(n => n.Message));

			var messageSent = GetLastSentMessage();

			AssertEquals("Interchange Receiver", "Shipping_Instruction", messageSent.Interchange.EI_To);
			AssertEquals("Interchange Status", EDIInterchangeStatusList.Codes.eHubQueued, messageSent.Interchange.EI_Status);
			AssertEquals("Interchange TransportType", EDIInterchangeTransportTypeList.Codes.eHub, messageSent.Interchange.EI_TransportType);

			AssertEquals("Message Direction", EDICommunicationsModeCommsDirectionList.Codes.Transmit, messageSent.EM_ReceiveTransmit);
			AssertEquals("Message Type", EDIInterchangeTypeList.Codes.XDC, messageSent.EM_MessageType);
			AssertEquals("Message Sub Type", EDIMessageSubTypeList.Codes.XmlUniversalShipment, messageSent.EM_MessageSubType);

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>";

			AssertUXmlExcludingDataContext(messageSent, expectedXml);

			var messageSentLog = documentData
				.Logs
				.Find(log => log.SL_SE_NKEvent == Events.MessageSentCode)
				.FirstOrDefault();

			AssertNotNull("MSN event has been created", messageSentLog);
			AssertContainsExactElementsInAnyOrder("MSN event paremeters", new[]
			{
				"MST|Shipping Instruction",
				"DEP|Carrier"
			},
			messageSentLog.Parameters.Select(p => $"{p.Key}|{p.Value}"));
		}

		#endregion

		#region TestSend_EDICommsSetup_SendToEAdaptor

		public void TestSend_EDICommsSetup_SendToEAdaptor()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Module = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			mode.EK_FileFormat = WorkflowTriggerActionTypeConstants.Codes.SendFormBuilderXml;
			mode.EK_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;
			mode.EK_Destination = "zzz";

			Factory.Save();

			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };

			var notifications = new DummyNotifications();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Shipping Instruction");

			var data = dataObject.MakeDynamic();

			var document = new Mock<IDocument>();
			var deliveryService = new Mock<IDocumentDeliveryService>();

			document.Setup(d => d.Data).Returns(data);

			var communicationSettings = new EDICommunicationSettings(
				new ZArchitecture.Core.CodeDescriptionPair("XXX", "Notify Party"),
				new ZArchitecture.Core.CodeDescriptionPair("AAA", "As per bla"),
				new[] { mode });

			deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications))).Returns(communicationSettings);

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var parameters = new UXmlSender.Parameters
				{
					Instructions = messageInstructions,
					Document = document.Object,
					DocumentData = documentData,
					DeliveryService = deliveryService.Object
				};

				var sender = new UXmlSender(parameters);

				Assert("message has been sent", sender.Send(notifications));

				deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Once);
			}

			AssertContainsExactElementsInAnyOrder("no notifications",
				Array.Empty<string>(),
				notifications.Notifications.Select(n => n.Message));

			var messageSent = GetLastSentMessage();

			AssertEquals("Interchange Receiver", "zzz", messageSent.Interchange.EI_To);
			AssertEquals("Interchange Status", EDIInterchangeStatusList.Codes.eAdaptorQueued, messageSent.Interchange.EI_Status);
			AssertEquals("Interchange TransportType", EDIInterchangeTransportTypeList.Codes.eAdaptor, messageSent.Interchange.EI_TransportType);

			AssertEquals("Message Direction", EDICommunicationsModeCommsDirectionList.Codes.Transmit, messageSent.EM_ReceiveTransmit);
			AssertEquals("Message Type", EDIInterchangeTypeList.Codes.XDC, messageSent.EM_MessageType);
			AssertEquals("Message Sub Type", EDIMessageSubTypeList.Codes.XmlUniversalShipment, messageSent.EM_MessageSubType);

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>";

			AssertUXmlExcludingDataContext(messageSent, expectedXml);

			var messageSentLog = documentData
				.Logs
				.Find(log => log.SL_SE_NKEvent == Events.MessageSentCode)
				.FirstOrDefault();

			AssertNotNull("MSN event has been created", messageSentLog);
			AssertContainsExactElementsInAnyOrder("MSN event paremeters", new[]
			{
				"MST|Shipping Instruction",
				"DEP|Notify Party"
			},
			messageSentLog.Parameters.Select(p => $"{p.Key}|{p.Value}"));
		}

		#endregion

		#region TestSend_EDICommsSetup_NoSettingsFound

		public void TestSend_EDICommsSetup_NoSettingsFound()
		{
			Factory.Save();

			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };

			var notifications = new DummyNotifications();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "TEST");

			var data = dataObject.MakeDynamic();

			var document = new Mock<IDocument>();
			var deliveryService = new Mock<IDocumentDeliveryService>();

			document.Setup(d => d.Data).Returns(data);

			var communicationSettings = new EDICommunicationSettings(
				new ZArchitecture.Core.CodeDescriptionPair("XXX", "Notify Party"),
				new ZArchitecture.Core.CodeDescriptionPair("APP", "As per bla"),
				Enumerable.Empty<IEDICommunicationsMode>());

			deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications))).Returns(communicationSettings);

			var parameters = new UXmlSender.Parameters
			{
				Instructions = messageInstructions,
				Document = document.Object,
				DocumentData = documentData,
				DeliveryService = deliveryService.Object
			};

			var sender = new UXmlSender(parameters);

			Assert("message has not been sent", !sender.Send(notifications));

			deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Once);

			AssertContainsExactElementsInAnyOrder("no notifications",
				new[] { "No communications modes found for Client ID []." },
				notifications.Notifications.Select(n => n.Message));

			var messageSent = GetLastSentMessage();

			AssertNull("No EDIMessage in db", messageSent);

			var messageSentLog = documentData
				.Logs
				.Find(log => log.SL_SE_NKEvent == Events.MessageSentCode)
				.FirstOrDefault();

			AssertNull("MSN event has not been created", messageSentLog);
		}

		#endregion

		#region TestSend_NoOverride

		public void TestSend_NoOverride_NoMessageFilter() => AssertSend_NoOverride();

		public void TestSend_NoOverride_WithMessageFilter()
		{
			var filter = Factory.New<EDIMessageContentFilter>();
			filter.ECF_Name = "Exclude addresses";
			filter.ECF_FilterType = "EXC";

			var filterLine = filter.UniversalShipment.Lines.AddNew();
			filterLine.SchemaElement = nameof(Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment.OrganizationAddressCollection);

			var appQuery = new ZQuery(EDIMessagePurposeSchema.EMP_Code, "APP");
			var app = Factory.LoadTop1<EDIMessagePurpose>(appQuery);
			AssertNotNull("Loaded system defined 'APP' message purpose", app);

			app.EMP_ECF_Filter = filter.PK;

			Factory.Save();

			AssertSend_NoOverride();
		}

		void AssertSend_NoOverride()
		{
			var dataObject = GetNewShipmentDataObject(consol);

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "ShippingLineAddress",
				Country = new UniversalDataBuss.DataObjects.Universal.Country
				{
					Code = "AU",
					Name = "Australia"
				}
			};

			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				orgAddress
			});

			Factory.Save();

			var notifications = new DummyNotifications();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "TEST");

			var data = dataObject.MakeDynamic();

			var document = new Mock<IDocument>();
			var deliveryService = new Mock<IDocumentDeliveryService>();

			document.Setup(d => d.Data).Returns(data);

			deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

			var parameters = new UXmlSender.Parameters
			{
				Instructions = messageInstructions,
				Document = document.Object,
				DocumentData = documentData,
				DeliveryService = deliveryService.Object
			};

			var sender = new UXmlSender(parameters);

			sender.Send(notifications);

			deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Never);

			AssertEquals(0, notifications.Notifications.Count);

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <Country Name=""Australia"">AU</Country>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			var messageSent = messages[0];

			AssertUXmlExcludingDataContext(messageSent, expectedXml);
		}

		#endregion

		#region TestSendWithBadFactory

		public void TestSendWithBadFactory()
		{
			var dataObject = GetNewShipmentDataObject(consol);

			Factory.Save();
			Globals.IsUserInteractive = true;
			documentData.Save();

			var notifications = new DummyNotifications();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document"":EHubClientID="""":MessageRecipient=""Carrier"":
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "TEST");

			var data = dataObject.MakeDynamic();

			var document = new Mock<IDocument>();
			var deliveryService = new Mock<IDocumentDeliveryService>();

			document.Setup(d => d.Data).Returns(data);

			var communications = new MockEDICommunicationsSettings();
			deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications))).Returns<IBusiness, INotifications>((f, g) => communications);

			var parameters = new UXmlSender.Parameters
			{
				Instructions = messageInstructions,
				Document = document.Object,
				DocumentData = documentData,
				DeliveryService = deliveryService.Object
			};

			var sender = new UXmlSender(parameters);

			sender.Send(notifications);

			deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Once);

			AssertEquals(0, notifications.Notifications.Count);

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
		}

		internal class MockEDICommunicationsSettings : IEDICommunicationSettings
		{
			public MockEDICommunicationsSettings()
			{
			}

			public ICodeDescription Recipient => new ZArchitecture.Core.CodeDescriptionPair("foor", "basr");

			public ICodeDescription Purpose => new ZArchitecture.Core.CodeDescriptionPair("foo", "bar");

			public IEnumerable<IEDICommunicationsMode> CommunicationsModes
			{
				get { yield return new MockEDICommunicationsMode(); }
			}
		}

		internal class MockEDICommunicationsMode : IEDICommunicationsMode
		{
			public ZGuid EK_ECC_CommunicationPartyConfig { get; set; }

			public ZString EK_CommunicationsTransport => "UDB";

			public ZString EK_Destination => "somewhere";

			public ZString EK_FileFormat => "format";

			public ZString EK_Filename => "file";

			public ZDateTime EK_LastFailed { get; set; }

			public ZString EK_LocalPartyVanID => "str";

			public ZString EK_MessagePurpose => "foobar";

			public ZString EK_RelatedPartyVanID => "bar";

			public ZString EK_ServerAddressSubject => "foo";

			public ZString EK_LoginName => "username";

			public ZString EK_Password => "password";

			public ZInt EK_PortNumber => 123;

			public ZBool EK_PublishInternalMilestones => true;

			public IOrgHeader Organisation => null;
		}
		#endregion

		#region TestSendAdditionalDocuments

		public void TestSendAdditionalDocuments()
		{
			using (Factory.AddDisposableService())
			{
				var bizObj = Factory.New<DummyWithUXmlSupport>();
				var documentData = bizObj.LoadOrCreateDocumentData("zzz");
				bizObj.Z0_AnotherNumber = 123;
				bizObj.SubType1 = "123";
				bizObj.LoadPort = "AUSYD";
				Factory.Save();

				var notifications = new DummyNotifications();

				var worksheet = DummyWorksheet.Parse(
	@"#Config:Name=""Test Document"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
					new MacroScope(documentData),
					Array.Empty<IMacroLibrary>().CreateContext(), "TEST");

				var document = new Mock<IDocument>();
				var deliveryService = new Mock<IDocumentDeliveryService>();
				var messagingExtensions = new Mock<IMessagingExtensions>();

				var data = new object().MakeDynamic();
				bizObj.MessagingExtensions = messagingExtensions.Object;

				document.Setup(d => d.Data).Returns(data);
				deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

				var parameters = new UXmlSender.Parameters
				{
					Instructions = messageInstructions,
					Document = document.Object,
					DocumentData = documentData,
					DeliveryService = deliveryService.Object
				};

				var sender = new UXmlSender(parameters);
				sender.Send(notifications);

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals("message has been created", 3, messages.Length);

				foreach (var message in messages)
				{
					AssertContains("PopulateDataContext DocumentaryOverride", @"      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>TEST</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>", message.EM_MessageText);
				}
			}
		}

		#endregion

		#region TestAdditionalDocumentsWithCustomNamespace

		public void TestAdditionalDocumentsWithCustomNamespace()
		{
			using (Factory.AddDisposableService())
			{
				var bizObj = Factory.New<DummyWithUXmlSupport>();
				var documentData = bizObj.LoadOrCreateDocumentData("zzz");
				bizObj.Z0_AnotherNumber = 123;
				bizObj.SubType1 = "123";
				bizObj.LoadPort = "AUSYD";
				bizObj.DischargePort = "AUMEL";
				Factory.Save();

				var notifications = new DummyNotifications();

				var worksheet = DummyWorksheet.Parse(
	@"#Config:Name=""Test Document"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
					new MacroScope(documentData),
					Array.Empty<IMacroLibrary>().CreateContext(), "TEST");

				var document = new Mock<IDocument>();
				var deliveryService = new Mock<IDocumentDeliveryService>();

				var data = new DocDataObjects.Address(Factory)
				{
					CompanyName = "WiseTech"
				}.MakeDynamic();
				document.Setup(d => d.Data).Returns(data);

				deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

				var parameters = new UXmlSender.Parameters
				{
					Instructions = messageInstructions,
					Document = document.Object,
					DocumentData = documentData,
					DeliveryService = deliveryService.Object
				};

				var sender = new UXmlSender(parameters);
				sender.Send(notifications);

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals("message has been created", 3, messages.Length);

				AssertContainsExactElementsInAnyOrder(
					new[] {
						"http://www.cargowise.com/Schemas/Universal/2012/11/WiseTech",
						"http://www.cargowise.com/Schemas/Universal/2012/11/CargoWise",
						"http://www.cargowise.com/Schemas/Universal/2012/11/WiseTechNext" },
					new[]
					{
						XDocument.Parse(messages[0].EM_MessageText).Root.GetDefaultNamespace().NamespaceName,
						XDocument.Parse(messages[1].EM_MessageText).Root.GetDefaultNamespace().NamespaceName,
						XDocument.Parse(messages[2].EM_MessageText).Root.GetDefaultNamespace().NamespaceName
					});
			}
		}

		#endregion

		#region TestAdditionalDocumentsWithCustomDocumentaryOverrideDocumentName

		public void TestAdditionalDocumentsWithCustomDocumentaryOverrideDocumentName()
		{
			using (Factory.AddDisposableService())
			{
				var bizObj = Factory.New<DummyWithUXmlSupport>();
				var documentData = bizObj.LoadOrCreateDocumentData("zzz");
				bizObj.Z0_AnotherNumber = 123;
				bizObj.SubType1 = "123";
				bizObj.LoadPort = "AUSYD";
				bizObj.DischargePort = "AUMEL";
				Factory.Save();

				var notifications = new DummyNotifications();

				var worksheet = DummyWorksheet.Parse(
	@"#Config:Name=""Test Document"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
					new MacroScope(documentData),
					Array.Empty<IMacroLibrary>().CreateContext(), "TEST");

				var document = new Mock<IDocument>();
				var deliveryService = new Mock<IDocumentDeliveryService>();

				var data = new DocDataObjects.Address(Factory)
				{
					CompanyName = "WiseTech"
				}.MakeDynamic();
				document.Setup(d => d.Data).Returns(data);

				deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

				var parameters = new UXmlSender.Parameters
				{
					Instructions = messageInstructions,
					Document = document.Object,
					DocumentData = documentData,
					DeliveryService = deliveryService.Object
				};

				var sender = new UXmlSender(parameters);
				sender.Send(notifications);

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals("message has been created", 3, messages.Length);

				var pattern = @"<DataContext>.*?<DocumentaryOverride>.*?<DocumentName>(.*?)<\/DocumentName>.*?<\/DocumentaryOverride>.*?<\/DataContext>";

				AssertContainsExactElementsInAnyOrder(
					new[] {
						"TEST",
						"CargoWise",
						"WiseTechNext" },
					new[]
					{
						Regex.Match(messages[0].EM_MessageText, pattern, RegexOptions.Singleline).Groups[1].Value,
						Regex.Match(messages[1].EM_MessageText, pattern, RegexOptions.Singleline).Groups[1].Value,
						Regex.Match(messages[2].EM_MessageText, pattern, RegexOptions.Singleline).Groups[1].Value,
					});
			}
		}

		#endregion

		#region TestSendWrapperWithCustomNamespace

		public void TestSendWrapperWithCustomNamespace()
		{
			using (Factory.AddDisposableService())
			{
				var bizObj = Factory.New<DummyWithUXmlSupport>();
				var documentData = bizObj.LoadOrCreateDocumentData("zzz");

				var notifications = new DummyNotifications();

				var worksheet = DummyWorksheet.Parse(
	@"#Config:Name=""Test Document"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
					new MacroScope(documentData),
					Array.Empty<IMacroLibrary>().CreateContext(), "TEST");

				var document = new Mock<IDocument>();
				var deliveryService = new Mock<IDocumentDeliveryService>();
				var messagingExtensions = new Mock<IMessagingExtensions>();

				var data = new object().MakeDynamic();
				bizObj.MessagingExtensions = messagingExtensions.Object;

				const string customNamespace = "/CUSTOM/1";

				document.Setup(d => d.Data).Returns(data);

				deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

				messagingExtensions.Setup(ext => ext.GetXmlNamespace()).Returns(customNamespace);

				var parameters = new UXmlSender.Parameters
				{
					Instructions = messageInstructions,
					Document = document.Object,
					DocumentData = documentData,
					DeliveryService = deliveryService.Object
				};

				var sender = new UXmlSender(parameters);

				sender.Send(notifications);

				deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Never);

				messagingExtensions.Verify(ext => ext.GetXmlNamespace(), Times.Once);

				AssertEquals(0, notifications.Notifications.Count);

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals("message has been created", 1, messages.Length);

				var messageXml = XDocument.Parse(messages[0].EM_MessageText);
				var xmlNamespace = messageXml.Root.GetDefaultNamespace();

				AssertEquals("custom namespace has been applied",
					$"http://www.cargowise.com/Schemas/Universal/2012/11{customNamespace}", xmlNamespace.NamespaceName);
			}
		}

		#endregion

		#region TestNamespaceDefinitionOverride

		public void TestNamespaceDefinitionOverride()
		{
			AssertNamespaceDefinitionOverride("/ShippingInstruction/1", "http://www.cargowise.com/Schemas/Universal/2012/11/ShippingInstruction/1");
			AssertNamespaceDefinitionOverride("ShippingInstruction/1", "http://www.cargowise.com/Schemas/Universal/2012/11/ShippingInstruction/1");
			AssertNamespaceDefinitionOverride("http://www.xml.com/ShippingInstruction/1", "http://www.xml.com/ShippingInstruction/1");
			AssertNamespaceDefinitionOverride("http://www.xml.com/你/1", "<UniversalShipment xmlns=\"http://www.xml.com/%E4%BD%A0/1\" version=\"2.0\">");
		}

		public void TestInvalidURIThrowsException()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			var dynamicData = dataObject.MakeDynamic();

			IXmlWriter xmlWriter = new DynamicDataUXmlWriter(dynamicData, "file://c:/directory/filename");
			using (var outputStream = (SubStreamableStream)new MemoryStream())
			{
				AssertExceptionThrown(typeof(InvalidNamespaceException), () => xmlWriter.WriteXML(dataObject, outputStream));
			}
		}

		void AssertNamespaceDefinitionOverride(string namespaceConfig, string expectedNameSpace)
		{
			var dataObject = GetNewShipmentDataObject(consol);
			var dynamicData = dataObject.MakeDynamic();

			IXmlWriter xmlWriter = new DynamicDataUXmlWriter(dynamicData, namespaceConfig);
			var outputStream = (SubStreamableStream)new MemoryStream();

			xmlWriter.WriteXML(dataObject, outputStream);

			var expected = string.Format("<UniversalShipment xmlns=\"{0}\" version=\"2.0\">", expectedNameSpace);
			using (var reader = new StreamReader(outputStream))
			{
				Assert(reader.ReadToEnd().Contains(expectedNameSpace));
			}
		}

		#endregion

		#region TestSend_SimpleOverride

		public void TestSend_SimpleOverride()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var dynamicData = dataObject.MakeDynamic();
			dynamicData.GetDynamicProperty("PortOfLoading.Code").SetValue("AUXXX");

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <DocumentaryOverride>
      <PortOfLoading Name=""Sydney"">AUXXX</PortOfLoading>
    </DocumentaryOverride>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.LoadTop1<IXmlEDIMessage>(new ZQuery());
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
		}

		#endregion

		#region TestSend_OverrideLoadedFromXml

		public void TestSend_OverrideLoadedFromXml_OriginalCollectionIsEmpty()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };

			var dynamicData = dataObject.MakeDynamic();

			dynamicData.MergeDataFromXml(XDocument.Parse(
@"<Entity DataMajorVersion=""2"" DataMinorVersion=""0"">
  <Property Name=""BillOfLadingClauseCollection"">
    <EntityCollection>
      <Items>
        <Entity State=""Added"">
          <Property Name=""Type"">
            <Entity>
              <Property Name=""Code"" NaturalKey=""true"">
                <Value>LOB</Value>
              </Property>
              <Property Name=""Description"">
                <Value>Laden on Board</Value>
              </Property>
            </Entity>
          </Property>
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
</Entity>"));

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <DocumentaryOverride Type=""Addition"">
          <Type Description=""Laden on Board"">LOB</Type>
        </DocumentaryOverride>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.LoadTop1<IXmlEDIMessage>(new ZQuery());
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
		}

		public void TestSend_OverrideLoadedFromXml_OriginalCollectionIsNotEmpty_CollectionDoesNotHaveNaturalKey()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.SetBillOfLadingClauseCollection(() => new List<BillOfLadingClause>
			{
				new BillOfLadingClause
				{
					Type = new CodeDescriptionPair
					{
						Code = "AAA",
						Description = "AAA desc"
					},
					Detail = "detail"
				}
			});

			var dynamicData = dataObject.MakeDynamic();

			dynamicData.MergeDataFromXml(XDocument.Parse(
@"<Entity DataMajorVersion=""2"" DataMinorVersion=""0"">
  <Property Name=""BillOfLadingClauseCollection"">
    <EntityCollection>
      <Items>
        <Entity State=""Added"">
          <Property Name=""Type"">
            <Entity>
              <Property Name=""Code"" NaturalKey=""true"">
                <Value>LOB</Value>
              </Property>
              <Property Name=""Description"">
                <Value>Laden on Board</Value>
              </Property>
            </Entity>
          </Property>
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
</Entity>"));

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""AAA desc"">AAA</Type>
        <Detail>detail</Detail>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <DocumentaryOverride Type=""Addition"">
          <Type Description=""Laden on Board"">LOB</Type>
        </DocumentaryOverride>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.LoadTop1<IXmlEDIMessage>(new ZQuery());
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
		}

		public void TestSend_OverrideLoadedFromXml_OriginalCollectionIsNotEmpty_CollectionHasNaturalKey()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.SetBillOfLadingClauseCollection(() => new List<BillOfLadingClause>
			{
				new BillOfLadingClause
				{
					Type = new CodeDescriptionPair
					{
						Code = "AAA",
						Description = "AAA desc"
					},
					Detail = "detail"
				}
			});

			var dynamicData = dataObject.MakeDynamic();
			"@data.BillOfLadingClauseCollection.SetNaturalKey(\"Type.Code\")".Evaluate(dynamicData, new MetaDataLibrary());

			dynamicData.MergeDataFromXml(XDocument.Parse(
@"<Entity DataMajorVersion=""2"" DataMinorVersion=""0"">
  <Property Name=""BillOfLadingClauseCollection"">
    <EntityCollection>
      <Items>
        <Entity State=""Added"">
          <Property Name=""Type"">
            <Entity>
              <Property Name=""Code"" NaturalKey=""true"">
                <Value>LOB</Value>
              </Property>
              <Property Name=""Description"">
                <Value>Laden on Board</Value>
              </Property>
            </Entity>
          </Property>
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
</Entity>"));

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""AAA desc"">AAA</Type>
        <Detail>detail</Detail>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <DocumentaryOverride Type=""Addition"">
          <Type Description=""Laden on Board"">LOB</Type>
        </DocumentaryOverride>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.LoadTop1<IXmlEDIMessage>(new ZQuery());
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
		}

		public void TestSend_OverrideLoadedFromXml_MergedElementFromOverride()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.SetBillOfLadingClauseCollection(() => new List<BillOfLadingClause>
			{
				new BillOfLadingClause
				{
					Type = new CodeDescriptionPair
					{
						Code = "LOB",
						Description = "original desc"
					},
					Detail = "detail"
				}
			});

			var dynamicData = dataObject.MakeDynamic();
			"@data.BillOfLadingClauseCollection.SetNaturalKey(\"Type.Code\")".Evaluate(dynamicData, new MetaDataLibrary());

			dynamicData.MergeDataFromXml(XDocument.Parse(
@"<Entity DataMajorVersion=""2"" DataMinorVersion=""0"">
  <Property Name=""BillOfLadingClauseCollection"">
    <EntityCollection>
      <Items>
        <Entity State=""Added"">
          <Property Name=""Type"">
            <Entity>
              <Property Name=""Code"" NaturalKey=""true"">
                <Value>LOB</Value>
              </Property>
              <Property Name=""Description"">
                <Value>Laden on Board</Value>
              </Property>
            </Entity>
          </Property>
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
</Entity>"));

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""original desc"">LOB</Type>
        <Detail>detail</Detail>
        <DocumentaryOverride>
          <Type Description=""Laden on Board"">LOB</Type>
        </DocumentaryOverride>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.LoadTop1<IXmlEDIMessage>(new ZQuery());
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
		}

		#endregion

		#region TestSend_SimpleCustomField

		public void TestSend_SimpleCustomField()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var dynamicData = dataObject.MakeDynamic();

			var customField = dynamicData.Properties.GetOrCreate("CustomField", () => 12.34m);

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <DocumentaryOverride>
      <CustomField>12.34</CustomField>
    </DocumentaryOverride>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.LoadTop1<IXmlEDIMessage>(new ZQuery());
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
			AssertDocumentDataHasLog(consol, Events.MessageSentCode);
		}

		#endregion

		#region TestSend_ComplexCustomField

		public void TestSend_ComplexCustomField()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var dynamicData = dataObject.MakeDynamic();

			var contact = new Contact
			{
				Name = "Kookoo"
			};

			var customField = dynamicData.Properties.GetOrCreate("CustomField", () => contact);

			AssertEquals("Name", "Kookoo", customField.GetDynamicProperty("Name").Value);

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = GetLastSentMessage();
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
			AssertDocumentDataHasLog(consol, Events.MessageSentCode);
		}

		#endregion

		#region TestSend_MixedCustomFields

		public void TestSend_MixedCustomFields()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var dynamicData = dataObject.MakeDynamic();

			var contact = new Contact
			{
				Name = "Kookoo"
			};

			var complexCustomField = dynamicData.Properties.GetOrCreate("ComplexCustomField", () => contact);

			AssertEquals("Name", "Kookoo", complexCustomField.GetDynamicProperty("Name").Value);

			var simpleCustomField = dynamicData.Properties.GetOrCreate("SimpleCustomField", () => 12.34m);

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <DocumentaryOverride>
      <SimpleCustomField>12.34</SimpleCustomField>
    </DocumentaryOverride>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.Load<IXmlEDIMessage>(new ZQuery()).Where(m => m.Interchange != null).First();
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
			AssertDocumentDataHasLog(consol, Events.MessageSentCode);
		}

		#endregion

		#region TestSend_FormatCustomFieldValue

		public void TestSend_CustomField_DateTime()
		{
			AssertCustomFieldValue(new DateTime(2015, 7, 25), "2015-07-25T00:00:00");
		}

		public void TestSend_CustomField_ZDateTime()
		{
			AssertCustomFieldValue(new ZDateTime(2015, 7, 25), "2015-07-25T00:00:00");
		}

		public void TestSend_CustomField_ZDate()
		{
			AssertCustomFieldValue(new ZDate(2015, 7, 25), "2015-07-25");
		}

		public void TestSend_CustomField_ZDecimal()
		{
			var originalCultureInfo = Thread.CurrentThread.CurrentCulture;

			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
				AssertCustomFieldValue(new ZDecimal(23.45), "23.45");
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = originalCultureInfo;
			}
		}

		public void TestSend_CustomField_Decimal()
		{
			var originalCultureInfo = Thread.CurrentThread.CurrentCulture;

			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
				AssertCustomFieldValue(23.45m, "23.45");
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = originalCultureInfo;
			}
		}

		void AssertCustomFieldValue(object customFieldValue, string expectedCustomFieldXml)
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var dynamicData = dataObject.MakeDynamic();
			var property = dynamicData.Properties.GetOrCreate("CustomFieldForTest", () => customFieldValue);

			Send(dynamicData, consol);

			var message = GetLastSentMessage();
			var interchange = message.Interchange;

			string expectedXml = string.Format(
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <DocumentaryOverride>
      <CustomFieldForTest>{3}</CustomFieldForTest>
    </DocumentaryOverride>
  </Shipment>
</UniversalShipment>", interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum, expectedCustomFieldXml);

			AssertLastUXmlSentExcludingDataContext(expectedXml);
		}

		#endregion

		#region TestSend_SimpleOverride_FlattenedElements

		public void TestSend_SimpleOverride_FlattenedElements()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var dynamicData = dataObject.MakeDynamic();
			dynamicData.GetDynamicProperty("PortOfLoading.Name").SetValue("Sid knee");

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <DocumentaryOverride>
      <PortOfLoading Name=""Sid knee"">AUSYD</PortOfLoading>
    </DocumentaryOverride>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.LoadTop1<IXmlEDIMessage>(new ZQuery());
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
			AssertDocumentDataHasLog(consol, Events.MessageSentCode);
		}

		#endregion

		#region TestSend_NewComplexProperty

		public void TestSend_NewComplexProperty()
		{
			var dataObject = GetNewShipmentDataObject(consol);

			var dynamicData = dataObject.MakeDynamic();

			var currency = dynamicData.GetDynamicProperty("JobCosting.Currency");

			currency.GetDynamicProperty("Code").SetValue("GCS");
			currency.GetDynamicProperty("Description").SetValue("Galactic Credit Standard");

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <JobCosting>
      <DocumentaryOverride>
        <Currency Description=""Galactic Credit Standard"">GCS</Currency>
      </DocumentaryOverride>
    </JobCosting>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.LoadTop1<IXmlEDIMessage>(new ZQuery());
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
			AssertDocumentDataHasLog(consol, Events.MessageSentCode);
		}

		#endregion

		#region TestSend_AllAddedProperties

		public void TestSend_AllAddedProperties()
		{
			var dataObject = GetNewShipmentDataObject(consol);

			var dynamicData = dataObject.MakeDynamic();

			var prop1 = dynamicData.Properties.GetOrCreate("Prop1", "ab&c", typeof(string));
			var prop2 = dynamicData.Properties.GetOrCreate("Prop2", 2, typeof(int));

			prop2.SetValue(3);

			AssertEquals("prerequisite: prop1.IsOverridden", false, prop1.IsOverriddenIncludingChildren);
			AssertEquals("prerequisite: prop2.IsOverridden", true, prop2.IsOverriddenIncludingChildren);

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <DocumentaryOverride>
      <Prop1>ab&amp;c</Prop1>
      <Prop2>3</Prop2>
    </DocumentaryOverride>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = GetLastSentMessage();
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
			AssertDocumentDataHasLog(consol, Events.MessageSentCode);
		}

		#endregion

		#region TestSend_Collections

		public void TestSend_Collections()
		{
			var dataObject = GetNewShipmentDataObject(consol);

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Country = new UniversalDataBuss.DataObjects.Universal.Country
				{
					Code = "AU",
					Name = "Australia"
				}
			};

			var orgAddress2 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Country = new UniversalDataBuss.DataObjects.Universal.Country
				{
					Code = "US",
					Name = "US and A"
				}
			};

			var orgAddress3 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Country = new UniversalDataBuss.DataObjects.Universal.Country
				{
					Code = "CA",
					Name = "Canada"
				}
			};

			var addressBO = Factory.New<DummyBusinessObject>();

			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				orgAddress,
				orgAddress2,
				orgAddress3
			});

			var linkManager = new UXmlLinkManager();
			((IDataWritingInformationCollector)linkManager).NotifyExported(orgAddress, addressBO);

			var metaDataProvider = new UXmlMetaDataProvider(linkManager);

			var dynamicData = dataObject.MakeDynamic(metaDataProvider);

			var dynamicDataCollection = (IDynamicDataCollection)dynamicData.Properties.GetOrCreate(nameof(dataObject.OrganizationAddressCollection));
			dynamicDataCollection.First().GetDynamicProperty("Country.Name").SetValue("OverriddenCountry");

			dynamicDataCollection.Remove(dynamicDataCollection.Skip(1).First());

			var newElement = dynamicDataCollection.GetOrCreate("TestAddressType");
			newElement.GetDynamicProperty("CompanyName").SetValue("Shipping Company");

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <Country Name=""Australia"">AU</Country>
        <DocumentaryOverride>
          <Country Name=""OverriddenCountry"">AU</Country>
        </DocumentaryOverride>
      </OrganizationAddress>
      <OrganizationAddress>
        <Country Name=""Canada"">CA</Country>
      </OrganizationAddress>
      <OrganizationAddress>
        <DocumentaryOverride Type=""Addition"">
          <AddressType>TestAddressType</AddressType>
          <CompanyName>Shipping Company</CompanyName>
        </DocumentaryOverride>
      </OrganizationAddress>
      <OrganizationAddress>
        <Country Name=""US and A"">US</Country>
        <DocumentaryOverride Type=""Removed""></DocumentaryOverride>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = GetLastSentMessage();
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
			AssertDocumentDataHasLog(consol, Events.MessageSentCode);
		}

		#endregion

		#region TestSend_Collections_AddedElementToNullCollection

		public void TestSend_Collections_AddedElementToNullCollection()
		{
			var dataObject = GetNewShipmentDataObject(consol);

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Country = new UniversalDataBuss.DataObjects.Universal.Country
				{
					Code = "AU",
					Name = "Australia"
				}
			};

			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				orgAddress
			});

			var addressBO = Factory.New<DummyBusinessObject>();

			var linkManager = new UXmlLinkManager();
			((IDataWritingInformationCollector)linkManager).NotifyExported(orgAddress, addressBO);

			var metaDataProvider = new UXmlMetaDataProvider(linkManager);

			var dynamicData = dataObject.MakeDynamic(metaDataProvider);

			var dynamicDataCollection = (IDynamicDataCollection)dynamicData.Properties.GetOrCreate("BillOfLadingClauseCollection");
			dynamicDataCollection.Create(new MacroMap(new Dictionary<string, object>
			{
				{ "Type", new MacroMap(new Dictionary<string, object>
					{
						{ "Code", "RFS" },
						{ "Description", "Received For Shipment" }
					}) },
				{ "Detail", "detail" }
			}));

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <DocumentaryOverride Type=""Addition"">
          <Type Description=""Received For Shipment"">RFS</Type>
          <Detail>detail</Detail>
        </DocumentaryOverride>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <Country Name=""Australia"">AU</Country>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = GetLastSentMessage();
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
			AssertDocumentDataHasLog(consol, Events.MessageSentCode);
		}

		#endregion

		#region TestAddedPropertiesPropogateThroughCollectionsAndComplexTypes

		public void TestAddedPropertiesPropogateThroughCollectionsAndComplexTypes()
		{
			var dataObject = GetNewShipmentDataObject(consol);

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.Country = new UniversalDataBuss.DataObjects.Universal.Country();
			orgAddress.Country.Code = "AU";
			orgAddress.Country.Name = "Australia";

			var addressBO = Factory.New<DummyBusinessObject>();

			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			dataObject.OrganizationAddressCollection.Add(orgAddress);

			var linkManager = new UXmlLinkManager();
			((IDataWritingInformationCollector)linkManager).NotifyExported(orgAddress, addressBO);

			var metaDataProvider = new UXmlMetaDataProvider(linkManager);

			var dynamicData = dataObject.MakeDynamic(metaDataProvider);
			var dynamicDataCollection = (IDynamicDataCollection)dynamicData.Properties.GetOrCreate("OrganizationAddressCollection");

			var dynamicAddress = dynamicDataCollection.First();
			var customProperty = dynamicAddress.Properties.GetOrCreate("CustFieldBlah", "Test123", typeof(string));

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <Country Name=""Australia"">AU</Country>
        <DocumentaryOverride>
          <CustFieldBlah>Test123</CustFieldBlah>
        </DocumentaryOverride>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = GetLastSentMessage();
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
			AssertDocumentDataHasLog(consol, Events.MessageSentCode);
		}

		#endregion

		#region TestSubmissionAndDataVersion

		public void TestSubmissionAndDataVersion()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			var dynamicData = dataObject.MakeDynamic();

			Send(dynamicData, consol);

			AssertLastMessageSentUXmlDocumentaryOverride(1, 1);
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX" });

			AddMessageAcceptedLog();
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA" });

			Send(dynamicData, consol);

			AssertLastMessageSentUXmlDocumentaryOverride(2, 2);
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA", "MSN", "DEX" });

			AddMessageAcceptedLog();
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA", "MSN", "DEX", "MAA" });

			AddWithdrawalRequestedLog();
			AddMessageWithdrawalAcceptedLog();
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA", "MSN", "DEX", "MAA", "MWR", "MWA" });

			Send(dynamicData, consol);
			AssertLastMessageSentUXmlDocumentaryOverride(1, 3);
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA", "MSN", "DEX", "MAA", "MWR", "MWA", "MSN", "DEX" });

			AddMessageAcceptedLog();
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA", "MSN", "DEX", "MAA", "MWR", "MWA", "MSN", "DEX", "MAA" });

			Send(dynamicData, consol);
			AssertLastMessageSentUXmlDocumentaryOverride(2, 4);
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA", "MSN", "DEX", "MAA", "MWR", "MWA", "MSN", "DEX", "MAA", "MSN", "DEX" });

			AddInterchangeRejectedLog();
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA", "MSN", "DEX", "MAA", "MWR", "MWA", "MSN", "DEX", "MAA", "MSN", "DEX", "IRJ" });

			Send(dynamicData, consol);
			AssertLastMessageSentUXmlDocumentaryOverride(2, 5);
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA", "MSN", "DEX", "MAA", "MWR", "MWA", "MSN", "DEX", "MAA", "MSN", "DEX", "IRJ", "MSN", "DEX" });

			AddMessageAcceptedLog();
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA", "MSN", "DEX", "MAA", "MWR", "MWA", "MSN", "DEX", "MAA", "MSN", "DEX", "IRJ", "MSN", "DEX", "MAA" });

			Send(dynamicData, consol);
			AssertLastMessageSentUXmlDocumentaryOverride(3, 6);
			AssertDocumentDataLogs(consol, new[] { "MSN", "DEX", "MAA", "MSN", "DEX", "MAA", "MWR", "MWA", "MSN", "DEX", "MAA", "MSN", "DEX", "IRJ", "MSN", "DEX", "MAA", "MSN", "DEX" });
		}

		void AssertLastMessageSentUXmlDocumentaryOverride(int expectedDataVersion, int expectedSubmissionVersion)
		{
			var message = GetLastSentMessage();

			AssertNotNull("Found sent edimessage", message);

			var xml = XDocument.Parse(message.EM_MessageText);

			var ns = xml.Root.GetDefaultNamespace();
			var docOverride = xml.Descendants(ns + "DocumentaryOverride").First().ToString();

			var purpose = expectedDataVersion > 1
				? "AMD"
				: "ORG";

			var expectedXml =
$@"<DocumentaryOverride xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <DataVersion>{expectedDataVersion}</DataVersion>
  <DocumentName>Test Document</DocumentName>
  <IsSystemDefined>true</IsSystemDefined>
  <Purpose>{purpose}</Purpose>
  <SubmissionVersion>{expectedSubmissionVersion}</SubmissionVersion>
</DocumentaryOverride>";

			AssertMultilineASCIIEquals("DocumentaryOverride", expectedXml, docOverride);
		}

		void AssertDocumentDataLogs(BusinessObject dummy, IEnumerable<string> expectedLogs)
		{
			var documentData = dummy.LoadOrCreateDocumentData("xxx");

			var logs = documentData.Logs.GetAllLogs().Cast<StmALog>().Select(log => log.SL_SE_NKEvent.ToString());

			AssertContainsExactElementsInAnyOrder("Expected DocumentData logs", expectedLogs, logs);
		}

		void AddMessageAcceptedLog()
		{
			documentData.Logs.AddNew(Events.MessageAccepted, ZDateTimeOffset.Now, false, new KeyValuePair<string, string>("MST", "Test Document"));
			Thread.Sleep(10);
			Factory.Save();
		}

		void AddInterchangeRejectedLog()
		{
			documentData.Logs.AddNew(Events.InterchangeRejected, ZDateTimeOffset.Now, false, new KeyValuePair<string, string>("MST", "Test Document"));
			Thread.Sleep(10);
			Factory.Save();
		}

		void AddWithdrawalRequestedLog()
		{
			documentData.Logs.AddNew(Events.MessageWithdrawCancelRequest, ZDateTimeOffset.Now, false, new KeyValuePair<string, string>("MST", "Test Document"));
			Thread.Sleep(10);
			Factory.Save();
		}

		void AddMessageWithdrawalAcceptedLog()
		{
			documentData.Logs.AddNew(Events.MessageWithdrawCancelAccepted, ZDateTimeOffset.Now, false, new KeyValuePair<string, string>("MST", "Test Document"));
			Thread.Sleep(10);
			Factory.Save();
		}

		#endregion

		#region TestResendUnsavedDataSoreDoesNotThrowException

		public void TestSendWithUnsavedDataStoreDoesNotThrowException()
		{
			var dataObject = GetNewShipmentDataObject(consol);

			Factory.Save();

			var dynamicData = dataObject.MakeDynamic();
			var documentData = consol.LoadOrCreateDocumentData("zzz");

			Assert("document data is not in the db", !documentData.IsInDatabase);

			Send(dynamicData, consol, saveDocumentData: false);
			Send(dynamicData, consol, saveDocumentData: false);
		}

		#endregion

		#region TestSendCreatedComplexTypeThatDidNotExistOnTheOriginalDataObject

		public void TestSendCreatedComplexTypeThatDidNotExistOnTheOriginalDataObject()
		{
			var dataObject = GetNewShipmentDataObject(consol);

			var dynamicData = dataObject.MakeDynamic();
			var deliveryMode = dynamicData.GetDynamicProperty(nameof(dataObject.DeliveryMode));

			var code = deliveryMode.GetDynamicProperty(nameof(dataObject.DeliveryMode.Code));
			var description = deliveryMode.GetDynamicProperty(nameof(dataObject.DeliveryMode.Description));

			code.SetValue("XXX");
			description.SetValue("XXX DESC");

			var order = dynamicData.GetDynamicProperty(nameof(dataObject.Order));
			var pickOption = order.GetDynamicProperty(nameof(dataObject.Order.PickOption));

			code = pickOption.GetDynamicProperty(nameof(dataObject.Order.PickOption.Code));
			code.SetValue("AAA");

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <Order>
      <DocumentaryOverride>
        <PickOption>AAA</PickOption>
      </DocumentaryOverride>
    </Order>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <DocumentaryOverride>
      <DeliveryMode Description=""XXX DESC"">XXX</DeliveryMode>
    </DocumentaryOverride>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.LoadTop1<IXmlEDIMessage>(new ZQuery());
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContext(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
		}

		#endregion

		#region TestSendAssemblyMasterShipment

		[TestDate(2016, 12, 21)]
		public void TestSendAssemblyMasterShipment()
		{
			var dataObject = GetNewShipmentDataObject(consol);
			dataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new DataSource
				{
					Key = "CONSOL1",
					Type = "ForwardingConsol"
				}
			};

			var asmShipment = new Shipment(DefaultDataObjectWriterStrategy.Instance)
			{
				DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
				{
					DataSource = new DataSource
					{
						Key = "Shipment1",
						Type = "ForwardingShipment"
					}
				},
				ShipmentType = new CodeDescriptionPair
				{
					Code = "ASM",
					Description = "Assembly Master"
				},
			};
			asmShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>
				{
					new Shipment(DefaultDataObjectWriterStrategy.Instance)
					{
						DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
						{
							DataSource = new DataSource
							{
								Key = "Shipment2",
								Type = "ForwardingShipment"
							}
						},
						ShipmentType = new CodeDescriptionPair
						{
							Code = "STD",
							Description = "Standard"
						}
					}
				});

			dataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			{
				asmShipment
			});

			var dynamicData = dataObject.MakeDynamic();

			const string expectedXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Test Document</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>Shipment1</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>
        <ShipmentType Description=""Assembly Master"">ASM</ShipmentType>
        <SubShipmentCollection>
          <SubShipment>
            <DataContext>
              <DataSource>
                <Key>Shipment2</Key>
                <Type>ForwardingShipment</Type>
              </DataSource>
            </DataContext>
            <ShipmentType Description=""Standard"">STD</ShipmentType>
          </SubShipment>
        </SubShipmentCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

			Send(dynamicData, consol);

			var message = Factory.LoadTop1<IXmlEDIMessage>(new ZQuery());
			var interchange = message.Interchange;

			AssertLastUXmlSentExcludingDataContextWorkflow(string.Format(expectedXml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum));
		}

		#endregion

		#region TestHandleExceptionDuringSaving

		public void TestHandleExceptionDuringSaving()
		{
			using (Factory.AddDisposableService())
			{
				var consol2 = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

				var dataObject = GetNewShipmentDataObject(consol2);

				Factory.Save();

				documentData = consol2.LoadOrCreateDocumentData("xxx");

				var action = new SaveInTransactionActionForThrowingException(consol);
				Factory.SaveInTransactionActions.Add(action);

				var notifications = new DummyNotifications();

				var worksheet = DummyWorksheet.Parse(
					@"#Config:Name=""Test Document"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");

				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(
					worksheetTemplate,
					new MacroScope(documentData),
					Array.Empty<IMacroLibrary>().CreateContext(),
					"Test document");

				var data = dataObject.MakeDynamic();

				var document = new Mock<IDocument>();
				var deliveryService = new Mock<IDocumentDeliveryService>();

				document.Setup(d => d.Data).Returns(data);
				deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

				var parameters = new UXmlSender.Parameters
				{
					Instructions = messageInstructions,
					Document = document.Object,
					DocumentData = documentData,
					DeliveryService = deliveryService.Object
				};

				var sender = new UXmlSender(parameters);
				var res = sender.Send(notifications);

				deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Never);

				AssertEquals("message failed", false, res);

				AssertEquals(1, notifications.Notifications.Count);
				AssertMultilineASCIIEquals("notifications",
					"Error|The system has encountered an error while sending message. Please try again later.",
					string.Join(System.Environment.NewLine, notifications.Notifications.Select(n => $"{n.Type}|{n.Message}")));

				AssertEquals("error report has been sent", true, ErrorReporter.HasBeenReported("UXmlSender.ProcessWithSaveExceptionHandling"));
				ErrorReporter.Clear();
			}
		}

		sealed class SaveInTransactionActionForThrowingException : SaveInTransactionActionWithFactory
		{
			public SaveInTransactionActionForThrowingException(BusinessObject bizObj)
				: base(bizObj.Factory)
			{
				this.bizObj = bizObj;
			}

			readonly BusinessObject bizObj;

			protected override IChangedTableNames SaveInTransaction()
			{
				var invalidOperationException = new InvalidOperationException();
				var row = ((INeedRow)bizObj).Row;
				var dataException = new ZDataException(invalidOperationException, row, Db.Connection);

				throw new ZSaveException(dataException, bizObj.Factory);
			}
		}

		#endregion

		#region TestSave

		public void TestSave_CargoWiseOne_User() => AssertSave(
			isUserInteractive: true,
			isWeb: false,
			isWebService: false,
			expectToSave: true);

		public void TestSave_CargoWiseOne_ServiceTask() => AssertSave(
			isUserInteractive: false,
			isWeb: false,
			isWebService: false,
			expectToSave: false);

		public void TestSave_CargoWiseOne_Glow() => AssertSave(
			isUserInteractive: false,
			isWeb: false,
			isWebService: true,
			expectToSave: true);

		void AssertSave(bool isUserInteractive, bool isWeb, bool isWebService, bool expectToSave)
		{
			using (Factory.AddDisposableService())
			{
				Globals.IsUserInteractive = isUserInteractive;
				Globals.IsWeb = isWeb;
				Globals.IsWebService = isWebService;

				var dataObject = GetNewShipmentDataObject(consol);
				var dynamicData = dataObject.MakeDynamic();

				Send(dynamicData, consol);

				var messages = Factory.Load<IEDIMessage>(new ZQuery())
					.Where(m => m.IsInDatabase)
					.ToArray();

				if (expectToSave)
				{
					AssertEquals("message has been saved in the db", 1, messages.Length);
				}
				else
				{
					AssertEquals("no messages have been saved in the db", 0, messages.Length);
					AssertNoExceptionThrown(Factory.Save);
				}
			}
		}

		#endregion

		#region TestSendWithAdditionalParametersForEvent

		public void TestSendWithAdditionalParametersForEvent()
		{
			using (Factory.AddDisposableService())
			{
				var bizObj = Factory.New<DummyWithUXmlSupport>();
				var documentData = bizObj.LoadOrCreateDocumentData("zzz");

				var worksheet = DummyWorksheet.Parse(
	@"#Config:Name=""Test Document"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End");
				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
					new MacroScope(documentData),
					Array.Empty<IMacroLibrary>().CreateContext(), "TEST");

				var document = new Mock<IDocument>();
				var data = new object().MakeDynamic();
				document.Setup(d => d.Data).Returns(data);

				var deliveryService = new Mock<IDocumentDeliveryService>();
				var notifications = new DummyNotifications();
				deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

				var messagingExtensions = new Mock<IMessagingExtensions>();
				bizObj.MessagingExtensions = messagingExtensions.Object;

				var additionalParametersForEvent = new[]
				{
				new KeyValuePair<string, string>("ABC", "!@#")
			};
				messagingExtensions.Setup(ext => ext.GetAdditionalParametersForEvent()).Returns(additionalParametersForEvent);

				var parameters = new UXmlSender.Parameters
				{
					DocumentData = documentData,
					Instructions = messageInstructions,
					Document = document.Object,
					DeliveryService = deliveryService.Object
				};

				var sender = new UXmlSender(parameters);
				sender.Send(notifications);

				deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Never);
				messagingExtensions.Verify(ext => ext.GetAdditionalParametersForEvent(), Times.Once);
				AssertEquals(0, notifications.Notifications.Count);

				var stmLog = bizObj.Logs.GetAllLogs().Find(log => log.SL_Reference.Contains("ABC=!@#")).FirstOrDefault();
				AssertNotNull(stmLog);
			}
		}

		#endregion

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(JobDocumentDataSchema.Constants.TableName);

			menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "VizTestDoc";

			template = Factory.New<StmTemplate>();
			template.SO_Name = "Test Template";

			pivot = Factory.New<VisualizerMenuTemplatePivot>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;

			consol = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			documentData = consol.LoadOrCreateDocumentData("xxx");

			Factory.Save();
		}

		VisualizerMenuItem menuItem;
		StmTemplate template;
		VisualizerMenuTemplatePivot pivot;
		BusinessObject consol;
		VisualizerDocumentData documentData;

		#endregion

		#region Implementation

		sealed class Contact
		{
			public string Name { get; set; }
		}

		Shipment GetNewShipmentDataObject(BusinessObject bizO)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11)
			};
			shipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, bizO.PK.ToString());

			return shipment;
		}

		void Send(IDynamicData data, BusinessObject bizO, bool saveDocumentData = true, string config = null)
		{
			var documentData = bizO.LoadOrCreateDocumentData("xxx");

			if (saveDocumentData && !documentData.IsInDatabase)
			{
				Factory.Save();
			}

			var notifications = new DummyNotifications();

			string configText = string.IsNullOrWhiteSpace(config) ? @"#Config:Name=""Test Document"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier"":
#End" : config;

			var worksheet = DummyWorksheet.Parse(configText);

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(documentData),
				Array.Empty<IMacroLibrary>().CreateContext(), "Test Document");

			var document = new Mock<IDocument>();
			var deliveryService = new Mock<IDocumentDeliveryService>();

			document.Setup(d => d.Data).Returns(data);
			deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

			var parameters = new UXmlSender.Parameters
			{
				Instructions = messageInstructions,
				Document = document.Object,
				DocumentData = documentData,
				DeliveryService = deliveryService.Object
			};

			var sender = new UXmlSender(parameters);
			sender.Send(notifications);

			deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Never);
		}

		IEDIMessage GetLastSentMessage()
		{
			return Factory.Load<IEDIMessage>(new ZQuery())
				.OrderByDescending(m => m.EM_SystemCreateTimeUtc)
				.FirstOrDefault();
		}

		void AssertLastUXmlSentExcludingDataContext(string expectedXml)
		{
			AssertLastUXmlSent(expectedXml, nameof(Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11.DataContext));
		}

		void AssertLastUXmlSentExcludingDataContextWorkflow(string expectedXml)
		{
			AssertLastUXmlSent(expectedXml, nameof(Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11.Workflow));
		}

		void AssertLastUXmlSent(string expectedXml, params string[] elementsToExclude)
		{
			var message = GetLastSentMessage();

			AssertUXml(message.EM_MessageText, expectedXml, elementsToExclude);
		}

		void AssertUXmlExcludingDataContext(IEDIMessage messageSent, string expectedXml)
		{
			var expectedXmlWithMessageNumberCollection = string.Format(expectedXml,
				messageSent.Interchange.EI_SessionGUID,
				messageSent.Interchange.EI_InterchangeNum,
				messageSent.EM_MessageNum);

			AssertUXml(messageSent.EM_MessageText, expectedXmlWithMessageNumberCollection, nameof(Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11.DataContext));
		}

		void AssertUXml(string actualXml, string expectedXml, params string[] elementsToExclude)
		{
			var xml = XDocument.Parse(actualXml);

			AssertNotNull("Messsage xml found");
			AssertNotNull("Xml is not empty", xml.Root);

			if (elementsToExclude != null && elementsToExclude.Any())
			{
				var ns = xml.Root.GetDefaultNamespace();

				foreach (var elementToExclude in elementsToExclude)
				{
					xml.Root.Descendants(ns + elementToExclude).First().Remove();
				}
			}

			AssertMultilineASCIIEquals("UXml", expectedXml, xml.ToString());
		}

		void AssertDocumentDataHasLog(BusinessObject parent, string expectedEventCode)
		{
			var documentData = parent.LoadOrCreateDocumentData("xxx");

			var logs = documentData.Logs.Find(log => log.SL_SE_NKEvent == expectedEventCode);

			Assert($"Expected to find at least one {expectedEventCode} event", logs.Any());
		}

		#endregion
	}
}
