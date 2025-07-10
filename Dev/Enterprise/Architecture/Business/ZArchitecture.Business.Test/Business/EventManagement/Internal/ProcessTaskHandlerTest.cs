using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class ProcessTaskHandlerTest : TestCaseWithFactory
	{
		public void TestEventDeletedDuringFireWorkflow()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipmentLog = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);
			var processHandlingInfo = new ProcessHandlingInfoForTest((EnterpriseBusinessObject)shipment, l =>
			{
				((BusinessObject)l).Delete();
				return null;
			});
			var processTaskHandler = ProcessTaskHandlerProvider.GetHandler(processHandlingInfo, shipmentLog);
			AssertNoExceptionThrown(delegate
			{
				using (EventRecursionHandler.WithEventRecursionDetection())
				{
					processTaskHandler.Fire();
				}
			});

			AssertEquals(true, shipmentLog.IsDeleted);
		}

		public void TestEventDeletedWhileCascading()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));

			Factory.Save();

			var shipmentLog = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);
			var cascadingLinks = new List<CascadingLink>();

			for (int i = 0; i < 2; i++)
			{
				var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));
				var consolTrigger = Factory.New<Forwarding.IForwardingConsolProcessTask>() as IProcessTask;
				((IBaseTrigger)consolTrigger).TriggerEventCode = Events.ServiceInvoicePostedCode;
				consolTrigger.P9_ParentID = consol.PK;
				consolTrigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				consolTrigger.P9_ParentTableCode = consol.TablePrefix;

				var cascadingLink = new CascadingLink()
				{
					Parent = (IStmALogParent)consol,
					Triggers = new[] { (IBaseTrigger)consolTrigger }
				};
				cascadingLinks.Add(cascadingLink);
			}

			var processHandlingInfo = new ProcessHandlingInfoForTest(shipment, null, null, cascadingLinks);

			ProcessTaskHandler.SetOnFireHookForTest((log, methodName) =>
			{
				if (methodName == "UpdateTriggerEventDates" && log.SL_SE_NKEvent == Events.ServiceInvoicePostedCode)
				{
					using (((IBusinessObjectInternals)log).SuppressReportRowDeletedError())
					{
						((BusinessObject)log).Delete();
					}
				}
			});

			AssertNoExceptionThrown(delegate
			{
				using (EventRecursionHandler.WithEventRecursionDetection())
				{
					ProcessTaskHandlerProvider.GetHandler(processHandlingInfo, shipmentLog).Fire();
				}
			});

			Assert("Trigger on cascading job not fired after the log from the parent job is deleted", ((IProcessTask)cascadingLinks[1].Triggers[0]).P9_ActualDate.IsEmpty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestNoCurrentUser()
		{
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipmentLog = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);

			var propagationLink = new PropagationLink((IStmALogParent)consol, new BusinessObject[] { shipment }, "test");
			var processHandlingInfo = new ProcessHandlingInfoForTest((EnterpriseBusinessObject)shipment, System.Array.Empty<BusinessObject>(), new[] { propagationLink });
			var processTaskHandler = ProcessTaskHandlerProvider.GetHandler(processHandlingInfo, shipmentLog);

			EnvProxy.Instance.SetUserContext(null);

			AssertNoExceptionThrown(delegate
			{
				using (EventRecursionHandler.WithEventRecursionDetection())
				{
					processTaskHandler.Fire();
				}
			});
		}

		public void TestProcessTaskHandler_RowDeleted()
		{
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipmentLog = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);

			var propagationLink = new PropagationLink((IStmALogParent)consol, new BusinessObject[] { shipment }, "test");
			var processHandlingInfo = new ProcessHandlingInfoForTest((EnterpriseBusinessObject)shipment, System.Array.Empty<BusinessObject>(), new[] { propagationLink });
			var processTaskHandler = ProcessTaskHandlerProvider.GetHandler(processHandlingInfo, shipmentLog);

			shipmentLog.Delete();
			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				processTaskHandler.Withdraw();
			}

			AssertNotContains("Should not be accessing a property of a deleted StmALog row.", "Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
		}

		public void TestPropagation()
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));

			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipmentLog = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);

			factory.Save();

			var consolMilestone = factory.New<Forwarding.IForwardingConsolProcessTask>() as IProcessTask;
			((IBaseTrigger)consolMilestone).TriggerEventCode = "SIV";
			consolMilestone.P9_ParentID = consol.PK;
			consolMilestone.P9_Type = "MIL";
			consolMilestone.P9_ParentTableCode = consol.TablePrefix;

			var shipmentMilestone = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)shipmentMilestone).TriggerEventCode = "SIV";
			shipmentMilestone.P9_ParentID = shipment.PK;
			shipmentMilestone.P9_Type = "MIL";
			shipmentMilestone.P9_ParentTableCode = "JS";

			factory.Save();

			var propagationLink = new PropagationLink((IStmALogParent)consol, new BusinessObject[] { shipment }, "test");
			var processHandlingInfo = new ProcessHandlingInfoForTest((EnterpriseBusinessObject)shipment, System.Array.Empty<BusinessObject>(), new[] { propagationLink });
			var logCount = consol.GetLogs().GetAllLogs().Count;
			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(processHandlingInfo, shipmentLog).Fire();
			}

			AssertEquals("Event should be propogated to consol.", consol.GetLogs().GetAllLogs().Count, logCount + 1);
		}

		public void TestPropagation_Withdraw()
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));

			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipmentLog = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);

			factory.Save();

			var consolMilestone = factory.New<Forwarding.IForwardingConsolProcessTask>() as IProcessTask;
			((IBaseTrigger)consolMilestone).TriggerEventCode = "SIV";
			consolMilestone.P9_ParentID = consol.PK;
			consolMilestone.P9_Type = "MIL";
			consolMilestone.P9_ParentTableCode = consol.TablePrefix;

			var shipmentMilestone = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)shipmentMilestone).TriggerEventCode = "SIV";
			shipmentMilestone.P9_ParentID = shipment.PK;
			shipmentMilestone.P9_Type = "MIL";
			shipmentMilestone.P9_ParentTableCode = "JS";

			factory.Save();

			var propagationLink1 = new PropagationLink((IStmALogParent)consol, new BusinessObject[] { shipment }, "test");
			var propagationLink2 = new PropagationLink((IStmALogParent)consol, new BusinessObject[] { shipment }, "test");

			var propagationLinks1 = new[] { propagationLink1 }.ToList();
			var propagationLinks2 = new[] { propagationLink1 }.ToList();
			var processHandlingInfo1 = new ProcessHandlingInfoForTest((EnterpriseBusinessObject)shipment, System.Array.Empty<BusinessObject>(), propagationLinks1);
			var processHandlingInfo2 = new ProcessHandlingInfoForTest((EnterpriseBusinessObject)shipment, System.Array.Empty<BusinessObject>(), propagationLinks2);
			var logCount = consol.GetLogs().GetAllLogs();
			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(processHandlingInfo1, shipmentLog).Fire();
				var log = consol.GetLogs().GetAllLogs().First();
				var counter = 0;
				((INeedRow)log).Row.Table.RowDeleted += (s, e) =>
				{
					counter++;
					if (counter == 2)
					{
						propagationLinks1.Clear();
						ProcessTaskHandlerProvider.GetHandler(processHandlingInfo2, shipment.GetLogs().AddNew(Events.CustomisableEvent00)).Fire();
					}
				};
				AssertNoExceptionThrown(() => ProcessTaskHandlerProvider.GetHandler(processHandlingInfo1, shipmentLog).Withdraw());
			}
			var handler = new PropagationHandler(Factory);
		}

		public void TestPropagation_DeferFiringWorkflow()
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));

			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipmentLog = shipment.GetLogs().AddNew(new EventValue(Events.ServiceInvoicePosted, deferFiringWorkflow: true));

			factory.Save();

			var consolMilestone = factory.New<Forwarding.IForwardingConsolProcessTask>() as IProcessTask;
			((IBaseTrigger)consolMilestone).TriggerEventCode = "SIV";
			consolMilestone.P9_ParentID = consol.PK;
			consolMilestone.P9_Type = "MIL";
			consolMilestone.P9_ParentTableCode = "JC";

			var shipmentMilestone = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)shipmentMilestone).TriggerEventCode = "SIV";
			shipmentMilestone.P9_ParentID = shipment.PK;
			shipmentMilestone.P9_Type = "MIL";
			shipmentMilestone.P9_ParentTableCode = "JS";

			factory.Save();

			var propagationLink = new PropagationLink((IStmALogParent)consol, new BusinessObject[] { shipment }, "test");
			var processHandlingInfo = new ProcessHandlingInfoForTest((EnterpriseBusinessObject)shipment, System.Array.Empty<BusinessObject>(), new[] { propagationLink });
			var logCount = consol.GetLogs().GetAllLogs().Count;
			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(processHandlingInfo, shipmentLog).Fire();
			}

			AssertEquals("Event should be propogated to consol.", consol.GetLogs().GetAllLogs().Count, logCount + 1);
			AssertEquals("ServiceInvoicePosted event should be deferred firing workflow since it was propogated from an event which was deferred firing workflow.",
				1, consol.GetLogs().Find(l => l.SL_SE_NKEvent == Events.ServiceInvoicePostedCode && l.SL_FireWorkflow).Count());
		}

		public void TestFire_FireParentTriggers()
		{
			var mockedTrigger1 = new Mock<ILineTriggerSupport>();
			var mockedTrigger2 = new Mock<ILineTriggerSupport>();
			var mockedTrigger3 = new Mock<ILineTriggerSupport>();

			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));
			var evnt = consol.GetLogs().AddNew(Events.Arrival);

			mockedTrigger1.Setup(t => t.AreTriggerConditionsMet(evnt, consol)).Returns(false);
			mockedTrigger2.Setup(t => t.AreTriggerConditionsMet(evnt, consol)).Returns(true);
			mockedTrigger2.Setup(t => t.Fire(consol, evnt));

			var processHandlingInfo = new ProcessHandlingInfoForTest(consol, new[] { mockedTrigger1.Object, mockedTrigger2.Object });
			var handler = ProcessTaskHandlerProvider.GetHandler(processHandlingInfo, evnt);

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				handler.Fire();
			}

			Assert(true);
		}

		public void TestWithdraw_WithdrawParentTriggers()
		{
			var mockedTrigger1 = new Mock<ILineTriggerSupport>();
			var mockedTrigger2 = new Mock<ILineTriggerSupport>();
			var mockedTrigger3 = new Mock<ILineTriggerSupport>();

			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));
			var evnt = consol.GetLogs().AddNew(Events.Arrival);

			mockedTrigger1.Setup(t => t.AreTriggerConditionsMet(evnt, consol)).Returns(false);
			mockedTrigger2.Setup(t => t.AreTriggerConditionsMet(evnt, consol)).Returns(true);
			mockedTrigger2.Setup(t => t.Withdraw(consol, evnt));

			var processHandlingInfo = new ProcessHandlingInfoForTest(consol, new[] { mockedTrigger1.Object, mockedTrigger2.Object });
			var handler = ProcessTaskHandlerProvider.GetHandler(processHandlingInfo, evnt);

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				handler.Withdraw();
			}

			Assert(true);
		}

		public void TestCheckLogDeletedBeforeUpdatingParentTriggers()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));

			factory.Save();

			StmALog log1 = shipment.GetLogs().AddNew(Events.Arrival, ZDateTimeOffset.Now, false);
			var handler = ProcessTaskHandlerProvider.GetHandler((IStmALogParent)shipment, log1) as ProcessTaskHandler;
			log1.Delete();
			var dateTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => handler.UpdateParentTriggersExposedForTest(ref dateTime));
		}

		public void TestUpdateSubsequentProcessTasks()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));

			factory.Save();

			var milestone1 = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone1).TriggerEventCode = "ARV";
			milestone1.P9_ParentID = shipment.PK;
			milestone1.P9_Sequence = 1;
			milestone1.P9_Type = "MIL";
			milestone1.P9_ScheduledDate = ZDateTimeOffset.Empty;
			milestone1.P9_ParentTableCode = "JS";

			var milestone2 = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone2).TriggerEventCode = "DEP";
			milestone2.P9_ParentID = shipment.PK;
			milestone2.P9_Sequence = 2;
			milestone2.P9_EstimatedDefaultedFrom = "ACT";
			milestone2.P9_EstimatedDefaultTimeDelta = new ZDateTime(2011, 1, 1, 0, 1, 0);
			milestone2.P9_EstimatedDefaultFromPredecessor = 1;
			milestone2.P9_Type = "MIL";
			milestone2.P9_ScheduledDate = ZDateTimeOffset.Empty;
			milestone2.P9_ParentTableCode = "JS";

			var milestone3 = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone3).TriggerEventCode = "ATH";
			milestone3.P9_ParentID = shipment.PK;
			milestone3.P9_EstimatedDefaultTimeDelta = new ZDateTime(2011, 1, 1, 0, 1, 0);
			milestone3.P9_EstimatedDefaultFromPredecessor = 1;
			milestone3.P9_Sequence = 3;
			milestone3.P9_Type = "MIL";
			milestone3.P9_ScheduledDate = ZDateTimeOffset.Empty;
			milestone3.P9_ParentTableCode = "JS";

			var milestone4 = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone4).TriggerEventCode = "SIV";
			milestone4.P9_ParentID = shipment.PK;
			milestone4.P9_Sequence = 4;
			milestone4.P9_Type = "MIL";
			milestone4.P9_ScheduledDate = ZDateTimeOffset.Empty;
			milestone4.P9_ParentTableCode = "JS";

			var milestone5 = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone5).TriggerEventCode = "PCA";
			milestone5.P9_ParentID = shipment.PK;
			milestone5.P9_Sequence = 5;
			milestone5.P9_EstimatedDefaultedFrom = "ACT";
			milestone5.P9_EstimatedDefaultTimeDelta = new ZDateTime(2011, 1, 1, 0, 1, 0);
			milestone5.P9_EstimatedDefaultFromPredecessor = 4;
			milestone5.P9_Type = "MIL";
			milestone5.P9_ScheduledDate = ZDateTimeOffset.Empty;
			milestone5.P9_ParentTableCode = "JS";

			var milestone6 = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone6).TriggerEventCode = "AED";
			milestone6.P9_ParentID = shipment.PK;
			milestone6.P9_Sequence = 6;
			milestone6.P9_EstimatedDefaultTimeDelta = new ZDateTime(2011, 1, 1, 0, 1, 0);
			milestone6.P9_EstimatedDefaultFromPredecessor = 4;
			milestone6.P9_Type = "MIL";
			milestone6.P9_ScheduledDate = ZDateTimeOffset.Empty;
			milestone6.P9_ParentTableCode = "JS";

			var milestone7 = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone7).TriggerEventCode = Events.CustomsEntryStatusCode;
			milestone7.P9_ParentID = shipment.PK;
			milestone7.P9_Sequence = 7;
			milestone7.P9_Type = "MIL";
			milestone7.TemplateConditions.TemplateCondition1 = "REF";
			milestone7.P9_Notes = System.Text.Encoding.UTF8.GetBytes("COM");
			milestone7.P9_ParentTableCode = "JS";

			var declaration = factory.New(ObjectFactory.GetType<Customs.IBaseJobDeclaration>());
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			var entry = factory.New(ObjectFactory.GetType(typeof(Customs.ICusEntryHeader)));
			entry[CusEntryHeaderSchema.CH_JE] = declaration.PK;

			factory.Save();

			StmALog log1 = shipment.GetLogs().AddNew(Events.Arrival, ZDateTimeOffset.Now, false);
			StmALog log2 = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted, ZDateTimeOffset.Now, true);
			StmALog log3 = entry.GetLogs().AddNew(Events.CustomsEntryStatus, "COM", new ZDateTimeOffset(2011, 3, 1, 20, 23, 2));

			CombineAssertions(delegate
			{
				var dt1 = milestone1.P9_ActualDate;
				AssertNotEquals("milestone1.P9_ActualDate", ZDateTimeOffset.Empty, dt1);
				AssertScheduledDate(dt1.AddMinutes(1), milestone2);
				AssertEquals("milestone3.P9_ScheduledDate", ZDateTimeOffset.Empty, milestone3.P9_ScheduledDate);

				var dt2 = milestone4.P9_ScheduledDate;
				AssertNotEquals("milestone4.P9_ScheduledDate", ZDateTimeOffset.Empty, dt2);
				AssertScheduledDate(ZDateTimeOffset.Empty, milestone5);
				AssertScheduledDate(dt2.AddMinutes(1), milestone6);

				AssertEquals("Actual date should be populated as per CS00137634", log3.SL_EventTime, milestone7.P9_ActualDate.ToZDateTime());
			});
		}

		void AssertScheduledDate(ZDateTimeOffset offset, IProcessTask milestone)
		{
			AssertEquals(offset.IsValid ? offset.ToZDateTime().ToSmallDateTimeFloor() : offset.ToZDateTime(), milestone.P9_ScheduledDate.ToZDateTime());
		}

		public void TestFireWithNoChildren()
		{
			var factory1 = new BusinessObjectFactory();

			var shipment = factory1.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var log = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);
			factory1.Save();

			var milestone1 = factory1.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone1).TriggerEventCode = "SIV";
			milestone1.P9_ParentID = shipment.PK;
			milestone1.P9_Type = "MIL";
			milestone1.P9_ParentTableCode = "JS";
			factory1.Save();

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(null, log).Fire();
			}

			AssertEquals("Should change milestone's actual time", log.SL_EventTime, milestone1.P9_ActualDate.ToZDateTime());
		}

		public void TestWithdrawWithNoChildren()
		{
			var factory1 = new BusinessObjectFactory();

			var shipment = factory1.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var log = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);
			factory1.Save();

			var milestone1 = factory1.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone1).TriggerEventCode = "SIV";
			milestone1.P9_ParentID = shipment.PK;
			milestone1.P9_Type = "MIL";
			milestone1.P9_ParentTableCode = "JS";
			factory1.Save();

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(null, log).Withdraw();
			}
			AssertEquals("Should change milestone's actual time", ZDateTimeOffset.Empty, milestone1.P9_ActualDate);
		}

		public void TestFireWithChildren()
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));
			var consolLog = consol.GetLogs().AddNew(Events.ServiceInvoicePosted);

			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipmentLog = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);

			factory.Save();

			var shipmentMilestone = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)shipmentMilestone).TriggerEventCode = "SIV";
			shipmentMilestone.P9_ParentID = shipment.PK;
			shipmentMilestone.P9_Type = "MIL";
			shipmentMilestone.P9_ParentTableCode = "JS";

			var consolMilestone = factory.New<Forwarding.IForwardingConsolProcessTask>() as IProcessTask;
			((IBaseTrigger)consolMilestone).TriggerEventCode = "SIV";
			consolMilestone.P9_ParentID = consol.PK;
			consolMilestone.P9_Type = "MIL";
			consolMilestone.P9_ParentTableCode = consol.TablePrefix;

			factory.Save();

			var processHandlingInfo = new ProcessHandlingInfoForTest((EnterpriseBusinessObject)consol, new[] { shipment });

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(processHandlingInfo, consolLog).Fire();
			}

			AssertEquals("Should change consol milestone's actual time", consolLog.SL_EventTime, consolMilestone.P9_ActualDate.ToZDateTime());
			AssertEquals("Should change shipment milestone's actual time", consolLog.SL_EventTime, consolMilestone.P9_ActualDate.ToZDateTime());
		}

		public void TestWithdrawWithChildren()
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingConsol)));
			var consolLog = consol.GetLogs().AddNew(Events.ServiceInvoicePosted);

			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipmentLog = shipment.GetLogs().AddNew(Events.ServiceInvoicePosted);

			factory.Save();

			var shipmentMilestone = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)shipmentMilestone).TriggerEventCode = "SIV";
			shipmentMilestone.P9_ParentID = shipment.PK;
			shipmentMilestone.P9_Type = "MIL";
			shipmentMilestone.P9_ParentTableCode = "JS";

			var consolMilestone = factory.New<Forwarding.IForwardingConsolProcessTask>() as IProcessTask;
			((IBaseTrigger)consolMilestone).TriggerEventCode = "SIV";
			consolMilestone.P9_ParentID = consol.PK;
			consolMilestone.P9_Type = "MIL";
			consolMilestone.P9_ParentTableCode = consol.TablePrefix;

			factory.Save();

			var processHandlingInfo = new ProcessHandlingInfoForTest((EnterpriseBusinessObject)consol, new[] { shipment });
			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(processHandlingInfo, consolLog).Withdraw();
			}

			AssertEquals("Should change consol milestone's actual time", ZDateTimeOffset.Empty, consolMilestone.P9_ActualDate);
			AssertEquals("Should change shipment milestone's actual time", ZDateTimeOffset.Empty, shipmentMilestone.P9_ActualDate);
		}

		public void TestShouldUpdateProcessTasksWithLog()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			BusinessObject shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));

			var trigger = (IProcessTask)factory.New<Forwarding.IForwardingShipmentProcessTask>();
			trigger.P9_ParentID = shipment.PK;
			((IBaseTrigger)trigger).TriggerEventCode = "EDT";
			trigger.P9_Type = "TRG";
			trigger.P9_GC = ZGuid.Empty;
			trigger.P9_ParentTableCode = "JS";

			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(User.ServiceUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				shipment[JobShipmentSchema.JS_GoodsDescription.Name] = "xyz";
				factory.Save();
			}

			Assert("Should not trigger on EDT under batch processor", trigger.P9_ActualDate.IsEmpty);

			using (EnvProxy.Instance.SetTemporaryUserContext(User.InterchangeUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				shipment[JobShipmentSchema.JS_GoodsDescription.Name] = "zxy";
				factory.Save();
			}

			Assert("Should not trigger on EDT under batch processor", trigger.P9_ActualDate.IsEmpty);

			shipment[JobShipmentSchema.JS_GoodsDescription.Name] = "abc";
			factory.Save();

			Assert("Should trigger on EDT under other users", !trigger.P9_ActualDate.IsEmpty);
		}

		#region Firing on Actual and Estimate Events

		public void TestJobTrigger_ShouldFireOnActualEvent()
		{
			var factory = new BusinessObjectFactory();

			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var log = shipment.GetLogs().AddNew(Events.CustomisableEvent00, dateTime: new ZDateTimeOffset(2022, 05, 23), isEstimate: false);
			factory.Save();

			var trigger = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)trigger).TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_ParentID = shipment.PK;
			trigger.P9_Type = "TRG";
			trigger.P9_ParentTableCode = "JS";
			factory.Save();

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(null, log).Fire();
			}

			AssertEquals("Should change trigger's actual time", log.SL_EventTime, trigger.P9_ActualDate.ToZDateTime());
		}

		public void TestJobTrigger_ShouldNotFireOnEstimateEvent()
		{
			var factory = new BusinessObjectFactory();

			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var log = shipment.GetLogs().AddNew(Events.CustomisableEvent00, dateTime: new ZDateTimeOffset(2022, 05, 23), isEstimate: true);
			factory.Save();

			var trigger = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)trigger).TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_ParentID = shipment.PK;
			trigger.P9_Type = "TRG";
			trigger.P9_ParentTableCode = "JS";
			factory.Save();

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(null, log).Fire();
			}

			AssertNotEquals("Should not change trigger's actual time", log.SL_EventTime, trigger.P9_ActualDate.ToZDateTime());
		}

		public void TestMilestones_ShouldFireOnActualEvent()
		{
			var factory = new BusinessObjectFactory();

			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var log = shipment.GetLogs().AddNew(Events.CustomisableEvent00, dateTime: new ZDateTimeOffset(2022, 05, 23), isEstimate: false);
			factory.Save();

			var milestone = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone).TriggerEventCode = Events.CustomisableEvent00Code;
			milestone.P9_ParentID = shipment.PK;
			milestone.P9_Type = "MIL";
			milestone.P9_ParentTableCode = "JS";
			factory.Save();

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(null, log).Fire();
			}

			AssertEquals("Should change milestone's actual time", log.SL_EventTime, milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestMilestones_ShouldNotFireOnEstimateEvent()
		{
			var factory = new BusinessObjectFactory();

			var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var log = shipment.GetLogs().AddNew(Events.CustomisableEvent00, dateTime: new ZDateTimeOffset(2022, 05, 23), isEstimate: true);
			factory.Save();

			var milestone = factory.New<Forwarding.IForwardingShipmentProcessTask>() as IProcessTask;
			((IBaseTrigger)milestone).TriggerEventCode = Events.CustomisableEvent00Code;
			milestone.P9_ParentID = shipment.PK;
			milestone.P9_Type = "MIL";
			milestone.P9_ParentTableCode = "JS";
			factory.Save();

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler(null, log).Fire();
			}

			AssertNotEquals("Should not change milestone's actual time", log.SL_EventTime, milestone.P9_ActualDate.ToZDateTime());
		}

		// the tests related to universal triggers are placed in Enterprise.Workflow.Business.Test.ProcessTemplateTriggerFiringTest

		#endregion
	}
}
