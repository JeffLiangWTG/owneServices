using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer.Native;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.ServiceManager.Tasks.LogWalker;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class DataContextElementTest : TestCaseWithFactory
	{
		public void TestNativeOrganisationValidatesWhenSentFromWorkflow()
		{
			var savedName = GlbCompany.CurrentCompany.GC_Name;
			try
			{
				GlbCompany.CurrentCompany.GC_Name = "I THINK THIS NAME MIGHT BE OVER 35 CHARACTERS....."; // It's 50.
				using (var tempDirectory = new TempDirectory())
				{
					var importService = new NativeXmlImportService();
					importService.GenerateAndSaveAllXSDs(tempDirectory);
					importService.WriteUniversalCommonSchemaForTesting(tempDirectory);

					var nativeRootSchemaPath = Path.Combine(tempDirectory, "Native.xsd");
					var nativeRootSchema = XDocument.Load(nativeRootSchemaPath);

					var commonSchemaPath = Path.Combine(tempDirectory, UniversalXmlInfo.CommonSchemaName);
					var commonSchema = XDocument.Load(commonSchemaPath);

					var organizationSchemaPath = Path.Combine(tempDirectory, "NativeOrganization.xsd");
					var organizationSchema = XDocument.Load(organizationSchemaPath);

					string organizationXMLText = SetupAndExportNativeOrganisationViaWorkflow();
					var organizationXML = XDocument.Parse(organizationXMLText);

					using (var organizationSchemaReader = organizationSchema.CreateReader())
					using (var nativeRootSchemaReader = nativeRootSchema.CreateReader())
					using (var commonSchemaReader = commonSchema.CreateReader())
					{
						var schemaSet = new XmlSchemaSet();
						schemaSet.Add(NativeXmlInfo.Namespace_2011_11, organizationSchemaReader);
						schemaSet.Add(NativeXmlInfo.Namespace_2011_11, nativeRootSchemaReader);
						schemaSet.Add(UniversalXmlInfo.Namespace_2011_11, commonSchemaReader);

						var errors = new List<string>();
						AssertNoExceptionThrown(() => organizationXML.Validate(schemaSet, (o, e) => { errors.Add(e.Message); }));
						AssertMultilineASCIIEquals("Should be no validation errors...", "", string.Join("\r\n", errors.ToArray()));
					}
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_Name = savedName;
			}
		}

		[TestDate(2010, 02, 12)]
		public void TestNativeDataHasDataContextWhenSentFromWorkflow()
		{
			string header = GetHeader(SetupAndExportNativeOrganisationViaWorkflow());
			var specificMessage = Factory.LoadTop1<IEDIMessage>(new ZQuery());
			var trackingID = specificMessage.Interchange.EI_SessionGUID;
			var interchangeNumber = specificMessage.Interchange.EI_InterchangeNum;
			var messageNumber = specificMessage.EM_MessageNum;
			AssertMultilineASCIIEquals("Exported Data Header", $@"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{NativeXmlInfo.Version_2011_11}"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
    <nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
      <DataSourceCollection>
        <DataSource>
          <Type>Organization</Type>
          <Key>IAMHENAKL</Key>
        </DataSource>
      </DataSourceCollection>
      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventType>
        <Code>CCI</Code>
        <Description>Cargo Check-in</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-02-12T00:00:00+11:00</TriggerDate>
      <TriggerDescription>Got the Goods.</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </nv:DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{trackingID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{interchangeNumber}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{messageNumber}</MessageNumber>
    </MessageNumberCollection>
  </Header>
  <Body>
    <Organization version=""{NativeXmlInfo.Version_2011_11}"">
".Trim(), header);
		}

		public void TestNativeImport_EventFires()
		{
			var (message, orgHeader) = SetupAndExportNativeOrganisationViaWorkflowCore();
			var dimTrig = orgHeader.WorkflowItems.Triggers.AddNew();
			dimTrig.TriggerConditions.TriggerEventCode = Events.DataImportCode;
			dimTrig.P9_Description = "Japan";
			Factory.Save();

			var importedXml = message.EM_MessageText;
			var dimQuery = new ZQuery(StmALogSchema.SL_Parent, orgHeader.PK)
			{ ReLoadExistingRows = true }
				.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DataImportCode);
			AssertEquals(0, Factory.Load<StmALog>(dimQuery).Length);
			using (var stream = new MemoryStream(new UTF8Encoding().GetBytes(importedXml)))
			{
				var businessObjectFactory = new BusinessObjectFactory();
				var importService = new ImportHandler(new AncillaryImportServices());
				importService.Import(stream);
				importService.ErrorOccur = (x, e) => Fail(e.StackTrace);
				businessObjectFactory.Save();
			}
			var serviceTask1 = new LogWalkerMasterServiceTask { ServiceLogger = new TestServiceLogger() };
			var serviceTask2 = new LogWalkerServiceTask { ServiceLogger = new TestServiceLogger() };

			serviceTask1.RunTask();
			serviceTask2.RunTask();

			AssertEquals(1, Factory.Load<StmALog>(dimQuery).Length);
			dimTrig.Reload();
			AssertNotEquals(ZDateTimeOffset.Empty, dimTrig.P9_ActualDateForBinding);
		}

		(IEDIMessage, OrgHeader) SetupAndExportNativeOrganisationViaWorkflowCore()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "ORGCODEME";
			organisation.OH_FullName = "I am Henry";
			organisation.OH_RL_NKClosestPort = "NZAKL";

			var workflowParent = organisation as IWorkflowProvider;
			var trigger = workflowParent.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Got the Goods.";
			trigger.TriggerConditions.TriggerEventCode = Events.CargoCheckinCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

			var log = organisation.Logs.AddNew(Events.CargoCheckin);

			Factory.Save();

			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_Module = workflowParent.WorkflowType;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			communicationsMode.EK_Destination = "SOMEONE";

			var notifications = new NotificationBuffer();
			IProcessor processor = new NativeXmlWorkflowProcessor(new ActionWrapper(action, organisation, Lazy.Create<IStmALog>(() => log)), new Lazy<MessageProcessorCommunicationModesResult>(() => new MessageProcessorCommunicationModesResult(new IEDICommunicationsMode[] { communicationsMode }, null)));
			using (Factory.AddDisposableService())
			{
				processor.Process(notifications);
				Factory.Save();
			}

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", 1, messages.Length);
			var message = messages[0];
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.NativeDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlNativeOrganization, message.EM_MessageSubType);
			});

			return (message, organisation);
		}

		string SetupAndExportNativeOrganisationViaWorkflow()
		{
			return SetupAndExportNativeOrganisationViaWorkflowCore().Item1.EM_MessageTextDetail;
		}

		string GetHeader(ZString messageText)
		{
			var orgHeaderStartTag = @"<OrgHeader Action=""MERGE"">";
			AssertContains("Precondition: Org Header Data Start Tag", orgHeaderStartTag, messageText);

			return messageText.SubstringSafe(0, messageText.IndexOf(orgHeaderStartTag)).Trim();
		}

		public void TestNativeDataHasNoDataContextWhenNotSentFromWorkflow()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "ORG\r\nCODE\nME";
			Factory.Save();

			var exportService = (NativeXmlExportService)ObjectFactory.GetDesignerSafe<IExportService>("NativeXmlExportService");
			var serializer = (NativeXmlSerializer)exportService.Serializer;
			var result = string.Empty;
			using (var outputStream = serializer.SerializeToStream(organisation))
			{
				result = outputStream.WriteToString();
			}

			AssertContains("Serialization should strip CRLF from properties with max_length < 200.", "ORG CODE ME", result);
			var header = GetHeader(result);

			AssertMultilineASCIIEquals("Exported Data Header", string.Format(@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{0}"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""{0}"">
".Trim(), NativeXmlInfo.Version_2011_11), header);
		}

		public void TestDataContextDoesNotAffectDataImporting()
		{
			const string sourceXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
    <OwnerCode>BENGOVSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
    <nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
      <DataSourceCollection>
        <DataSource>
          <Type>Organization</Type>
          <Key>ORGCODEME</Key>
        </DataSource>
      </DataSourceCollection>
      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventType>
        <Code>CCI</Code>
        <Description>Cargo Checkin</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-02-12T00:00:00+11:00</TriggerDate>
      <TriggerDescription>Got the Goods.</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </nv:DataContext>
  </Header>
  <Body>
    <Organization version=""1.0"">
      <OrgHeader Action=""MERGE"">
        <Code>BENGOVSYD</Code>
        <FullName>Ben Govett</FullName>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUMEL</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

			var session = new AncillaryImportServices();
			var handler = new ImportHandler(session);

			using (var inputStream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(sourceXML)))
			{
				handler.Import(inputStream);
			}

			var loggerInfos = new List<string>(((MemoryLogger)session.Logger).Buffer.Infos);
			AssertEquals("Log Text", @"
OrgHeader - 1 inserts, 0 updates, 0 deletes
".Trim(), string.Join("\r\n", loggerInfos.ToArray()));
		}
	}
}
