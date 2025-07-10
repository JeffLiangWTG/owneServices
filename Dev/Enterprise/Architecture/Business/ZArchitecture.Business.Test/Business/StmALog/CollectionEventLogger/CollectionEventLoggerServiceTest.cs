using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CollectionEventLoggerServiceTest : TestCaseWithFactory
	{
		public void TestLogEventOnParentOnSave()
		{
			TestCollectionEventLogger logger = CollectionEventLoggerService.LogEventOnParentOnSave(Dummy, delegate
			{ return new TestCollectionEventLogger(Dummy, Dummy.Dependents); });
			TestCollectionEventLogger logger2 = CollectionEventLoggerService.LogEventOnParentOnSave(Dummy, delegate
			{ return new TestCollectionEventLogger(Dummy, Dummy.Dependents); });
			AssertEquals("Same CollectionEventLogger instance should be used", true, logger == logger2);
		}

		public void TestLoggersAreNotClearedAfterSave()
		{
			CollectionEventLoggerService.LogEventOnParentOnSave(Dummy, delegate
			{ return new TestCollectionEventLogger(Dummy, Dummy.Dependents); });
			CollectionEventLoggerService service = Factory.ServiceContainer.GetService<CollectionEventLoggerService>();
			AssertNotNull(service);
			AssertEquals(1, service.LoggersCount);
			Factory.Save();
			service = Factory.ServiceContainer.GetService<CollectionEventLoggerService>();
			AssertNotNull(service);
			AssertEquals("removed after factory saving", 1, service.LoggersCount);
		}

		#region Test Classes

		class TestCollectionEventLogger : CollectionEventLogger
		{
			public TestCollectionEventLogger(IStmALogParent parent, IEnumerable collection)
				: base(parent, collection)
			{
			}

			protected override KeyValuePair<SchemaDateTimeOffsetColumn, Event>[] DatePropertiesTrackedAsEventsOnParent
			{
				get
				{
					List<KeyValuePair<SchemaDateTimeOffsetColumn, Event>> result = new List<KeyValuePair<SchemaDateTimeOffsetColumn, Event>>();
					result.Add(new KeyValuePair<SchemaDateTimeOffsetColumn, Event>(DummyBizoSchema.Z0_DateTimeOffset, Events.Arrival));
					return result.ToArray();
				}
			}
		}

		#endregion

		#region Implementation

		DummyWithDependentsEnterpriseBusinessObject Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithDependentsEnterpriseBusinessObject>();
				}
				return dummy;
			}
		}
		DummyWithDependentsEnterpriseBusinessObject dummy;

		#endregion
	}
}
