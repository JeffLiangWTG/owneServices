using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class UpdateEventDateMediatorTest : TestCaseWithFactory
	{
		public void TestUpdate_MatchedByCodeAndByEstimateActual()
		{
			var dummy = Factory.New<DummyBOWithDateUpdateAttributes>();

			CombineAssertions("Precondition: dates are empty", () =>
				{
					Assert(dummy.FreightLoaded_ActualDate.IsEmpty);
					Assert(dummy.FreightLoaded_EstimatedDate.IsEmpty);
				});

			var estimateEventTime = ZDateTimeOffset.Now;

			dummy.GetLogs().AddNew(Events.FreightLoaded, estimateEventTime, true);
			AssertEquals("Actual not updated", ZDateTime.Empty, dummy.FreightLoaded_ActualDate);
			AssertEquals("Esimated updated", estimateEventTime.ToZDateTime(), dummy.FreightLoaded_EstimatedDate);

			var actualEventTime = ZDateTimeOffset.Now.AddMinutes(10);

			dummy.GetLogs().AddNew(Events.FreightLoaded, actualEventTime);
			AssertEquals("Actual date updated", actualEventTime.ToZDateTime(), dummy.FreightLoaded_ActualDate);
			AssertEquals("Esimated not updated", estimateEventTime.ToZDateTime(), dummy.FreightLoaded_EstimatedDate);
		}

		public void TestCancel_MatchedByCodeAndByEstimateActual()
		{
			var dummy = Factory.New<DummyBOWithDateUpdateAttributes>();

			var estimatedLog = dummy.GetLogs().AddNew(Events.FreightLoaded, ZDateTimeOffset.Now, true);
			var actualLog = dummy.GetLogs().AddNew(Events.FreightLoaded, ZDateTimeOffset.Now.AddMinutes(10));

			AssertEquals(actualLog.SL_EventTime, dummy.FreightLoaded_ActualDate);
			AssertEquals(estimatedLog.SL_EventTime, dummy.FreightLoaded_EstimatedDate);

			actualLog.Cancel();

			AssertEquals("Actual date was emptied", ZDateTime.Empty, dummy.FreightLoaded_ActualDate);
			AssertEquals("Estimated date not changed", estimatedLog.SL_EventTime, dummy.FreightLoaded_EstimatedDate);

			estimatedLog.Cancel();

			AssertEquals("Actual date remains empty", ZDateTime.Empty, dummy.FreightLoaded_ActualDate);
			AssertEquals("Estimated date was emptied", ZDateTime.Empty, dummy.FreightLoaded_EstimatedDate);
		}

		public void TestUpdate_ShouldOnlyUpdateEmptyDateOption()
		{
			var dummy = Factory.New<DummyBOWithDateUpdateAttributes>();
			AssertEquals("Precondition: empty", ZDateTime.Empty, dummy.BookingConfirmed);

			var eventTime = ZDateTimeOffset.Now;
			dummy.GetLogs().AddNew(Events.BookingConfirmed, eventTime);
			AssertEquals("Empty date was updated", eventTime.ToZDateTime(), dummy.BookingConfirmed);

			var newEventTime = ZDateTimeOffset.Now.AddMinutes(10);
			dummy.GetLogs().AddNew(Events.BookingConfirmed, newEventTime);
			AssertEquals("Non-empty date was not updated", eventTime.ToZDateTime(), dummy.BookingConfirmed);
		}

		public void TestUpdate_ConditionalUpdate_IEventDatePropertyChecker()
		{
			var dummy = Factory.New<DummyBOWithDateUpdateAttributes>();

			CombineAssertions("Precondition: dates are empty", () =>
			{
				Assert(dummy.ArrivalDate.IsEmpty);
				Assert(dummy.DepartureDate.IsEmpty);
			});

			dummy.EventsToBeExcludedFromUpdate.Add(Events.DepartureCode);

			var eventTime = ZDateTimeOffset.Now;
			dummy.GetLogs().AddNew(Events.Arrival, eventTime);
			dummy.GetLogs().AddNew(Events.Departure, eventTime);

			AssertEquals("Arrival event passed the additional check and was updated", eventTime.ToZDateTime(), dummy.ArrivalDate);
			AssertEquals("Departure event have not passed the additional check and was not updated", ZDateTime.Empty, dummy.DepartureDate);

			dummy.EventsToBeExcludedFromUpdate.Clear();
			dummy.EventsToBeExcludedFromUpdate.Add(Events.ArrivalCode);

			var newEventTime = ZDateTimeOffset.Now.AddMinutes(10);
			dummy.GetLogs().AddNew(Events.Arrival, newEventTime);
			dummy.GetLogs().AddNew(Events.Departure, newEventTime);

			AssertEquals("Arrival event have not passed the additional check and was not updated", eventTime.ToZDateTime(), dummy.ArrivalDate);
			AssertEquals("Departure event passed the additional check and was updated", newEventTime.ToZDateTime(), dummy.DepartureDate);
		}

		public void TestUpdate_ConditionalUpdate_PropertyInfosWithSameEventAttribute()
		{
			var today = ZDateTimeOffset.Today;
			var dummy = Factory.New<DummyBOWithDateUpdateAttributes>();
			Assert("Pre-condition: should be empty by default", dummy.GateInContainerYardDate.IsEmpty);
			Assert("Pre-condition: should be empty by default", dummy.GateInWharfDate.IsEmpty);

			var log = dummy.GetLogs().AddNew(AutoEvents.GateIn, today);

			AssertEquals("Dummy date should be updated from event log", today.ToZDateTime(), dummy.GateInContainerYardDate);
			AssertEquals("Other dummy date should updated by the same event", today.ToZDateTime(), dummy.GateInWharfDate);

			log.Cancel();

			Assert("Date should be cleared", dummy.GateInContainerYardDate.IsEmpty);
			Assert("Date should be cleared", dummy.GateInWharfDate.IsEmpty);

			dummy.GetLogs().AddNew(AutoEvents.GateIn, today.AddMinutes(50));

			AssertEquals(today.AddMinutes(50).ToZDateTime(), dummy.GateInContainerYardDate);
			AssertEquals(today.AddMinutes(50).ToZDateTime(), dummy.GateInWharfDate);

			dummy.GateInWharfDate = ZDateTime.Empty;

			Assert("Date should be cleared", dummy.GateInWharfDate.IsEmpty);
			AssertEquals("Expected no change. Both dates are updated by the same event, but are independent once set intially", today.AddMinutes(50).ToZDateTime(), dummy.GateInContainerYardDate);
		}

		public void TestUpdate_DateIsUpdatedOnOtherObjects()
		{
			var dummyBO = Factory.New<DummyBOWithDateUpdateAttributes>();
			var anotherDummyBO = Factory.New<AnotherDummyBOWithDateUpdateAttribute>();
			dummyBO.SetEventHandledObjects(anotherDummyBO);

			AssertEquals("Precondition: dummyBO.Z0_Date", ZDateTime.Empty, dummyBO.Z0_Date);
			AssertEquals("Precondition: anotherDummyBO.Z0_Date", ZDateTime.Empty, anotherDummyBO.Z0_Date);

			dummyBO.GetLogs().AddNew(AutoEvents.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(2013, 1, 1, 10, 0, 0));
			AssertEquals("dummyBO.Z0_Date", ZDateTime.Empty, dummyBO.Z0_Date);
			AssertEquals("anotherDummyBO.Z0_Date", new ZDateTime(2013, 1, 1, 10, 0, 0), anotherDummyBO.Z0_Date);
		}

		#region Implementation

		class DummyBOWithDateUpdateAttributes : DummyEnterpriseBusinessObject, IHandleEventsForOtherObjects, IEventDatePropertyChecker
		{
			public DummyBOWithDateUpdateAttributes(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[EventDateProperty(Events.FreightLoadedCode, EstimateActual.Actual)]
			public ZDateTime FreightLoaded_ActualDate
			{
				get;
				set;
			}

			public ZPropertyInfo FreightLoaded_ActualDateInfo
			{
				get { return GetZPropertyInfo(nameof(FreightLoaded_ActualDate)); }
			}

			[EventDateProperty(Events.FreightLoadedCode, EstimateActual.Estimate)]
			public ZDateTime FreightLoaded_EstimatedDate
			{
				get;
				set;
			}

			public ZPropertyInfo FreightLoaded_EstimatedDateInfo
			{
				get { return GetZPropertyInfo(nameof(FreightLoaded_EstimatedDate)); }
			}

			[EventDateProperty(Events.BookingConfirmedCode, EstimateActual.Actual, shouldOnlyUpdateEmptyDate: true)]
			public ZDateTime BookingConfirmed
			{
				get;
				set;
			}

			public ZPropertyInfo BookingConfirmedInfo
			{
				get { return GetZPropertyInfo(nameof(BookingConfirmed)); }
			}

			[EventDateProperty(Events.ArrivalCode, EstimateActual.Actual)]
			public ZDateTime ArrivalDate
			{
				get;
				set;
			}

			public ZPropertyInfo ArrivalDateInfo
			{
				get { return GetZPropertyInfo(nameof(ArrivalDate)); }
			}

			[EventDateProperty(Events.DepartureCode, EstimateActual.Actual)]
			public ZDateTime DepartureDate
			{
				get;
				set;
			}

			public ZPropertyInfo DepartureDateInfo
			{
				get { return GetZPropertyInfo(nameof(DepartureDate)); }
			}

			[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Actual)]
			public ZDateTime GateInWharfDate
			{
				get;
				set;
			}

			public ZPropertyInfo GateInWharfDateInfo
			{
				get { return GetZPropertyInfo(nameof(GateInWharfDate)); }
			}

			[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Actual)]
			public ZDateTime GateInContainerYardDate
			{
				get;
				set;
			}

			public ZPropertyInfo GateInContainerYardDateInfo
			{
				get { return GetZPropertyInfo(nameof(GateInContainerYardDate)); }
			}

			#region IHandleEventsForOtherObjects

			public void SetEventHandledObjects(BusinessObject anotherBO)
			{
				this.anotherBO = anotherBO;
			}

			BusinessObject[] IHandleEventsForOtherObjects.GetHandledObjects()
			{
				return anotherBO != null ? new[] { anotherBO } : System.Array.Empty<BusinessObject>();
			}
			BusinessObject anotherBO;

			#endregion

			#region IEventDatePropertyChecker

			public List<string> EventsToBeExcludedFromUpdate = new List<string>();

			bool IEventDatePropertyChecker.CanUpdateProperty(IStmALog eventLog, ZPropertyInfo property)
			{
				return !EventsToBeExcludedFromUpdate.Contains(eventLog.SL_SE_NKEvent);
			}

			#endregion
		}

		class AnotherDummyBOWithDateUpdateAttribute : DummyEnterpriseBusinessObject
		{
			public AnotherDummyBOWithDateUpdateAttribute(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[EventDateProperty(AutoEvents.DeliveryCartageCompleteFinalisedCode, EstimateActual.Actual)]
			public override ZDateTime Z0_Date
			{
				get { return base.Z0_Date; }
				set { base.Z0_Date = value; }
			}
		}

		#endregion
	}
}
