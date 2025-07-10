using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCargoWorkflowDescriptorTest : TestCaseWithFactory
	{
		public void TestDestinationDepotWithoutUnderbondsAndConsistentChilds()
		{
			using (Factory.AddDisposableService())
			{
				var defaultDestinationPremiseIDs = new DefaultDestinationPremiseIDCollection();
				var defaultPremiseID = defaultDestinationPremiseIDs.AddNew();
				defaultPremiseID.AirlineCode = "QF";
				defaultPremiseID.PortOfDischarge = "AUSYD";
				defaultPremiseID.PremiseID = "9999Z";
				AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs = defaultDestinationPremiseIDs;

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "DDP_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = workflowDescriptor.Code;

				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
				orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				orgCusCode.OK_CustomsRegNo = "9999Z";
				orgCusCode.OK_OH = orgHeader.PK;

				var cusMAWB = Factory.New<CusMAWB>();
				cusMAWB.CM_FlightNo = "QF667";
				cusMAWB.ChildBills.AddNew().CS_RL_NKDestination = "AUSYD";
				cusMAWB.ChildBills.AddNew().CS_RL_NKDestination = "AUSYD";
				cusMAWB.ChildBills.AddNew().CS_RL_NKDestination = "AUSYD";
				var cusMAWBForWorkflow = (IWorkflowProvider)cusMAWB;

				var trigger = cusMAWBForWorkflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test";
				trigger.TriggerConditions.TriggerEventCode = Events.CargoReceivedAtDepotCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.DeConsolidator;

				cusMAWB.Logs.AddNew(Events.CargoReceivedAtDepot);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				var triggerWTELog = triggerWTELogs[0];

				var processor = workflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
				processor.Process(new NotificationBuffer());
				Factory.Save();

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, cusMAWB.PK));
				AssertEquals("Message Sent", 1, messages.Length);
				AssertEquals("Correct Destination", "DDP_DummyDestination", messages[0].Interchange.EI_To);
			}
		}

		public void TestDestinationDepotWithoutUnderbondsAndInconsistentChilds()
		{
			var defaultDestinationPremiseIDs = new DefaultDestinationPremiseIDCollection();
			var defaultPremiseID = defaultDestinationPremiseIDs.AddNew();
			defaultPremiseID.AirlineCode = "QF";
			defaultPremiseID.PortOfDischarge = "AUSYD";
			defaultPremiseID.PremiseID = "9999Z";
			defaultPremiseID.UseDischargePort = true;
			AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs = defaultDestinationPremiseIDs;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationMode.EK_Destination = "DummyDestination";
			communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationMode.EK_Module = workflowDescriptor.Code;

			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			orgCusCode.OK_CustomsRegNo = "9999Z";
			orgCusCode.OK_OH = orgHeader.PK;

			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_FlightNo = "QF667";
			cusMAWB.ChildBills.AddNew().CS_RL_NKDestination = "AUSYD";
			cusMAWB.ChildBills.AddNew().CS_RL_NKDestination = "AUSYD";
			cusMAWB.ChildBills.AddNew().CS_RL_NKDestination = "USCHI"; // Different destination
			var cusMAWBForWorkflow = (IWorkflowProvider)cusMAWB;

			var trigger = cusMAWBForWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test";
			trigger.TriggerConditions.TriggerEventCode = Events.CargoReceivedAtDepotCode;
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.DeConsolidator;

			cusMAWB.Logs.AddNew(Events.CargoReceivedAtDepot);

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			var triggerWTELog = triggerWTELogs[0];

			var processor = workflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
			processor.Process(new NotificationBuffer());

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, cusMAWB.PK));
			AssertEquals("Message Not Sent", 0, messages.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			workflowDescriptor = new Customs.Business.AirCargoWorkflowDescriptor();
			outboundAdapterServiceUrlTemporaryValue = eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://");
		}

		Customs.Business.AirCargoWorkflowDescriptor workflowDescriptor;
		IDisposable outboundAdapterServiceUrlTemporaryValue;

		protected override void TearDown()
		{
			outboundAdapterServiceUrlTemporaryValue.Dispose();
			base.TearDown();
		}
	}
}
