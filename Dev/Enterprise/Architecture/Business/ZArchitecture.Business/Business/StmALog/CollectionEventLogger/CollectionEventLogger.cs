using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class CollectionEventLogger
	{
		protected CollectionEventLogger(IStmALogParent parent, IEnumerable collection)
		{
			Parent = parent;
			Collection = collection;
		}

		public readonly IStmALogParent Parent;
		public readonly IEnumerable Collection;

		protected void LogEvent(Event ev)
		{
			var isNotEstimateQuery = new ZQuery(StmALogSchema.SL_IsEstimate, "N");
			if (Parent.Logs.MostRecentLogByEventTime(ev, isNotEstimateQuery) == null)
			{
				Parent.Logs.CreateRecreateOrUpdateEventLog(ev, EstimateActual.Actual, ZDateTimeOffset.Now);
			}
		}

		protected void CancelEvent(Event ev)
		{
			var isNotEstimateQuery = new ZQuery(StmALogSchema.SL_IsEstimate, "N");
			var log = Parent.Logs.MostRecentLogByEventTime(ev, isNotEstimateQuery);
			if (log != null)
			{
				log.Cancel();
			}
		}

		protected virtual void OnBeforeLogEvent()
		{
		}

		#region Implementation

		internal void CreateOrCancelEventsOnParentForCollectionIfRequired()
		{
			OnBeforeLogEvent();

			if (ShouldCreateOrCancelEvents())
			{
				var eventsEmptyDate = GetTrackedEventsWithEmptyDate();
				foreach (var ev in ParentEventTypes)
				{
					if (eventsEmptyDate.Contains(ev))
					{
						CancelEvent(ev);
					}
					else
					{
						LogEvent(ev);
					}
				}
			}
		}

		IEnumerable<Event> GetTrackedEventsWithEmptyDate()
		{
			var result = new HashSet<Event>();
			foreach (EnterpriseBusinessObject bizObj in Collection)
			{
				foreach (var datePropertyAndEvent in DatePropertiesTrackedAsEventsOnParent)
				{
					var dateProperty = datePropertyAndEvent.Key;
					var ev = datePropertyAndEvent.Value;
					AddEventToListIfEmpty(result, ev, (ZDateTimeOffset)bizObj[dateProperty]);
				}
			}
			return result;
		}

		void AddEventToListIfEmpty(HashSet<Event> set, Event ev, ZDateTimeOffset date)
		{
			if (!date.IsValid)
			{
				set.Add(ev);
			}
		}

		protected virtual bool ShouldCreateOrCancelEvents()
		{
			foreach (var item in Collection)
			{
				return true;
			}
			return false;
		}

		IEnumerable<Event> ParentEventTypes
		{
			get
			{
				var result = new List<Event>(DatePropertiesTrackedAsEventsOnParent.Length);
				foreach (var ev in DatePropertiesTrackedAsEventsOnParent)
				{
					result.Add(ev.Value);
				}
				return result;
			}
		}

		protected abstract KeyValuePair<SchemaDateTimeOffsetColumn, Event>[] DatePropertiesTrackedAsEventsOnParent { get; }

		#endregion
	}
}
