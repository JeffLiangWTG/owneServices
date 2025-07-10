using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Events = Enterprise.ZArchitecture.Business.Events;
using IEDICommunicationsMode = Enterprise.MasterFiles.Integration.IEDICommunicationsMode;

namespace Enterprise.DataTransfer.MessageDelivery.Testing
{
	sealed class XmlMessageDeliverTest : TestCaseWithFactory
	{
		#region Interchange

		MessageProcessorCommunicationModesResult GetModes(IList<IEDICommunicationsMode> modes)
		{
			return new MessageProcessorCommunicationModesResult(modes, null);
		}

		public void TestProcessDeliverMessages_Interchange()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;
			mode.EK_Filename = "TestProcessDeliverMessages.txt";
			mode.EK_LocalPartyVanID = "sender";
			mode.EK_RelatedPartyVanID = "receiver";

			var testBizObj = Factory.NewWithValidTestData<OrgHeader>();
			testBizObj.OH_Code = "JobNumber";

			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			action.PQ_MessagePurpose = "APP";

			var interchange = new XmlInterchange();
			interchange.Payload.Data = new BusinessObject[] { testBizObj };
			interchange.Payload.DataAdapter = new OrganisationValueObjectDataAdapter();

			delivery = new XmlMessageDeliver(GetModes(new[] { mode }), testBizObj, interchange, action);

			string xmlStringFromFile;

			using (Factory.AddDisposableService())
			using (var tempDir = new TempDirectory())
			{
				mode.EK_Destination = tempDir.DirectoryName;

				delivery.Process(new NotificationCollection());
				Factory.Save();

				var fileName = Path.Combine(mode.EK_Destination, mode.EK_Filename);
				AssertEquals("Should create the target file", true, File.Exists(fileName));
				xmlStringFromFile = File.ReadAllText(fileName);
			}

			const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<XmlInterchange xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://www.edi.com.au/EnterpriseService/\">\r\n  <InterchangeInfo>\r\n    <Source>\r\n      <SenderCode>sender</SenderCode>\r\n      <Purpose>APP</Purpose>\r\n    </Source>\r\n    <Target>\r\n      <ReceiverCode>receiver</ReceiverCode>\r\n    </Target>\r\n  </InterchangeInfo>\r\n  <Payload>\r\n    <Organisations>\r\n      <Organisation EDICode=\"JobNumber\" OwnerCode=\"JobNumber\">\r\n        <OrganisationDetails>\r\n          <Addresses>\r\n            <Address AddressType=\"MAIN\">\r\n              <AddressLine1>#1</AddressLine1>\r\n              <AddressCode>#1</AddressCode>\r\n              <Language>EN</Language>\r\n              <Sequence>1</Sequence>\r\n              <AddressCapabilities>\r\n                <AddressCapability AddressType=\"MAIN\" />\r\n                <AddressCapability IsMainAddress=\"true\" AddressType=\"OFC\" />\r\n              </AddressCapabilities>\r\n            </Address>\r\n          </Addresses>\r\n        </OrganisationDetails>\r\n      </Organisation>\r\n    </Organisations>\r\n  </Payload>\r\n</XmlInterchange>";
			AssertXMLContains(expected, xmlStringFromFile);
		}

		#endregion

		[TestDate(2000, 1, 1)]
		public void TestProcessDeliverMessages_NoCommunicationModes()
		{
			using (Factory.AddDisposableService())
			{
				var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
				SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var testBizObj = Factory.NewWithValidTestData<OrgHeader>();
				testBizObj.OH_Code = "JobNumber";
				var dataAdapter = new OrganisationValueObjectDataAdapter();
				var xmlBytesFromBizObj = GetExpectedBizObjXmlBytes(testBizObj, dataAdapter);

				var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
				delivery = new XmlMessageDeliver(GetModes(Array.Empty<EDICommunicationsMode>()), testBizObj, dataAdapter, action);

				delivery.Process(notifications);
				Factory.Save();

				AssertMultilineASCIIEquals(
					"GIVEN no communication modes, WHEN executing process, THEN warning should be logged",
					"No communication modes found.\r\n",
					notifications.AsString);
			}
		}

		[TestDate(2000, 1, 1)]
		public void TestProcessDeliverMessages_SaveToFile()
		{
			using (Factory.AddDisposableService())
			{
				var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
				SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;
				mode.EK_Filename = "TestProcessDeliverMessages.txt";

				var testBizObj = Factory.NewWithValidTestData<OrgHeader>();
				testBizObj.OH_Code = "JobNumber";
				var dataAdapter = new OrganisationValueObjectDataAdapter();
				var xmlBytesFromBizObj = GetExpectedBizObjXmlBytes(testBizObj, dataAdapter);

				//Xml file format
				var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
				delivery = new XmlMessageDeliver(GetModes(new[] { mode }), testBizObj, dataAdapter, action);
				AssertFileWasCreatedWithCorrectData(xmlBytesFromBizObj, mode);

				//Descartes Xml file format
				IDbConnected factory = testBizObj.Factory;
				factory.Connection.BeginTransaction();
				try
				{
					SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml;
					delivery = new XmlMessageDeliver(GetModes(new[] { mode }), testBizObj, dataAdapter, action);
					var dxlBytesFromBizObj = GetExpectedBizObjDxlBytes(new MemoryStream(xmlBytesFromBizObj), testBizObj, mode);
					AssertFileWasCreatedWithCorrectData(dxlBytesFromBizObj, mode);
				}
				finally
				{
					factory.Connection.RollbackTransaction();
				}
			}
		}

		[TestDate(2000, 1, 1)]
		public void TestProcessDeliverMessages_EmailAsAttachment()
		{
			using (Factory.AddDisposableService())
			{
				var bizObjToExport = Factory.NewWithValidTestData<OrgHeader>();
				bizObjToExport.OH_Code = "JobNumber";
				var dataAdapter = new OrganisationValueObjectDataAdapter();

				var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				mode.EK_ServerAddressSubject = "Subject (*Greeting*)";
				mode.EK_Destination = "test@test.com";
				mode.EK_Filename = "TestProcessDeliverMessages (*JobNumber*).txt";

				var xmlBytesFromBizObj = GetExpectedBizObjXmlBytes(bizObjToExport, dataAdapter);
				delivery = new XmlMessageDeliver(GetModes(new[] { mode }), bizObjToExport, dataAdapter, null);
				delivery.ExtraDataSubstitution = (ProcessTaskNotification action, BusinessObject bizo, ZString data) =>
				{
					return data.Replace("(*Greeting*)", "HELLO WORLD");
				};

				delivery.Process(notifications);
				Factory.Save();

				AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Subject", "Subject HELLO WORLD", createdEmail.Subject);
				AssertEquals("Should send it to correct recepient", true, createdEmail.Recipients.Contains(mode.EK_Destination));
				AssertEquals("Should send it to correct recepients", 1, createdEmail.Recipients.Count);
				AssertEquals("Attachment name", mode.EK_Filename.Replace("(*JobNumber*)", "JobNumber"), createdEmail.Attachments[0].DisplayName);
				AssertEquals("Didn't create correct attachment", xmlBytesFromBizObj, createdEmail.Attachments[0].Data);
			}
		}

		[TestDate(2000, 1, 1)]
		public void TestProcessDeliverMessagesToEHub_DoNotSaveFactoryOnDeliver()
		{
			using (Factory.AddDisposableService())
			{
				var noOfInterchangesBeforeDelivery = Factory.GetDatabaseCount(typeof(EDIInterchange));

				var bizObjToExport = Factory.NewWithValidTestData<OrgHeader>();
				bizObjToExport.OH_Code = "JobNumber";
				var dataAdapter = new OrganisationValueObjectDataAdapter();

				var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				mode.EK_Destination = "EDIDATDAT";

				var xmlBytesFromBizObj = GetExpectedBizObjXmlBytes(bizObjToExport, dataAdapter);
				delivery = new XmlMessageDeliverForTest(new[] { mode }, bizObjToExport, dataAdapter, null);
				delivery.Process(notifications);

				var noOfInterchangesAfterDelivery = Factory.GetDatabaseCount(typeof(EDIInterchange));
				AssertEquals("noOfInterchangesAfterDelivery", noOfInterchangesBeforeDelivery, noOfInterchangesAfterDelivery);

				Factory.Save();
				var noOfInterchangesAfterSave = Factory.GetDatabaseCount(typeof(EDIInterchange));
				AssertEquals("noOfInterchangesAfterSave", noOfInterchangesBeforeDelivery + 1, noOfInterchangesAfterSave);
			}
		}

		public void TestConstructorThatTakesJobNumber()
		{
			using (Factory.AddDisposableService())
			{
				var bizObjToExport = Factory.NewWithValidTestData<OrgHeader>();
				var jobNumber = Factory.NewWithValidTestData<OrgHeader>();
				jobNumber.OH_Code = "JobNumber";
				var dataAdapter = new OrganisationValueObjectDataAdapter();

				var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				mode.EK_Destination = "test@test.com";
				mode.EK_Filename = "TestProcessDeliverMessages (*JobNumber*).txt";

				delivery = new XmlMessageDeliver(GetModes(new[] { mode }), bizObjToExport, jobNumber, dataAdapter, null);
				delivery.Process(notifications);
				Factory.Save();

				AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Attachment name", mode.EK_Filename.Replace("(*JobNumber*)", "JobNumber"), createdEmail.Attachments[0].DisplayName);
			}
		}

		public void TestXmlMessageDeliveryNoFactorySaves()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;

			//Setup
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OHD";
			orgHeader.OH_FullName = "FullName";

			var mock = new Mock<IFtpProcessor>();
			mock.Setup(m => m.UploadStreamSeveralAttempts(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));

			using (Factory.AddDisposableService())
			using (ObjectFactory.Substitute(mock.Object))
			{
				var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
				mode.EK_Destination = "ftp://whatever.com";

				orgHeader.EDICommunicationsModes.Add(mode);

				var group = Factory.NewWithValidTestData<GlbGroup>();
				var staff = group.Staff.AddNew();
				staff.GS_EmailAddress = "alex.jones@cargowise.com";
				staff.GS_Code = "ZAC";

				NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

				Factory.Save();

				var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

				Factory.Save();

				var bizObjToExport = Factory.NewWithValidTestData<OrgHeader>();
				var jobNumber = Factory.NewWithValidTestData<OrgHeader>();
				jobNumber.OH_Code = "JobNumber";
				var dataAdapter = new OrganisationValueObjectDataAdapter();

				delivery = new XmlMessageDeliver(GetModes(new[] { mode }), bizObjToExport, jobNumber, dataAdapter, null);

				int factoryCount = 0;
				BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) => factoryCount++);

				//Act
				delivery.Process(notifications);

				//Assert
				AssertEquals("No Factory Saves", 0, factoryCount);

				Factory.Save();
			}

			mock.VerifyAll();
		}

		public void TestGetBizObjSerializerForInitialisationHandlesEXLEvent()
		{
			using (Factory.AddDisposableService())
			{
				var bizObjToExport = Factory.NewWithValidTestData<OrgHeader>();
				var jobNumber = Factory.NewWithValidTestData<OrgHeader>();
				jobNumber.OH_Code = "JobNumber";
				var dataAdapter = new StorageDocsBaseValueObjectDataAdatper();

				var eDoc = (bizObjToExport as IDocManagerSupport).DocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, "SomeFilename", "MSC");

				var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				mode.EK_Destination = "test@test.com";
				mode.EK_Filename = "TestProcessDeliverMessages (*JobNumber*).txt";

				var trigger = bizObjToExport.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Such a good description";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Enterprise.ZArchitecture.Business.AutoEvents.DocumentImportedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;

				bizObjToExport.Logs.AddNew(ZArchitecture.Business.AutoEvents.DocumentImported, eDoc.UniqueKey.ToString());

				Factory.Save();

				var wteLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

				var wteLogs = Factory.Load<StmALog>(wteLogsQuery);
				AssertEquals(1, wteLogs.Length);

				foreach (var wteLog in wteLogs)
				{
					var queuedLog = new QueuedLogForTesting(wteLog, trigger);
					var eventInfo = new EventInfoProvider(queuedLog, action.Parent, action.Parent.GetJob());
					delivery = new XmlMessageDeliver(GetModes(new[] { mode }), bizObjToExport, jobNumber, dataAdapter, action, eventInfo);
					AssertNotNull(delivery);

					delivery.Process(notifications);
					Factory.Save();
					AssertEquals("EDocSerializer should be returned for SendEDocXml event", typeof(EDocSerializer), delivery.Serializer.GetType());
				}

				AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Attachment name", mode.EK_Filename.Replace("(*JobNumber*)", "JobNumber"), createdEmail.Attachments[0].DisplayName);
			}
		}

		public void TestMessageDeliveryEmailRecipient()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "BLAH";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "test@test.com";

			var docManager = (orgHeader as IDocManagerSupport).DocManagerInfo;
			var eDoc = docManager.AddFileOrDocument(new byte[] { 0x1, 0x2 }, "SomeFilename", "MSC");
			orgHeader.Logs.AddNew(Events.DocumentImported, eDoc.CreateReference());

			docManager.MasterFactory.Save();
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Attachment name", $"{orgHeader.OH_Code}.xml", createdEmail.Attachments[0].DisplayName);
		}

		public void TestMessageDeliveryEDICommunicationMode_NoFileName()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_Module = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.EXL;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			mode.EK_Destination = "test@test.com";
			mode.EK_Filename = "";
			orgHeader.EDICommunicationsModes.Add(mode);

			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Such a good description";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.EDICommunication;

			var docManager = (orgHeader as IDocManagerSupport).DocManagerInfo;
			var eDoc = docManager.AddFileOrDocument(new byte[] { 0x1, 0x2 }, "SomeFilename", "MSC");
			orgHeader.Logs.AddNew(Events.DocumentImported, eDoc.CreateReference());

			docManager.MasterFactory.Save();
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Attachment name", $"{orgHeader.OH_Code}.xml", createdEmail.Attachments[0].DisplayName);
		}

		#region Implementation

		void AssertFileWasCreatedWithCorrectData(byte[] expectedXmlBytesFromBizObj, EDICommunicationsMode mode)
		{
			byte[] xmlBytesFromFile = null;

			using (TempDirectory tempDir = new TempDirectory())
			{
				mode.EK_Destination = tempDir.DirectoryName;

				delivery.Process(notifications);
				Factory.Save();

				string fileName = Path.Combine(mode.EK_Destination.Replace("(*JobNumber*)", "JobNumber"), mode.EK_Filename);
				AssertEquals("Should create the target file", true, File.Exists(fileName));
				xmlBytesFromFile = File.ReadAllBytes(fileName);
			}

			AssertEquals("File content is not correct", expectedXmlBytesFromBizObj, xmlBytesFromFile);
		}

		byte[] GetExpectedBizObjXmlBytes(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter)
		{
			var result = bizObj.Factory.SubscribeForDispose((SubStreamableStream)new MemoryStream());
			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
			serializer.ExportXmlData(result, dataAdapter, new BusinessObject[] { bizObj }, new ValueObjectExportContext(notifications), "", "", "");
			result.Position = 0;
			return result.ConvertToByteArrayAndCloseStream();
		}

		byte[] GetExpectedBizObjDxlBytes(MemoryStream xmlStream, BusinessObject bizObj, EDICommunicationsMode mode)
		{
			DxlBusinessObjectSerializerForTesting serializer = new DxlBusinessObjectSerializerForTesting(bizObj);
			var result = bizObj.Factory.SubscribeForDispose(serializer.ConvertXmlToDxl(xmlStream, mode));
			result.Position = 0;
			return result.ConvertToByteArrayAndCloseStream();
		}

		XmlMessageDeliver delivery;
		readonly NotificationBuffer notifications = new NotificationBuffer();

		class XmlMessageDeliverForTest : XmlMessageDeliver
		{
			public XmlMessageDeliverForTest(IEDICommunicationsMode[] modes, BusinessObject bizObjToDeliver, IValueObjectDataAdapter dataAdapter, ProcessTaskNotification action, EventInfoProvider eventInfoProvider = null)
				: base(new MessageProcessorCommunicationModesResult(modes, null), bizObjToDeliver, bizObjToDeliver as IJobNumber, dataAdapter, null, action, eventInfoProvider)
			{
			}

			protected override DeliveryContext GetDeliveryContext()
			{
				var result = base.GetDeliveryContext();
				result.MessageSubTypeCode = Messaging.Integration.EDIMessageSubTypeList.Codes.Consols;
				return result;
			}
		}
		#endregion
	}
}
