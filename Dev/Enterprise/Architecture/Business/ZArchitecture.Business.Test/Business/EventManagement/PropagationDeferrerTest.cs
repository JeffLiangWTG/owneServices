using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class PropagationDeferrerTest : TestCaseWithFactory
	{
		public void TestGetInstance_NullFactory_Throws() => AssertExceptionThrown<ArgumentNullException>(() => PropagationDeferrer.GetInstance(null));
		public void TestDeferEventPropagation_NullFactory_Throws() => AssertExceptionThrown<ArgumentNullException>(() => PropagationDeferrer.DeferEventPropagation(null));

		public void TestGetInstance()
		{
			var instance1 = PropagationDeferrer.GetInstance(Factory);
			var instance2 = PropagationDeferrer.GetInstance(new BusinessObjectFactory());
			AssertNotEquals("Should return unique instances per factory.", instance1, instance2);
			AssertEquals("Should return the same instance for a given factory.", instance1, PropagationDeferrer.GetInstance(Factory));
		}

		public void TestDeferEventPropagation_Deduplicates_Parent()
		{
			TestDeferEventPropagation_Deduplicates(m => { }, differentParent: true);
		}

		public void TestDeferEventPropagation_Deduplicates_SL_SE_NKEvent()
		{
			TestDeferEventPropagation_Deduplicates(m => m.Setup(l => l.SL_SE_NKEvent).Returns("OTH"));
		}

		public void TestDeferEventPropagation_Deduplicates_SL_Reference()
		{
			TestDeferEventPropagation_Deduplicates(m => m.Setup(l => l.SL_Reference).Returns("DIFFERENT"));
		}

		public void TestDeferEventPropagation_Deduplicates_SL_IsEstimate()
		{
			TestDeferEventPropagation_Deduplicates(m => m.Setup(l => l.SL_IsEstimate).Returns(true));
		}

		void TestDeferEventPropagation_Deduplicates(Action<Mock<IStmALog>> deduplicateLog, bool differentParent = false)
		{
			var log1 = CreateLogMock("T1", ZGuid.BrettsGuid, "EVT", "SOME EVENT", false, false).Object;
			var log2 = CreateLogMock("T1", ZGuid.BrettsGuid, "EVT", "SOME EVENT", false, false).Object;
			var log3Mock = CreateLogMock("T1", ZGuid.BrettsGuid, "EVT", "SOME EVENT", false, false);
			var log3 = log3Mock.Object;

			var logParent1 = Factory.New<DummyEnterpriseBusinessObject>();
			var parent1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			var parent2 = parent1;
			parent1.Z0_Guid = logParent1.PK;

			if (differentParent)
			{
				var logParent2 = Factory.New<DummyEnterpriseBusinessObject>();
				parent2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
				parent2.Z0_Guid = logParent2.PK;
			}
			var processHandlingInfo1 = new DummyProcessHandlingInfo(parent1);
			var processHandlingInfo2 = new DummyProcessHandlingInfo(parent1);
			var processHandlingInfo3 = new DummyProcessHandlingInfo(parent2);

			var instance = PropagationDeferrer.GetInstance(Factory);
			var handler = new Mock<IPropagationHandler>();

			using (PropagationDeferrer.DeferEventPropagation(Factory, handler.Object))
			{
				AssertEquals("Should be deferred.", true, instance.IsDeferred);

				instance.AddLogToPropagate(processHandlingInfo1, log1);
				instance.AddLogToPropagate(processHandlingInfo2, log2);
				instance.AddLogToPropagate(processHandlingInfo3, log3);
				handler.VerifyNoOtherCalls();

				deduplicateLog(log3Mock); // Ensure de-duplicates at dispose time
			}

			handler.Verify(h => h.Propagate(processHandlingInfo1, log1), Times.Once);
			handler.Verify(h => h.Propagate(processHandlingInfo3, log3), Times.Once);
			handler.VerifyNoOtherCalls();
		}

		public void TestEndToEnd_DeferEventPropagation()
		{
			// Note this test tests a combination of PropagationDeferrer.DeferEventPropagation and PropagationHandler.Propagate
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var instance = PropagationDeferrer.GetInstance(Factory);
			var eventsOnParent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				AssertEquals("Should be deferred.", true, instance.IsDeferred);

				using (PropagationDeferrer.DeferEventPropagation(Factory))
				{
					AssertEquals("Should be deferred.", true, instance.IsDeferred);

					child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
					child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

					AssertEquals("Should *not* have propagated events yet.", false, eventsOnParent.Any());
				}

				AssertEquals("Should *not* have propagated events yet.", false, eventsOnParent.Any());
			}

			AssertEquals("Should *not* be deferred.", false, instance.IsDeferred);

			var propagatedEvent = eventsOnParent.Single();
			AssertEquals("Event propagated", "Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);

			propagatedEvent.Delete();
			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
			}

			AssertEquals("Should have reset the queue of events to propagate.", false, eventsOnParent.Any());
		}

		public void TestEndToEnd_DeferEventPropagation_CancelledEvent()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var instance = PropagationDeferrer.GetInstance(Factory);
			var eventsOnParent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				AssertEquals("Should be deferred.", true, instance.IsDeferred);

				var log1 = child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
				var log2 = child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

				log2.Cancel();
				AssertEquals("Precondition.", true, log2.IsCancelled);
			}

			AssertEquals("Should *not* be deferred.", false, instance.IsDeferred);

			var propagatedEvent = eventsOnParent.SingleOrDefault();
			AssertNull("Should *not* have propagated.", propagatedEvent);
		}

		public void TestEndToEnd_DeferEventPropagation_DeletedEvent()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var instance = PropagationDeferrer.GetInstance(Factory);
			var eventsOnParent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				AssertEquals("Should be deferred.", true, instance.IsDeferred);

				var log1 = child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
				var log2 = child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

				log2.Delete();
				AssertEquals("Precondition.", true, log2.IsDeleted);
			}

			AssertEquals("Should *not* be deferred.", false, instance.IsDeferred);

			var propagatedEvent = eventsOnParent.SingleOrDefault();
			AssertNull("Should *not* have propagated.", propagatedEvent);
		}

		public void TestEndToEnd_DeferEventPropagation_EstimatedEvent()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var instance = PropagationDeferrer.GetInstance(Factory);
			var eventsOnParent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				AssertEquals("Should be deferred.", true, instance.IsDeferred);

				var time = ZDateTimeOffset.Now;
				var log1 = child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL", time, false);
				var log2 = child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL", time, true);
			}

			AssertEquals("Should *not* be deferred.", false, instance.IsDeferred);

			var propagatedEvent = eventsOnParent.SingleOrDefault();
			AssertNull("Should *not* have propagated.", propagatedEvent);
		}

		public void TestEndToEnd_DeferEventPropagation_EstimatedEvents()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var instance = PropagationDeferrer.GetInstance(Factory);
			var eventsOnParent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				AssertEquals("Should be deferred.", true, instance.IsDeferred);

				var time = ZDateTimeOffset.Now;
				var log1 = child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL", time, true);
				var log2 = child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL", time, true);
			}

			AssertEquals("Should *not* be deferred.", false, instance.IsDeferred);

			var propagatedEvent = eventsOnParent.SingleOrDefault();
			AssertEquals("Event propagated", "Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);
			AssertEquals("Should be an estimate.", true, propagatedEvent.SL_IsEstimate);
		}

		public void TestEndToEnd_DeferEventPropagation_Duplicates()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var instance = PropagationDeferrer.GetInstance(Factory);
			var eventsOnParent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				AssertEquals("Should be deferred.", true, instance.IsDeferred);

				child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
				child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
				child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			}

			AssertEquals("Should *not* be deferred.", false, instance.IsDeferred);

			var propagatedEvent = eventsOnParent.Single();
			AssertEquals("Event propagated once.", "Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);
		}

		[TestDate]
		public void TestEndToEnd_DeferEventPropagation_Duplicates_PropagateOnParameterChange()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-10);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				child1.Logs.AddNew(Events.BookingConfirmed, "this text|CMP=One");

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				child1.Logs.AddNew(Events.BookingConfirmed, "shouldn't|CMP=One Two");

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				child1.Logs.AddNew(Events.BookingConfirmed, "matter|CMP=One Two Three");
			}

			var propagatedEvents = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled);
			AssertContainsExactElementsInAnyOrder("Propagated: All Dummy Siblings|CMP=One".Yield().Append("Propagated: All Dummy Siblings|CMP=One Two").Append("Propagated: All Dummy Siblings|CMP=One Two Three"), propagatedEvents.Select(log => log.SL_Reference));
		}

		[TestDate]
		public void TestEndToEnd_DeferEventPropagation_Duplicates_DoNotPropagateOnParameterChange()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-10);
			var propagationSettings = new DummyPropagationSettings(propagatedOnParamChange: false);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				child1.Logs.AddNew(Events.BookingConfirmed, "this text|CMP=One", ZDateTimeOffset.UtcNow, false, propagationSettings);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				child1.Logs.AddNew(Events.BookingConfirmed, "shouldn't|CMP=One Two", ZDateTimeOffset.UtcNow, false, propagationSettings);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				child1.Logs.AddNew(Events.BookingConfirmed, "matter|CMP=One Two Three", ZDateTimeOffset.UtcNow, false, propagationSettings);
			}

			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|CMP=One Two Three", propagatedEvent.SL_Reference);
		}

		public void TestEndToEnd_DeferEventPropagation_Duplicates_CancelledEvent_FirstCancelled() => TestEndToEnd_DeferEventPropagation_Duplicates_CancelledEvent(firstCancelled: true);
		public void TestEndToEnd_DeferEventPropagation_Duplicates_CancelledEvent_SecondCancelled() => TestEndToEnd_DeferEventPropagation_Duplicates_CancelledEvent(firstCancelled: false);

		void TestEndToEnd_DeferEventPropagation_Duplicates_CancelledEvent(bool firstCancelled)
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var instance = PropagationDeferrer.GetInstance(Factory);
			var eventsOnParent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				AssertEquals("Should be deferred.", true, instance.IsDeferred);

				child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
				var log1 = child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
				var log2 = child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

				if (firstCancelled)
				{
					using (log1.LockForUpdatingKeyFields())
					{
						log1.Cancel();
					}

					AssertEquals("Precondition.", true, log1.IsCancelled);
				}
				else
				{
					using (log2.LockForUpdatingKeyFields())
					{
						log2.Cancel();
					}

					AssertEquals("Precondition.", true, log2.IsCancelled);
				}
			}

			AssertEquals("Should *not* be deferred.", false, instance.IsDeferred);

			var propagatedEvent = eventsOnParent.Single();
			AssertEquals("Event propagated", "Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);
		}

		public void TestEndToEnd_DeferEventPropagation_Duplicates_DeletedEvent_FirstDeleted() => TestEndToEnd_DeferEventPropagation_Duplicates_DeletedEvent(firstDeleted: true);
		public void TestEndToEnd_DeferEventPropagation_Duplicates_DeletedEvent_SecondDeleted() => TestEndToEnd_DeferEventPropagation_Duplicates_DeletedEvent(firstDeleted: false);

		void TestEndToEnd_DeferEventPropagation_Duplicates_DeletedEvent(bool firstDeleted)
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var instance = PropagationDeferrer.GetInstance(Factory);
			var eventsOnParent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				AssertEquals("Should be deferred.", true, instance.IsDeferred);

				child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
				var log1 = child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
				var log2 = child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

				if (firstDeleted)
				{
					log1.Delete();
					AssertEquals("Precondition.", true, log1.IsDeleted);
				}
				else
				{
					log2.Delete();
					AssertEquals("Precondition.", true, log2.IsDeleted);
				}
			}

			AssertEquals("Should *not* be deferred.", false, instance.IsDeferred);

			var propagatedEvent = eventsOnParent.Single();
			AssertEquals("Event propagated", "Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);
		}

		public void TestEndToEnd_DeferEventPropagation_Duplicates_MixedEstimatedEvents_FirstEstimated() => TestEndToEnd_DeferEventPropagation_Duplicates_MixedEstimatedEvents(firstEstimated: true);
		public void TestEndToEnd_DeferEventPropagation_Duplicates_MixedEstimatedEvents_SecondEstimated() => TestEndToEnd_DeferEventPropagation_Duplicates_MixedEstimatedEvents(firstEstimated: false);

		void TestEndToEnd_DeferEventPropagation_Duplicates_MixedEstimatedEvents(bool firstEstimated)
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var instance = PropagationDeferrer.GetInstance(Factory);
			var eventsOnParent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode);

			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				AssertEquals("Should be deferred.", true, instance.IsDeferred);

				child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

				var time = ZDateTimeOffset.Now;
				var log1 = child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL", time, firstEstimated);
				var log2 = child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL", time, !firstEstimated);
			}

			AssertEquals("Should *not* be deferred.", false, instance.IsDeferred);

			var propagatedEvent = eventsOnParent.Single();
			AssertEquals("Event propagated", "Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);
		}

		public void TestEndToEnd_DeferEventPropagation_PropertyHits()
		{
			// Without PropagationDeferrer.DeferEventPropagation
			// 10: 1262, 1000: 3089073 (ratio ~2448)
			//
			// With PropagationDeferrer.DeferEventPropagation
			// 10: 1139, 1000: 109050 (ratio ~96)
			var propertiesHit10 = RunDeferEventPropagation(10);
			var propertiesHit1000 = RunDeferEventPropagation(1000);

			var ratio = (propertiesHit1000 / propertiesHit10);
			AssertLessThanOrEqualTo("Expected ratio of calling with 100x elements to be roughly 100.", ratio, 125);

			int RunDeferEventPropagation(int n)
			{
				return GetPersistentPropertiesHitCount(() =>
				{
					var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

					var children = new List<DummyProcessHandlingInfoProviderBizo>(n);
					for (int i = 0; i < n; i++)
					{
						var child = Factory.New<DummyProcessHandlingInfoProviderBizo>();
						child.Z0_Guid = parentDummy.PK;
						children.Add(child);
					}

					var instance = PropagationDeferrer.GetInstance(Factory);
					var eventsOnParent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode);

					using (PropagationDeferrer.DeferEventPropagation(Factory))
					{
						foreach (var child in children)
						{
							child.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
						}
					}
				});
			}
		}

		Mock<IStmALog> CreateLogMock(string table, ZGuid parent, string eventType, string reference, bool cancelled, bool estimate)
		{
			var logMock = new Mock<IStmALog>();
			logMock.Setup(l => l.PK).Returns(ZGuid.NewZGuid());
			logMock.Setup(l => l.SL_Table).Returns(table);
			logMock.Setup(l => l.SL_Parent).Returns(parent);
			logMock.Setup(l => l.SL_SE_NKEvent).Returns(eventType);
			logMock.Setup(l => l.SL_Reference).Returns(reference);
			logMock.Setup(l => l.SL_IsCancelled).Returns(cancelled);
			logMock.Setup(l => l.SL_IsEstimate).Returns(estimate);
			return logMock;
		}
	}
}
