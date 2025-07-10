using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestsSubclassesOf(typeof(CollectionEventLogger))]
	public abstract class CollectionEventLoggerTest<ParentT, CollectionEventLoggerT> : TestCaseWithFactory
			where ParentT : BusinessObject
			where CollectionEventLoggerT : CollectionEventLogger
	{
		public virtual void TestLogEventsOnParentForCollection()
		{
			foreach (KeyValuePair<SchemaDateTimeOffsetColumn, Event> property in DatePropertiesTrackedAsEventsOnParent)
			{
				TestLogEventsOnParentForCollection(property.Key, property.Value);
				parent = null;
			}
		}

		public virtual void TestCancelEventsOnParentForCollection()
		{
			foreach (KeyValuePair<SchemaDateTimeOffsetColumn, Event> property in DatePropertiesTrackedAsEventsOnParent)
			{
				TestCancelEventsOnParentForCollection(property.Key, property.Value);
				parent = null;
			}
		}

		public virtual void TestAddAndCancelEventsOnParentCollection_WithEstimateEvent()
		{
			foreach (KeyValuePair<SchemaDateTimeOffsetColumn, Event> property in DatePropertiesTrackedAsEventsOnParent)
			{
				TestAddAndCancelEventsOnParentCollection_WithEstimateEvent(property.Key, property.Value);
				parent = null;
			}
		}

		protected virtual void TestLogEventsOnParentForCollection(SchemaDateTimeOffsetColumn property, Event ev)
		{
			BusinessObject businessObject1 = NewCollectionElement(Parent);
			BusinessObject businessObject2 = NewCollectionElement(Parent);
			Factory.Save();
			AssertEventNotRaised("Event not raised initially", ev);

			businessObject1[property.Name] = ZDateTimeOffset.Now.AddSeconds(-1);
			Factory.Save();
			AssertEventNotRaised("Event not raised until all container dates set", ev);

			//Parent is CONSOL
			//So the problem here is that the parent isn't getting the events because the children aren't actually being set as we check the FAC.
			businessObject2[property.Name] = ZDateTimeOffset.Now.AddSeconds(-1);
			Factory.Save();
			AssertEventRaised("Event raised after all container dates set", ev);

			DeleteBusinessObject(businessObject1);
			DeleteBusinessObject(businessObject2);
			Factory.Save();
		}

		public virtual void TestLogEventsOnParentForCollection_WhenDeletingCollectionElement()
		{
			Event anEventType = DatePropertiesTrackedAsEventsOnParent[0].Value;
			SchemaDateTimeOffsetColumn eventDateProperty = DatePropertiesTrackedAsEventsOnParent[0].Key;

			BusinessObject businessObject = NewCollectionElement(Parent);
			AssertEventNotRaised("Event not raised after creating a collection element", anEventType);
			Factory.Save();
			DeleteBusinessObject(businessObject);
			Factory.Save();
			AssertEventNotRaised("Event not raised when there are no registered containers", anEventType);

			BusinessObject businessObject1 = NewCollectionElement(Parent);
			BusinessObject businessObject2 = NewCollectionElement(Parent);
			businessObject1[eventDateProperty.Name] = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEventNotRaised("Event not raised when only 1 container has it's date set", DatePropertiesTrackedAsEventsOnParent[0].Value);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			businessObject2 = LoadCollectionElementFromOtherFactory(newFactory, businessObject2);
			DeleteBusinessObject(businessObject2);
			newFactory.Save();
			AssertEventRaised("Event logged when a business object is deleted, and the only business object left has it's date set", DatePropertiesTrackedAsEventsOnParent[0].Value);
		}

		protected virtual void TestCancelEventsOnParentForCollection(SchemaDateTimeOffsetColumn property, Event ev)
		{
			BusinessObject businessObject1 = NewCollectionElement(Parent);
			businessObject1[property.Name] = ZDateTimeOffset.Now.AddSeconds(-1);

			BusinessObject businessObject2 = NewCollectionElement(Parent);
			businessObject2[property.Name] = ZDateTimeOffset.Now.AddSeconds(-1);

			Factory.Save();
			AssertEventRaised("Event raised after all container dates set", ev);

			businessObject2[property.Name] = ZDateTimeOffset.Empty;
			Factory.Save();

			AssertEventNotRaised("Event has been cancelled", ev);
		}

		protected virtual void TestAddAndCancelEventsOnParentCollection_WithEstimateEvent(SchemaDateTimeOffsetColumn property, Event ev)
		{
			Parent.GetLogs().CreateRecreateOrUpdateEventLog(ev, EstimateActual.Estimate, ZDateTimeOffset.Now);

			Factory.Save();

			BusinessObject businessObject1 = NewCollectionElement(Parent);
			businessObject1[property.Name] = ZDateTimeOffset.Now.AddSeconds(-1);

			BusinessObject businessObject2 = NewCollectionElement(Parent);
			businessObject2[property.Name] = ZDateTimeOffset.Now.AddSeconds(-1);

			Factory.Save();
			AssertEventRaised("Event raised after all container dates set", ev);

			Parent.GetLogs().CreateRecreateOrUpdateEventLog(ev, EstimateActual.Estimate, ZDateTimeOffset.Now);
			Factory.Save();

			businessObject2[property.Name] = ZDateTimeOffset.Empty;
			Factory.Save();

			AssertEventNotRaised("Event has been cancelled", ev);
		}

		protected abstract CollectionEventLoggerT NewCollectionEventLogger();
		protected abstract BusinessObject NewCollectionElement(ParentT parent);

		protected virtual ParentT NewParentBusinessObject()
		{
			return Factory.New<ParentT>();
		}

		protected virtual BusinessObject LoadCollectionElementFromOtherFactory(BusinessObjectFactory factory, BusinessObject businessObject)
		{
			return factory.Load(businessObject.GetType(), businessObject.PK);
		}

		protected virtual void DeleteBusinessObject(BusinessObject businessObject)
		{
			businessObject.Delete();
		}

		#region Implementation

		protected void AssertEventNotRaised(string message, Event ev)
		{
			((ILogsInternals)Parent.GetLogs()).ReloadFromDB();

			var isNotEstimateQuery = new ZQuery(StmALogSchema.SL_IsEstimate, "N");
			StmALog log = Parent.GetLogs().MostRecentLogByEventTime(ev, isNotEstimateQuery);
			AssertNull(message + "; event " + ev.Code + " not expected to be raised", log);
		}

		protected void AssertEventRaised(string message, Event ev)
		{
			((ILogsInternals)Parent.GetLogs()).ReloadFromDB();

			var isNotEstimateQuery = new ZQuery(StmALogSchema.SL_IsEstimate, "N");
			StmALog log = Parent.GetLogs().MostRecentLogByEventTime(ev, isNotEstimateQuery);
			AssertNotNull(message + "; expected event " + ev.Code + " to be raised", log);
			if (log != null)
			{
				AssertEventRaised(message, ev, log);
			}
		}

		protected virtual void AssertEventRaised(string message, Event ev, StmALog log)
		{
			AssertEquals("SL_SE_NKEvent", ev.Code, log.SL_SE_NKEvent);
		}

		protected ParentT Parent
		{
			get
			{
				if (parent == null)
				{
					parent = NewParentBusinessObject();
				}
				return parent;
			}
		}
		ParentT parent;

		protected CollectionEventLoggerT CollectionEventLogger
		{
			get
			{
				if (collectionEventLogger == null)
				{
					collectionEventLogger = NewCollectionEventLogger();
				}
				return collectionEventLogger;
			}
		}
		CollectionEventLoggerT collectionEventLogger;

		protected virtual KeyValuePair<SchemaDateTimeOffsetColumn, Event>[] DatePropertiesTrackedAsEventsOnParent
		{
			get
			{
				return (KeyValuePair<SchemaDateTimeOffsetColumn, Event>[])typeof(CollectionEventLogger).InvokeMember("DatePropertiesTrackedAsEventsOnParent", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, CollectionEventLogger, null);
			}
		}

		#endregion
	}
}
