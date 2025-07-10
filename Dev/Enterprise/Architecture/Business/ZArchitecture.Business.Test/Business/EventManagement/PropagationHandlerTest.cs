using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class PropagationHandlerTest : TestCaseWithFactory
	{
		public void TestIsPropagatedEventLog()
		{
			Action<bool, string> assertIsPropagatedEventLog = (isPropagated, reference) =>
				{
					var parent = Factory.New<DummyEnterpriseBusinessObject>();
					AssertEquals(isPropagated, PropagationHandler.IsPropagatedEventLog(parent.Logs.AddNew(Events.Arrival, reference)));
				};

			assertIsPropagatedEventLog(false, "");
			assertIsPropagatedEventLog(false, "Foo");
			assertIsPropagatedEventLog(false, "Prop");

			assertIsPropagatedEventLog(true, "Propagated:");
			assertIsPropagatedEventLog(true, PropagationHandler.PropagatedReferencePrefix.NormalEvent);
			assertIsPropagatedEventLog(true, PropagationHandler.PropagatedReferencePrefix.StartingEvent);
			assertIsPropagatedEventLog(true, PropagationHandler.PropagatedReferencePrefix.FinishingEvent);
		}

		#region Propagation (matching by parameters)

		public void TestPropagation_PropagateIfEventParametersAreMatched()
		{
			var emptyParametersList = Array.Empty<KeyValuePair<string, string>>();

			EnsureEventIsPropagated(
				parametersToPropagate: Array.Empty<string>(),
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV") },
				event2Parameters: null,
				parametersInPropagatedEvent: null);
			EnsureEventIsPropagated(
				parametersToPropagate: new[] { Params.Location },
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV") },
				event2Parameters: null,
				parametersInPropagatedEvent: null);

			EnsureEventIsPropagated(
				parametersToPropagate: new[] { Params.Location },
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV") },
				event2Parameters: new[] { Params.Location.AsKeyFor("AUSYD") },
				parametersInPropagatedEvent: null);
			EnsureEventIsPropagated(
				parametersToPropagate: Array.Empty<string>(),
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV") },
				event2Parameters: new[] { Params.Location.AsKeyFor("AUSYD") },
				parametersInPropagatedEvent: emptyParametersList);

			EnsureEventIsPropagated(
				parametersToPropagate: new[] { Params.Location },
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV") },
				event2Parameters: new[] { Params.Location.AsKeyFor("UAIEV") },
				parametersInPropagatedEvent: new[] { Params.Location.AsKeyFor("UAIEV") });
			EnsureEventIsPropagated(
				parametersToPropagate: Array.Empty<string>(),
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV") },
				event2Parameters: new[] { Params.Location.AsKeyFor("UAIEV") },
				parametersInPropagatedEvent: new[] { Params.Location.AsKeyFor("UAIEV") });

			EnsureEventIsPropagated(
				parametersToPropagate: new[] { Params.Location, Params.Facility },
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CTO") },
				event2Parameters: new[] { Params.Location.AsKeyFor("UAIEV") },
				parametersInPropagatedEvent: null);
			EnsureEventIsPropagated(
				parametersToPropagate: new[] { Params.Location },
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CTO") },
				event2Parameters: new[] { Params.Location.AsKeyFor("UAIEV") },
				parametersInPropagatedEvent: new[] { Params.Location.AsKeyFor("UAIEV") });

			EnsureEventIsPropagated(
				parametersToPropagate: new[] { Params.Location, Params.Facility },
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CTO") },
				event2Parameters: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CFS") },
				parametersInPropagatedEvent: null);
			EnsureEventIsPropagated(
				parametersToPropagate: new[] { Params.Location },
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CTO") },
				event2Parameters: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CFS") },
				parametersInPropagatedEvent: new[] { Params.Location.AsKeyFor("UAIEV") });

			EnsureEventIsPropagated(
				parametersToPropagate: new[] { Params.Location, Params.Facility },
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CTO") },
				event2Parameters: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CTO") },
				parametersInPropagatedEvent: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CTO") });
			EnsureEventIsPropagated(
				parametersToPropagate: new[] { Params.Facility },
				event1Parameters: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CTO") },
				event2Parameters: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CTO") },
				parametersInPropagatedEvent: new[] { Params.Location.AsKeyFor("UAIEV"), Params.Facility.AsKeyFor("CTO") });
		}

		public void TestPropagation_NoExceptionInCaseOfNullParameterValues()
		{
			EnsureEventIsPropagated(
				parametersToPropagate: new[] { Params.Location },
				event1Parameters: new[] { Params.Location.AsKeyFor(null) },
				event2Parameters: new[] { Params.Location.AsKeyFor(null) },
				parametersInPropagatedEvent: new[] { Params.Location.AsKeyFor(null) });
		}

		void EnsureEventIsPropagated(
			string[] parametersToPropagate,
			IEnumerable<KeyValuePair<string, string>> event1Parameters = null,
			IEnumerable<KeyValuePair<string, string>> event2Parameters = null,
			IEnumerable<KeyValuePair<string, string>> parametersInPropagatedEvent = null)
		{
			var parent = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parent.PK;
			((DummyProcessHandlingInfo)((IProcessHandlingInfoProvider)child1).ProcessHandlingInfo).OverrideParametersToPropagate(parametersToPropagate);

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parent.PK;
			((DummyProcessHandlingInfo)((IProcessHandlingInfoProvider)child2).ProcessHandlingInfo).OverrideParametersToPropagate(parametersToPropagate);

			if (event1Parameters != null)
			{
				child1.Logs.AddNew(Events.BookingConfirmed, event1Parameters.ToArray());
			}

			if (event2Parameters != null)
			{
				child2.Logs.AddNew(Events.BookingConfirmed, event2Parameters.ToArray());
			}

			var propagatedEvent = parent.Logs.MostRecentLogByEventTime(Events.BookingConfirmed);
			AssertEquals("The event has been propagated", parametersInPropagatedEvent != null, propagatedEvent != null);

			if (parametersInPropagatedEvent != null)
			{
				var expectedParameters = parametersInPropagatedEvent.Select(p => string.Format("{0}={1}", p.Key, p.Value));
				var actualParameters = propagatedEvent.Parameters.Select(p => string.Format("{0}={1}", p.Key, p.Value));

				AssertContainsExactElementsInAnyOrder("Parameters in the propagated event", expectedParameters, actualParameters);
			}
		}

		#endregion

		public void TestPropagation_ChildrenInDifferentState_InDb_Loaded_Unloaded_NewLogs()
		{
			var eventParameters = new[] { Params.Location.AsKeyFor("UAIEV") };

			var parent = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parent.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parent.PK;

			var child3 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child3.Z0_Guid = parent.PK;

			var child4 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child4.Z0_Guid = parent.PK;

			child1.Logs.AddNew(Events.BookingConfirmed, eventParameters.ToArray());
			child2.Logs.AddNew(Events.BookingConfirmed, eventParameters.ToArray());
			child3.Logs.AddNew(Events.BookingConfirmed, eventParameters.ToArray());

			var propagatedEvent = parent.Logs.MostRecentLogByEventTime(Events.BookingConfirmed);
			AssertNull("The event has not been propagated", propagatedEvent);

			Factory.Save();

			ReleaseFactory();

			parent = Factory.Load<DummyEnterpriseBusinessObject>(parent.PK);
			propagatedEvent = parent.Logs.MostRecentLogByEventTime(Events.BookingConfirmed);
			AssertNull("The event has not been propagated", propagatedEvent);

			var child5 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child5.Z0_Guid = parent.PK;

			var child6 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child6.Z0_Guid = parent.PK;

			child1 = Factory.Load<DummyProcessHandlingInfoProviderBizo>(child1.PK);
			child1.Logs.GetAllLogs();

			Factory.ResetDatabaseLoadCount();

			child4 = Factory.Load<DummyProcessHandlingInfoProviderBizo>(child4.PK);
			child4.Logs.AddNew(Events.BookingConfirmed, eventParameters.ToArray());
			propagatedEvent = parent.Logs.MostRecentLogByEventTime(Events.BookingConfirmed);
			AssertNull("The event has not been propagated", propagatedEvent);

			child5.Logs.AddNew(Events.BookingConfirmed, eventParameters.ToArray());
			propagatedEvent = parent.Logs.MostRecentLogByEventTime(Events.BookingConfirmed);
			AssertNull("The event has not been propagated", propagatedEvent);

			//2 db hits, loads children events with fetch hints and then looks for parent event in Logs.FindActiveLog
			child6.Logs.AddNew(Events.BookingConfirmed, eventParameters.ToArray());
			propagatedEvent = parent.Logs.MostRecentLogByEventTime(Events.BookingConfirmed);
			AssertNotNull("The event has been propagated", propagatedEvent);

			var expectedParameters = eventParameters.Select(p => string.Format("{0}={1}", p.Key, p.Value));
			var actualParameters = propagatedEvent.Parameters.Select(p => string.Format("{0}={1}", p.Key, p.Value));

			AssertContainsExactElementsInAnyOrder("Parameters in the propagated event", expectedParameters, actualParameters);

			AssertEquals(4, Factory.GetTableHitCount(StmALogSchema.Constants.TableName));
		}

		public void TestPropagate_TargetHasNoExistingEvents_PropagateNew()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Single();
			AssertEquals("Event propagated", "Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);
		}

		public void TestPropagationRecursion()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var consol = Factory.New<IForwardingConsol>();
			consol.AddShipment(shipment);
			var consolBizo = consol as BusinessObject;
			var shipmentBizo = shipment as BusinessObject;
			shipmentBizo.GetLogs().AddNew(Events.CustomisableEvent00);

			consolBizo.GetLogs().eventsInTheProcessOfBeingAdded.Push(Events.CustomisableEvent00Code);
			//There was a bug making the next line hit a null reference. This event is not propagated to the consol as it is already being added
			shipmentBizo.GetLogs().AddNew(Events.CustomisableEvent00);

			AssertEquals("The event should be propagated once", 1, consolBizo.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).Count());
		}

		public void TestPropagate_TargetAlreadyHasStandAloneEvent_SameEventTime_DoNothing()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var existingEvent = parentDummy.Logs.AddNew(Events.BookingConfirmed, "I AM AN EXISTING STANDALONE EVENT|LOC=AUSYD|FAC=TERMINAL");

			child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL", existingEvent.SL_EventTimeOffset);
			child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL", existingEvent.SL_EventTimeOffset);

			var allEvents = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Where(log => !log.SL_IsCancelled);
			AssertContainsExactElementsInAnyOrder(new[] { existingEvent }, allEvents);
		}

		[TestDate]
		public void TestPropagate_TargetAlreadyHasStandAloneEvent_DifferentEventTime_PropagateNew_DoNotTouchExistingOne()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-2);
			var existingEvent = parentDummy.Logs.AddNew(Events.BookingConfirmed, "I AM AN EXISTING STANDALONE EVENT|LOC=AUSYD|FAC=TERMINAL");

			TestDateAttribute.Date = DateTime.Now;
			child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

			var allEvents = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Where(log => !log.SL_IsCancelled);
			var propagatedEvent = allEvents.Single(e => e.SL_Reference.StartsWith("Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD"));

			AssertContainsExactElementsInAnyOrder(new[] { existingEvent, propagatedEvent }, allEvents);
		}

		public void TestPropagate_TargetHasPreviouslyPropagatedEvent_SameEventTime_DoNothing()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Single();
			AssertEquals("Existing active propagated event", false, propagatedEvent.SL_IsCancelled);
			AssertEquals("Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);

			child2.Logs.AddNew(Events.BookingConfirmed, "HEY! UPDATE IS HERE|LOC=AUSYD|FAC=TERMINAL");

			var allEvents = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Where(log => !log.SL_IsCancelled);
			AssertContainsExactElementsInAnyOrder(new[] { propagatedEvent }, allEvents);
		}

		[TestDate]
		public void TestPropagate_PropagatedEventIsRemovedIfChildEventDeleted()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var firstChild1Log = child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			TestDateAttribute.Date = DateTime.Now.AddMinutes(-2);

			var secondChild1Log = child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Single();
			AssertEquals("Existing active propagated event", false, propagatedEvent.SL_IsCancelled);
			AssertEquals("Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);

			firstChild1Log.Delete();
			AssertEquals("Only first child event is gone, so propagated event should remain", true, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Any());

			secondChild1Log.Delete();
			AssertEquals("Second Child event is gone, so propagated event should also go", false, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Any());
		}

		public void TestPropagate_PropagatedEventIsNotRemovedIfSavedWhenChildEventCancelled()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var firstChild1Log = child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			AssertEquals("Should be one propagated log", 1, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());
			Factory.Save();

			firstChild1Log.Cancel();
			AssertEquals("Cancelling a child log should not remove the resultant propagation once save has occured", 1, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Count());
		}

		[TestDate]
		public void TestPropagate_PropagatedEventIsNotUpdatedAfterSave_IfAddThenDeleteChildEvent()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			var firstChild1Log = child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			AssertEquals("Should be one propagated log", 1, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());
			Factory.Save();

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-2);

			var secondChild1Log = child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			AssertEquals("After save we should propagate again if a new child event occurs", 2, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());
			AssertEquals("Propagation should cancel the old event raise a new one", 1, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Count());

			secondChild1Log.Delete();
			AssertEquals("In memory propagated log should be cleared", 1, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());

			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Existing active propagated event", false, propagatedEvent.SL_IsCancelled);
			AssertEquals("Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);
			AssertNotEquals("We should be back to the original propagated event", TestDateAttribute.Date, propagatedEvent.SL_EventTime);
		}

		public void TestPropagate_PropagatedEventIsWithdrawnAfterSave_IfAddThenDeleteChildEvent()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			AssertEquals("Should be no propagated log yet", 0, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());
			Factory.Save();

			var firstChild1Log = child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			AssertEquals("After save we should propagate if a new child event completes the propagation critera", 1, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());

			firstChild1Log.Delete();
			AssertEquals("In memory propagated log should be cleared", 0, parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Count());
		}

		[TestDate]
		public void TestPropagate_PropagatedEventIsUpdatedIfReferenceChanges_DoNotPropagateOnParameterChange_SingleChild()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-10);
			var propagationSettings = new DummyPropagationSettings(propagatedOnParamChange: false);

			child1.Logs.AddNew(Events.BookingConfirmed, "this text|WHS=One", ZDateTimeOffset.UtcNow, false, propagationSettings);
			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "shouldn't|WHS=One Two", ZDateTimeOffset.UtcNow, false, propagationSettings);
			propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One Two", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "matter|WHS=One Two Three", ZDateTimeOffset.UtcNow, false, propagationSettings);
			propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One Two Three", propagatedEvent.SL_Reference);
		}

		[TestDate]
		public void TestPropagate_PropagatedEventIsDuplicatedIfReferenceChanges_PropagateOnParameterChange_SingleChild()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-10);

			child1.Logs.AddNew(Events.BookingConfirmed, "this text|WHS=One");
			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "shouldn't|WHS=One Two");
			var propagatedEvents = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled);
			AssertContainsExactElementsInAnyOrder("Propagated: All Dummy Siblings|WHS=One".Yield().Append("Propagated: All Dummy Siblings|WHS=One Two"), propagatedEvents.Select(log => log.SL_Reference));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "matter|WHS=One Two Three");
			propagatedEvents = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled);
			AssertContainsExactElementsInAnyOrder("Propagated: All Dummy Siblings|WHS=One".Yield().Append("Propagated: All Dummy Siblings|WHS=One Two").Append("Propagated: All Dummy Siblings|WHS=One Two Three"), propagatedEvents.Select(log => log.SL_Reference));
		}

		[TestDate]
		public void TestPropagate_PropagatedEventIsDuplicatedIfReferenceChanges_DoNotPropagateOnParameterChange_MultipleEventTypes()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-10);
			var propagationSettings = new DummyPropagationSettings(propagatedOnParamChange: false);

			child1.Logs.AddNew(Events.BookingConfirmed, "this text|WHS=One", ZDateTimeOffset.UtcNow, false, propagationSettings);
			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "shouldn't|WHS=One Two", ZDateTimeOffset.UtcNow, false, propagationSettings);
			propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One Two", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.Departure, "matter|WHS=One", ZDateTimeOffset.UtcNow, false, propagationSettings);
			propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.DepartureCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.Departure, "shouldn't|WHS=One Two", ZDateTimeOffset.UtcNow, false, propagationSettings);
			propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.DepartureCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One Two", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "matter|WHS=One Two Three", ZDateTimeOffset.UtcNow, false, propagationSettings);
			propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One Two Three", propagatedEvent.SL_Reference);
		}

		[TestDate]
		public void TestPropagate_PropagatedEventIsUpdatedIfReferenceChanges_DoNotPropagateOnParameterChange_SingleChild_PropagatedEventDeleted()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-10);
			var propagationSettings = new DummyPropagationSettings(propagatedOnParamChange: false);

			child1.Logs.AddNew(Events.BookingConfirmed, "this text|WHS=One", ZDateTimeOffset.UtcNow, false, propagationSettings);
			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "shouldn't|WHS=One Two", ZDateTimeOffset.UtcNow, false, propagationSettings);
			propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One Two", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			propagatedEvent.Delete();

			child1.Logs.AddNew(Events.BookingConfirmed, "matter|WHS=One Two Three", ZDateTimeOffset.UtcNow, false, propagationSettings);
			propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|WHS=One Two Three", propagatedEvent.SL_Reference);
		}

		[TestDate]
		public void TestPropagate_PropagatedEventIsUpdatedIfReferenceChanges_PropagateOnParameterChange_TwoChildren()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-10);

			child2.Logs.AddNew(Events.BookingConfirmed, "this text|CMP=One");
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "and this text|CMP=One");
			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|CMP=One", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "shouldn't|CMP=One Two");
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child2.Logs.AddNew(Events.BookingConfirmed, "matter|CMP=One Two");
			var propagatedEvents = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled);
			AssertContainsExactElementsInAnyOrder("Propagated: All Dummy Siblings|CMP=One".Yield().Append("Propagated: All Dummy Siblings|CMP=One Two"), propagatedEvents.Select(log => log.SL_Reference));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "random|CMP=One Two Three");
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child2.Logs.AddNew(Events.BookingConfirmed, "chaos|CMP=One Two Three");
			propagatedEvents = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled);
			AssertContainsExactElementsInAnyOrder("Propagated: All Dummy Siblings|CMP=One".Yield().Append("Propagated: All Dummy Siblings|CMP=One Two").Append("Propagated: All Dummy Siblings|CMP=One Two Three"), propagatedEvents.Select(log => log.SL_Reference));
		}

		[TestDate]
		public void TestPropagate_PropagatedEventIsUpdatedIfReferenceChanges_DoNotPropagateOnParameterChange_TwoChildren()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-10);
			var propagationSettings = new DummyPropagationSettings(propagatedOnParamChange: false);

			child2.Logs.AddNew(Events.BookingConfirmed, "this text|CMP=One", ZDateTimeOffset.UtcNow, false, propagationSettings);
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "and this text|CMP=One", ZDateTimeOffset.UtcNow, false, propagationSettings);
			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|CMP=One", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "shouldn't|CMP=One Two", ZDateTimeOffset.UtcNow, false, propagationSettings);
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child2.Logs.AddNew(Events.BookingConfirmed, "matter|CMP=One Two", ZDateTimeOffset.UtcNow, false, propagationSettings);
			propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|CMP=One Two", propagatedEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child1.Logs.AddNew(Events.BookingConfirmed, "random|CMP=One Two Three", ZDateTimeOffset.UtcNow, false, propagationSettings);
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			child2.Logs.AddNew(Events.BookingConfirmed, "chaos|CMP=One Two Three", ZDateTimeOffset.UtcNow, false, propagationSettings);
			propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.IsCancelled).Single();
			AssertEquals("Propagated: All Dummy Siblings|CMP=One Two Three", propagatedEvent.SL_Reference);
		}

		[TestDate]
		public void TestPropagate_TargetHasPreviouslyPropagatedEventInMemory_DifferentEventTime_UpdateExistingOne()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-2);

			child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Single();
			AssertEquals("Existing active propagated event", false, propagatedEvent.SL_IsCancelled);
			AssertEquals("Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);
			AssertEquals("Event Date on the propagated event", TestDateAttribute.Date, propagatedEvent.SL_EventTime);

			TestDateAttribute.Date = DateTime.Now;

			child2.Logs.AddNew(Events.BookingConfirmed, "HEY! UPDATE IS HERE|LOC=AUSYD|FAC=TERMINAL");

			AssertEquals("Event Date on the old propagated event", TestDateAttribute.Date, propagatedEvent.SL_EventTime);
		}

		[ExpectNoExceptions]
		[TestDate]
		public void TestPropagate_ExceedMaxLength()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-2);

			var refLength1024 = "|LOC=AUSYD|FAC=TERMINALpadding to 1024 characters:" + new string('A', 974);
			AssertEquals("Precondition", 1024, refLength1024.Length);

			child1.Logs.AddNew(Events.BookingConfirmed, refLength1024);
			child2.Logs.AddNew(Events.BookingConfirmed, refLength1024);

			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Single();
			AssertEquals("Existing active propagated event", false, propagatedEvent.SL_IsCancelled);
			AssertEquals("|FAC=TERMINALpadding to 1024 characters:" + new string('A', 974) + "|LOC=AUSYD", propagatedEvent.SL_Reference);
			AssertEquals("Event Date on the propagated event", TestDateAttribute.Date, propagatedEvent.SL_EventTime);

			TestDateAttribute.Date = DateTime.Now;

			child2.Logs.AddNew(Events.BookingConfirmed, refLength1024);

			AssertEquals("Event Date on the old propagated event", TestDateAttribute.Date, propagatedEvent.SL_EventTime);
		}

		[TestDate]
		public void TestPropagate_TargetHasPreviouslyPropagatedEvent_DifferentEventTime_PropagateNew_CancelExistingOne()
		{
			var parentDummy = Factory.New<DummyEnterpriseBusinessObject>();

			var child1 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child1.Z0_Guid = parentDummy.PK;

			var child2 = Factory.New<DummyProcessHandlingInfoProviderBizo>();
			child2.Z0_Guid = parentDummy.PK;

			TestDateAttribute.Date = DateTime.Now.AddMinutes(-2);

			child1.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");
			child2.Logs.AddNew(Events.BookingConfirmed, "|LOC=AUSYD|FAC=TERMINAL");

			var propagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Single();
			AssertEquals("Existing active propagated event", false, propagatedEvent.SL_IsCancelled);
			AssertEquals("Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", propagatedEvent.SL_Reference);

			Factory.Save();
			TestDateAttribute.Date = DateTime.Now;

			child2.Logs.AddNew(Events.BookingConfirmed, "HEY! UPDATE IS HERE|LOC=AUSYD|FAC=TERMINAL");
			var newPropagatedEvent = parentDummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingConfirmedCode).Single(log => !log.SL_IsCancelled);

			AssertEquals("Old propagated event was cancelled", true, propagatedEvent.SL_IsCancelled);
			AssertEquals("Propagated: All Dummy Siblings|FAC=TERMINAL|LOC=AUSYD", newPropagatedEvent.SL_Reference);
		}
	}
}
