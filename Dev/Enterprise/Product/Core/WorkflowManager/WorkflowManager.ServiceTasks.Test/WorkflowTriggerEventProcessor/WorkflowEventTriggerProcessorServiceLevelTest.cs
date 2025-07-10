using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ServiceManager.Tasks.LogWalker.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	[TestedType(typeof(WorkflowEventTriggerProcessor))]
	sealed class WorkflowEventTriggerProcessorServiceLevelTest : LogSubscriberServiceLevelTest<WorkflowEventTriggerProcessor>
	{
		public void TestNoMessagesCreatedOnConcurrencyError()
		{
			var initialMessageCount = Factory.Load<IEDIMessage>(new ZQuery()).Length;

			var factory = NewFactoryForExceptionThrowing;
			factory.ExceptionToThrow = CreateNewConcurrencyException(factory);
			factory.CreateNewFactoryMethod = delegate
			{ return factory; };

			GetFactoryMethod = delegate
			{ return factory; };
			using (GetNewTestEnvironment())
			{
				InitialiseSampleData(factory);
				factory.ThrowExceptionOnSave = true;

				InitialiseAndRunTaskSchedule();

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals("Messages created: ", 0, messages.Length - initialMessageCount);
				AssertNotNull("This should blow up, since the log walker should never have concurrency errors here.", ErrorReporter.LastMessageReported);
				ErrorReporter.Instance.Clear();
			}
		}

		public void TestNoMessagesCreateOnConcurrencyErrorAtEServiceDelivery()
		{
			var initialMessageCount = Factory.Load<IEDIMessage>(new ZQuery()).Length;

			var factory = NewFactoryForExceptionThrowing;
			factory.CreateNewFactoryMethod = delegate
			{
				var newFactory = new TestBusinessObjectFactoryForExceptionThrowing() { ThrowExceptionOnSave = true };
				newFactory.ExceptionToThrow = CreateNewConcurrencyException(newFactory);
				return newFactory;
			};

			GetFactoryMethod = delegate
			{ return factory; };
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (GetNewTestEnvironment())
			{
				InitialiseSampleData(factory);
				InitialiseAndRunTaskSchedule();

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals("Messages created: ", 1, messages.Length - initialMessageCount);
			}
		}

		public void TestNoMessagesCreateOnConcurrencyExceptionAtNewsTransmitter()
		{
			var initialMessageCount = Factory.Load<IEDIMessage>(new ZQuery()).Length;

			GetFactoryMethod = delegate
			{
				var newFactory = new TestBusinessObjectFactoryForExceptionThrowing() { ThrowExceptionOnSave = true };
				newFactory.ExceptionToThrow = CreateNewConcurrencyException(newFactory);
				return newFactory;
			};
			using (GetNewTestEnvironment())
			{
				InitialiseSampleData(Factory);
				InitialiseAndRunTaskSchedule();

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals("Messages created: ", 0, messages.Length - initialMessageCount);
				AssertNotNull("This should blow up, since the log walker should never have concurrency errors here.", ErrorReporter.LastMessageReported);
				ErrorReporter.Instance.Clear();
			}
		}

		public void TestEServicesDelivery()
		{
			var initialMessageCount = Factory.Load<IEDIMessage>(new ZQuery()).Length;

			GetFactoryMethod = delegate
			{ return new TestBusinessObjectFactoryForExceptionThrowing() { ThrowExceptionOnSave = false }; };
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (GetNewTestEnvironment())
			{
				InitialiseSampleData(Factory);
				InitialiseAndRunTaskSchedule();

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals("Messages created: ", 1, messages.Length - initialMessageCount);
			}
		}

		public void TestSeparateDEXEventsCreated()
		{
			GetFactoryMethod = () => new BusinessObjectFactory();

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (GetNewTestEnvironment())
			{
				var communicationMode = GlbCompany.GetCurrentCompany(Factory).OrgProxy.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "DDP_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = "SHP";

				var shipmentType = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
				var shipment = Factory.New(shipmentType);
				shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

				var trigger1 = ((IWorkflowProvider)shipment).WorkflowItems.Milestones.AddNew();
				trigger1.P9_Description = "Send Data";
				trigger1.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
				trigger1.P9_OriginalScheduledDateLocalForBinding = DateTime.Today;

				var notification1 = trigger1.ProcessTaskNotifications.AddNew();
				notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				var shipmentLogs = shipment.GetLogs();
				shipmentLogs.AddNew(Events.Authorised);

				var trigger2 = ((IWorkflowProvider)shipment).WorkflowItems.Milestones.AddNew();
				trigger2.P9_Description = "Send Data";
				trigger2.TriggerConditions.TriggerEventCode = Events.ManifestSubmittedToForwarderCode;
				trigger2.P9_OriginalScheduledDateLocalForBinding = DateTime.Today;

				var notification2 = trigger2.ProcessTaskNotifications.AddNew();
				notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification2.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				shipmentLogs.AddNew(Events.ManifestSubmittedToForwarder);

				Factory.Save();

				InitialiseAndRunTaskSchedule();

				var factory = new BusinessObjectFactory();
				var shipmentReloaded = (BusinessObject)factory.LoadTop1<Forwarding.IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001010"));
				var shipmentLogsReloaded = shipmentReloaded.GetLogs().Find(log => log.SL_SE_NKEvent == Events.DataExportCode).ToArray();
				AssertEquals(2, shipmentLogsReloaded.Length);
				var source1 = shipmentLogsReloaded[0].RelatedEDIMessage.DataContext.ContextKeyValuePairs.First(kvp => kvp.Key == "Data Source Trigger Event").Value;
				var source2 = shipmentLogsReloaded[1].RelatedEDIMessage.DataContext.ContextKeyValuePairs.First(kvp => kvp.Key == "Data Source Trigger Event").Value;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "ATH - Authorised", "MSF - Manifest Submitted To Forwarder" }, new[] { source1, source2 });
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(StmJobQueueSchema.Constants.TableName, null, StmJobQueueSchema.Constants.SJ_Status + "=QUE"),
				};
			}
		}

		IDisposable GetNewTestEnvironment()
		{
			var currentCompanyOrg = Factory.New<OrgHeader>();
			currentCompanyOrg.OH_Code = "~OC";
			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			currentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			currentCompany.GC_OH_OrgProxy = currentCompanyOrg.PK;
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = currentCompany.PK;
			currentBranch.GB_RL_NKHomePort = "AUBNE";
			Factory.Save();

			return DisposableEnvironment.ForBranch(currentBranch.PK.ToGuid());
		}

		TestBusinessObjectFactoryForExceptionThrowing NewFactoryForExceptionThrowing
		{
			get { return new TestBusinessObjectFactoryForExceptionThrowing(); }
		}

		ZSaveConcurrencyException CreateNewConcurrencyException(BusinessObjectFactory factory)
		{
			return new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, null), factory);
		}

		static void InitialiseSampleData(BusinessObjectFactory factory)
		{
			var communicationMode = GlbCompany.GetCurrentCompany(factory).OrgProxy.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationMode.EK_Destination = "DDP_DummyDestination";
			communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationMode.EK_Module = "SHP";

			var shipmentType = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
			var shipment = factory.New(shipmentType);
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Core.Constants.Workflow.MilestoneType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			trigger.P9_OriginalScheduledDateLocalForBinding = DateTime.Today;

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			shipment.GetLogs().AddNew(Events.Authorised);
			factory.Save();
		}
	}
}
