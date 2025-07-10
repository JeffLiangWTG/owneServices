using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class WorkflowModifiedFieldChangeTriggerProcessorTest : TestCaseWithFactory
	{
		[TestDate(2012, 10, 10)]
		public void TestFieldChangeTriggerWorksFineWithUniversalShipment()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (GetNewTestEnvironment())
			{
				var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
				consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";

				var trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
				trigger.TriggerConditions.TriggerFieldName = JobConsolSchema.JK_MasterBillNum.Name;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.P9_Description = "Received Goods";
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				consolBO[JobConsolSchema.JK_MasterBillNum] = "MB123456";

				var communicationsMode = GlbCompany.GetCurrentCompany(Factory).OrgProxy.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = "CON";
				communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "VASILIY";

				Factory.Save();

				var changeLogs = Factory.Load<StmChangeLog>(new ZQuery(StmChangeLogSchema.SY_ParentID, consolBO.PK));
				AssertEquals("changeLogs.Length", 1, changeLogs.Length);
				var changeLog = changeLogs[0];
				AssertMultilineASCIIEquals("Precondition: changeLog.SY_Changes", "JK_MasterBillNum||MB123456", changeLog.SY_Changes);

				TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(2).ToDateTime();

				new WorkflowServiceTaskTester().RunChain();

				var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_To, "VASILIY");
				interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.eAdaptorQueued);
				var interchange = Factory.LoadTop1<IEDIInterchange>(interchangeQuery);
				AssertNotNull(interchange);
				CombineAssertions(delegate
				{
					AssertEquals("interchange.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
					AssertEquals("interchange.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
					AssertEquals("interchange.EM_MessageType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				});

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
				AssertNotNull(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertStartsWith("message.EM_MessageText", ExpectedUniversalShipmentMessageStart, message.EM_MessageText);
					AssertContains("<WayBillNumber>MB123456</WayBillNumber>", message.EM_MessageText);
				});
			}
		}

		[TestDate(2012, 10, 10)]
		public void TestTriggerCountNumberInXmlToEqualCorrectValueDependentOnExecutionPositionViaFieldChange()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (GetNewTestEnvironment())
			{
				var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
				consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";

				var trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
				trigger.TriggerConditions.TriggerFieldName = JobConsolSchema.JK_MasterBillNum.Name;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				var communicationsMode = GlbCompany.GetCurrentCompany(Factory).OrgProxy.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = "CON";
				communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "VASILIY";

				var wstt = new WorkflowServiceTaskTester();

				for (int i = 0; i < 5; i++)
				{
					consolBO[JobConsolSchema.JK_MasterBillNum] = $"MB12345{i}";
					Factory.Save();
					wstt.WmfTask.Process(wstt.Logs, CancellationToken.None);
					TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(2).ToDateTime();
				}

				wstt.RunChain();

				var query = new ZQuery();
				query.OrderBy = $"{EDIMessageSchema.Constants.EM_MessageNum}";

				var message = Factory.Load<IEDIMessage>(query);
				AssertNotNull(message);

				for (int i = 0; i < message.Length; i++)
				{
					AssertContains($"<TriggerCount>{i + 1}</TriggerCount>", message[i].EM_MessageText);
				}
			}
		}

		[TestDate(2012, 10, 10)]
		public void TestTriggerCountNumberInXmlToEqualCorrectValueDependentOnExecutionPositionViaLogAdded()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (GetNewTestEnvironment())
			{
				var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
				consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";

				var trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
				trigger.TriggerConditions.TriggerFieldName = JobConsolSchema.JK_MasterBillNum.Name;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				var communicationsMode = GlbCompany.GetCurrentCompany(Factory).OrgProxy.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = "CON";
				communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "VASILIY";

				for (int i = 0; i < 5; i++)
				{
					consolBO.GetLogs().AddNew(Events.Received, ZDateTime.Now.ToDateTime());
					Factory.Save();
					TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(2).ToDateTime();
				}

				new WorkflowServiceTaskTester().RunChain();

				var query = new ZQuery();
				query.OrderBy = EDIMessageSchema.Constants.EM_MessageNum;
				var message = Factory.Load<IEDIMessage>(query);
				AssertNotNull(message);

				for (int i = 0; i < message.Length; i++)
				{
					AssertContains($"<TriggerCount>{i + 1}</TriggerCount>", message[i].EM_MessageText);
				}
			}
		}

		#region ExpectedUniversalShipmentMessageStart

		const string ExpectedUniversalShipmentMessageStart = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001010</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DAN</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Current Company For Test</Name>
      </Company>
      <DataProvider>EDIDATDAN</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>7VQ</Code>
        <Name>Test Branch</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>RCV</Code>
        <Description>Received</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2012-10-10T10:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Received Goods</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organisation Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>";
		#endregion

		[TestDate(2012, 10, 07)]
		public void TestFieldChangeTriggerWorksFineWithUniversalEvent()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (GetNewTestEnvironment())
			{
				var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001020";
				consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUMEL";
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZCHC";

				var trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
				trigger.TriggerConditions.TriggerFieldName = JobConsolSchema.JK_MasterBillNum.Name;
				trigger.P9_Type = Constants.Workflow.MilestoneType;
				trigger.P9_Description = "Got em all ready";
				trigger.TriggerConditions.TriggerEventCode = Events.OriginGoodsReadyForShippingCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				consolBO[JobConsolSchema.JK_MasterBillNum] = "MB123789";

				var communicationsMode = GlbCompany.GetCurrentCompany(Factory).OrgProxy.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = "CON";
				communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "BLUE_VEIN";

				Factory.Save();

				var changeLogs = Factory.Load<StmChangeLog>(new ZQuery(StmChangeLogSchema.SY_ParentID, consolBO.PK));
				AssertEquals("changeLogs.Length", 1, changeLogs.Length);
				var changeLog = changeLogs[0];
				AssertMultilineASCIIEquals("Precondition: changeLog.SY_Changes", "JK_MasterBillNum||MB123789", changeLog.SY_Changes);

				TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(2).ToDateTime();

				new WorkflowServiceTaskTester().RunChain();

				var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_To, "BLUE_VEIN");
				interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.eAdaptorQueued);
				var interchange = Factory.Load<IEDIInterchange>(interchangeQuery).Single();
				AssertNotNull(interchange);
				CombineAssertions(delegate
				{
					AssertEquals("interchange.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
					AssertEquals("interchange.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
					AssertEquals("interchange.EM_MessageType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				});

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
				AssertNotNull(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_MessageText", string.Format(ExpectedUniversalEventMessage, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), (string)message.EM_MessageText);
				});
			}
		}

		#region ExpectedUniversalEventMessage

		const string ExpectedUniversalEventMessage = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001020</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DAN</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Current Company For Test</Name>
      </Company>
      <DataProvider>EDIDATDAN</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>7VQ</Code>
        <Name>Test Branch</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ORS</Code>
        <Description>Origin - Goods Ready for Shipping</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2012-10-07T10:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Got em all ready</TriggerDescription>
      <TriggerType>Milestone</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organisation Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2012-10-07T10:00:00.000+10:00</EventTime>
    <EventType>ORS</EventType>
    <CreatedTime>2012-10-07T00:02:00.000+00:00</CreatedTime>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MB123789</Value>
      </Context>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>AUMEL</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>NZCHC</Value>
      </Context>
    </ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>
";

		#endregion

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_WithNewRecordUnchangedField()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));

			ProcessTask fieldTrigger = this.FieldTrigger;
			Dummy.Z0_VarCharMax = "";

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();

			Processor.RunChain();
			AssertEquals("Trigger not fired for new record with unchanged field", "", FieldNotification.Parent.Description);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_WithNewRecordModifiedField()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));

			ProcessTask fieldTrigger = this.FieldTrigger;
			Dummy.Z0_VarCharMax = "Modified";

			Factory.Save();
			var changeLogs = Factory.Load<StmChangeLog>(new ZQuery());
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			Processor.RunChain();
			((BusinessObject)FieldNotification.Parent).Reload();
			AssertEquals("Trigger fired when field modified", "Trigger Fired", FieldNotification.Parent.Description);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_WithNewRecordModifiedField_OptOutWithRegistryItem()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));
			WorkflowDataRegistry.Instance.AllowFieldChangeTriggersToFireForNewJobs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var fieldTrigger = this.FieldTrigger;
			AssertEquals(false, Dummy.IsInDatabase);
			Dummy.Z0_VarCharMax = "Modified";

			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			Processor.RunChain();
			((BusinessObject)FieldNotification.Parent).Reload();
			AssertEquals("Trigger does not fired when field modified on new job", "", FieldNotification.Parent.Description);

			AssertEquals(true, Dummy.IsInDatabase);
			Dummy.Z0_VarCharMax = "Modified Again";

			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 2, 0));

			Processor.RunChain();
			((BusinessObject)FieldNotification.Parent).Reload();
			AssertEquals("Trigger fired when field modified once the job is already saved", "Trigger Fired", FieldNotification.Parent.Description);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_WithSavedRecordModifiedField()
		{
			ProcessTask fieldTrigger = this.FieldTrigger;
			Factory.Save();

			Dummy.Z0_VarCharMax = "Modified";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			Processor.RunChain();
			((BusinessObject)FieldNotification.Parent).Reload();
			AssertEquals("Trigger fired when field modified", "Trigger Fired", FieldNotification.Parent.Description);
		}

		[TestDate(2014, 4, 30)]
		public void TestFireTriggerOnFieldChange_WithActionedMilestone()
		{
			var milestone = this.FieldTrigger;
			milestone.IsMilestone = true;
			Factory.Save();

			Dummy.Z0_VarCharMax = "AAA";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));
			Processor.RunChain();

			milestone.Reload();
			AssertEquals("Milestone should fire first time", "Trigger Fired", milestone.P9_Description);

			milestone.P9_Description = "";
			Dummy.Z0_VarCharMax = "BBB";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));
			Processor.RunChain();

			milestone.Reload();
			AssertEquals("Milestone should not fire second time", "", milestone.P9_Description);
		}

		[TestDate(2014, 4, 30)]
		public void TestDoNotApplyWorkflowTemplatesInTheWMFServiceTask()
		{
			// Set up a milestone with a field trigger condition AND event code so that it creates an event in the WMF service task.
			var milestone = this.FieldTrigger;
			milestone.IsMilestone = true;
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02Code;
			Factory.Save();

			Dummy.Z0_VarCharMax = "AAA";
			Factory.Save();

			// Create a workflow template in a new factory to avoid it being applied anywhere.
			var newFactory = Factory.CreateNewFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent03Code;
			trigger.P9_Description = "I messed up";
			newFactory.Save();

			// We override this so that DummyBizo does what most bizos do, i.e. Apply Workflow Templates because it got loaded.
			DummyWithWorkflow.ProcessLog_Override.Value = (d, l) =>
			{
				if (l.SL_SE_NKEvent == Events.CustomisableEvent02Code)
				{
					d.ApplyWorkflowTemplates();
				}
			};
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));
			new WorkflowModifiedFieldChangeTriggerProcessor().Process(null);

			var triggers = newFactory.CreateNewFactory().Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_Description, trigger.P9_Description)
				.AddToFilter(ProcessTasksSchema.P9_ParentID, FieldTrigger.P9_ParentID));
			AssertEquals("We should not have applied workflow templates, since we are not controlling the user context during save.", 0, triggers.Length);
		}

		[TestDate(2017, 6, 1)]
		public void TestFireTriggerOnFieldChange_WithTriggerCondition()
		{
			var fieldTrigger = Dummy.WorkflowItems.Triggers.AddNew();
			fieldTrigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			var fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
			fieldNotification.PQ_TriggerType = "XML";
			fieldTrigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			fieldTrigger.TriggerConditions.TriggerConditionValue = @"Source.Z0_VarCharMax == ""Updated""";
			Dummy.Z0_VarCharMax = "Modified";

			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			Processor.RunChain();
			((BusinessObject)fieldNotification.Parent).Reload();
			AssertEquals("Trigger should not be fired when condition not met", "", fieldNotification.Parent.Description);

			Dummy.Z0_VarCharMax = "Updated";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 2, 0));

			Processor.RunChain();
			((BusinessObject)fieldNotification.Parent).Reload();
			AssertEquals("Trigger fired when condition is met", "Trigger Fired", fieldNotification.Parent.Description);
		}

		[TestDate(2017, 6, 1)]
		public void TestFireTriggerOnFieldChange_WithNewTriggerCondition()
		{
			var fieldTrigger = Dummy.WorkflowItems.Triggers.AddNew();
			fieldTrigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			var fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
			fieldNotification.PQ_TriggerType = "XML";
			fieldTrigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			fieldTrigger.TriggerConditions.TriggerConditionValue = @"Z0_VarCharMax == ""Updated""";
			Dummy.Z0_VarCharMax = "Modified";

			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			Processor.RunChain();
			((BusinessObject)fieldNotification.Parent).Reload();
			AssertEquals("Trigger should not be fired when condition not met", "", fieldNotification.Parent.Description);

			Dummy.Z0_VarCharMax = "Updated";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 2, 0));

			Processor.RunChain();
			((BusinessObject)fieldNotification.Parent).Reload();
			AssertEquals("Trigger fired when condition is met", "Trigger Fired", fieldNotification.Parent.Description);
		}

		[TestDate(2017, 6, 1)]
		public void TestFireTriggerOnFieldChange_WithUDFTriggerCondition()
		{
			var fieldTrigger = Dummy.WorkflowItems.Triggers.AddNew();
			fieldTrigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			var fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
			fieldNotification.PQ_TriggerType = "XML";
			fieldTrigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			fieldTrigger.TriggerConditions.TriggerConditionValue = @"""<Z0_VarCharMax>"" == ""Updated""";
			Dummy.Z0_VarCharMax = "Modified";

			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			Processor.RunChain();
			((BusinessObject)fieldNotification.Parent).Reload();
			AssertEquals("Trigger should not be fired when condition not met", "", fieldNotification.Parent.Description);

			Dummy.Z0_VarCharMax = "Updated";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 2, 0));

			Processor.RunChain();
			((BusinessObject)fieldNotification.Parent).Reload();
			AssertEquals("Trigger fired when condition is met", "Trigger Fired", fieldNotification.Parent.Description);
		}

		[TestDate(2010, 9, 17)]
		public void TestLogging()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);

			TestCaseHelper.ClearTable(StmChangeLogSchema.Constants.TableName);
			DummyWithWorkflow dummy1 = Factory.New<DummyWithWorkflow>();
			ProcessTask fieldTrigger1 = dummy1.RelatedWorkflowProvider.WorkflowItems.Triggers.AddNew();
			fieldTrigger1.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			ProcessTaskNotification fieldNotification1 = fieldTrigger1.ProcessTaskNotifications.AddNew();
			fieldNotification1.PQ_P9 = fieldTrigger1.PK;

			DummyWithWorkflow dummy2 = Factory.New<DummyWithWorkflow>();
			ProcessTask fieldTrigger2 = dummy2.RelatedWorkflowProvider.WorkflowItems.Triggers.AddNew();
			fieldTrigger2.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			ProcessTaskNotification fieldNotification2 = fieldTrigger2.ProcessTaskNotifications.AddNew();
			fieldNotification2.PQ_P9 = fieldTrigger2.PK;

			ProcessTask fieldMilestone = dummy2.RelatedWorkflowProvider.WorkflowItems.Milestones.AddNew();
			fieldMilestone.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			ProcessTaskNotification fieldNotification3 = fieldMilestone.ProcessTaskNotifications.AddNew();
			fieldNotification3.PQ_P9 = fieldTrigger2.PK;

			Factory.Save();

			dummy1.Z0_VarCharMax = "Modified";
			dummy2.Z0_VarCharMax = "Modified";

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			Processor.RunChain();

			fieldTrigger1.Reload();
			fieldTrigger2.Reload();
			fieldMilestone.Reload();

			var logs = Processor.Logs.GetAllLogs();
			AssertContains("Processing 2 change log(s)", logs);

			AssertEquals(3, new Regex("Action completed").Matches(logs).Count);
			AssertNotEquals(0, new Regex(fieldTrigger1.P9_TaskID).Matches(logs));
		}

		[TestDate(2018, 9, 17)]
		public void TestEndToEnd_CorrectUserEditsLogs()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);
			TestCaseHelper.ClearTable(StmChangeLogSchema.Constants.TableName);
			DummyWithWorkflow dummy1 = Factory.New<DummyWithWorkflow>();
			var trigger = dummy1.RelatedWorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			trigger.TriggerConditions.TriggerEventCode = "";
			var fieldNotification1 = trigger.ProcessTaskNotifications.AddNew();
			fieldNotification1.PQ_TriggerType = "XML";

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var newFactory = new BusinessObjectFactory();
				newFactory.Load<DummyWithWorkflow>(dummy1.PK).Z0_VarCharMax = "Modified";
				newFactory.Save();
			}

			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));
			Processor.RunChain();
			trigger.Reload();
			var wteLog = new BusinessObjectFactory().Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, trigger.PK)).Single(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			AssertEquals("The user that did the edit should have the log.", staff.GS_Code, wteLog.SL_GS_NKUser);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_FiringManyTriggers()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);

			string alpabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			Random rand = new Random(1);
			List<string> values = new List<string>();
			for (int i = 0; i < 150; i++)
			{
				DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
				ProcessTask fieldTrigger = dummy.RelatedWorkflowProvider.WorkflowItems.Triggers.AddNew();
				fieldTrigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
				ProcessTaskNotification fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
				fieldNotification.PQ_P9 = fieldTrigger.PK;
				string val = GenerateRandomField(rand, values, alpabet);
				values.Add(val);
				fieldNotification.PQ_TriggerType = val;
				dummy.Z0_VarCharMax = "Modified";

				if (i == 102)
				{
					Factory.Save();
					TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();
					Processor.RunChain();
				}
			}
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));
			Processor.RunChain();
			ZQuery firedTriggersQuery = new ZQuery();
			firedTriggersQuery.AddToFilter(ProcessTasksSchema.P9_Description, "Trigger Fired");
			firedTriggersQuery.ReLoadExistingRows = true;
			ProcessTask[] triggersFired = Factory.Load<ProcessTask>(firedTriggersQuery);

			AssertEquals("Correct number of triggers fired", 150, triggersFired.Length);
		}

		[TestDate(2017, 11, 22)]
		public void TestFieldChangeTrigger_ConccurencyRetryOnce()
		{
			var fieldTrigger = FieldTrigger;
			Factory.Save();

			Dummy.Z0_VarCharMax = "Modified";
			Factory.Save();

			ResetDummyFieldTriggerAndNotification();

			fieldTrigger = FieldTrigger;
			Factory.Save();

			Dummy.Z0_VarCharMax = "Modified";
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			var processor = new WorkflowServiceTaskTester(new WorkflowModifiedFieldChangeTriggerProcessorForTest { LogIndexWithSaveExceptionToThrow = new[] { 1 } });
			processor.RunChain();
			var logs = processor.Logs.GetAllLogs();
			AssertEquals("Should encounter exception and retry", 1, new Regex("Retrying change logs").Matches(logs).Count);

			((BusinessObject)FieldNotification.Parent).Reload();
			AssertEquals("Trigger fired when field modified", "Trigger Fired", FieldNotification.Parent.Description);
		}

		[TestDate(2017, 11, 27)]
		public void TestFieldChangeTrigger_ConccurencyTry3TimesAndFailed()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));

			var fieldTrigger = FieldTrigger;
			Factory.Save();

			Dummy.Z0_VarCharMax = "Modified";
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			var processor = new WorkflowServiceTaskTester(new WorkflowModifiedFieldChangeTriggerProcessorForTest { LogIndexWithSaveExceptionToThrow = new[] { 1 }, LogIndexWithNonCriticalExceptionToThrow = new[] { 2, 3, 4 } });
			AssertNoExceptionThrown(processor.RunChain);
			var logs = processor.Logs.GetAllLogs();

			AssertEquals("Unhandled errors dont cause retries.", 1, new Regex("Unhandled error occurred").Matches(logs).Count);
			ErrorReporter.Clear();

			((BusinessObject)FieldNotification.Parent).Reload();
			AssertNotEquals("Trigger should not be updated when failed", "Trigger Fired", FieldNotification.Parent.Description);
		}

		[TestDate(2017, 11, 27)]
		public void TestFieldChangeTrigger_MultipleSaveException()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));

			var fieldTrigger = FieldTrigger;
			Factory.Save();

			Dummy.Z0_VarCharMax = "Modified";
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			var processor = new WorkflowServiceTaskTester(new WorkflowModifiedFieldChangeTriggerProcessorForTest { LogIndexWithSaveExceptionToThrow = new[] { 1, 2, 3 } });
			AssertNoExceptionThrown(processor.RunChain);
			var logs = processor.Logs.GetAllLogs();
			AssertEquals("Should fail on first retry", 1, new Regex("Unhandled error").Matches(logs).Count);
			ErrorReporter.Clear();

			((BusinessObject)FieldNotification.Parent).Reload();
			AssertNotEquals("Trigger should not be updated when failed", "Trigger Fired", FieldNotification.Parent.Description);
		}

		[TestDate(2017, 11, 27)]
		public void TestFieldChangeTrigger_NonCriticalException()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));

			var fieldTrigger = FieldTrigger;
			Factory.Save();

			Dummy.Z0_VarCharMax = "Modified";
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			var processor = new WorkflowServiceTaskTester(new WorkflowModifiedFieldChangeTriggerProcessorForTest { LogIndexWithSaveExceptionToThrow = new[] { 1 }, LogIndexWithNonCriticalExceptionToThrow = new[] { 2 } });
			AssertNoExceptionThrown(processor.RunChain);
			var logs = processor.Logs.GetAllLogs();
			AssertEquals("Unhandled errors dont cause retries.", 1, new Regex("Unhandled error occurred").Matches(logs).Count);
			ErrorReporter.Clear();

			((BusinessObject)FieldNotification.Parent).Reload();
			AssertNotEquals("Trigger should not be updated when failed", "Trigger Fired", FieldNotification.Parent.Description);
		}

		public void TestTriggerFieldsDefinedByWorkflowDescriptorsAreProcessedIfUpdatedByDBTrigger()
		{
			var sb = new StringBuilder();
			var propertyDependencies = new WorkflowModifiedFieldChangeTriggerProcessorForTest { LogIndexWithSaveExceptionToThrow = new[] { 1 } }.PropertyDependenciesExposed;

			foreach (var workflowDescriptor in WorkflowDescriptors.Instance.Values)
			{
				foreach (var triggerFieldColumn in workflowDescriptor.GetWorkflowTriggerFieldColumns())
				{
					foreach (var trigger in GetTriggersUpdatingColumn(triggerFieldColumn.Name))
					{
						var triggerSourceColumn = trigger.Item1;
						if (!propertyDependencies.ContainsKey(triggerSourceColumn) || propertyDependencies[triggerSourceColumn] != triggerFieldColumn.Name)
						{
							sb.AppendLine(FormattableString.Invariant($"{triggerFieldColumn.Name} defined by {workflowDescriptor.GetType().FullName} is updated from {triggerSourceColumn} by {trigger.Item2}"));
						}
					}
				}
			}

			if (sb.Length > 0)
			{
				Fail(FormattableString.Invariant($@"The following columns are defined as Workflow Trigger Field Columns, and yet they also appear to be updatable by one or more Database Triggers. In order to ensure correct functioning of Workflow Trigger Fields, you must add a dependency relationship to WorkflowModifiedFieldChangeTriggerProcessor.TriggerFieldDependencies.
{sb.ToString()}"));
			}

			Assert(true);
		}

		List<Tuple<string, string>> GetTriggersUpdatingColumn(ZString columnName)
		{
			var result = new List<Tuple<string, string>>();

			var sql = FormattableString.Invariant($@"
SELECT 
    name AS trigger_name,
    OBJECT_DEFINITION(object_id) AS trigger_definition
FROM sys.objects 
WHERE type = 'TR'
AND OBJECT_DEFINITION(object_id) LIKE '%{columnName}%'");

			using (var reader = Db.Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					var triggerDefinition = reader["trigger_definition"].ToString();
					var match = Regex.Match(triggerDefinition, FormattableString.Invariant($"{columnName}\\s*=\\s*(\\w+\\.*\\w*)\\b"));
					if (match.Success)
					{
						var triggerColumn = match.Value.Split('=')[1].Trim();
						if (triggerColumn.Contains("."))
						{
							triggerColumn = triggerColumn.Split('.').Last();
						}

						result.Add(Tuple.Create(triggerColumn, reader["trigger_name"].ToString()));
					}
				}
			}

			return result;
		}

		string GenerateRandomField(Random rand, List<string> values, string alpabet)
		{
			string newStr = string.Empty;
			while (string.IsNullOrEmpty(newStr))
			{
				newStr += alpabet[rand.Next(0, alpabet.Length - 1)];
				newStr += alpabet[rand.Next(0, alpabet.Length - 1)];
				newStr += alpabet[rand.Next(0, alpabet.Length - 1)];
				if (values.Contains(newStr))
				{
					newStr = string.Empty;
				}
			}
			return newStr;
		}

		#region Implementation

		WorkflowServiceTaskTester Processor
		{
			get { return processor ?? (processor = new WorkflowServiceTaskTester()); }
		}
		WorkflowServiceTaskTester processor;

		ProcessTask FieldTrigger
		{
			get
			{
				SetFieldTriggerAndNotificationIfRequired();
				return fieldTrigger;
			}
		}
		ProcessTask fieldTrigger;

		ProcessTaskNotification FieldNotification
		{
			get
			{
				SetFieldTriggerAndNotificationIfRequired();
				return fieldNotification;
			}
		}
		ProcessTaskNotification fieldNotification;

		void SetFieldTriggerAndNotificationIfRequired()
		{
			if (fieldTrigger == null || fieldNotification == null)
			{
				fieldTrigger = Dummy.RelatedWorkflowProvider.WorkflowItems.Triggers.AddNew();
				fieldTrigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
				fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
				fieldNotification.PQ_TriggerType = "XML";
			}
		}

		void ResetDummyFieldTriggerAndNotification()
		{
			fieldNotification = null;
			fieldTrigger = null;
			dummy = null;
		}

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		protected override void SetUp()
		{
			base.SetUp();
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_Description);
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
		}

		IDisposable GetNewTestEnvironment()
		{
			var currentCompanyOrg = Factory.New<OrgHeader>();
			currentCompanyOrg.OH_Code = "~OC";
			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			currentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			currentCompany.GC_OH_OrgProxy = currentCompanyOrg.PK;
			currentCompany.GC_Name = "Current Company For Test";
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = currentCompany.PK;
			currentBranch.GB_RL_NKHomePort = "AUBNE";
			Factory.Save();

			var user = Env.CurrentUser;
			var environment = DisposableEnvironment.ForBranch(currentBranch.PK.ToGuid());
			var userContext = Env.SetTemporaryUserContext(user.PK, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			return new DisposableAction(() =>
			{
				userContext.Dispose();
				environment.Dispose();
			});
		}

		#endregion
	}
}
