using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LogWalker.Test
{
	[UseSnapshotProtection]
	class NewsTransmitterSnapshotProtectionTest : TestCase
	{
		#region Setup

		public ProcessTask AddTrigger(IWorkflowProvider provider, Event eventType)
		{
			var trigger = provider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = eventType.Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_EmailAddr = "turkey@dresses.milnor";

			return trigger;
		}

		DummyWithWorkflow GetDummy(BusinessObjectFactory factory, DummyWithWorkflow dummyWithWorkflow) => factory.Load<DummyWithWorkflow>(dummyWithWorkflow.PK);

		public MockNewsTransmitter GetNewsTransmitter(LogSubscriber subscriber, params LogSubscriber[] otherSubscribers) => new MockNewsTransmitter(subscriber, otherSubscribers.Append(subscriber).ToArray(), new SubscriberParameters() { Logger = Logger });

		#endregion

		public LoggerForLogWalkerTest Logger => logger ?? (logger = new LoggerForLogWalkerTest());
		LoggerForLogWalkerTest logger;

		public void TestNewsTransmitterAllowRecursion_MultiSubscriber()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);
			var subscriber1 = new MockSubscriber(logs => GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent01), name: "MingMing");
			var subscriber2 = new MockSubscriber(logs => GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent01), name: "Boky");
			var transmitter = GetNewsTransmitter(subscriber1, subscriber2);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			AssertContains("recurring with subscriber [MingMing] and depth 1 for 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
			AssertContains("recurring with subscriber [Boky] and depth 1 for 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
		}

		public void TestNewsTransmitterAllowRecursion_IsDelayFired()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);
			var subscriber1 = new MockSubscriber(logs => GetDummy(logs[0].Factory, dummy).Logs.AddNew(new EventValue(Events.CustomisableEvent01, deferFiringWorkflow: true)), name: "MingMing");
			var transmitter = GetNewsTransmitter(subscriber1, new SubscriberProvider().GetAllSubscribersWithValidationAndAppendingToNotificationLog(Logger));
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			AssertContains("recurring with subscriber [TasksAndMilestonesLoader]", string.Join(System.Environment.NewLine, Logger.LogEntries));
		}

		public void TestNewsTranmitterAllowRecursion_LogOnSave()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);
			var subscriber1 = new MockSubscriber(logs => logs[0].Factory.Saving += (f) => GetDummy(f, dummy).Logs.AddNew(Events.CustomisableEvent01), name: "MingMing");
			var transmitter = GetNewsTransmitter(subscriber1);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			AssertContains("recurring with subscriber [MingMing] and depth 1 for 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
		}

		public void TestNewsTransmitterDisposeRecurringLogHandler()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);
			var subscriberFactorySaveCount = 0;
			var subscriber1 = new MockSubscriber(logs =>
			{
				subscriberFactorySaveCount++;
				logs[0].Factory.Saving += (f) =>
				{
					if (subscriberFactorySaveCount % 2 == 0) //add a new log each time after processing to cause recursion within log processing
					{
						GetDummy(f, dummy).Logs.AddNew(Events.CustomisableEvent01);
					}
				};
				logs[0].Factory.Saved += (_, c) =>
				{
					if (subscriberFactorySaveCount == 1)
					{
						throw new Exception();
					}
					else if (subscriberFactorySaveCount == 2)
					{
						Db.Connection.ExecuteNonQuery("ROLLBACK");
						throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Boom a conflict has occured."), ((INeedRow)dummy).Row, Db.Connection), factory);
					}
				};
			}, name: "WorkflowEventTrigger");
			dummy.Logs.AddNew(Events.CustomisableEvent00);

			factory.Save();
			LogWalkerRunner.Master().Process(Logger, CancellationToken.None);

			var transmitter = GetNewsTransmitter(subscriber1);
			transmitter.ProcessLogsFromDb(1);
			var processedLogs = factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent01Code));
			AssertEquals(1, processedLogs.Length);
		}

		public void TestPartialTemplateApplication_FireWorkflow()
		{
			// Set up a trigger that will add a milestone. When that milestone is added it ought to immediately fire.
			var factory = new BusinessObjectFactory();
			var partial = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partial.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			partial.P0_IsPartialTemplate = true;
			var milestone = partial.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Benefro";
			milestone.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;
			var notification = milestone.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "ISaw@patronage.com";

			factory.Save();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Vivaldi";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification2 = trigger.ProcessTaskNotifications.AddNew();
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			notification2.PQ_P0_WorkflowTemplate = partial.PK;
			dummy.Logs.AddNew(Events.CustomisableEvent00);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			dummy.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			factory.Save();

			LogWalkerRunner.Master().Process(Logger, CancellationToken.None);
			LogWalkerRunner.Default().Process(Logger, CancellationToken.None);
			LogWalkerRunner.Purge().Process(Logger, CancellationToken.None);

			dummy.WorkflowItems.Reload(true);
			var m = (ProcessTask)dummy.WorkflowItems.Milestones.Single();
			Assert(m.P9_ActualDate.IsValid);
			AssertNotNull("Workflow should have fired on the new milestone", m.GetLogs().GetAllLogs().Cast<StmALog>().SingleOrDefault(s => s.SL_SE_NKEvent == Events.WorkflowTriggerEventCode));
		}

		public void TestSaveInTheSubscriber_ShouldNotReallySave()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_Description = "Javascript";
			factory.Save();
			var subscriber1 = new MockSubscriber(logs =>
			{
				var badFactory = new BusinessObjectFactory();
				var reloadedDummy = GetDummy(badFactory, dummy);
				reloadedDummy.Z0_Description = "Common Lisp is a good language right?";
				badFactory.Save();
				throw new InvalidOperationException();
			},
			name: "())(())(()))))");
			var transmitter = GetNewsTransmitter(subscriber1);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });

			var anotherReloadedDummy = GetDummy(new BusinessObjectFactory(), dummy);
			AssertEquals("Javascript", anotherReloadedDummy.Z0_Description);
			ErrorReporter.Clear();
		}

		public void TestSaveInTheSubscriber_ShouldNotReallySave_UnlessWeTurnOffProtection()
		{
			SystemDataRegistry.Instance.LogSubscriberUseSharedTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_Description = "Javascript";
			factory.Save();
			var subscriber1 = new MockSubscriber(logs =>
			{
				var badFactory = new BusinessObjectFactory();
				var reloadedDummy = GetDummy(badFactory, dummy);
				reloadedDummy.Z0_Description = "Common Lisp is a good language right?";
				badFactory.Save();
				throw new InvalidOperationException();
			},
			name: "())(())(()))))");
			var transmitter = GetNewsTransmitter(subscriber1);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });

			var anotherReloadedDummy = GetDummy(new BusinessObjectFactory(), dummy);
			AssertEquals("Common Lisp is a good language right?", anotherReloadedDummy.Z0_Description);
			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		public void TestTransactionQuietlyRolledback()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var shipmentType = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
			var shipment = factory.New(shipmentType);
			shipment["JS_UniqueConsignRef"] = "S00001011";

			var trigger1 = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Send Data";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var notification1 = trigger1.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			var shipmentLogs = shipment.GetLogs();
			shipment.GetLogs().AddNew(Events.CustomisableEvent00);
			factory.Save();

			var lastSave = false;
			BusinessObjectFactory.SetOnFactorySaveHookForTest((f) =>
			{
				if (f.NameForDebugging == "Subscriber: WorkflowEventTriggerProcessor" || f.NameForDebugging == "NewsTransmitter Read Factory")
				{
					if (lastSave)
					{
						f.Saved += (_, s) =>
						{
							Db.Connection.RollbackTransaction();
						};
						lastSave = false;
					}
					else
					{
						lastSave = true;
					}
				}
			});

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var res = MasterFilesTestHelper.RunLogWalker();
				AssertContains("A delayed transaction needs to be rolled back", res);
			}

			var stmJobQueueStatus = Db.Connection.ExecuteScalar<string>("SELECT TOP 1 SJ_Status FROM dbo.StmJobQueue WHERE SJ_SE_NKEvent = 'WTE'");
			AssertEquals("FAI", stmJobQueueStatus);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestTransactionExceptionCaughtInternallyRecoversOnRetry()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var shipment = factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment["JS_UniqueConsignRef"] = "S00001011";

			var trigger1 = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var notification1 = trigger1.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			shipment.GetLogs().AddNew(Events.CustomisableEvent00);
			factory.Save();

			var isProcessingSave = false;
			var saveCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest((f) =>
			{
				if (f.NameForDebugging == "Subscriber: WorkflowEventTriggerProcessor")
				{
					if (isProcessingSave && saveCount < 3) // save 0 is journaling save before processing, save 1 is after first processing attempt, next saves are for retries
					{
						f.Saved += (_, s) =>
						{
							var connection = ((IDbConnected)factory).Connection;
							connection.BeginTransaction();
							connection.ExecuteNonQuery("ROLLBACK");
							connection.CommitTransaction();
						};
						isProcessingSave = false;
						saveCount++;
					}
					else
					{
						isProcessingSave = true;
					}
				}
			});

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var res = MasterFilesTestHelper.RunLogWalker();
				AssertContains("This SqlTransaction has completed; it is no longer usable.", res);
				AssertContains("Action completed: XUS", res);
			}

			AssertEquals(JobQueueStatus.StatusProcessed, Db.Connection.ExecuteScalar<string>("SELECT TOP 1 SJ_Status FROM dbo.StmJobQueue WHERE SJ_SE_NKEvent = 'WTE'"));
		}

		[UseSnapshotProtection]
		public void TestTransactionQuietlyRolledback_RecoverOnRetry()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var shipmentType = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
			var shipment = factory.New(shipmentType);
			shipment["JS_UniqueConsignRef"] = "S00001011";

			var trigger1 = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Send Data";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var notification1 = trigger1.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			var shipmentLogs = shipment.GetLogs();
			shipment.GetLogs().AddNew(Events.CustomisableEvent00);
			factory.Save();

			var saveCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest((f) =>
			{
				if (f.NameForDebugging == "Subscriber: WorkflowEventTriggerProcessor")
				{
					if (saveCount == 1)// save 0 is journaling save before processing, save 1 is after first processing attempt, next saves are for retries
					{
						f.Saved += (_, s) =>
						{
							Db.Connection.RollbackTransaction();
						};
					}
					saveCount++;
				}
			});

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var res = MasterFilesTestHelper.RunLogWalker();
				AssertContains("A delayed transaction needs to be rolled back", res);
				AssertContains("Action completed: XUS", res);
			}
			AssertEquals(JobQueueStatus.StatusProcessed, Db.Connection.ExecuteScalar<string>("SELECT TOP 1 SJ_Status FROM dbo.StmJobQueue WHERE SJ_SE_NKEvent = 'WTE'"));
		}

		[UseSnapshotProtection]
		public void TestXUSTriggerAction_SentInternally_ConcurrencyError()
		{
			var currentOrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			try
			{
				var factory = new BusinessObjectFactory();
				var orgProxy = GlbCompany.CurrentCompany.GetNewOrgProxy(factory);
				factory.Save();
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;
				GlbCompany.CurrentCompany.Factory.Save();

				var shipment = factory.New<Forwarding.IForwardingShipment>();
				((BusinessObject)shipment).FillWithValidTestData();

				var workflow = (IWorkflowProviderIncludingRelated)shipment;
				var trigger = workflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Estimate Pickup";
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action1 = trigger.ProcessTaskNotifications.AddNew();
				action1.PQ_SU_Document = factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
				action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;
				action1.PQ_OH_Recipient = orgProxy.PK;
				action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.DeliveryCartage;
				// as the recipient is an OrgProxy the xml will be sent internally via SendUniversalXmlInternally which currently calls save

				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					factory.Save();

					((BusinessObject)shipment).GetLogs().AddNew(Events.Authorised);
					factory.Save();

					var exceptionThrown = false;
					BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
					{
						if (!exceptionThrown && f is IUniversalBusinessObjectFactory)
						{
							exceptionThrown = true;
							throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Boom a conflict has occured."), ((INeedRow)trigger).Row, Db.Connection), factory);
						}
					});

					var logs = MasterFilesTestHelper.RunLogWalker();
					AssertContains("First attempt should fail and the exception should be handled in NewsTransmitter", "A conflict during save will cause logs to be processed one at a time", logs);
					AssertContains("Second attempt should succeed", "Action completed: XUS", logs);

					var messages = factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(1, messages.Length);
					var message = messages[0];
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					AssertEquals(ReceiveTransmitList.Codes.Internal, message.EM_ReceiveTransmit);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = currentOrgProxy;
			}
		}

		[UseSnapshotProtection]
		public void TestXUS_DontUseDifferentFactoriesToModifyShipment()
		{
			var currentOrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			try
			{
				var factory = new BusinessObjectFactory();
				var orgProxy = GlbCompany.CurrentCompany.GetNewOrgProxy(factory);
				factory.Save();
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;
				GlbCompany.CurrentCompany.Factory.Save();

				var shipment = factory.New<Forwarding.IForwardingShipment>();
				((BusinessObject)shipment).FillWithValidTestData();

				var workflow = (IWorkflowProviderIncludingRelated)shipment;
				var trigger = workflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Estimate Pickup";
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action1 = trigger.ProcessTaskNotifications.AddNew();
				action1.PQ_SU_Document = factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
				action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;
				action1.PQ_OH_Recipient = orgProxy.PK;
				action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.DeliveryCartage;
				// as the recipient is an OrgProxy the xml will be sent internally via SendUniversalXmlInternally which currently calls save

				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				using (new DataRefreshManager.DisableRefreshForServiceTask())
				{
					factory.Save();

					((BusinessObject)shipment).GetLogs().AddNew(Events.Authorised);
					factory.Save();

					int i = 0;
					BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
					{
						if (f is IUniversalBusinessObjectFactory || f.NameForDebugging.Contains("Subscriber:"))
						{
							var shp = f.Load<Forwarding.IForwardingShipment>(shipment.PK);
							shp.JS_ShipmentStatus = $"CN{i}";
							i = (i + 1) % 10;
						}
					});

					var logs = MasterFilesTestHelper.RunLogWalker();

					var retryCount = Db.Connection.ExecuteScalar<byte>("SELECT TOP 1 SJ_RetryCount FROM dbo.StmJobQueue WHERE SJ_SE_NKEvent = 'WTE'");
					AssertEquals("Expecting 1 failure and DataRefresh is enabled in the retry which should prevent internal concurrency", (short)2, Convert.ToInt16(retryCount));
					AssertContains("First attempt should fail and the exception should be handled in NewsTransmitter", "A conflict during save will cause logs to be processed one at a time", logs);
					AssertContains("Should succeed with out concurrency errrors", "Action completed: XUS", logs);

					var messages = factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(1, messages.Length);
					var message = messages[0];
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					AssertEquals(ReceiveTransmitList.Codes.Internal, message.EM_ReceiveTransmit);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = currentOrgProxy;
			}
		}

		[UseSnapshotProtection]
		public void TestProcessTaskCountdownMergeInLWK()
		{
			int mainFactorySaveCount = 0;
			int transmitterFactorySaveCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest((f) =>
			{
				if (f.NameForDebugging == "main")
				{
					mainFactorySaveCount++;
				}
				else
				{
					transmitterFactorySaveCount++;
				}
			});

			var factory = new BusinessObjectFactory() { NameForDebugging = "main", RefreshEnabled = false };
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = AddTrigger(dummy, Events.CustomisableEvent00);

			AssertEquals((ZShort)100, trigger.P9_TriggerFiredCountdown);

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "alex@iscool.com";

			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			factory.Save();

			AssertEquals((ZShort)99, trigger.P9_TriggerFiredCountdown);

			var subscriber = new MockSubscriber(logs =>
			{
				var transmitterFactory = logs[0].Factory;
				var dummyTransmitterFactory = GetDummy(transmitterFactory, dummy);

				var triggerTransmitterFactory = dummyTransmitterFactory.WorkflowItems.Triggers[0];
				using (triggerTransmitterFactory.TemporarilyAllowSettingCondition("P9_TriggerFiredCountdown"))
				{
					triggerTransmitterFactory.P9_TriggerFiredCountdown = 98;
				}

				if (mainFactorySaveCount == 1)
				{
					using (trigger.TemporarilyAllowSettingCondition("P9_TriggerFiredCountdown"))
					{
						trigger.P9_TriggerFiredCountdown = 97;
					}
					factory.Save();
				}
			}, name: "WorkflowEventTrigger");
			LogWalkerRunner.Master().Process(new LoggerForTest(), CancellationToken.None);

			var transmitter = GetNewsTransmitter(subscriber);

			transmitter.ProcessLogsFromDb(1);

			AssertEquals(2, mainFactorySaveCount);
			AssertEquals(2, transmitterFactorySaveCount);

			var triggerReloaded = new BusinessObjectFactory().Load<ProcessTask>(trigger.PK).P9_TriggerFiredCountdown;
			AssertEquals((ZShort)98, triggerReloaded);
		}

		[UseSnapshotProtection]
		public void TestNewsTransmitterAllowRecursion_HandlesAfterOnSavingBusinessObjectEvents()
		{
			var service = new MockAfterOnSavingServiceWithEvent();
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			var trigger = AddTrigger(dummy, Events.CustomisableEvent01);
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<Z0_Code>";
			action.PQ_FieldValue = "Test";

			var subscriber = new MockSubscriber(logs => GetDummy(logs[0].Factory, dummy).Z0_Number++);
			var allSubscribers = new SubscriberProvider().GetAllSubscribersWithValidationAndAppendingToNotificationLog(Logger);
			var wteSubscriber = allSubscribers.First(sub => sub.Name == "WorkflowEventTrigger");
			wteSubscriber.SetDefaultLogger(Logger);
			var transmitter = GetNewsTransmitter(subscriber, wteSubscriber);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			dummy.Reload();

			AssertEquals("Trigger fired", 99, (int)trigger.P9_TriggerFiredCountdown);
			AssertEquals("WTE event added", 1, trigger.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode)).Length);
			AssertEquals("Trigger action run", "Test", dummy.Z0_Code);
			AssertContains("Recurs in LWK", "recurring with subscriber [WorkflowEventTrigger] and depth 1 for 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
		}

		class MockAfterOnSavingServiceWithEvent : IAfterOnSavingBOProcessingService
		{
			public MockAfterOnSavingServiceWithEvent()
			{
				bool addedService = false;
				BusinessObjectFactory.SetOnFactorySaveInTransactionForTest(factory =>
				{
					if (factory.ServiceContainer.GetAfterOnSavingService<RecurringLogHandler>() != null && !addedService)
					{
						factory.ServiceContainer.AddAfterOnSavingService(this);
						addedService = true;
					}
				});
			}

			void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				foreach (var bizo in businessObjectsInOnSavingOrder)
				{
					if (bizo is DummyWithWorkflow && bizo.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent01Code)).Length == 0)
					{
						bizo.GetLogs().AddNew(Events.CustomisableEvent01);
					}
				}
			}
		}
	}
}
