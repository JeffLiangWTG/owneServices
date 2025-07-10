using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	class WorkflowTriggerActionManagerTest : TestCaseWithFactory
	{
		public void TestRunTriggerActionsForCommercialInvoiceAttachedOnDeclaration()
		{
			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			var invoiceHeader = Factory.New<Customs.Shared.IBaseJobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceNumber = "INV00001";
			invoiceHeader.JZ_JE = declaration.PK;
			Factory.Save();

			var messageType1 = invoiceHeader.JZ_MessageType;
			AssertNotEquals(messageType1, "ASN");

			var workflowProvider1 = (IWorkflowProvider)invoiceHeader;
			var trigger1 = workflowProvider1.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Test";
			trigger1.TriggerConditions.TriggerEventCode = Events.EditedARecordCode;
			trigger1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;

			var triggerAction1 = trigger1.ProcessTaskNotifications.AddNew();
			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerAction1.PQ_FieldName = "<JZ_MessageType>";
			triggerAction1.PQ_FieldValue = "ASN";

			var provider1 = (IWorkflowProvider)invoiceHeader;
			var log1 = provider1.Logs.CreateRecreateOrUpdateEventLog(Events.StatusChange, EstimateActual.Actual, ZDateTimeOffset.Now);
			var notifications1 = new NotificationBuffer();

			new WorkflowTriggerActionManager(notifications1).Run((BusinessObject)invoiceHeader, trigger1, new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = Events.WorkflowTriggerEventCode, SJ_Reference = log1.PK.ToString() });
			AssertEquals(messageType1, invoiceHeader.JZ_MessageType);

			var logs = notifications1.AsString;
			AssertContains("Trigger action can not be performed because Commercial Invoice INV00001 is attached to Customs Declaration B00001000.", logs);
		}

		public void TestProcessTriggerComparer()
		{
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification taskNotification = trigger.ProcessTaskNotifications.AddNew();

			IEnumerable<Guid?> LotsOfGuids()
			{
				for (int i = 0; i < 1000; ++i)
				{
					yield return Guid.NewGuid();
				}
			}

			var processTriggerComparer = new WorkflowTriggerActionManager.ProcessTriggerComparer();
			AssertNoExceptionThrown(() => processTriggerComparer.GetHashCode((taskNotification, LotsOfGuids())));
		}

		public void TestProcessNotificationMustHaveWorkflowProviderOfCorrectType()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var triggerAction = Factory.New<ProcessTaskNotification>();
			var trigger = Factory.New<DummyProcessTask>();
			triggerAction.PQ_P9 = trigger.PK;
			trigger.P9_ParentID = parent.PK;
			trigger.P9_LineTriggerType = WorkflowDescriptors.AccComplianceReportCode;

			var provider = (IWorkflowProvider)parent;
			var log = provider.Logs.CreateRecreateOrUpdateEventLog(Events.StatusChange, EstimateActual.Actual, ZDateTimeOffset.Now);
			var notifications = new NotificationBuffer();
			new WorkflowTriggerActionManager(notifications).Run(parent, trigger, new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = Events.WorkflowTriggerEventCode, SJ_Reference = log.PK.ToString() });
			AssertEquals("Error should be reported if trigger actions workflow provider does not match it's expected type", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestTriggerActionValidatorsCanValidateAndStopWorkflowTriggerActionManager()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();

			var notifications = new NotificationBuffer();

			var actionRunCount = 0;
			var actionRunner = new Mock<IWorkflowTriggerActionRunner>();
			actionRunner.Setup(m => m.Run(It.IsAny<IWorkflowTriggerAction>())).Callback(() =>
			{
				actionRunCount++;
			});

			var manager = new WorkflowTriggerActionManagerForTesting(actionRunner.Object, notifications);

			var validator = new Mock<IWorkflowTriggerActionValidator>();
			validator.Setup(v => v.ShouldValidate(It.IsAny<IBaseTrigger>(), It.IsAny<ITriggerAction>(), It.IsAny<IQueuedLog>(), It.IsAny<IStmALog>(), It.IsAny<IWorkflowDescriptor>())).Returns(true);
			validator.Setup(v => v.IsValid(It.IsAny<IBaseTrigger>(), It.IsAny<ITriggerAction>(), It.IsAny<IQueuedLog>(), It.IsAny<IStmALog>(), It.IsAny<IWorkflowDescriptor>(), dummy, notifications)).Returns((IBaseTrigger t, ITriggerAction a, IQueuedLog q, IStmALog l, IWorkflowDescriptor w, BusinessObject b, INotifications log) =>
			{
				log.AddWarning("Goddamn! Cannot Execute!");
				return false;
			});

			using (ObjectFactory.Substitute("WorkflowTriggerActionValidators", new[] { validator.Object }))
			{
				manager.Run(dummy, trigger, new QueuedLogForTesting(Factory));

				AssertEquals(0, actionRunCount);
				Assert(notifications.Events.Any(e => e.Message == "Goddamn! Cannot Execute!"));
			}

			validator.Setup(v => v.ShouldValidate(It.IsAny<IBaseTrigger>(), It.IsAny<ITriggerAction>(), It.IsAny<IQueuedLog>(), It.IsAny<IStmALog>(), It.IsAny<IWorkflowDescriptor>())).Returns(false);

			using (ObjectFactory.Substitute("WorkflowTriggerActionValidators", new[] { validator.Object }))
			{
				manager.Run(dummy, trigger, new QueuedLogForTesting(Factory));

				AssertEquals(1, actionRunCount);
			}

			validator.Setup(v => v.ShouldValidate(It.IsAny<IBaseTrigger>(), It.IsAny<ITriggerAction>(), It.IsAny<IQueuedLog>(), It.IsAny<IStmALog>(), It.IsAny<IWorkflowDescriptor>())).Returns(true);
			validator.Setup(v => v.IsValid(It.IsAny<IBaseTrigger>(), It.IsAny<ITriggerAction>(), It.IsAny<IQueuedLog>(), It.IsAny<IStmALog>(), It.IsAny<IWorkflowDescriptor>(), dummy, notifications)).Returns((IBaseTrigger t, ITriggerAction a, IQueuedLog q, IStmALog l, IWorkflowDescriptor w, BusinessObject b, INotifications log) =>
			{
				log.AddWarning("Passed with Warning!");
				return true;
			});

			using (ObjectFactory.Substitute("WorkflowTriggerActionValidators", new[] { validator.Object }))
			{
				manager.Run(dummy, trigger, new QueuedLogForTesting(Factory));

				AssertEquals(2, actionRunCount);
				Assert(notifications.Events.Any(e => e.Message == "Passed with Warning!"));
			}
		}

		public void TestINotificationsIsPassedToActionImplementation()
		{
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification taskNotification = trigger.ProcessTaskNotifications.AddNew();
			taskNotification.PQ_TriggerType = "";

			var processor = new Mock<IProcessor>(MockBehavior.Strict);

			DummyWorkflowDescriptor.Instance.WorkflowTriggerActionProcessorOverride = processor.Object;
			AssertNotNull("Precondition", Notifications);

			processor.Setup(m => m.Process(Notifications, It.IsAny<CancellationToken>()));

			Manager.Run(Dummy, trigger);
			processor.VerifyAll();
		}

		public void TestRun_ExceptionHandling()
		{
			GlbGroup mailGroup = Factory.NewWithValidTestData<GlbGroup>();
			mailGroup.Staff.AddNew().GS_EmailAddress = "need.to.have.non-empty.group.to.test.mail.sending@cargowise.com";
			Factory.Save();

			WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mailGroup.PK.ToGuid());

			var actionRunner = new Mock<IWorkflowTriggerActionRunner>(MockBehavior.Strict);
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();

			actionRunner.Setup(m => m.Run(It.IsAny<IWorkflowTriggerAction>()))
				.Throws(new WorkflowValidationException("Problems, officer?.."));

			var manager = new WorkflowTriggerActionManagerForTesting(actionRunner.Object);

			manager.Run(dummy, trigger, new QueuedLogForTesting(Factory));
			actionRunner.VerifyAll();

			EmailDef mailCreatedByWorkflowFailureHandler = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals($"{Core.Constants.ProductName} Workflow Trigger Processor", mailCreatedByWorkflowFailureHandler.FromDisplayName);
			AssertEquals(true, mailCreatedByWorkflowFailureHandler.Body.Contains("Problems, officer?.."));

			actionRunner.Setup(m => m.Run(It.IsAny<IWorkflowTriggerAction>()))
				.Throws(new OutOfMemoryException());

			AssertEquals("Precondition", true, new OutOfMemoryException().IsCriticalException());
			AssertExceptionThrown(typeof(OutOfMemoryException), delegate
			{
				manager = new WorkflowTriggerActionManagerForTesting(actionRunner.Object);
				manager.Run(dummy, trigger);
			});
			actionRunner.VerifyAll();

			AssertNoExceptionThrown(() =>
			{
				manager = new WorkflowTriggerActionManagerForTesting(null);
				manager.Run(dummy, trigger);
			});

			actionRunner.Setup(m => m.Run(It.IsAny<IWorkflowTriggerAction>()))
				.Throws(new EmailNotCompleteException("Invalid email address for sender Ben"));

			var loggerToSet = new LoggerForTesting();
			var logger = new WorkflowEventTriggerCategoryLogger(loggerToSet);
			var notifications = new NotificationProxyForTesting(logger);

			var manager2 = new WorkflowTriggerActionManagerForTestingWithLogger(actionRunner.Object, notifications);
			AssertNoExceptionThrown(() =>
			{
				manager2.Run(dummy, trigger, new QueuedLogForTesting(Factory));
			});

			var hasLogError = ((LoggerForTesting)logger.GetLogger()).NotifiedEventList.Any(log => log.Contains("Invalid email address for sender Ben"));
			AssertEquals("Should have related logs in service manager", true, hasLogError);

			actionRunner.VerifyAll();
		}

		public void TestProcessTaskShouldNotCombineEventAndShipmentTriggersUsingRecipientRolesFromSameCompany()
		{
			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Module = "SHP";

			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			orgToUse.EDICommunicationsModes.Add(communicationsMode);
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				TestProcessTaskShouldCombineTriggersCore(
				  orgToUse.PK,
				  orgToUse.PK,
				  WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				  WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML,
				  "PUR",
				  "PUR",
  @"\[WorkflowEventTrigger\] \[Default\] Action types in this batch: XUS, XUE");
				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(2, messages.Length);
			}
		}

		public void TestSetFieldMacrosFallbackToWTELog()
		{
			var job = Factory.New<DummyWithWorkflow>();
			DummyWithWorkflow.AutoLogState.Value = EnterpriseBusinessObject.AutologState.AutoLoggedToQueueOnly;
			job.Z0_IsSystem = false;
			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<Z0_Description>";
			action.PQ_FieldValue = "triggering code: <TriggeringEvent.SL_SE_NKEvent>";

			Factory.Save();

			var logwalker = MasterFilesTestHelper.RunLogWalker();
			job.Reload();
			AssertEquals("triggering code: " + Events.AddedARecordToTheSystem.Code, job.Z0_Description);
		}

		public void TestProcessTaskShouldCombineTriggersUsingRecipientRolesFromSameCompany_XUS()
		{
			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Module = "SHP";

			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			orgToUse.EDICommunicationsModes.Add(communicationsMode);
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				TestProcessTaskShouldCombineTriggersCore(
			orgToUse.PK,
			orgToUse.PK,
			WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
			WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
			"PUR",
			"PUR",
			@"\[WorkflowEventTrigger\] \[Default\] Action types in this batch: XUS");
				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(1, messages.Length);
				using (var reader = messages[0].GetEM_MessageTextReader())
				{
					var messageAsElement = XElement.Load(reader);
					var recipientRoleCollection = messageAsElement.Descendants(XName.Get("RecipientRoleCollection", "http://www.cargowise.com/Schemas/Universal/2011/11")).First();
					AssertEquals("Both recipient roles should be included in XML", 2,
						recipientRoleCollection.Elements().Count());
					AssertXMLContains("<Code>CNE</Code>", recipientRoleCollection.ToString());
					AssertXMLContains("<Code>CNR</Code>", recipientRoleCollection.ToString());
				}
			}
		}

		public void TestProcessTaskShouldCombineTriggersUsingRecipientRolesFromSameCompany_XUS_DifferentPurposeCodes()
		{
			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Module = "SHP";

			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			orgToUse.EDICommunicationsModes.Add(communicationsMode);
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				TestProcessTaskShouldCombineTriggersCore(
				orgToUse.PK,
				orgToUse.PK,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				"PUR",
				"RUP",
				@"\[WorkflowEventTrigger\] \[Default\] Action types in this batch: XUS, XUS");

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(2, messages.Length);
				var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";

				var assertion = new Action<XElement>(element =>
				{
					var recipientRoleCollection = element.Descendants(XName.Get("RecipientRoleCollection", nameSpace)).First();
					var roleCollectionAsString = recipientRoleCollection.ToString();
					var purpose = element.Descendants(XName.Get("ActionPurpose", nameSpace)).First();
					if (roleCollectionAsString.Contains("<Code>CNE</Code>"))
					{
						AssertXMLContains("<Code>PUR</Code>", purpose.ToString());
					}
					else if (roleCollectionAsString.Contains("<Code>CNR</Code>"))
					{
						AssertXMLContains("<Code>RUP</Code>", purpose.ToString());
					}
					else
					{
						Assert("The recipient role should be either CNE or CNR", false);
					}
				});

				using (var reader = messages[0].GetEM_MessageTextReader())
				{
					assertion(XElement.Load(reader));
				}
				using (var reader = messages[1].GetEM_MessageTextReader())
				{
					assertion(XElement.Load(reader));
				}
			}
		}

		public void TestProcessTaskShouldNotCombineTriggersUsingRecipientRolesFromSameCompanyIfNotAUniversalTrigger()
		{
			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Module = "SHP";

			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			orgToUse.EDICommunicationsModes.Add(communicationsMode);
			TestProcessTaskShouldCombineTriggersCore(
				orgToUse.PK,
				orgToUse.PK,
				WorkflowTriggerActionTypeConstants.Codes.SendDocument,
				WorkflowTriggerActionTypeConstants.Codes.SendDocument,
				"PUR",
				"PUR",
				@"\[WorkflowEventTrigger\] \[Default\] Action types in this batch: DOC, DOC
\[WorkflowEventTrigger\] \[Default\] Firing action: DOC
\[WorkflowEventTrigger\] \[Default\] Action completed: DOC \(\d+ objects loaded, \d+ table selects\)
\[WorkflowEventTrigger\] \[Default\] Firing action: DOC
\[WorkflowEventTrigger\] \[Default\] Action completed: DOC \(\d+ objects loaded, \d+ table selects\)
\[WorkflowEventTrigger\] \[Default\] finished processing logs.");
		}

		[TestDate(2020, 01, 01)]
		public void TestProcessTaskShouldCombineTriggersUsingRecipientRolesFromSameCompany_XUE()
		{
			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Module = "SHP";

			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			orgToUse.EDICommunicationsModes.Add(communicationsMode);
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				TestProcessTaskShouldCombineTriggersCore(
				orgToUse.PK,
				orgToUse.PK,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML,
				"PUR",
				"PUR",
@"\[WorkflowEventTrigger\] \[Default\] Action types in this batch: XUE");

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(1, messages.Length);

				using (var reader = messages[0].GetEM_MessageTextReader())
				{
					var messageAsElement = XElement.Load(reader);
					AssertXMLContains("<CreatedTime>", messageAsElement.ToString());
					var recipientRoleCollection = messageAsElement.Descendants(XName.Get("RecipientRoleCollection", "http://www.cargowise.com/Schemas/Universal/2011/11")).First();
					AssertEquals("Both recipient roles should be included in XML", 2,
						recipientRoleCollection.Elements().Count());
					AssertXMLContains("<Code>CNE</Code>", recipientRoleCollection.ToString());
					AssertXMLContains("<Code>CNR</Code>", recipientRoleCollection.ToString());
				}
			}
		}

		public void TestProcessTaskShouldNotCombineTriggersUsingRecipientRolesFromDifferentCompanies_XUS()
		{
			var communicationsMode1 = Factory.New<EDICommunicationsMode>();
			communicationsMode1.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode1.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			communicationsMode1.EK_Destination = "SOMEONE";
			communicationsMode1.EK_Module = "SHP";

			var orgToUse1 = Factory.NewWithValidTestData<OrgHeader>();
			orgToUse1.EDICommunicationsModes.Add(communicationsMode1);

			var communicationsMode2 = Factory.New<EDICommunicationsMode>();
			communicationsMode2.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode2.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode2.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			communicationsMode2.EK_Destination = "SOMEONE";
			communicationsMode2.EK_Module = "SHP";

			var orgToUse2 = Factory.NewWithValidTestData<OrgHeader>();
			orgToUse2.EDICommunicationsModes.Add(communicationsMode2);
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				TestProcessTaskShouldCombineTriggersCore(
				orgToUse1.PK,
				orgToUse2.PK,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				"PUR",
				"PUR",
@"\[WorkflowEventTrigger\] \[Default\] Action types in this batch: XUS, XUS");
				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(2, messages.Length);
			}
		}

		public void TestFailureLogging_NoCommunicationModesForOrg()
		{
			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgToUse.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = orgToUse.PK;

			var workflow = (IWorkflowProviderIncludingRelated)shipment;
			var trigger = workflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send the XML";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action1.PQ_MessagePurpose = "PUR";

			((BusinessObject)shipment).GetLogs().AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var logs = MasterFilesTestHelper.RunLogWalker();
			AssertContains(FormattableString.Invariant($"Action [Type=XUS,Recipient=CNE,Purpose=PUR] failed for trigger [Z00-Send the XML] because Organization [{orgToUse.OH_Code}] for Company [{GlbCompany.CurrentCompany.GC_Code}] has no matching Communication Modes."), logs);
			AssertEquals(0, Factory.Load<IEDIMessage>(new ZQuery()).Length);
		}

		public void TestFailureLogging_NoOrgIdentified()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();
			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;

			var workflow = (IWorkflowProviderIncludingRelated)shipment;
			var trigger = workflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send the XML";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.DeliveryAgent;
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action1.PQ_MessagePurpose = "PUR";

			((BusinessObject)shipment).GetLogs().AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var logs = MasterFilesTestHelper.RunLogWalker();
			AssertContains(FormattableString.Invariant($"Action [Type=XUS,Recipient=DAG,Purpose=PUR] failed for trigger [Z00-Send the XML] because Recipient Organization not found."), logs);
			AssertEquals(0, Factory.Load<IEDIMessage>(new ZQuery()).Length);
		}

		public void TestCannotGetWorkFlowTriggerActionBecauseOfInvalid()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();

			var workflow = (IWorkflowProviderIncludingRelated)shipment;
			var trigger = workflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test the test";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalManifestEventXML; //Invalid for specific workflow type

			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = "INV"; //Invalid & Doesn't exist

			((BusinessObject)shipment).GetLogs().AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var logs = MasterFilesTestHelper.RunLogWalker();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertContains("Error: [WorkflowEventTrigger] [Default] There wasn't a valid processor for this trigger action of type INV", logs);
			AssertContains("Trigger Action Type XME is no real use.", logs);

			ErrorReporter.Clear();
		}

		public void TestFailureLogging_OrgProxyHadNoCommunicationModes()
		{
			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			company.GC_OH_OrgProxy = orgToUse.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				((BusinessObject)shipment).FillWithValidTestData();

				var workflow = (IWorkflowProviderIncludingRelated)shipment;
				var trigger = workflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Send the XML";
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

				var action1 = trigger.ProcessTaskNotifications.AddNew();
				action1.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
				action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
				action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action1.PQ_MessagePurpose = "PUR";

				((BusinessObject)shipment).GetLogs().AddNew(Events.CustomisableEvent00);
				Factory.Save();

				var logs = MasterFilesTestHelper.RunLogWalker();
				AssertContains(FormattableString.Invariant($"Action [Type=XUS,Recipient=ORP,Purpose=PUR] failed for trigger [Z00-Send the XML] because Organization [{orgToUse.OH_Code}] for Company [{company.GC_Code}] has no matching Communication Modes."), logs);
				AssertEquals(0, Factory.Load<IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestCommunicationModeMatchesEvent()
		{
			eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Dingo");
			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode1 = orgToUse.EDICommunicationsModes.AddNew();
			communicationsMode1.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode1.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			communicationsMode1.EK_Destination = "SOMEONE";
			communicationsMode1.EK_Module = "SHP";
			communicationsMode1.EK_EventCode = Events.CustomisableEvent00Code;
			communicationsMode1.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			communicationsMode1.EK_EventReferenceConditionValue = "*TEST*";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgToUse.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = orgToUse.PK;

			var workflow = (IWorkflowProviderIncludingRelated)shipment;
			var trigger = workflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send the XML";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action1.PQ_MessagePurpose = "PUR";

			((BusinessObject)shipment).GetLogs().AddNew(Events.CustomisableEvent00, "TEST");
			Factory.Save();

			var logs = MasterFilesTestHelper.RunLogWalker();
			AssertEquals(1, Factory.Load<IEDIMessage>(new ZQuery()).Length);
		}

		public void TestXUECanExportCancelledEvent()
		{
			eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Dingo");
			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode1 = orgToUse.EDICommunicationsModes.AddNew();
			communicationsMode1.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode1.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			communicationsMode1.EK_Destination = "SOMEONE";
			communicationsMode1.EK_Module = "SHP";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgToUse.PK;

			var workflow = (IWorkflowProvider)shipment;
			var trigger = workflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send the XML";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			Factory.Save();

			var log = workflow.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			log.Cancel();
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();
			shipment = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK);
			var dexLogs = ((IWorkflowProvider)shipment).Logs.Find(l => l.SL_SE_NKEvent == Events.DataExportCode);
			AssertEquals(1, dexLogs.Count());
		}

		public void TestProcessTaskShouldNotCombineTriggersWithNoCommMode_XUS()
		{
			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			TestProcessTaskShouldCombineTriggersCore(
				orgToUse.PK,
				orgToUse.PK,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				"PUR",
				"PUR",
@"failed for trigger \[ADD-Estimate Pickup\]");
			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(0, messages.Length);
		}

		void TestProcessTaskShouldCombineTriggersCore(ZGuid cnePK, ZGuid cnrPK, string triggerType1, string triggerType2, string purpose1, string purpose2, string expectedLogRegex)
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = cnePK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = cnrPK;

			var workflow = (IWorkflowProviderIncludingRelated)shipment;
			var trigger = workflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Estimate Pickup";
			trigger.TriggerConditions.TriggerEventCode = "ADD";

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			action1.PQ_TriggerType = triggerType1;
			action1.PQ_MessagePurpose = purpose1;

			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
			action2.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignor;
			action2.PQ_TriggerType = triggerType2;
			action2.PQ_MessagePurpose = purpose2;

			((BusinessObject)shipment).GetLogs().AddNew(Events.Authorised, new ZDateTimeOffset(2020, 01, 01));
			Factory.Save();

			AssertEquals(true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, ((BusinessObject)shipment).GetLogs().Find(log => log.SL_SE_NKEvent == Events.Authorised.Code).First(), (BusinessObject)shipment));

			var logs = MasterFilesTestHelper.RunLogWalker();
			AssertEquals($"The log should match the pattern expected:\r\n{logs}", true, Regex.IsMatch(logs, expectedLogRegex));
		}

		public void TestProcessTaskShouldCombineTriggers_OrgProxy()
		{
			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Module = "SHP";

			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			orgProxy.EDICommunicationsModes.Add(communicationsMode);

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgProxy.PK;

			var workflow = (IWorkflowProviderIncludingRelated)shipment;
			var trigger = workflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Estimate Pickup";
			trigger.TriggerConditions.TriggerEventCode = "ADD";

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action1.PQ_MessagePurpose = "PUR";

			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
			action2.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action2.PQ_MessagePurpose = "PUR";

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = newCompany.PK;

			var newOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			newCompany.GC_OH_OrgProxy = newOrgProxy.PK;

			var newUser = Factory.NewWithValidTestData<GlbStaff>();
			newUser.GS_GB_HomeBranch = newBranch.PK;

			var newDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				Factory.Save();

				using (Env.SetTemporaryUserContext(newUser.PK.ToGuid(), newBranch.PK.ToGuid(), newDepartment.PK.ToGuid()))
				{
					((BusinessObject)shipment).GetLogs().AddNew(Events.Authorised);
					Factory.Save();

					AssertEquals(true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, ((BusinessObject)shipment).GetLogs().Find(log => log.SL_SE_NKEvent == Events.Authorised.Code).First(), (BusinessObject)shipment));

					var logs = MasterFilesTestHelper.RunLogWalker();
					AssertEquals($"The log should match the pattern expected:\r\n{logs}", true, Regex.IsMatch(logs,
		@"\[WorkflowEventTrigger\] \[Default\] Action types in this batch: XUS, XUS"));

					var messages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(2, messages.Length);

					var bothCodes = string.Empty;
					foreach (var message in messages)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							var messageAsElement = XElement.Load(reader);
							var recipientRoleCollection = messageAsElement.Descendants(XName.Get("RecipientRoleCollection", "http://www.cargowise.com/Schemas/Universal/2011/11")).First();
							AssertEquals("We should only have one recipient role since the org proxy is swapped.", 1, recipientRoleCollection.Elements().Count());

							var code = recipientRoleCollection.Descendants(XName.Get("Code", "http://www.cargowise.com/Schemas/Universal/2011/11")).First().ToString();
							if (code.Contains("CNE") || code.Contains("ORP"))
							{
								bothCodes += code;
							}
							else
							{
								Fail("We should only have Consignee or OrgProxy");
							}
						}
					}
					if (!(bothCodes.Contains("CNE") || bothCodes.Contains("ORP")))
					{
						Fail("We should have both CNE and ORP");
					}
				}
			}
		}

		public void TestDataContextEventReferenceIsNotTruncated()
		{
			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Module = "SHP";

			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			orgProxy.EDICommunicationsModes.Add(communicationsMode);

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgProxy.PK;

			var workflow = (IWorkflowProviderIncludingRelated)shipment;
			var trigger = workflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Estimate Pickup";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			var longReference = new string('A', StmALogSchema.SL_Reference.MaxLength);
			((BusinessObject)shipment).GetLogs().AddNew(Events.CustomisableEvent00, longReference);
			Factory.Save();
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var logs = MasterFilesTestHelper.RunLogWalker();

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(1, messages.Length);
				AssertContains(longReference, messages[0].EM_MessageText);
			}
		}

		[TestDate(2019, 1, 1)]
		public void TestXTTCommunicationModeCreatesInterchange()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport =
				EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Module = "SHP";

			organization.EDICommunicationsModes.Add(communicationsMode);

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = organization.PK;

			var milestone = ((IWorkflowProvider)shipment).WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;

			var action1 = milestone.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action1.PQ_Calc_TriggerParty = "CNE";

			((BusinessObject)shipment).GetLogs().AddNew(Events.CustomisableEvent01);
			Factory.Save();

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var logs = MasterFilesTestHelper.RunLogWalker();
			}

			var interchange = Factory.LoadTop1<IEDIInterchange>(new ZQuery());
			AssertNotNull(interchange);

			AssertEquals(EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", 1, messages.Length);
		}

		[TestDate(2019, 1, 1)]
		public void TestMilestonesTriggerToSamePartyWithDifferentPurposeCreatesTwoMessages()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode.EK_CommunicationsTransport =
					EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
				communicationsMode.EK_Destination = "SOMEONE";
				communicationsMode.EK_Module = "SHP";

				var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
				orgToUse.EDICommunicationsModes.Add(communicationsMode);

				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				((BusinessObject)shipment).FillWithValidTestData();

				shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgToUse.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = orgToUse.PK;

				var milestone = ((IWorkflowProviderIncludingRelated)shipment).WorkflowItems.Milestones.AddNew();
				milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;

				var action1 = milestone.ProcessTaskNotifications.AddNew();
				action1.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
				action1.PQ_Calc_TriggerParty = "CNR";
				action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action1.PQ_MessagePurpose = "PUR";

				var action2 = milestone.ProcessTaskNotifications.AddNew();
				action2.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
				action2.PQ_Calc_TriggerParty = "CNE";
				action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action2.PQ_MessagePurpose = "RUP";

				((BusinessObject)shipment).GetLogs().AddNew(Events.CustomisableEvent01);
				Factory.Save();

				AssertEquals(true,
					TriggerConditionEvaluator.AreTriggerConditionsMet(milestone,
						((BusinessObject)shipment).GetLogs().Find(log => log.SL_SE_NKEvent == Events.CustomisableEvent01.Code).First(),
						(BusinessObject)shipment));

				var logs = MasterFilesTestHelper.RunLogWalker();
				AssertEquals($"The log should match the pattern expected:\r\n{logs}", true, Regex.IsMatch(logs,
					@"\[WorkflowEventTrigger\] \[Default\] processing 1 logged event\(s\).
\[WorkflowEventTrigger\] \[Default\] Processing: Shipment .+?
Task .+? \(Type: MIL, Event: Z01, Actual Date: 01-Jan-19 00:00:00 \+00:00, Parent Table Code: JS, Parent ID: .+?, PK: .+?\)"));

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(2, messages.Length);
				var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";

				var assertion = new Action<XElement>(element =>
				{
					var recipientRoleCollection = element.Descendants(XName.Get("RecipientRoleCollection", nameSpace)).First();
					var roleCollectionAsString = recipientRoleCollection.ToString();
					var purpose = element.Descendants(XName.Get("ActionPurpose", nameSpace)).First();
					if (roleCollectionAsString.Contains("<Code>CNE</Code>"))
					{
						AssertXMLContains("<Code>RUP</Code>", purpose.ToString());
					}
					else if (roleCollectionAsString.Contains("<Code>CNR</Code>"))
					{
						AssertXMLContains("<Code>PUR</Code>", purpose.ToString());
					}
					else
					{
						Assert("The recipient role should be either CNE or CNR", false);
					}
				});

				using (var reader = messages[0].GetEM_MessageTextReader())
				{
					assertion(XElement.Load(reader));
				}
				using (var reader = messages[1].GetEM_MessageTextReader())
				{
					assertion(XElement.Load(reader));
				}
			}
		}

		public void TestLoggingLongRunningTriggerActionsExeedingTimeLimit()
		{
			WorkflowDataRegistry.Instance.MillisecondsBeforeLoggingTriggerActions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			var trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "abc@email.com";

			Factory.Save();

			var actionRunner = new Mock<IWorkflowTriggerActionRunner>();
			actionRunner.Setup(m => m.Run(It.IsAny<IWorkflowTriggerAction>())).Callback(() =>
			{
				Thread.Sleep(2);
			});

			var manager = new WorkflowTriggerActionManagerForTesting(actionRunner.Object, Notifications);
			manager.Run((BusinessObject)WorkflowProvider, trigger);

			Assert("Trigger Action time is greater than 1ms.", Regex.IsMatch(Notifications.AsString, $@"Trigger Action time: ([2-9]|\d{{2,}})ms - Action: {notification.PQ_TriggerType}"));
		}

		#region Implementation

		class WorkflowTriggerActionManagerForTesting : WorkflowTriggerActionManager
		{
			public WorkflowTriggerActionManagerForTesting(IWorkflowTriggerActionRunner actionRunner, INotifications notifications = null)
				: base(notifications)
			{
				this.actionRunner = actionRunner;
			}
			readonly IWorkflowTriggerActionRunner actionRunner;

			protected override IWorkflowTriggerActionRunner GetActionRunner(ProcessTaskNotification triggerAction)
			{
				return actionRunner;
			}
		}

		class WorkflowTriggerActionManagerForTestingWithLogger : WorkflowTriggerActionManager
		{
			public WorkflowTriggerActionManagerForTestingWithLogger(IWorkflowTriggerActionRunner actionRunner, INotifications logsCollector)
				: base(logsCollector)
			{
				this.actionRunner = actionRunner;
			}
			readonly IWorkflowTriggerActionRunner actionRunner;

			protected override IWorkflowTriggerActionRunner GetActionRunner(ProcessTaskNotification triggerAction)
			{
				return actionRunner;
			}
		}

		class NotificationProxyForTesting : INotifications
		{
			public NotificationProxyForTesting(ICategoryLogger<WorkflowEventTriggerCategories> logger)
			{
				this.logger = logger;
			}

			#region INotifications Members

			public void Add(INotification notification)
			{
				logger.Log(notification.Type.ToLogType(), notification.Message);
			}

			#endregion

			readonly ICategoryLogger<WorkflowEventTriggerCategories> logger;
		}

		#endregion

		#region Integration Tests

		public void TestNotifications()
		{
			DummyWorkflowDescriptor.Instance.WorkflowTriggerActionProcessorOverride = new DummyWorkflowProcess();
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "XML";
			trigger.SetMilestoneActualDateForTest(ZDateTime.Now);

			Factory.Save();
			Dummy.Logs.AddNew(Events.CustomisableEvent10); // Add a nice event to make loading templates deterministic.
			Manager.Run(Dummy, trigger);

			AssertMultilineASCIIEquals("", @"Action types in this batch: XML
Firing action: XML
test message
Action completed: XML (Some regex replace value, since I don't care about this test)", Regex.Replace(Notifications.AsString, "[0-9]+ objects loaded, [0-9]+ table selects", "Some regex replace value, since I don't care about this test"));
		}

		public void TestRunSendDocumentsWorkflowTrigger()
		{
			ProcessTask trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			ProcessTaskNotification notification1 = trigger.ProcessTaskNotifications.AddNew();
			notification1.PQ_SU_Document = DocumentCommand.PK;
			notification1.PQ_TriggerType = "DOC";
			ProcessTaskNotification notification2 = trigger.ProcessTaskNotifications.AddNew();
			notification2.PQ_SU_Document = DocumentCommand.PK;
			notification2.PQ_TriggerType = "DOC";
			ProcessTaskNotification notification3 = trigger.ProcessTaskNotifications.AddNew();
			notification3.PQ_SU_Document = DocumentCommand2.PK;
			notification3.PQ_TriggerType = "DOC";
			var log = WorkflowProvider.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();
			Manager.Run((BusinessObject)WorkflowProvider, trigger, new QueuedLogForTesting(trigger.Logs.GetAllLogs().Cast<StmALog>().First(l => l.SL_SE_NKEvent == "WTE"), trigger));

			IStmPrintJob[] printJobs = Factory.Load<IStmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, BusinessObject.PK));
			AssertEquals("3 documents delivered - despite a duplicate trigger action", 3, printJobs.Length);
		}

		public void TestRunSendDocumentsWorkflowTrigger_Print()
		{
			IStmPrintQueue printQueue = Factory.New<IStmPrintQueue>();
			printQueue.QueueName = "TEST";
			Factory.Save();

			var trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification1 = trigger.ProcessTaskNotifications.AddNew();
			notification1.PQ_SU_Document = DocumentCommand.PK;
			notification1.PQ_TriggerType = "DOC";
			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			notification1.PQ_SQ = printQueue.PK;
			var log = WorkflowProvider.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();
			Manager.Run((BusinessObject)WorkflowProvider, trigger, new QueuedLogForTesting(trigger.Logs.GetAllLogs().Cast<StmALog>().First(l => l.SL_SE_NKEvent == "WTE"), trigger));
			IStmPrintJob[] printJobs = Factory.Load<IStmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printQueue.PK));
			AssertEquals("document printed", 1, printJobs.Length);
		}

		#region TestRunSendDocumentsWorkflowTriggerLoginsToCorrectCompany

		[TestDate(2021, 1, 1)]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestRunSendDocumentsWorkflowTriggerLoginsToCorrectCompany()
		{
			CurrentCompany = GlbCompany.CurrentCompany;
			var initialUserContext = Env.CurrentUserContext;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
			try
			{
				SetupDataForTestRunSendDocumentsWorkflowTriggerLoginsToCorrectCompany();

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
				new WorkflowServiceTaskTester().RunChain();

				ZQuery query = new ZQuery(StmPrintJobSchema.SP_ParentGuid, Shipment.PK);
				query.OrderBy = "SP_Sequence";

				IStmPrintJob[] printJobs = Factory.Load<IStmPrintJob>(query);

				AssertEquals("If job implements IJobHeaderParent then take branch from Job; If Trigger is Event Trigger take User from Run() params", SecondCompanyStaff.GS_GB_HomeBranch, ((BusinessObject)printJobs[0])[StmPrintJobSchema.SP_GB]);
				AssertEquals("If job implements IJobHeaderParent then take branch from Job; If Trigger is Field Change Trigger take User from StmChangeLog.SY_GS_NKUser", SecondCompanyStaff.GS_GB_HomeBranch, ((BusinessObject)printJobs[1])[StmPrintJobSchema.SP_GB]);

				foreach (ProcessTaskNotification notification in EventTrigger2.ProcessTaskNotifications)
				{
					notification.Validation.ValidateAll();
					AssertNoErrors(notification);
				}

				printJobs = Factory.Load<IStmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, Order.PK));

				AssertEquals("The document does not print with no contact configured", 0, printJobs.Length);
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		void SetupDataForTestRunSendDocumentsWorkflowTriggerLoginsToCorrectCompany()
		{
			Env.SetUserContext(new UserContext(SecondCompanyStaff.GS_LoginName, SecondCompany.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			_ = EventTrigger1;
			_ = EventTrigger2;
			_ = FieldChangeTrigger;

			((Forwarding.IForwardingShipment)Shipment).JS_HouseBill = "ABC123";
			Factory.Save();

			Env.SetUserContext(new UserContext(CurrentCompanyStaff.GS_LoginName, CurrentCompanyStaff.GS_GB_HomeBranch.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
		}

		#endregion

		public void TestRunEDocWorkflowTrigger()
		{
			ProcessTask trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification notification1 = trigger.ProcessTaskNotifications.AddNew();
			notification1.PQ_SU_Document = DocumentCommand.PK;
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;

			ProcessTaskNotification notification2 = trigger.ProcessTaskNotifications.AddNew();
			notification2.PQ_SU_Document = DocumentCommand.PK;
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;

			ProcessTaskNotification notification3 = trigger.ProcessTaskNotifications.AddNew();
			notification3.PQ_SU_Document = DocumentCommand2.PK;
			notification3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			trigger.SetMilestoneActualDateForTest(ZDateTime.Now);

			Factory.Save();

			IStmPrintJob[] printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
			AssertEquals("precondtion", 0, printJobs.Length);
			Manager.Run((BusinessObject)WorkflowProvider, trigger);
			printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
			AssertEquals("3 print jobs created to add documents to eDocs", 3, printJobs.Length);
			for (int i = 0; i < printJobs.Length; i++)
			{
				AssertEquals("job type should be DDS - 'Doc Delivery Success'", "DDS", ((BusinessObject)printJobs[i])[StmPrintJobSchema.SP_JobType]);
			}
		}

		public void TestRunWorkflowTrigger_LoginToJobParentBranch()
		{
			using (SetupTestDataForRunWorkflowTrigger_LoginToBranchTests("ForwardingShipmentWorkflowDescriptor"))
			{
				ZGuid shipmentPK;
				var branchPK = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "TES").PK.ToGuid();
				using (DisposableEnvironment.ForBranch(branchPK))
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					BusinessObject shipmentBizO = (BusinessObject)newFactory.New<Forwarding.IForwardingShipment>();
					shipmentBizO.FillWithValidTestData();
					JobHeader jobHeader = new JobHeader.Loader((IJobHeaderParent)shipmentBizO).TryCreate();
					newFactory.Save();
					shipmentPK = shipmentBizO.PK;

					IWorkflowProvider workflowProvider = (IWorkflowProvider)Factory.Load<Forwarding.IForwardingShipment>(shipmentPK);
					ProcessTask trigger = CreateTriggerActionForRunWorkflowTrigger_LoginToBranchTests(workflowProvider);
					Factory.Save();
					AssertEquals("Precondition", 0, Env.OutgoingMailManager.EmailsCreated.Count);

					Manager.Run((BusinessObject)workflowProvider, trigger);
				}
				Factory.Save();
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var body = Env.OutgoingMailManager.EmailsCreated[0].Body;
				AssertContains("| Test Branch | Eagle Datamation International", Env.OutgoingMailManager.EmailsCreated[0].Body);
			}
		}

		public void TestRunWorkflowTrigger_LoginToStaffHomeBranch()
		{
			var newFactory = new BusinessObjectFactory();
			var staff = newFactory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Cristiano Ronaldo";
			staff.GS_LoginName = Guid.NewGuid().ToString();
			staff.GS_GB_HomeBranch = Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, (ZString)"SYD").PK;
			newFactory.Save();

			using (SetupTestDataForRunWorkflowTrigger_LoginToBranchTests("ForwardingShipmentWorkflowDescriptor"))
			{
				IWorkflowProvider workflowProvider = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
				((BusinessObject)workflowProvider).FillWithValidTestData();
				ProcessTask trigger = CreateTriggerActionForRunWorkflowTrigger_LoginToBranchTests(workflowProvider);
				Factory.Save();
				AssertEquals("Precondition", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				var userContext = new TemporaryUserContext() { StaffLoginName = staff.GS_LoginName };
				using (userContext.Set())
				{
					trigger.P9_LineTriggerType = "";
					trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
					workflowProvider.Logs.AddNew(Events.CustomisableEvent00);
					Factory.Save();
				}

				WorkflowServiceTaskTestHelper.RunLogWalker();
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("Cristiano Ronaldo | EDIHQ | Eagle Datamation International", Env.OutgoingMailManager.EmailsCreated[0].Body);
			}
		}

		public void TestRunWorkflowTrigger_LoginToTriggerCompanyBranch()
		{
			using (SetupTestDataForRunWorkflowTrigger_LoginToBranchTests("ForwardingShipmentWorkflowDescriptor"))
			{
				ProcessTask trigger;
				var branchPK = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN").PK.ToGuid();
				using (DisposableEnvironment.ForBranch(branchPK))
				{
					IWorkflowProvider workflowProvider = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
					((BusinessObject)workflowProvider).FillWithValidTestData();
					trigger = CreateTriggerActionForRunWorkflowTrigger_LoginToBranchTests(workflowProvider);
					trigger.P9_LineTriggerType = "";
					trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
					workflowProvider.Logs.AddNew(Events.CustomisableEvent00);
					Factory.Save();
					AssertEquals("Precondition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				}
				WorkflowServiceTaskTestHelper.RunLogWalker();
				Factory.Save();
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("| Singapore Branch | Eagle Datamation International Pte Ltd", Env.OutgoingMailManager.EmailsCreated[0].Body);
			}
		}

		public void TestRunWorkflowTrigger_CorrectMessageRecipientsSelected()
		{
			using (SetupTestDataForRunWorkflowTrigger_LoginToBranchTests("ForwardingShipmentWorkflowDescriptor"))
			{
				ProcessTask trigger;
				var branchPK = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN").PK.ToGuid();
				using (DisposableEnvironment.ForBranch(branchPK))
				{
					IWorkflowProvider workflowProvider = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
					((BusinessObject)workflowProvider).FillWithValidTestData();
					JobHeader jobHeader = new JobHeader.Loader((IJobHeaderParent)workflowProvider).TryLoadOrCreateWithoutMutexForTestOnly();
					jobHeader.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
					OrgHeader localCharges = CreateOrgHeaderWithShipmentNotificationEmailCommMode("ted.burhan@cargowise.com.sg", "For Singapore Bill To Party");
					jobHeader.LocalChargesPK = localCharges.PK;
					trigger = CreateTriggerActionForRunWorkflowTrigger(workflowProvider, MessageRecipientPartyTypeList.Codes.BillToParty);
					trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
					trigger.P9_LineTriggerType = "";
					workflowProvider.Logs.AddNew(Events.CustomisableEvent00);
					Factory.Save();
					AssertEquals("Precondition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				}

				JobHeader anotherJobHeader = new JobHeader.Loader((IJobHeaderParent)trigger.Parent).TryLoadOrCreateWithoutMutexForTestOnly();
				anotherJobHeader.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
				OrgHeader anotherLocalCharges = CreateOrgHeaderWithShipmentNotificationEmailCommMode("ted.burhan@cargowise.com", "For Sydney Bill To Party");
				anotherJobHeader.LocalChargesPK = anotherLocalCharges.PK;
				Factory.Save();
				WorkflowServiceTaskTestHelper.RunLogWalker();
				Factory.Save();
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("For Singapore Bill To Party", Env.OutgoingMailManager.EmailsCreated[0].Subject);
				AssertContains("ted.burhan@cargowise.com.sg", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			}
		}

		public void TestCreditControlledDocumentFailure()
		{
			using (SetupTestDataForRunWorkflowTrigger_LoginToBranchTests("ForwardingShipmentWorkflowDescriptor"))
			{
				SetupWorkflowNotificationRegistry();

				CreditControlledWorkflowProviderBizo bizo = Factory.New<CreditControlledWorkflowProviderBizo>();
				var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
				CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, true);
				bizo.OrganisationsForCreditChecksForTest = new[] { orgHeader };
				ProcessTask processTask = bizo.WorkflowItems.Triggers.AddNew();

				StmMenuItem creditControlledDocument = Factory.NewWithValidTestData<StmMenuItem>();
				creditControlledDocument.SU_MenuName = "TEST";
				creditControlledDocument.SU_BusinessContext = "TEST";
				creditControlledDocument.SU_ContactType = ContactType.Consignee.Code;
				creditControlledDocument.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);

				var notification = processTask.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
				notification.PQ_SU_Document = creditControlledDocument.PK;

				DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(CreditControlledWorkflowProviderBizo);
				DummyWorkflowDescriptor.Instance.OverriddenWorkflowProviderType = typeof(CreditControlledWorkflowProviderBizo);
				DummyWorkflowDescriptor.Instance.SetDocumentBusinessContext(Array.Empty<BusinessContext>());
				new WorkflowTriggerActionManager(Notifications).Run(bizo, processTask, new QueuedLogForTesting(Factory));
				Factory.Save();
				WorkflowServiceTaskTestHelper.RunLogWalker();

				AssertEquals("Failure notification email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("workflowstuff@cargowise.com", email.Recipients[0].Email);
				AssertEquals("WorkflowManager Trigger Action Failure - DummyBizo - Default", email.Subject);
				string expectedBody =
	@"The following trigger action failed to run:
Job: DummyBizo - Default
Trigger Event: 
Trigger Action: TEST";
				AssertStartsWith("Failure for whatever reason.", expectedBody, email.Body);
			}
		}

		OrgHeader CreateOrgHeaderWithShipmentNotificationEmailCommMode(string emailAddress, string subject)
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			EDICommunicationsMode mode = orgHeader.EDICommunicationsModes.AddNew();
			mode.EK_Module = "DUM";
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = emailAddress;
			mode.EK_ServerAddressSubject = subject;
			return orgHeader;
		}

		IDisposable SetupTestDataForRunWorkflowTrigger_LoginToBranchTests(string workflowDescriptorNameToBeSubstituted)
		{
			DummyWorkflowDescriptor descriptor = DummyWorkflowDescriptor.Instance;
			IDisposable workflowDescriptorSubsitute = ObjectFactory.Substitute(workflowDescriptorNameToBeSubstituted, descriptor);
			{
				DummyWorkflowDescriptor.Instance.ExtraDataSubstitutionForNotificationEmail = (action, bizo, data) =>
				{
					ZString result = data.ReplaceIgnoringCase("(*CurrentUser*)", Env.CurrentUser.FullName);
					result = result.ReplaceIgnoringCase("(*CurrentBranch*)", Env.CurrentBranch.Name);
					result = result.ReplaceIgnoringCase("(*CurrentCompany*)", Env.CurrentCompany.Name);
					return result;
				};
			}

			return new DisposableAction(workflowDescriptorSubsitute.Dispose);
		}

		ProcessTask CreateTriggerActionForRunWorkflowTrigger_LoginToBranchTests(IWorkflowProvider workflowProvider)
		{
			ProcessTask trigger = CreateTriggerActionForRunWorkflowTrigger(workflowProvider, MessageRecipientPartyTypeList.Codes.Email);
			ProcessTaskNotification triggerAction = trigger.ProcessTaskNotifications[0];
			triggerAction.PQ_EmailAddr = "ted.burhan@cargowise.com";
			triggerAction.PQ_EmailText = "(*CurrentUser*) | (*CurrentBranch*) | (*CurrentCompany*)";
			return trigger;
		}

		ProcessTask CreateTriggerActionForRunWorkflowTrigger(IWorkflowProvider workflowProvider, string triggerParty)
		{
			ProcessTask trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = "DUM";
			ProcessTaskNotification triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			triggerAction.PQ_TriggerParty = triggerParty;
			var descriptor = (DummyWorkflowDescriptor)trigger.GetWorkflowDescriptor();
			descriptor.OverriddenWorkflowProviderType = workflowProvider.GetType();
			return trigger;
		}

		void SetupWorkflowNotificationRegistry()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.FillWithValidTestData();
			staff.GS_EmailAddress = "workflowstuff@cargowise.com";
			Factory.Save();
			WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		#region Implementation

		class DummyWorkflowProcess : IProcessor
		{
			#region IProcessor Members

			void IProcessor.Process(INotifications notifications, CancellationToken token)
			{
				notifications.Notify(new InfoNotification("test message"));
			}

			#endregion
		}

		BusinessObject DocumentCommand
		{
			get
			{
				DocumentZQuery query = new DocumentZQuery(BusinessContext.Shipment, "Delay Alert");
				return (BusinessObject)Factory.LoadTop1<IDocumentCommand>(query);
			}
		}

		StmMenuItem OrderDocumentCommand
		{
			get
			{
				DocumentZQuery query = new DocumentZQuery(BusinessContext.Order);
				var doc = Factory.LoadTop1<StmMenuItem>(query);
				doc.SU_ContactType = ContactType.Consignor.Code;
				return doc;
			}
		}

		BusinessObject DocumentCommand2
		{
			get
			{
				DocumentZQuery query = new DocumentZQuery(BusinessContext.Shipment, "Arrival Notice");
				return (BusinessObject)Factory.LoadTop1<IDocumentCommand>(query);
			}
		}

		WorkflowTriggerActionManager Manager
		{
			get { return manager ?? (manager = new WorkflowTriggerActionManager(Notifications)); }
		}
		WorkflowTriggerActionManager manager;

		NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		IWorkflowProvider WorkflowProvider
		{
			get { return (IWorkflowProvider)BusinessObject; }
		}

		BusinessObject BusinessObject
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
					OrgDocument document2 = contact.Documents.AddNew();
					document2.OD_SU_MenuItem = DocumentCommand2.PK;

					businessObject["ConsigneePK"] = consignee.PK;
				}
				return businessObject;
			}
		}
		BusinessObject businessObject;

		GlbStaff CurrentCompanyStaff
		{
			get
			{
				if (fCurrentCompanyStaff == null)
				{
					fCurrentCompanyStaff = Factory.NewWithValidTestData<GlbStaff>();
					fCurrentCompanyStaff.GS_LoginName = "CurrentCompanyStaff";
					fCurrentCompanyStaff.GS_Code = "SS1";
					fCurrentCompanyStaff.GS_GB_HomeBranch = CurrentCompany.Branches[0].PK;

					Factory.Save();
				}

				return fCurrentCompanyStaff;
			}
		}
		GlbStaff fCurrentCompanyStaff;

		GlbStaff SecondCompanyStaff
		{
			get
			{
				if (fSecondCompanyStaff == null)
				{
					fSecondCompanyStaff = Factory.NewWithValidTestData<GlbStaff>();
					fSecondCompanyStaff.GS_LoginName = "SecondCompanyStaff";
					fSecondCompanyStaff.GS_Code = "SS2";
					fSecondCompanyStaff.GS_GB_HomeBranch = SecondCompany.Branches[0].PK;

					Factory.Save();
				}

				return fSecondCompanyStaff;
			}
		}
		GlbStaff fSecondCompanyStaff;

		GlbCompany SecondCompany
		{
			get
			{
				if (fSecondCompany == null)
				{
					GlbCompany[] companies = Factory.Load<GlbCompany>(new ZQuery());
					Assert("Preconditional: Should exists test companies", companies.Length > 2);

					fSecondCompany = companies[0].PK == CurrentCompany.PK ? companies[1] : companies[0];

					Factory.Save();
				}

				return fSecondCompany;
			}
		}
		GlbCompany fSecondCompany;

		GlbCompany CurrentCompany;

		OrgHeader Consignee
		{
			get
			{
				if (fConsignee == null)
				{
					fConsignee = Factory.NewWithValidTestData<OrgHeader>();

					OrgContact contact = fConsignee.Contacts.AddNew();
					contact.OC_ContactName = "Test Contact";
					contact.OC_Email = "andriy.mazur@cargowise.com";

					OrgDocument document = contact.Documents.AddNew();
					document.OD_SU_MenuItem = DocumentCommand.PK;
					OrgDocument document2 = contact.Documents.AddNew();
					document2.OD_SU_MenuItem = DocumentCommand2.PK;
				}

				return fConsignee;
			}
		}
		OrgHeader fConsignee;

		BusinessObject Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
					fShipment["ConsigneePK"] = Consignee.PK;

					JobHeader job = new JobHeader.Loader((IJobHeaderParent)fShipment).TryLoadOrCreate();
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
					job.JH_GB = GlbStaff.CurrentUser.GS_GB_HomeBranch;
					job.JH_ParentID = fShipment.PK;
				}

				return fShipment;
			}
		}
		BusinessObject fShipment;

		BusinessObject Order
		{
			get
			{
				if (fOrder == null)
				{
					fOrder = (BusinessObject)Factory.New<Forwarding.IOrder>();
					fOrder["JD_OrderNumber"] = "1233";
					fOrder["JD_OA_BuyerAddress"] = Consignee.MainAddress.PK;
				}

				return fOrder;
			}
		}
		BusinessObject fOrder;

		ProcessTask EventTrigger1
		{
			get
			{
				if (fEventTrigger1 == null)
				{
					fEventTrigger1 = ((IWorkflowProvider)Shipment).WorkflowItems.Triggers.AddNew();
					ProcessTaskNotification notification = fEventTrigger1.ProcessTaskNotifications.AddNew();
					notification.PQ_SU_Document = DocumentCommand.PK;
					notification.PQ_TriggerType = "DOC";
					notification.PQ_Calc_TriggerParty = "ADV";
					fEventTrigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
					((IWorkflowProvider)Shipment).Logs.AddNew(Events.CustomisableEvent01);
				}

				return fEventTrigger1;
			}
		}
		ProcessTask fEventTrigger1;

		ProcessTask EventTrigger2
		{
			get
			{
				if (fEventTrigger2 == null)
				{
					fEventTrigger2 = ((IWorkflowProvider)Order).WorkflowItems.Triggers.AddNew();
					ProcessTaskNotification notification = fEventTrigger2.ProcessTaskNotifications.AddNew();
					notification.PQ_SU_Document = OrderDocumentCommand.PK;
					notification.PQ_TriggerType = "DOC";
					notification.PQ_Calc_TriggerParty = "ADV";
					fEventTrigger2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02Code;
					((IWorkflowProvider)Order).Logs.AddNew(Events.CustomisableEvent02);
				}

				return fEventTrigger2;
			}
		}
		ProcessTask fEventTrigger2;

		ProcessTask FieldChangeTrigger
		{
			get
			{
				if (fFieldChangeTrigger == null)
				{
					fFieldChangeTrigger = ((IWorkflowProvider)Shipment).WorkflowItems.Triggers.AddNew();
					ProcessTaskNotification notification = fFieldChangeTrigger.ProcessTaskNotifications.AddNew();
					notification.PQ_SU_Document = DocumentCommand.PK;
					notification.PQ_TriggerType = "DOC";
					notification.PQ_Calc_TriggerParty = "ADV";
					fFieldChangeTrigger.TriggerConditions.TriggerFieldName = JobShipmentSchema.JS_HouseBill.Name;
				}

				return fFieldChangeTrigger;
			}
		}
		ProcessTask fFieldChangeTrigger;

		#endregion

		#endregion
	}

	static class WorkflowTriggerActionManger_TestExtensions
	{
		public static void Run(this WorkflowTriggerActionManager manager, BusinessObject parent, ProcessTask trigger)
		{
			manager.Run(parent, trigger, new QueuedLogForTesting(new BusinessObjectFactory()) { SJ_SE_NKEvent = "WTE" });
		}
	}
}
