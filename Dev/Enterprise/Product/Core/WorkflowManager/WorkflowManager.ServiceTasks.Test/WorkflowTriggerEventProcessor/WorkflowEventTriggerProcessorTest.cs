using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	[TestedType(typeof(WorkflowEventTriggerProcessor))]
	sealed class WorkflowEventTriggerProcessorTest : LogSubscriberTest<WorkflowEventTriggerProcessor>
	{
		public void TestWTEConcurrencyErrorLog()
		{
			LogUtility.EnableLog(SystemDataRegistry.Instance.LogWalkerLogging, SystemDataRegistry.LogWalkerLoggingKeys.ConcurrencyErrorReport);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "FLD";

			Factory.Save();

			dummy.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();

			var lastSave = false;
			BusinessObjectFactory.SetOnFactorySaveInTransactionForTest(factory =>
			{
				if (factory.NameForDebugging == "Subscriber: WorkflowEventTriggerProcessor" || factory.NameForDebugging == "NewsTransmitter Read Factory")
				{
					if (lastSave)
					{
						lastSave = false;
						throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((INeedRow)dummy).Row, Db.Connection), factory);
					}
					else
					{
						lastSave = true;
					}
				}
			});

			RunLogWalkerCycleForTest();

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			var reportedException = ExceptionReporterTestListener.Instance[0] as AggregateException;
			var exceptionMessage = ExceptionReporterTestListener.Instance.GetExceptionMessage(0);

			AssertContains(
$@"Subscriber Name: WorkflowEventTrigger
Parent: {dummy.PK} Type: {dummy.GetType()}
Job Number: {dummy.JobNumber}
Trigger: {trigger.PK} Code: {trigger.P9_SE_NKMilestoneEvent}
Trigger Actions: FLD", exceptionMessage
			);
			AssertContains("TriggeringBranchCode:", exceptionMessage);
			AssertContains("TriggeringDepartmentCode:", exceptionMessage);
			AssertContains("ContextStaffCode:", exceptionMessage);
			AssertContains("ContextBranchCode:", exceptionMessage);
			AssertContains("ContextDepartmentCode:", exceptionMessage);

			AssertEquals(11, reportedException.InnerExceptions.Where(e => e is ZSaveConcurrencyException).Count());

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestWTEConcurrencyErrorLogForForwardingConsol()
		{
			LogUtility.EnableLog(SystemDataRegistry.Instance.LogWalkerLogging, SystemDataRegistry.LogWalkerLoggingKeys.ConcurrencyErrorReport);
			var forwardingConsol = Factory.New<Forwarding.IForwardingConsol>();
			forwardingConsol.JK_TransportMode = "AIR";
			forwardingConsol.JK_MasterBillNum = "MB1234";
			forwardingConsol.JK_RL_NKLoadPort = "GBLHR";
			forwardingConsol.JK_RL_NKDischargePort = "AUSYD";

			var workflowProvider = (IWorkflowProviderIncludingRelated)forwardingConsol;
			workflowProvider.Logs.AddNew(Events.CustomisableEvent00);

			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "FLD";

			Factory.Save();

			var cusMAWB1 = Factory.New<Customs.AU.ICusMAWB>();
			cusMAWB1.CM_JK = forwardingConsol.PK;
			cusMAWB1.CM_MAWB = "AU1234";
			cusMAWB1.CM_FlightNo = "F1";
			cusMAWB1.CM_ArrivalDate = ZDateTime.Today;

			var cusMAWB2 = Factory.New<Customs.GB.CCSUK.ICusMAWB>();
			cusMAWB2.CM_JK = forwardingConsol.PK;
			cusMAWB2.CM_MAWB = "GB1234";
			cusMAWB2.CM_FlightNo = "F2";
			cusMAWB2.CM_ArrivalDate = ZDateTime.Today;

			Factory.Save();

			var lastSave = false;
			BusinessObjectFactory.SetOnFactorySaveInTransactionForTest(factory =>
			{
				if (factory.NameForDebugging == "Subscriber: WorkflowEventTriggerProcessor" || factory.NameForDebugging == "NewsTransmitter Read Factory")
				{
					if (lastSave)
					{
						lastSave = false;
						throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((INeedRow)forwardingConsol).Row, Db.Connection), factory);
					}
					else
					{
						lastSave = true;
					}
				}
			});

			RunLogWalkerCycleForTest();

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);

			var expectedMessage =
$@"Subscriber Name: WorkflowEventTrigger
Parent: {forwardingConsol.PK} Type: {forwardingConsol.GetType()}
Job Number: {trigger.JobNumber}
Trigger: {trigger.PK} Code: {trigger.P9_SE_NKMilestoneEvent}
Trigger Actions: FLD";
			AssertContains(expectedMessage, ExceptionReporterTestListener.Instance.GetExceptionMessage(0));

			expectedMessage =
$@"Forwarding Consol Details:
JK_MasterBillNum: {forwardingConsol.JK_MasterBillNum}
JK_TransportMode: {forwardingConsol.JK_TransportMode}
JK_RL_NKLoadPort: {forwardingConsol.JK_RL_NKLoadPort}
JK_RL_NKDischargePort: {forwardingConsol.JK_RL_NKDischargePort}

MAWB Details:
CM_PK: {cusMAWB1.PK}
CM_JK: {forwardingConsol.PK}
CM_MAWB: {cusMAWB1.CM_MAWB}
CM_ApplicationCode: {cusMAWB1.CM_ApplicationCode}
CM_FlightNo: {cusMAWB1.CM_FlightNo}
CM_ArrivalDate: {ZDateTime.Today}
Type: {cusMAWB1.GetType()}
RowHashCode: ";
			AssertContains(expectedMessage, ExceptionReporterTestListener.Instance.GetExceptionMessage(0));

			expectedMessage =
$@"CM_PK: {((ICusMAWB)cusMAWB2).PK}
CM_JK: {forwardingConsol.PK}
CM_MAWB: {cusMAWB2.CM_MAWB}
CM_ApplicationCode: {cusMAWB2.CM_ApplicationCode}
CM_FlightNo: {cusMAWB2.CM_FlightNo}
CM_ArrivalDate: {ZDateTime.Today}
Type: {cusMAWB2.GetType()}
RowHashCode: ";
			AssertContains(expectedMessage, ExceptionReporterTestListener.Instance.GetExceptionMessage(0));

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestWTEProcessorShouldNotCreateNewFactoryInDocManagerInfo()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var trigger1 = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "trigger 1";
			trigger1.TriggerConditions.TriggerEventCode = "DDI";

			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = "XML";
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action1.PQ_EmailAddr = "abc@email.com";
			action1.PQ_EmailText = "Test XML";

			var action2 = trigger1.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = "EXL";
			action2.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action2.PQ_EmailAddr = "def@email.com";
			action2.PQ_EmailText = "Test EXL";

			var trigger2 = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "trigger 2";
			trigger2.TriggerConditions.TriggerEventCode = "DEX";

			var document = orgHeader.DocManagerInfo().AddFileOrDocument(new byte[] { 1, 2, 3 }, "ACVPublished.txt", "ACV");
			orgHeader.DocManagerInfo().AddLogsForNewDocument(orgHeader, document);
			orgHeader.DocManagerInfo().MasterFactory.Save();
			Factory.Save();

			AssertEquals(2, orgHeader.WorkflowItems.Triggers.Count);

			var triggerFiredCountdown = 90;
			var needToDoTwice = 0;
			BusinessObjectFactory.SetOnFactorySaveInTransactionForTest(factory =>
			{
				if (needToDoTwice++ > 2)
				{
					return;
				}
				var sql = $"update dbo.ProcessTasks set P9_TriggerFiredCountdown = {triggerFiredCountdown} where P9_SE_NKMilestoneEvent='DEX'";
				Db.Connection.ExecuteNonQuery(sql);
				triggerFiredCountdown--;
			});

			AssertNoExceptionThrown(RunLogWalkerCycleForTest);
		}

		public void TestSearchingForNullReferenceException()
		{
			var processor = new WorkflowEventTriggerProcessor_Testo();
			AssertExceptionThrown(typeof(ArgumentNullException), () => processor.Process_Exposed(Array.Empty<IQueuedLog>()));
			processor.Initialise_Exposed();
			AssertNoExceptionThrown("Now that we init, it shouldn't blow up", () => processor.Process_Exposed(Array.Empty<IQueuedLog>()));
			AssertExceptionThrown(typeof(ArgumentNullException), () => processor.Process_Exposed(null));
			AssertExceptionThrown(typeof(ArgumentNullException), () => processor.Process_Exposed(new IQueuedLog[] { null }));
		}

		public void TestWorkflowScopeRestriction()
		{
			var otherJob = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigga = job.WorkflowItems.Triggers.AddNew();
			trigga.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigga.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "FLD";
			job.Logs.AddNew(Events.CustomisableEvent00);
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			Factory.Save();
			var log = trigga.Logs.GetAllLogs().OfType<StmALog>().Single(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			Factory.Save();
			otherJob.SetWorkflowTemplateScopeToMessage();
			var processor = new WorkflowEventTriggerProcessor_Testo();
			var queuedLog = new QueuedLogForTesting(log, trigga);
			AssertEquals("Precondition", false, job.GetWorkflowTemplateScopeEnforcer().ShouldApplyTemplate(job, template, Factory));
			processor.Initialise_Exposed();
			processor.Process_Exposed(queuedLog);
			Assert("Pre:condition.", !job.HasChanges);
			AssertEquals("Expecting that after running the service task we allow template application in this factory.", true, job.GetWorkflowTemplateScopeEnforcer().ShouldApplyTemplate(job, template, Factory));
		}

		[Serializable]
		class WorkflowEventTriggerProcessor_Testo : WorkflowEventTriggerProcessor
		{
			public void Process_Exposed(params IQueuedLog[] logs)
			{
				this.ProcessBatch(logs);
			}

			public void Initialise_Exposed()
			{
				this.SetDefaultLogger(this.DefaultLogger);
			}
		}

		public void TestTriggersWithDifferentDescriptionAndSameAction()
		{
			var dummyBizO = Factory.New<DummyWithWorkflow>();

			var trigger1 = dummyBizO.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "trigger 1";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action1.PQ_EmailAddr = "abc@email.com";
			action1.PQ_EmailText = "Test LWK";

			var trigger2 = dummyBizO.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "trigger 2";
			trigger2.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			var action2 = trigger2.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action2.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action2.PQ_EmailAddr = "abc@email.com";
			action2.PQ_EmailText = "Test LWK";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			dummyBizO.Logs.AddNew(Events.CustomsEntryStatus);

			Factory.Save();

			AssertEquals(2, dummyBizO.WorkflowItems.Triggers.Count);

			AssertContains("Precondition: All Event Types on DummyBO", Events.CustomsEntryStatus.Code, GetAllEventTypesApplied(dummyBizO));

			RunLogWalkerCycleForTest();

			AssertEquals("GIVEN 2 triggers with identical actions, WHEN running LogWalker, each action should fire i.e. 2 emails sent", 2, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertContainsExactElementsInAnyOrder("2 emails with subject contains trigger description should be sent",
				new[] { "Dummy Task Provider - trigger 1", "Dummy Task Provider - trigger 2" },
				Env.OutgoingMailManager.EmailsCreated.Select(e => e.Subject));
		}

		public void TestDoNotLoadTheStmALog()
		{
			var dummyBizO = Factory.New<DummyWithWorkflow>();

			var trigger1 = dummyBizO.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "trigger 1";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			var fieldName = DummyBizoSchema.Z0_NVarCharMax.Name;
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action1.PQ_FieldName = fieldName;
			action1.PQ_FieldValue = "Doop";

			dummyBizO.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();

			using (AssertDbHitsForAllFactories(new Dictionary<string, int> { { StmALogSchema.Constants.TableName, 0 } }, ignoreUnspecified: true))
			{
				AssertNoExceptionThrown(RunLogWalkerCycleForTest);
			}
		}

		public void TestTriggerDeletedRace()
		{
			var dummyBizO = Factory.New<DummyWithWorkflow>();

			var trigger1 = dummyBizO.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "trigger 1";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action1.PQ_EmailAddr = "abc@email.com";
			action1.PQ_EmailText = "Test LWK";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			dummyBizO.Logs.AddNew(Events.CustomsEntryStatus);

			Factory.Save();

			AssertEquals(1, dummyBizO.WorkflowItems.Triggers.Count);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var trigger = (ProcessTask)newFactory.Load("P9", trigger1.PK);
			trigger.Delete();
			newFactory.Save();

			AssertNoExceptionThrown(RunLogWalkerCycleForTest);
		}

		public void TestMilestoneMustHaveDate()
		{
			var dummyBizO = Factory.New<DummyWithWorkflow>();

			var milestone = dummyBizO.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "milestone 1";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			var action1 = milestone.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action1.PQ_EmailAddr = "abc@email.com";
			action1.PQ_EmailText = "Test LWK";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			dummyBizO.Logs.AddNew(Events.CustomsEntryStatus);

			Factory.Save();
			AssertEquals(1, dummyBizO.WorkflowItems.Milestones.Count);

			milestone.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "Did not fire event as [{0}] has empty event date.", milestone.HumanReadableName), Notifier.ToString());
		}

		public void TestTriggersMustHaveDate_ItIsTooLateToCancelYourEvent()
		{
			var dummyBizO = Factory.New<DummyWithWorkflow>();

			var trigger1 = dummyBizO.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "trigger 1";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action1.PQ_EmailAddr = "abc@email.com";
			action1.PQ_EmailText = "Test LWK";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var log = dummyBizO.Logs.AddNew(Events.CustomsEntryStatus);

			Factory.Save();
			AssertEquals(1, dummyBizO.WorkflowItems.Triggers.Count);

			log.Cancel();
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestImmediateFieldChangeIsIgnoredByLogWalker()
		{
			var dummyBizO = Factory.New<DummyWithWorkflow>();
			dummyBizO.Z0_IsSystem = false;

			var fieldName = DummyBizoSchema.Z0_NVarCharMax.Name;
			//var fieldName = DummyBizoSchema.Z0_Description.Name;

			var defaultFieldValue = "Some default value to ensure everything is deterministic...";
			dummyBizO.Z0_NVarCharMax = defaultFieldValue;

			// Make sure ImmediateFieldChange is ignored
			var ifcFieldValue = "IFC IFC IFC IFC";
			var trigger1 = dummyBizO.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Trigger 1";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = fieldName;
			action1.PQ_FieldValue = ifcFieldValue;
			Factory.Save();

			dummyBizO.Logs.AddNew(Events.CustomisableEvent01); // This should trigger ImmediateFieldChange without running LogWalker
			AssertEquals(ifcFieldValue, dummyBizO.Z0_NVarCharMax);
			dummyBizO.Z0_NVarCharMax = "";

			Factory.Save();

			RunLogWalkerCycleForTest();

			// Make sure IFC was ignored by log walker (by checking the bizo value)
			dummyBizO = Factory.Load<DummyWithWorkflow>(dummyBizO.PK);
			dummyBizO.Reload();
			AssertEquals("", dummyBizO.Z0_NVarCharMax);
		}

		public void TestImmediateFieldChangeDoesNotAffectOtherActions()
		{
			var dummyBizO = Factory.New<DummyWithWorkflow>();
			dummyBizO.Z0_IsSystem = false;

			var fieldName = DummyBizoSchema.Z0_NVarCharMax.Name;

			var defaultFieldValue = "Some default value to ensure everything is deterministic...";
			dummyBizO.Z0_NVarCharMax = defaultFieldValue;

			// Make sure ImmediateFieldChange is ignored
			var ifcFieldValue = "IFC IFC IFC IFC";
			var trigger1 = dummyBizO.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Trigger 1";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = fieldName;
			action1.PQ_FieldValue = ifcFieldValue;
			Factory.Save();

			// Make sure SetField takes effect
			var fldFieldValue = "FLD FLD FLD FLD";
			var trigger2 = dummyBizO.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "Trigger 2";
			trigger2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02.Code;
			var action2 = trigger2.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action2.PQ_FieldName = fieldName;
			action2.PQ_FieldValue = fldFieldValue;
			Factory.Save();

			dummyBizO.Logs.AddNew(Events.CustomisableEvent01); // This should trigger ImmediateFieldChange without running LogWalker
			AssertEquals(ifcFieldValue, dummyBizO.Z0_NVarCharMax);

			dummyBizO.Logs.AddNew(Events.CustomisableEvent02);
			Factory.Save();

			RunLogWalkerCycleForTest();

			// Need to reload the bizo in order to get the latest values
			var updatedDummyBizO = Factory.Load<DummyWithWorkflow>(dummyBizO.PK);
			updatedDummyBizO.Reload();
			AssertEquals(fldFieldValue, updatedDummyBizO.Z0_NVarCharMax);
		}

		public void TestEmailNotificationIsIncompatibleWithEAdaptorDeliveryModeTransport()
		{
			/// We should not be able to send an email notification to consignee via the eAdaptor interface.
			/// To test this, we need the following conditions met:
			///		a) eAdapter Outbound Service URL is set
			///		b) Consignee Organisation with an EDI Communcations Mode, with Transport = eAdaptor Interface (EDP)
			///		c) Shipment with the Organisation as Consignee and a Workflow Trigger that has a Trigger Action = Send Notification Email (NTF), Recipient = Consignee (CNE)
			///		d) Fire the trigger and run log walker
			///		
			///		Running log walker should log an error saying no email sent because NTF is incompatible with EDP

			// a) eAdapter Outbound Service URL is set
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				// b) Consignee Organisation with an EDI Communcations Mode, with Transport = eAdaptor Interface (EDP)
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "DDD";

				var communicationMode = org.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "DDP_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
				communicationMode.EK_Module = "SHP";

				// c) Shipment with the Organisation as Consignee and a Workflow Trigger that has a Trigger Action = Send Notification Email (NTF), Recipient = Consignee (CNE)
				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;

				string desc = "Try to send email";
				var trigger1 = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
				trigger1.P9_Description = desc;
				trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

				string triggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
				string triggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
				var action = trigger1.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = triggerType;
				action.PQ_TriggerParty = triggerParty;

				Factory.Save();

				// d) Fire the trigger and run log walker
				var log = ((BusinessObject)shipment).GetLogs().AddNew(Events.CustomisableEvent00);
				Factory.Save();
				RunLogWalkerCycleForTest();

				CombineAssertions(() =>
				{
					AssertEquals("Error Reporter should have no errors", 0, ErrorReporter.TotalErrorCount);
					AssertEquals("No emails should have been created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
					AssertEquals("Unknown Sub Type error should not have been logged", 0, NotifiedEventList.Count(s => s.Contains("EDI Messages with unknown Sub Type can't be created")));
					string compatibleModesError = $"Action [Type=NTF,Recipient=CNE,Purpose=] failed for trigger";
					AssertContains("NTF is incomptabile for EDP should have been logged", compatibleModesError, string.Join(System.Environment.NewLine, NotifiedEventList.ToArray()));
				});
			}
		}

		#region Recursive Relationships

		[TestDate(2000, 1, 1, 1, 0, 0)]
		public void TestDDATriggerWithEDCAction()
		{
			//If this unit test is suddenly failing after changing reference format for DDA event, check out ProcessTask.IsSourceLogDocumentEqualsToActionDocuments()
			var job = TasksProvider;

			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentAllocatedCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			action.PQ_SU_Document = DocumentCommand.PK;

			Factory.Save();

			var log = ((EnterpriseBusinessObject)job).Logs.AddNew(Events.DocumentAllocated, string.Concat("HBL|", Guid.NewGuid()));

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			((LoggerForTesting)Notifier).Clear();
			RunLogWalkerCycleForTest();

			var actualMessage = string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]")));

			AssertContains("GIVEN DDA Trigger with EDC action and DAL docType, WHEN event occurred with different docType i.e. HBL, should proces 1 trigger",
				"[WorkflowEventTrigger] [Default] Processing: Shipment S00001000", actualMessage);

			AssertContains("Last Fired Time: 01-Jan-00 01:00:00", actualMessage);

			AssertContains(string.Format(@"Task {0}", trigger.P9_TaskID).Trim(), actualMessage);

			AssertContains(string.Format(@"Parent ID: {0}", trigger.Parent.PK).Trim(), actualMessage);

			AssertContains(string.Format(@"PK: {0}", trigger.PK).Trim(), actualMessage);

			Factory.Save();

			var printJob = Factory.LoadTop1<IStmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, job.PK));
			AssertNotNull("Document delivered", printJob);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5); // 5 minutes later so we can assure no 1 minute deduplication in ProcessTask.P9_ActualDateInternal set

			var docJobTaskForTest = ObjectFactory.Get<IDocumentJobTaskForTest>();
			docJobTaskForTest.RunTask();

			Factory.Save();

			((LoggerForTesting)Notifier).Clear();
			RunLogWalkerCycleForTest();
			CombineAssertions("GIVEN DDA trigger with EDC action (recursive), WHEN calling LWK-DOD-LWK-DOD-... should not cause infinite loop", delegate
			{
				AssertMultilineASCIIEquals("Notifier Text starting with [WorkflowEventTrigger]", @"".Trim(), string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]"))).StripGUIDs());
			});
		}

		[TestDate(2000, 1, 1, 1, 0, 0)]
		public void TestDDITriggerWithEDCAction()
		{
			var job = TasksProvider;

			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			action.PQ_SU_Document = DocumentCommand.PK;

			Factory.Save();

			var log = ((EnterpriseBusinessObject)job).Logs.AddNew(Events.DocumentImported, string.Concat("DAL|", Guid.NewGuid()));

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			((LoggerForTesting)Notifier).Clear();
			RunLogWalkerCycleForTest();

			var actualMessage = string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]")));

			AssertContains("GIVEN DDI Trigger with EDC action and DAL docType, WHEN DDI event occurred with same DAL docType ,should proces 1 trigger because it wont cause infinite loop.",
				"[WorkflowEventTrigger] [Default] Processing: Shipment S00001000", actualMessage);

			AssertContains("Last Fired Time: 01-Jan-00 01:00:00", actualMessage);

			AssertContains(string.Format(@"Task {0}", trigger.P9_TaskID).Trim(), actualMessage);

			AssertContains(string.Format(@"Parent ID: {0}", trigger.Parent.PK).Trim(), actualMessage);

			AssertContains(string.Format(@"PK: {0}", trigger.PK).Trim(), actualMessage);
		}

		[TestDate(2000, 1, 1, 1, 0, 0)]
		public void TestTwoTriggersCannotCallEachOtherRecursively()
		{
			GetDummyWorkflowDescriptorSetupForChainTesting();
			RunLogWalkerCycleForTest(); // Run once to set high watermark.

			var dummyBO = Factory.New<DummyWithWorkflow>();

			var cadTrigger = AddNewTrigger(dummyBO, Events.CargoReceivedAtDepot, Events.CargoCheckin);
			var cciTrigger = AddNewTrigger(dummyBO, Events.CargoCheckin, Events.CargoReceivedAtDepot);

			dummyBO.Logs.AddNew(Events.CargoReceivedAtDepot);

			Factory.Save();

			AssertContains("Precondition: All Event Types on DummyBO", "CAD", GetAllEventTypesApplied(dummyBO));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			AssertEquals(new ZDateTime(TestDateAttribute.Date), new ZDateTime(2000, 1, 1, 1, 1, 0));

			((LoggerForTesting)Notifier).Clear();
			RunLogWalkerCycleForTest();

			var actualMessage = string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]")));

			CombineAssertions("Cycle 1", delegate
			{
				AssertContains("Notifier Text starting with [WorkflowEventTrigger]",
					"[WorkflowEventTrigger] [Default] Processing: Dummy Business Object Default", actualMessage);

				AssertContains(string.Format(@"Task {0} Cargo Check-in", cciTrigger.P9_TaskID).Trim(), actualMessage);

				AssertContains(string.Format(@"Parent ID: {0}", cciTrigger.Parent.PK).Trim(), actualMessage);

				AssertContains(string.Format(@"PK: {0}", cciTrigger.PK).Trim(), actualMessage);

				AssertContains("Last Fired Time: 01-Jan-00 01:00:00", actualMessage);

				AssertContains("[WorkflowEventTrigger] [Default] Adding Event Type [CCI]...", actualMessage);

				AssertContains("[WorkflowEventTrigger] [Default] recurring with subscriber [WorkflowEventTrigger] and depth 1 for 1 logged event(s).", actualMessage);

				AssertContains(string.Format(@"Task {0} Cargo Received at Depot", cadTrigger.P9_TaskID).Trim(), actualMessage);

				AssertContains(string.Format(@"Parent ID: {0}", cadTrigger.Parent.PK).Trim(), actualMessage);

				AssertContains(string.Format(@"PK: {0}", cadTrigger.PK).Trim(), actualMessage);

				AssertContains("Last Fired Time: 01-Jan-00 01:01:00", actualMessage);

				AssertContains("[WorkflowEventTrigger] [Default] Adding Event Type [CAD]...", actualMessage);

				AssertEquals("All Event Types on DummyBO", "CAD, CAD, CCI, WTM", GetAllEventTypesApplied(dummyBO));
			});

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			AssertEquals(new ZDateTime(TestDateAttribute.Date), new ZDateTime(2000, 1, 1, 1, 2, 0));

			((LoggerForTesting)Notifier).Clear();
			RunLogWalkerCycleForTest();

			CombineAssertions("Cycle 2", delegate
			{
				AssertMultilineASCIIEquals("Notifier Text starting with [WorkflowEventTrigger]", string.Empty, string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]"))));
				AssertEquals("All Event Types on DummyBO", "CAD, CAD, CCI, WTM", GetAllEventTypesApplied(dummyBO));
			});

			((LoggerForTesting)Notifier).Clear();
			RunLogWalkerCycleForTest();

			CombineAssertions("Cycle 3", delegate
			{
				AssertMultilineASCIIEquals("Notifier Text starting with [WorkflowEventTrigger]", @"
".Trim(), string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]"))).StripGUIDs());
				AssertEquals("All Event Types on DummyBO", "CAD, CAD, CCI, WTM", GetAllEventTypesApplied(dummyBO));
			});

			((LoggerForTesting)Notifier).Clear();
			RunLogWalkerCycleForTest();

			CombineAssertions("Cycle 4", delegate
			{
				AssertMultilineASCIIEquals("Notifier Text starting with [WorkflowEventTrigger]", @"
".Trim(), string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]"))).StripGUIDs());
				AssertEquals("All Event Types on DummyBO", "CAD, CAD, CCI, WTM", GetAllEventTypesApplied(dummyBO));
			});
		}

		[TestDate(2000, 1, 1, 1, 0, 0)]
		public void TestOneTriggerCanApplyALogFiringAnotherTrigger()
		{
			GetDummyWorkflowDescriptorSetupForChainTesting();
			RunLogWalkerCycleForTest(); // Run once to set high watermark.

			var dummyBO = Factory.New<DummyWithWorkflow>();

			var trigger1 = AddNewTrigger(dummyBO, Events.CargoReceivedAtDepot, Events.CargoCheckin);
			var trigger2 = AddNewTrigger(dummyBO, Events.CargoCheckin, Events.CargoAvailable);
			var trigger3 = AddNewTrigger(dummyBO, Events.CargoAvailable, Events.Delivered);

			dummyBO.Logs.AddNew(Events.CargoReceivedAtDepot);

			Factory.Save();

			AssertContains("Precondition: All Event Types on DummyBO", "CAD", GetAllEventTypesApplied(dummyBO));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			((LoggerForTesting)Notifier).Clear();
			RunLogWalkerCycleForTest();

			var actualMessage = string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]")));

			CombineAssertions("Cycle 1", delegate
			{
				AssertContains("Notifier Text starting with [WorkflowEventTrigger]", "[WorkflowEventTrigger] [Default] Processing: Dummy Business Object Default", actualMessage);

				AssertContains(string.Format(@"Task {0} Cargo Received at Depot", trigger1.P9_TaskID).Trim(), actualMessage);

				AssertContains(string.Format(@"Parent ID: {0}", trigger1.Parent.PK).Trim(), actualMessage);

				AssertContains(string.Format(@"PK: {0}", trigger1.PK).Trim(), actualMessage);

				AssertContains("Last Fired Time: 01-Jan-00 01:00:00", actualMessage);

				AssertContains("[WorkflowEventTrigger] [Default] Adding Event Type [CCI]...", actualMessage);

				AssertContains("[WorkflowEventTrigger] [Default] recurring with subscriber [WorkflowEventTrigger] and depth 1 for 1 logged event(s).", actualMessage);

				AssertContains(string.Format(@"Task {0} Cargo Check-in", trigger2.P9_TaskID).Trim(), actualMessage);

				AssertContains(string.Format(@"Parent ID: {0}", trigger2.Parent.PK).Trim(), actualMessage);

				AssertContains(string.Format(@"PK: {0}", trigger2.PK).Trim(), actualMessage);

				AssertContains("[WorkflowEventTrigger] [Default] Adding Event Type [CAV]...", actualMessage);

				AssertContains("[WorkflowEventTrigger] [Default] recurring with subscriber [WorkflowEventTrigger] and depth 2 for 1 logged event(s).", actualMessage);

				AssertContains(string.Format(@"Task {0} Cargo Available", trigger3.P9_TaskID).Trim(), actualMessage);

				AssertContains(string.Format(@"Parent ID: {0}", trigger3.Parent.PK).Trim(), actualMessage);

				AssertContains(string.Format(@"PK: {0}", trigger3.PK).Trim(), actualMessage);

				AssertContains("[WorkflowEventTrigger] [Default] Adding Event Type [DLV]...", actualMessage);

				AssertEquals("All Event Types on DummyBO", "CAD, CAV, CCI, DLV, WTM", GetAllEventTypesApplied(dummyBO));
			});

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			AssertEquals(new ZDateTime(TestDateAttribute.Date), new ZDateTime(2000, 1, 1, 1, 2, 0));

			((LoggerForTesting)Notifier).Clear();
			RunLogWalkerCycleForTest();

			Factory.Save();

			var loadedTrigger2 = Factory.Load<ProcessTask>(trigger2.PK);

			CombineAssertions("Cycle 2", delegate
			{
				AssertMultilineASCIIEquals("Notifier Text starting with [WorkflowEventTrigger]", string.Empty, string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]"))));
				AssertEquals("All Event Types on DummyBO", "CAD, CAV, CCI, DLV, WTM", GetAllEventTypesApplied(dummyBO));
			});
		}

		[TestDate(2000, 1, 1, 1, 0, 0)]
		public void TestTriggerCannotFireItself()
		{
			GetDummyWorkflowDescriptorSetupForChainTesting();
			var dummyBO = Factory.New<DummyWithWorkflow>();

			var trigger = AddNewTrigger(dummyBO, Events.CargoReceivedAtDepot, Events.CargoReceivedAtDepot);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			dummyBO.Logs.AddNew(Events.CargoReceivedAtDepot);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			AssertContains("Precondition: All Event Types on DummyBO", "CAD", GetAllEventTypesApplied(dummyBO));

			RunLogWalkerCycleForTest();

			var actualMessage = string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]")));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			CombineAssertions("Cycle 1", delegate
			{
				AssertContains("Notifier Text starting with [WorkflowEventTrigger] and WTE infinite feedback should not occur because HasWTEEventFromCurrentChainCore in ProcessTask.AddWorkflowLogIfRequired stopped its creation",
				string.Format(@"[WorkflowEventTrigger] [Default] Processing: Dummy Business Object Default").Trim(), actualMessage);

				AssertContains("Last Fired Time: 01-Jan-00 01:01:00", actualMessage);

				AssertContains(string.Format(@"Task {0} Cargo Received at Depot", trigger.P9_TaskID).Trim(), actualMessage);

				AssertContains(string.Format(@"Parent ID: {0}", trigger.Parent.PK).Trim(), actualMessage);

				AssertContains(string.Format(@"PK: {0}", trigger.PK).Trim(), actualMessage);

				AssertContains("[WorkflowEventTrigger] [Default] Adding Event Type [CAD]...", actualMessage);

				AssertEquals("All Event Types on DummyBO", "CAD, CAD, WTM", GetAllEventTypesApplied(dummyBO));
			});

			((LoggerForTesting)Notifier).Clear();
			RunLogWalkerCycleForTest();

			actualMessage = string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]")));

			CombineAssertions("Cycle 2", delegate
			{
				AssertMultilineASCIIEquals("Notifier Text starting with [WorkflowEventTrigger]", @"
".Trim(), actualMessage);
				AssertEquals("All Event Types on DummyBO", "CAD, CAD, WTM", GetAllEventTypesApplied(dummyBO));
			});
		}

		#endregion

		#region Logging

		public void TestAppendToNotificationLog()
		{
			DummyWorkflowDescriptor.Instance.WorkflowTriggerActionTypeList.AddPair("MEH", "MEH MEH Action");
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.FillWithValidTestData();
			staff.GS_EmailAddress = "workflowstuff@cargowise.com";
			Factory.Save();

			WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Dummy.Z0_Description = "DUM123";
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.Authorised.Code;
			ProcessTaskNotification notification = milestone.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "MEH";
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			Factory.Save();

			DummyIProcessor dummyProcessor = new DummyIProcessor();
			dummyProcessor.AddToNotificationLog =
				notifications =>
				{
					notifications.Add(CargoWise.EntityFramework.NotificationType.Error, "test message");
				};
			DummyWorkflowDescriptor.Instance.WorkflowTriggerActionProcessorOverride = dummyProcessor;

			RunLogWalkerCycleForTest();
			Assert(((LoggerForTesting)Notifier).NotifiedEventList.Contains("[WorkflowEventTrigger] [Default] test message"));
		}

		public void TestYieldDueToSave()
		{
			DummyWorkflowDescriptor.Instance.WorkflowTriggerActionTypeList.AddPair("MEH", "MEH MEH Action");
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.FillWithValidTestData();
			staff.GS_EmailAddress = "workflowstuff@cargowise.com";
			Factory.Save();

			WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Dummy.Z0_Description = "DUM123";
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.Authorised.Code;
			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "MEH";

			Dummy.Logs.AddNew(Events.Authorised);
			Dummy.Logs.AddNew(Events.Authorised);
			Dummy.Logs.AddNew(Events.Authorised);
			Dummy.Logs.AddNew(Events.Authorised);
			Dummy.Logs.AddNew(Events.Authorised);
			Factory.Save();

			var counter = 0;

			DummyWorkflowDescriptor.Instance.WorkflowTriggerActionProcessorOverride = new DummyIProcessor
			{
				AddToNotificationLog = notifications =>
				{
					counter++;
					Factory.Save();
				}
			};

			RunLogWalkerCycleForTest();
			Assert(string.Join(System.Environment.NewLine, ((LoggerForTesting)Notifier).NotifiedEventList).Contains("yielding batch due to save."));
			AssertEquals("Enough logs should be processed", 5, counter);
		}

		[TestDate(2000, 1, 1, 1, 0, 0)]
		public void TestLWKLogShowReferenceNumber_PK()
		{
			GetDummyWorkflowDescriptorSetupForChainTesting();
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflow);

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();

			trigger.P9_Description = "Simple Dummy Workflow TRG";
			trigger.TriggerConditions.TriggerEventCode = Events.CargoReceivedAtDepot.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = Events.CargoReceivedAtDepot.Code;

			dummyBO.Logs.AddNew(Events.CargoReceivedAtDepot);

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			RunLogWalkerCycleForTest();

			var actualMessage = string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]")));

			CombineAssertions("Cycle 1", delegate
			{
				AssertContains("GIVEN object without ReferenceNumber and raised event, WHEN LWK runs then object's PK should be shown.", "[WorkflowEventTrigger] [Default] Processing: Dummy Business Object Default", actualMessage);

				AssertContains("Last Fired Time: 01-Jan-00 01:00:00", actualMessage);

				AssertContains(string.Format(@"Task {0} Simple Dummy Workflow TRG", trigger.P9_TaskID).Trim(), actualMessage);

				AssertContains(string.Format(@"Parent ID: {0}", trigger.Parent.PK).Trim(), actualMessage);

				AssertContains(string.Format(@"PK: {0}", trigger.PK).Trim(), actualMessage);

				AssertContains("[WorkflowEventTrigger] [Default] Adding Event Type [CAD]...", actualMessage);
			});
		}

		[TestDate(2000, 1, 1, 1, 0, 0)]
		public void TestEnhancedLogging()
		{
			((LoggerForTesting)Notifier).AllowDebug = true;

			GetDummyWorkflowDescriptorSetupForChainTesting();
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();

			trigger.P9_Description = "Simple Dummy Workflow TRG";
			trigger.TriggerConditions.TriggerEventCode = Events.CargoReceivedAtDepot.Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = Events.CargoReceivedAtDepot.Code;

			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = Events.CargoReceivedAtDepot.Code;

			dummyBO.Logs.AddNew(Events.CargoReceivedAtDepot);

			Factory.Save();
			dummyBO.Logs.AddNew(Events.CustomisableEvent04); // Adding log to trick db hit counter.

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			RunLogWalkerCycleForTest();

			var actualMessage = string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]")));

			CombineAssertions("Cycle 1", delegate
			{
				AssertContains("GIVEN object without ReferenceNumber and raised event, WHEN LWK runs then object's PK should be shown.", String.Format(@"
[WorkflowEventTrigger] [Default] processing 1 logged event(s).
[WorkflowEventTrigger] [Default] Processing: Dummy Business Object Default
Task {0} Simple Dummy Workflow TRG", trigger.P9_TaskID).Trim(), new Regex("[0-9]+ objects loaded, [0-9]+ table selects").Replace(actualMessage, ""));

				AssertContains(string.Format(@"Parent ID: {0}", trigger.Parent.PK).Trim(), actualMessage);

				AssertContains(string.Format(@"PK: {0}", trigger.PK).Trim(), actualMessage);

				AssertContains(@"[WorkflowEventTrigger] [Default] Action types in this batch: CAD, CAD
[WorkflowEventTrigger] [Default] Firing action: CAD
[WorkflowEventTrigger] [Default] Adding Event Type [CAD]...
[WorkflowEventTrigger] [Default] Action completed: CAD ()
[WorkflowEventTrigger] [Default] Firing action: CAD
[WorkflowEventTrigger] [Default] Adding Event Type [CAD]...
[WorkflowEventTrigger] [Default] Action completed: CAD ()
[WorkflowEventTrigger] [Default] finished processing logs.", new Regex("[0-9]+ objects loaded, [0-9]+ table selects").Replace(actualMessage, ""));
			});
		}

		[TestDate(2000, 1, 1, 1, 0, 0)]
		public void TestLWKLogShowReferenceNumber_JobNumber()
		{
			GetDummyWorkflowDescriptorSetupForChainTesting();
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP001";

			var bizObj = (EnterpriseBusinessObject)shipment;
			var trigger = MasterFilesTestHelper.CreateTrigger((IWorkflowProvider)bizObj, Events.CargoReceivedAtDepot);
			MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.Email, emailAddress: "test@test.com");

			bizObj.Logs.AddNew(Events.CargoReceivedAtDepot);

			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			RunLogWalkerCycleForTest();

			AssertContains("GIVEN shipment with JS_UniqueConsignRef and raised event, WHEN LWK runs then Shipment JS_UniqueConsignRef should be shown instead of PK.", string.Format(@"
[WorkflowEventTrigger] [Default] Processing: Shipment SHP001", trigger.Parent.PK, trigger.PK).Trim(), string.Join("\r\n", ((LoggerForTesting)Notifier).NotifiedEventList.Where(o => o.StartsWith("[WorkflowEventTrigger]"))));
		}

		#endregion

		#region No De-Duplication Permutations

		[TestDateIncremental(0, 0, 1)]
		public void TestRunWorkflowMilestone_WithUniqueTriggerTypesInEachBatch()
		{
			var message = "Each action for the batch is unique.";
			AssertRunWorkflowTriggerNoDeduplication(message);
		}

		[TestDateIncremental(0, 0, 1)]
		public void TestRunWorkflowMilestone_WithDuplicatedTriggerTypesInEachBatch()
		{
			var message = "Some duplicate actions in each batch.";
			AssertRunWorkflowTriggerNoDeduplication(message, true);
		}

		void AssertRunWorkflowTriggerNoDeduplication(string message, bool duplicates = false)
		{
			var milestones = new List<ProcessTask>();

			var milestonesCount = 50;

			for (var index = 0; index < milestonesCount; index++)
			{
				var milestone = Dummy.WorkflowItems.Milestones.AddNew();
				var notification = milestone.ProcessTaskNotifications.AddNew(); // add processTaskNotification before setting MilestoneEvent, otherwise no WTE is created
				milestone.P9_Description = string.Format("milestone-{0}", index);
				milestone.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;

				var notificationTypeFlag = duplicates ? index % 5 : index % 10;
				switch (notificationTypeFlag)
				{
					case 0:
						notification.PQ_TriggerType = "XML";
						break;
					case 1:
						notification.PQ_TriggerType = "XM1";
						break;
					case 2:
						notification.PQ_TriggerType = "XM2";
						break;
					case 3:
						notification.PQ_TriggerType = "XM3";
						break;
					case 4:
						notification.PQ_TriggerType = "XM4";
						break;
					case 5:
						notification.PQ_TriggerType = "XM5";
						break;
					case 6:
						notification.PQ_TriggerType = "XM6";
						break;
					case 7:
						notification.PQ_TriggerType = "XM7";
						break;
					case 8:
						notification.PQ_TriggerType = "XM8";
						break;
					default:
						notification.PQ_TriggerType = "MEH";
						break;
				}

				milestone.SetMilestoneActualDateForTest(ZDateTime.Now);

				milestones.Add(milestone);
			}

			Factory.Save();
			AssertEquals(string.Format("{0} milestones should be created", milestonesCount), milestonesCount, milestones.Count);

			foreach (var milestone in milestones)
			{
				var wteLogs = milestone.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code));
				AssertEquals("GIVEN 50 milestones, WHEN event ADD triggerred THEN each milestone should only create 1 WTE log", 1, wteLogs.Length);
			}

			RunLogWalkerCycleForTest();

			AssertEquals(string.Format(@"GIVEN batch-size = 10 and 50 of the same triggers with 10 different TriggerType variations (XML, XM1, XM2, etc)
				WHEN running RunLogWalkerCycle
				THEN WorkflowTriggerActionRunCount should be 50
				BECAUSE deduplication was removed in Changeset 205112 on the 22/4/16.
				{0}", message),
				50,
				DummyWorkflowDescriptor.Instance.WorkflowTriggerActionRunCount);
		}

		#endregion

		#region Trigger Actions

		public void TestRunSendDocumentsWorkflowTrigger()
		{
			ProcessTask milestone = TasksProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			ProcessTaskNotification notification = milestone.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "DOC";
			notification.PQ_SU_Document = DocumentCommand.PK;
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			Factory.Save();

			RunLogWalkerCycleForTest();
			IStmPrintJob printJob = Factory.LoadTop1<IStmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, TasksProvider.PK));
			AssertNotNull("Document delivered", printJob);
		}

		#endregion

		#region Not Actually Testing Anything

		public void TestRunWorkflowTrigger_NoEvent()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTaskNotification notification = milestone.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "XML";
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);

			RunLogWalkerCycleForTest();
			AssertEquals("Workflow trigger not run - no milestone event", 0, DummyWorkflowDescriptor.Instance.WorkflowTriggerActionRunCount);
		}

		#endregion

		#region Standard Processing

		[TestDate(2008, 1, 1)]
		public void TestLogsOrder()
		{
			const int NumberOfTimes2TriggersShouldBeProcessed = 2;

			Factory.RefreshEnabled = false;

			ProcessTask trigger1 = Dummy.WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			trigger1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger1.TriggerConditions.TriggerConditionValue = "CIO";
			ProcessTaskNotification triggerNotification1 = trigger1.ProcessTaskNotifications.AddNew();
			triggerNotification1.PQ_TriggerType = "XML";

			ProcessTask trigger2 = Dummy.WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			ProcessTaskNotification triggerNotification3 = trigger2.ProcessTaskNotifications.AddNew();
			triggerNotification1.PQ_TriggerType = "XML";
			for (int i = 1; i <= 10; i++)
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1.1);
				Dummy.Logs.AddNew(Events.CustomsEntryStatus, "CIO");
				Factory.Save();

				RunLogWalkerCycleForTest();
				AssertEquals(NumberOfTimes2TriggersShouldBeProcessed * i, DummyWorkflowDescriptor.Instance.WorkflowTriggerActionRunCount);
				trigger1.Reload();
				trigger2.Reload();
				AssertEquals("Trigger Fired", trigger1.P9_Description);
				AssertEquals("Trigger Fired", trigger2.P9_Description);
			}
		}

		#endregion

		#region Trigger Conditions

		[TestDate(2008, 1, 1)]
		public void TestRunWorkflowTriggerWithAdditionalActionCondition()
		{
			Factory.RefreshEnabled = false;

			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestone.TriggerConditions.TriggerConditionValue = "CLR";
			ProcessTaskNotification milestoneNotification = milestone.ProcessTaskNotifications.AddNew();
			milestoneNotification.PQ_TriggerType = "XML";

			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger.TriggerConditions.TriggerConditionValue = "CSI";
			ProcessTaskNotification triggerNotification = trigger.ProcessTaskNotifications.AddNew();
			triggerNotification.PQ_TriggerType = "XML";

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "MEH");
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals(0, DummyWorkflowDescriptor.Instance.WorkflowTriggerActionRunCount);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1.1);
			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "CSI");
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals(1, DummyWorkflowDescriptor.Instance.WorkflowTriggerActionRunCount);
			trigger.Reload();
			AssertEquals("Trigger Fired", trigger.P9_Description);
			AssertEquals("", milestone.P9_Description);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1.1);
			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "CLR");
			Factory.Save();
			RunLogWalkerCycleForTest();
			AssertEquals(2, DummyWorkflowDescriptor.Instance.WorkflowTriggerActionRunCount);
			milestone.Reload();
			AssertEquals("Trigger Fired", milestone.P9_Description);
		}

		#endregion

		#region Edge-Case Handling

		public void TestRunWorkflowTrigger_Exception()
		{
			DummyWorkflowDescriptor.Instance.WorkflowTriggerActionTypeList.AddPair("MEH", "MEH MEH Action");
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.FillWithValidTestData();
			staff.GS_EmailAddress = "workflowstuff@cargowise.com";
			Factory.Save();

			WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			DummyIProcessor dummyProcessor = new DummyIProcessor();
			DummyWorkflowDescriptor.Instance.WorkflowTriggerActionProcessorOverride = dummyProcessor;
			dummyProcessor.shouldThrowException = true;

			Dummy.Z0_Description = "DUM123";
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.Authorised.Code;
			ProcessTaskNotification notification = milestone.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "MEH";
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("Failure notification email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("workflowstuff@cargowise.com", email.Recipients[0].Email);
			AssertEquals("WorkflowManager Trigger Action Failure - Dummy Business Object DUM123", email.Subject);
			string expectedBody =
@"The following trigger action failed to run:
Job: Dummy Business Object DUM123
Trigger Event: " + Events.Authorised.Description + @"
Trigger Action: MEH MEH Action
Error Message: DummyIProcessor Exception

You have received this email because you are a member of the staff group defined at System Registry: Workflow Manager -> Workflow Manager Notification Group.";
			AssertEquals(expectedBody, email.Body);
		}

		public void TestWorkflowLogNotInCache()
		{
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			var triggerNotification = trigger.ProcessTaskNotifications.AddNew();
			triggerNotification.PQ_TriggerType = "NTF";
			triggerNotification.PQ_Calc_TriggerParty = "EML";
			triggerNotification.PQ_EmailAddr = "abc@email.com";
			triggerNotification.PQ_EmailText = "Test LWK";

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			using (trigger.Logs.MostRecentLog.LockForUpdatingKeyFieldsForTesting())
			{
				trigger.Logs.MostRecentLog.SL_Reference = ZGuid.NewZGuid().ToString() + "|TST|TST";
			}
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1.1);
			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_2");
			Factory.Save();

			AssertNoExceptionThrown(RunLogWalkerCycleForTest);
		}

		#endregion

		#region How triggers handle StmALogs with identical References

		[TestDate(2015, 7, 22)]
		public void TestRunWorkflowTrigger_SameEventTime_SameReference_SameLWKRun()
		{
			SetupTrigger();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals("One email per log. That's the rules.", 2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 22)]
		public void TestRunWorkflowTrigger_SameEventTime_SameReference_DifferentLWKRun()
		{
			SetupTrigger();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals("One email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 22)]
		public void TestRunWorkflowTrigger_SameEventTime_DifferentReference_SameLWKRun()
		{
			SetupTrigger();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_2");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 22)]
		public void TestRunWorkflowTrigger_SameEventTime_DifferentReference_DifferentLWKRun()
		{
			SetupTrigger();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals("One email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_2");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 22)]
		public void TestRunWorkflowTrigger_DifferentEventTime_SameReference_SameLWKRun()
		{
			SetupTrigger();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1.1);
			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 22)]
		public void TestRunWorkflowTrigger_DifferentEventTime_SameReference_DifferentLWKRun()
		{
			SetupTrigger();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals("One email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1.1);
			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 22)]
		public void TestRunWorkflowTrigger_DifferentEventTime_DifferentReference_SameLWKRun()
		{
			SetupTrigger();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1.1);
			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_2");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 22)]
		public void TestRunWorkflowTrigger_DifferentEventTime_DifferentReference_DifferentLWKRun()
		{
			SetupTrigger();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals("One email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1.1);
			Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_2");
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion

		#region Line Triggers

		public void TestProcess_TriggerEventHasBeenGeneratedOnAnotherObject_ProcessInAContextOfAnotherObject()
		{
			GetDummyWorkflowDescriptorSetupForChainTesting();
			var dummyChild = Factory.New<DummyWithWorkflow>();
			var dummyParent = Factory.New<DummyWithWorkflow>();

			var triggeringEvent = dummyChild.Logs.AddNew(Events.Arrival);
			var wteData = new WorkflowTriggerEventData(triggeringEvent, dummyChild.PK, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code, "E", GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code);

			var parentTrigger = AddNewTrigger(dummyParent, Events.Arrival, Events.Delivered);
			using (parentTrigger.GetValidationSuspender())
			{
				dummyParent.Logs.AddNew(Events.Arrival);
				parentTrigger.Logs.CreateRecreateOrUpdateEventLog(Events.WorkflowTriggerEvent, EstimateActual.Actual, ZDateTimeOffset.Now, wteData.ToReference());
				parentTrigger.P9_LineTriggerType = "DUM";
				parentTrigger.OverriddenParentTypeForTest = typeof(DummyWithWorkflow);

				Factory.Save();

				RunLogWalkerCycleForTest();
				ErrorReporter.Clear();

				var log = dummyChild.Logs.MostRecentLogByEventTime(Events.Delivered);
				AssertNotNull("The Delivered event that expected to be generated on a child as an action of the parent trigger", log);
			}
		}

		#endregion

		#region New Triggers Firing For Old Events

		[TestDate(2015, 7, 14)]
		public void TestProcess_DefaultRegistryItemBehaviour_ShouldNotFireForEventsWhichHappenedBeforeTriggerCreated()
		{
			var templateFactory = Factory.CreateNewFactory();
			var template = templateFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			templateFactory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.GetLogs().AddNew(Events.Arrival);

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();

			AssertEquals(0, job.WorkflowItems.Triggers.Count);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateTrigger.P9_Description = "Arrival Trigger";
			var triggerAction = templateTrigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = "XML";

			templateFactory.Save();

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Triggers.Count);

			RunLogWalkerCycleForTest();

			AssertEquals("Trigger should not fire when applied from a template trigger created after the event was raised. This could cause documents to get sent out or other bad things to happen as templates change and old jobs are edited.",
				0, DummyWorkflowDescriptor.Instance.WorkflowTriggerActionRunCount);
		}

		[TestDate(2015, 7, 14)]
		public void TestProcess_DefaultRegistryItemBehaviour_ShouldFireForEventsWhichHappenedAfterTriggerCreatedButBeforeTriggerApplied()
		{
			var templateFactory = Factory.CreateNewFactory();
			var template = templateFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_IsActive = false;

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateTrigger.P9_Description = "Arrival Trigger";
			var triggerAction = templateTrigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = "XML";

			templateFactory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.GetLogs().AddNew(Events.Arrival);

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();

			AssertEquals(0, job.WorkflowItems.Triggers.Count);

			template.P0_IsActive = true;
			templateFactory.Save();

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Triggers.Count);

			RunLogWalkerCycleForTest();

			AssertEquals("Trigger should fire when applied from a template that is now applicable, for an event that happened after the template trigger was created.", 1, DummyWorkflowDescriptor.Instance.WorkflowTriggerActionRunCount);
		}

		[TestDate(2015, 7, 14)]
		public void TestProcess_WhenRegistryItemEnabled_ShouldFireForEventsWhichHappenedBeforeTriggerCreated()
		{
			WorkflowDataRegistry.Instance.AllowTriggersToFireForExistingEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var templateFactory = Factory.CreateNewFactory();
			var template = templateFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			templateFactory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.GetLogs().AddNew(Events.Arrival);

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();

			AssertEquals(0, job.WorkflowItems.Triggers.Count);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateTrigger.P9_Description = "Arrival Trigger";
			var triggerAction = templateTrigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = "XML";

			templateFactory.Save();

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Triggers.Count);

			RunLogWalkerCycleForTest();

			AssertEquals("Trigger should fire when applied from a template trigger created after the event was raised. We've enabled it in the registry so we must know what we're doing.", 1, DummyWorkflowDescriptor.Instance.WorkflowTriggerActionRunCount);
		}

		#endregion

		#region Test Grouping

		public void TestGroupLogs()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "ABC";
			staff.GS_FullName = "President Alex";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "DEP";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company.PK;
			branch2.GB_GC = company.PK;
			branch3.GB_GC = company.PK;
			var branch1PK = branch1.PK;
			var branch2PK = branch2.PK;
			var branch3PK = branch3.PK;
			Factory.Save();

			var log1 = CreateQueuedLog(staff, branch1, department, new ZDateTime(2011, 11, 15, 8, 43, 0));
			var log2 = CreateQueuedLog(staff, branch3, department, new ZDateTime(2011, 11, 15, 8, 40, 0));
			var log3 = CreateQueuedLog(staff, branch1, department, new ZDateTime(2011, 11, 15, 8, 41, 0));
			var log4 = CreateQueuedLog(staff, branch2, department, new ZDateTime(2011, 11, 15, 8, 42, 0));
			var log5 = CreateQueuedLog(staff, branch2, department, new ZDateTime(2011, 11, 15, 8, 38, 0));
			var log6 = CreateQueuedLog(staff, branch3, department, new ZDateTime(2011, 11, 15, 8, 25, 0));
			var log7 = CreateQueuedLog(staff, branch1, department, new ZDateTime(2011, 11, 15, 8, 50, 0));
			var log8 = CreateQueuedLog(staff, branch1, department, new ZDateTime(2011, 11, 15, 8, 45, 0));
			var log9 = CreateQueuedLog(staff, branch2, department, new ZDateTime(2011, 11, 15, 8, 44, 0));

			var logs = new[] { log1, log2, log3, log4, log5, log6, log7, log8, log9 };

			var groups =
				new WorkflowEventTriggerProcessorForTest()
				.GroupLogsExposed(logs.Select(s => new AppLockedItem<IQueuedLog>(s, null)).ToList())
				.Select(s => s.Select(si => si.Item).ToList()).ToList();

			AssertEquals(3, groups.Count);

			// Groups should be ordered by the oldest log.
			AssertEquals(2, groups[0].Count);
			AssertEquals(log6.PK, groups[0][0].PK);
			AssertEquals(log2.PK, groups[0][1].PK);
			AssertNotEquals(Factory, groups[0][0].Factory);
			AssertEquals(groups[0][0].Factory, groups[0][1].Factory);

			AssertEquals(3, groups[1].Count);
			AssertEquals(log5.PK, groups[1][0].PK);
			AssertEquals(log4.PK, groups[1][1].PK);
			AssertEquals(log9.PK, groups[1][2].PK);
			AssertNotEquals(Factory, groups[1][0].Factory);
			AssertEquals(groups[1][0].Factory, groups[1][1].Factory);
			AssertEquals(groups[1][0].Factory, groups[1][2].Factory);

			AssertEquals(4, groups[2].Count);
			AssertEquals(log3.PK, groups[2][0].PK);
			AssertEquals(log1.PK, groups[2][1].PK);
			AssertEquals(log8.PK, groups[2][2].PK);
			AssertEquals(log7.PK, groups[2][3].PK);
			AssertNotEquals(Factory, groups[2][0].Factory);
			AssertEquals(groups[2][0].Factory, groups[2][1].Factory);
			AssertEquals(groups[2][0].Factory, groups[2][2].Factory);
			AssertEquals(groups[2][0].Factory, groups[2][3].Factory);

			AssertNotEquals(groups[0][0].Factory, groups[1][0].Factory);
			AssertNotEquals(groups[0][0].Factory, groups[2][0].Factory);
			AssertNotEquals(groups[1][0].Factory, groups[2][0].Factory);
		}

		public void TestSetContextForLogsGroup()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "ABC";
			staff.GS_FullName = "President Alex";

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "DEP";

			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_Code = "TS1";
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var log = CreateQueuedLog(staff, branch, department, ZDateTime.UtcNow);

			AssertNotEquals("New branch is not yet selected", branch.PK, GlbBranch.CurrentBranch.PK);
			using (new WorkflowEventTriggerProcessorForTest().SetContextForLogsGroupExposed(new[] { log }))
			{
				AssertEquals("New branch should be set for temporary context", branch.PK, GlbBranch.CurrentBranch.PK);
			}
			AssertNotEquals("Temporary context should be switched off", branch.PK, GlbBranch.CurrentBranch.PK);
		}

		#region Help classes and methods

		IQueuedLog CreateQueuedLog(GlbStaff staff, GlbBranch branch, GlbDepartment department, ZDateTime dateTime)
		{
			var bizo = Factory.New<DummyWithWorkflow>();
			var trigger = bizo.WorkflowItems.Triggers.AddNew();
			var sourceLog = bizo.Logs.AddNew(Events.CustomisableEvent00);
			var eventSource = new EventSource(sourceLog);

			var log = (BusinessObject)Factory.New<IQueuedLog>();
			log[StmJobQueueSchema.SJ_SE_NKEvent] = Events.WorkflowTriggerEventCode;
			log[StmJobQueueSchema.SJ_ParentID] = trigger.PK;
			log[StmJobQueueSchema.SJ_ParentTableCode] = ProcessTasksSchema.Constants.Prefix;
			log[StmJobQueueSchema.SJ_PostedTimeUtc] = dateTime;
			log[StmJobQueueSchema.SJ_Reference] = new WorkflowTriggerEventData(eventSource, ZGuid.Empty, "BRN", "ABC", staff.GS_Code, branch.GB_Code, department.GE_Code).ToReference();

			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = bizo.PK;
			header.JH_GC = trigger.P9_GC;
			header.JH_GB = branch.PK;

			return (IQueuedLog)log;
		}

		[Serializable]
		class WorkflowEventTriggerProcessorForTest : WorkflowEventTriggerProcessor
		{
			public LogBatchCollection GroupLogsExposed(IList<AppLockedItem<IQueuedLog>> queuedLogs)
			{
				return GroupLogs(queuedLogs);
			}

			public IDisposable SetContextForLogsGroupExposed(IEnumerable<IQueuedLog> queuedLogs)
			{
				var log = queuedLogs.First();
				var trigger = log.LoadTrigger();
				var job = trigger?.GetParentBusinessObject(log);
				return GetLogBatcher().SetContextForLogsGroup(WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, job, log, new DummyLogger()), queuedLogs);
			}
		}

		sealed class DummyLogger : INotifications
		{
			public List<INotification> Notes = new List<INotification>();

			public void Add(INotification notification)
			{
				Notes.Add(notification);
			}
		}

		#endregion

		#endregion

		OrgHeader CreateJobWithPartialTemplateTrigger(IBMTestHelper helper, ProcessTaskTemplate partialTemplate, out IProcessHeader orgHeaderWorkflow)
		{
			var jobHeader = helper.CreateJobHeader<OrgHeader>(Factory);
			var orgHeader = (OrgHeader)jobHeader.Parent;
			orgHeaderWorkflow = jobHeader.ProcessHeaders.AddNew();
			orgHeaderWorkflow.FH_CompletionStatement = "Workflow 1";
			helper.CreateTask(orgHeaderWorkflow, taskType: "ABC", description: "This task was already here.");

			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = partialTemplate.PK;

			orgHeader.Logs.AddNew(Events.CustomisableEvent00);

			return orgHeader;
		}

		ProcessTaskTemplate CreatePartialTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_Name = "Added by Trigger";
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			var templateJobHeader = template.ProcessHeaders.AddNew();
			templateJobHeader.FH_CompletionStatement = "Job is Complete";

			var templateWorkflow = template.ProcessHeaders.AddNew();
			templateWorkflow.FH_FH_ParentHeader = templateJobHeader.PK;
			templateWorkflow.FH_CompletionStatement = "Workflow 1";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;
			templateTask.P9_Description = "This is a new task!";

			return template;
		}

		public void TestApplyTemplateTrigger_DoNotApplyInactiveTemplate()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);

			var template = CreatePartialTemplate();
			var orgHeader = CreateJobWithPartialTemplateTrigger(helper, template, out IProcessHeader orgHeaderWorkflow);

			Factory.Save();

			template.P0_IsActive = false;

			Factory.Save();

			AssertEquals(1, orgHeader.WorkflowItems.Tasks.Count);

			RunLogWalkerCycleForTest();
			var loadedOrgHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);

			AssertEquals("Template shouldn't have applied.", 1, loadedOrgHeader.WorkflowItems.Tasks.Count);
		}

		public void TestApplyTemplateTrigger_WithMatchingWorkflow_ShouldAddTasksToWorkflow()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);

			var template = CreatePartialTemplate();
			var orgHeader = CreateJobWithPartialTemplateTrigger(helper, template, out IProcessHeader orgHeaderWorkflow);

			Factory.Save();

			AssertEquals(1, orgHeader.WorkflowItems.Tasks.Count);

			RunLogWalkerCycleForTest();
			var loadedOrgHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);

			AssertEquals("A new task should have been added, and yet...", 2, loadedOrgHeader.WorkflowItems.Tasks.Count);

			var newTask = loadedOrgHeader.WorkflowItems.Tasks.Cast<ProcessTask>().Single(x => x.P9_Description == "This is a new task!");
			AssertNotNull(newTask);

			var workflow = newTask.ProcessHeader;
			var workflows = workflow.JobHeader.ProcessHeaders.Cast<IProcessHeader>();
			AssertContainsExactElementsInAnyOrder(new[] { "Job Workflow", "Workflow 1" }, workflows.Select(x => x.FH_CompletionStatement.ToString()));
			AssertEquals("The new task should have been created in the matching workflow rather than creating a new one, and yet...", orgHeaderWorkflow.PK, workflow.PK);
			AssertContainsExactElementsInAnyOrder(new[] { "This task was already here.", "This is a new task!" }, workflow.Tasks.Select(x => x.P9_Description.ToString()));

			AssertEquals(template.PK, newTask.SourceTemplatePK);
			AssertEquals(template.P0_Name, newTask.SourceTemplateName);
		}

		public void TestApplyTemplateTrigger_WithoutMatchingWorkflow_ShouldCreateNewWorkflow()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_Name = "Added by Trigger";
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			var templateJobHeader = template.ProcessHeaders.AddNew();
			templateJobHeader.FH_CompletionStatement = "Job is Complete";
			var templateWorkflow = template.ProcessHeaders.AddNew();
			templateWorkflow.FH_FH_ParentHeader = templateJobHeader.PK;
			templateWorkflow.FH_CompletionStatement = "Workflow From Template";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;
			templateTask.P9_Description = "This is a new task!";

			var jobHeader = helper.CreateJobHeader<OrgHeader>(Factory);
			var orgHeader = (OrgHeader)jobHeader.Parent;
			var orgHeaderWorkflow = jobHeader.ProcessHeaders.AddNew();
			orgHeaderWorkflow.FH_CompletionStatement = "Workflow 1";
			helper.CreateTask(orgHeaderWorkflow, taskType: "ABC", description: "This task was already here.");

			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = template.PK;

			orgHeader.Logs.AddNew(Events.CustomsEntryStatus);

			Factory.Save();

			AssertEquals(1, orgHeader.WorkflowItems.Tasks.Count);

			RunLogWalkerCycleForTest();
			var loadedOrgHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);

			AssertEquals("A new task should have been added, and yet...", 2, loadedOrgHeader.WorkflowItems.Tasks.Count);

			var newTask = loadedOrgHeader.WorkflowItems.Tasks.Cast<ProcessTask>().Single(x => x.P9_Description == "This is a new task!");
			AssertNotNull(newTask);

			var workflow = newTask.ProcessHeader;
			var workflows = workflow.JobHeader.ProcessHeaders.Cast<IProcessHeader>();
			AssertContainsExactElementsInAnyOrder(new[] { "Job Workflow", "Workflow 1", "Workflow From Template" }, workflows.Select(x => x.FH_CompletionStatement.ToString()));
			AssertNotEquals("The new task should have been created in a new workflow, and yet...", orgHeaderWorkflow.PK, workflow.PK);
			AssertContainsExactElementsInAnyOrder(new[] { "This is a new task!" }, workflow.Tasks.Cast<ProcessTask>().Select(x => x.P9_Description.ToString()));
			AssertEquals(template.PK, newTask.SourceTemplatePK);
			AssertEquals(template.P0_Name, newTask.SourceTemplateName);
		}

		public void TestApplyTemplateTrigger_WithoutBufferManagement_ShouldApplyCorrectProcessTasks()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			template.P0_Name = "Added by Trigger";
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			templateMilestone.P9_Description = "Moyulstone!";

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.P9_Description = "Trigga";
			templateTrigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.WorkflowItems.Milestones.AddNew().P9_Description = "I was already here.";
			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			trigger.P9_Description = "This should really get things started.";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = template.PK;

			orgHeader.Logs.AddNew(Events.CustomsEntryStatus);

			Factory.Save();

			RunLogWalkerCycleForTest();
			var loadedOrgHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("The new milestone should have been added, and yet...", new[] { "I was already here.", "Moyulstone!" }, loadedOrgHeader.WorkflowItems.Milestones.Cast<ProcessTask>().Select(x => x.P9_Description.ToString()));
				AssertContainsExactElementsInAnyOrder("The new trigger should have been added, and yet...", new[] { "This should really get things started.", "Trigga" }, loadedOrgHeader.WorkflowItems.Triggers.Cast<ProcessTask>().Select(x => x.P9_Description.ToString()));

				var newMilestone = loadedOrgHeader.WorkflowItems.Milestones.Cast<ProcessTask>().Single(x => x.P9_Description == "Moyulstone!");
				AssertEquals(template.PK, newMilestone.SourceTemplatePK);
				AssertEquals(template.P0_Name, newMilestone.SourceTemplateName);

				var newTrigger = loadedOrgHeader.WorkflowItems.Triggers.Cast<ProcessTask>().Single(x => x.P9_Description == "Trigga");
				AssertEquals(template.PK, newTrigger.SourceTemplatePK);
				AssertEquals(template.P0_Name, newTrigger.SourceTemplateName);
			});

			RunLogWalkerCycleForTest();
			loadedOrgHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("The milestones should not be double-added, and yet...", new[] { "I was already here.", "Moyulstone!" }, loadedOrgHeader.WorkflowItems.Milestones.Cast<ProcessTask>().Select(x => x.P9_Description.ToString()));
				AssertContainsExactElementsInAnyOrder("The triggers should not be double-added, and yet...", new[] { "This should really get things started.", "Trigga" }, loadedOrgHeader.WorkflowItems.Triggers.Cast<ProcessTask>().Select(x => x.P9_Description.ToString()));
			});
		}

		public void TestSendMailWithDifferentStaff()
		{
			SetupTrigger();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ABC";
			staff1.GS_LoginName = "ABC";
			staff1.GS_FullName = "President Alex";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "EDF";
			staff2.GS_LoginName = "EDF";
			staff2.GS_FullName = "President Brandon";

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "DEP";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "TS1";
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_1");
				Factory.Save();
			}
			using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				Dummy.Logs.AddNew(Events.CustomsEntryStatus, "REF_2");
				Factory.Save();
			}

			RunLogWalkerCycleForTest();

			AssertEquals("Two mails have been sent", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			var senderStaffPKs = Env.OutgoingMailManager.EmailsCreated
				.Select(email => email.SenderStaffPK)
				.ToList();

			Assert("An email should have been sent by the first staff member.", senderStaffPKs.Contains(staff1.PK));
			Assert("An email should have been sent by the second staff member.", senderStaffPKs.Contains(staff2.PK));
		}

		public void TestSetContextForLogsGroupWithDifferentStaff()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ABC";
			staff1.GS_LoginName = "ABC";
			staff1.GS_FullName = "President Alex";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "EDF";
			staff2.GS_LoginName = "EDF";
			staff2.GS_FullName = "President Brandon";

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "DEP";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "TS1";
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			var bizo = Factory.New<DummyWithWorkflow>();
			var trigger = bizo.WorkflowItems.Triggers.AddNew();
			var sourceLog = bizo.Logs.AddNew(Events.CustomisableEvent00);
			var eventSource = new EventSource(sourceLog);

			Factory.Save();

			var log1 = CreateBatchQueuedLog(trigger, eventSource, staff1, branch, department, ZDateTime.UtcNow);
			var log2 = CreateBatchQueuedLog(trigger, eventSource, staff2, branch, department, ZDateTime.UtcNow);
			var log3 = CreateBatchQueuedLog(trigger, eventSource, staff1, branch, department, ZDateTime.UtcNow);

			var logs = new[] { log1, log2, log3 };

			var groups = new WorkflowEventTriggerProcessorForTest()
			.GroupLogsExposed(logs.Select(s => new AppLockedItem<IQueuedLog>(s, null)).ToList());

			var logGroups = groups.ToDictionary(g => ((UserContextSwitcher)g.Key.Key).StaffPK, g => g.Select(g => g.Item).ToList());

			AssertEquals(2, logGroups.Count);

			AssertEquals("staff1 batch has 2 logs.", 2, logGroups[staff1.PK].Count);
			AssertEquals("staff2 batch has 1 log.", 1, logGroups[staff2.PK].Count);
		}

		public void TestSetContextForLogsGroupWithDifferentBranchOrDepartment()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "ABC";
			staff.GS_FullName = "President Alex";

			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DE1";

			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "DE2";

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "TS1";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "TS2";
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			var bizo = Factory.New<DummyWithWorkflow>();
			var trigger = bizo.WorkflowItems.Triggers.AddNew();
			var sourceLog = bizo.Logs.AddNew(Events.CustomisableEvent00);
			var eventSource = new EventSource(sourceLog);

			Factory.Save();

			var log1 = CreateBatchQueuedLog(trigger, eventSource, staff, branch1, department1, ZDateTime.UtcNow);
			var log2 = CreateBatchQueuedLog(trigger, eventSource, staff, branch2, department1, ZDateTime.UtcNow);
			var log3 = CreateBatchQueuedLog(trigger, eventSource, staff, branch1, department1, ZDateTime.UtcNow);
			var log4 = CreateBatchQueuedLog(trigger, eventSource, staff, branch1, department2, ZDateTime.UtcNow);

			var logs = new[] { log1, log2, log3, log4 };

			using (new WorkflowEventTriggerProcessorForTest().SetContextForLogsGroupExposed(logs))
			{
				var groups = new WorkflowEventTriggerProcessorForTest()
				.GroupLogsExposed(logs.Select(s => new AppLockedItem<IQueuedLog>(s, null)).ToList());

				var logGroups = groups.ToDictionary(
					g =>
					{
						var key = (UserContextSwitcher)g.Key.Key;
						return key.BranchPK.ToString() + key.DepartmentPK.ToString();
					},
					g => g.Select(item => item.Item).ToList()
				);

				AssertEquals(3, logGroups.Count);

				AssertEquals("branch1, department1 batch has 2 logs.", 2, logGroups[branch1.PK.ToString() + department1.PK.ToString()].Count);
				AssertEquals("branch1, department2 batch has 1 log.", 1, logGroups[branch1.PK.ToString() + department2.PK.ToString()].Count);
				AssertEquals("branch2, department1 batch has 1 log.", 1, logGroups[branch2.PK.ToString() + department1.PK.ToString()].Count);
			}
		}

		IQueuedLog CreateBatchQueuedLog(ProcessTask trigger, EventSource eventSource, GlbStaff staff, GlbBranch branch, GlbDepartment department, ZDateTime dateTime)
		{
			var log = (BusinessObject)Factory.New<IQueuedLog>();
			log[StmJobQueueSchema.SJ_SE_NKEvent] = Events.WorkflowTriggerEventCode;
			log[StmJobQueueSchema.SJ_ParentID] = trigger.PK;
			log[StmJobQueueSchema.SJ_ParentTableCode] = ProcessTasksSchema.Constants.Prefix;
			log[StmJobQueueSchema.SJ_PostedTimeUtc] = dateTime;
			log[StmJobQueueSchema.SJ_Reference] = new WorkflowTriggerEventData(eventSource, ZGuid.Empty, "BRN", "ABC", staff.GS_Code, branch.GB_Code, department.GE_Code).ToReference();

			return (IQueuedLog)log;
		}

		#region Implementation

		StmMenuItem DocumentCommand
		{
			get
			{
				DocumentZQuery query = new DocumentZQuery(BusinessContext.Shipment, "Delay Alert");
				return (StmMenuItem)Factory.LoadTop1<IDocumentCommand>(query);
			}
		}

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		IWorkflowProvider TasksProvider
		{
			get
			{
				if (businessObject == null)
				{
					businessObject = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
					OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
					OrgContact contact = consignee.Contacts.AddNew();
					contact.OC_ContactName = "Test Contact";
					contact.OC_Email = "clinton.volzke@cargowise.com";

					OrgDocument document = contact.Documents.AddNew();
					document.OD_SU_MenuItem = DocumentCommand.PK;

					businessObject["ConsigneePK"] = consignee.PK;
				}
				return (IWorkflowProvider)businessObject;
			}
		}
		BusinessObject businessObject;

		static DummyProcessTask AddNewTrigger(DummyWithWorkflow dummyBO, Event triggeringEvent, Event actionEvent)
		{
			var trigger = (DummyProcessTask)dummyBO.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = triggeringEvent.Description;
			trigger.TriggerConditions.TriggerEventCode = triggeringEvent.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = actionEvent.Code;

			return trigger;
		}

		static string GetAllEventTypesApplied(DummyWithWorkflow dummyBO)
		{
			return string.Join(", ", new BusinessObjectFactory().Load<DummyWithWorkflow>(dummyBO.PK).Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Select(o => o.SL_SE_NKEvent)
				.OrderBy(o => o));
		}

		void SetupTrigger()
		{
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;
			var triggerNotification = trigger.ProcessTaskNotifications.AddNew();
			triggerNotification.PQ_TriggerType = "NTF";
			triggerNotification.PQ_Calc_TriggerParty = "EML";
			triggerNotification.PQ_EmailAddr = "abc@email.com";
			triggerNotification.PQ_EmailText = "Test LWK";
		}

		DummyWorkflowDescriptor GetDummyWorkflowDescriptorSetupForChainTesting()
		{
			var descriptor = DummyWorkflowDescriptor.Instance;
			descriptor.WorkflowTriggerActionTypeList.AddPair(Events.CargoReceivedAtDepotCode, Events.CargoReceivedAtDepot.Description);
			descriptor.WorkflowTriggerActionTypeList.AddPair(Events.CargoCheckinCode, Events.CargoCheckin.Description);
			descriptor.WorkflowTriggerActionTypeList.AddPair(Events.CargoAvailableCode, Events.CargoAvailable.Description);

			descriptor.WorkflowTriggerActionGetter = (job, action, queuedLog) =>
			{
				switch (action.PQ_TriggerType)
				{
					case Events.CargoReceivedAtDepotCode:
						return new LogAddingProcessor(job, Events.CargoReceivedAtDepot);
					case Events.CargoCheckinCode:
						return new LogAddingProcessor(job, Events.CargoCheckin);
					case Events.CargoAvailableCode:
						return new LogAddingProcessor(job, Events.CargoAvailable);
					case Events.DeliveredCode:
						return new LogAddingProcessor(job, Events.Delivered);
				}

				return null;
			};

			return descriptor;
		}

		class LogAddingProcessor : IProcessor
		{
			public LogAddingProcessor(BusinessObject job, Event eventType)
			{
				this.eventType = eventType;
				this.job = job;
			}

			readonly Event eventType;
			readonly BusinessObject job;

			public void Process(INotifications notifications, CancellationToken token = new CancellationToken())
			{
				var dummyBO = job as DummyWithWorkflow;
				dummyBO.Logs.AddNew(eventType);
				notifications.Add(CargoWise.EntityFramework.NotificationType.Error, string.Format("Adding Event Type [{0}]...", eventType.Code));
			}
		}

		#endregion

		#region DummyIProcessor

		class DummyIProcessor : IProcessor
		{
			public bool shouldThrowException;
			public AddToNotificationLogDelegate AddToNotificationLog;
			public delegate void AddToNotificationLogDelegate(INotifications notifications);

			#region IProcessor Members

			public void Process(INotifications notifications, CancellationToken token = new CancellationToken())
			{
				if (shouldThrowException)
				{
					throw new WorkflowValidationException("DummyIProcessor Exception");
				}
				if (AddToNotificationLog != null)
				{
					AddToNotificationLog(notifications);
				}
			}

			#endregion
		}

		#endregion
	}
}
