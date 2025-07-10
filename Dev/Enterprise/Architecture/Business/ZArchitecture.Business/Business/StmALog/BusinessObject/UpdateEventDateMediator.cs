using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public class UpdateEventDateMediator
	{
		public UpdateEventDateMediator(IStmALog log, IStmALogParent master)
		{
			this.master = master;
			this.log = log;
		}
		readonly IStmALog log;
		readonly IStmALogParent master;

		public void PerformUpdateEventDateProperty()
		{
			if (!log.SL_IsCancelled)
			{
				var properties = GetRelatedProperties();
				if (properties.Count == 0)
				{
					return;
				}
				var dateTime = log.SL_EventTime;
				var dateTimeOffset = log.SL_EventTimeOffset;
				foreach (var property in properties)
				{
					var type = property.Property.PropertyType;
					if (type == typeof(ZDateTime))
					{
						if (!property.Property.Value.Equals(dateTime))
						{
							property.Property.Value = dateTime;
						}
					}
					else if (type == typeof(ZDateTimeOffset))
					{
						if (!property.Property.Value.Equals(dateTimeOffset))
						{
							property.Property.Value = dateTimeOffset;
						}
					}
				}
			}
		}

		public void CancelUpdateEventDateProperty()
		{
			var properties = GetRelatedProperties();

			using ((master as IUpdateEventDateSupporter)?.SetCancellingEventDatePropertyContext(log))
			{
				foreach (var property in properties)
				{
					var type = property.Property.PropertyType;
					if (type == typeof(ZDateTime))
					{
						property.Property.Value = ZDateTime.Empty;
					}
					else if (type == typeof(ZDateTimeOffset))
					{
						property.Property.Value = ZDateTimeOffset.Empty;
					}
				}
			}
		}

		List<EventDatePropertyAttribute.EventDatePropertyInfo> GetRelatedProperties()
		{
			var results = new List<EventDatePropertyAttribute.EventDatePropertyInfo>();

			if (master != null && master is BusinessObject bizo)
			{
				var eventType = Events.All[log.SL_SE_NKEvent];
				var estimateActual = log.SL_IsEstimate ? EstimateActual.Estimate : EstimateActual.Actual;

				var propertyInfos = EventDatePropertyAttribute.FindPropertyInfos(bizo, eventType, estimateActual);
				results = propertyInfos.Where(propertyInfo => CanUpdateEventDateProperty(propertyInfo, log)).ToList();

				if (!results.Any())
				{
					return GetRelatedPropertiesFromRelatedObjects(master, eventType, estimateActual);
				}
			}

			return results;
		}

		List<EventDatePropertyAttribute.EventDatePropertyInfo> GetRelatedPropertiesFromRelatedObjects(IStmALogParent logOwner, Event eventType, EstimateActual estimateActual)
		{
			var results = new List<EventDatePropertyAttribute.EventDatePropertyInfo>();

			var handlesEventsForOtherObjects = logOwner as IHandleEventsForOtherObjects;
			if (handlesEventsForOtherObjects != null)
			{
				var relatedObjects = handlesEventsForOtherObjects.GetHandledObjects();

				foreach (var relatedObject in relatedObjects)
				{
					var relatedPropertyInfos = EventDatePropertyAttribute.FindPropertyInfos(relatedObject, eventType, estimateActual);
					results.AddRange(relatedPropertyInfos.Where(relatedPropertyInfo => CanUpdateEventDateProperty(relatedPropertyInfo, log)));
				}
			}

			return results;
		}

		static bool CanUpdateEventDateProperty(EventDatePropertyAttribute.EventDatePropertyInfo datePropertyInfo, IStmALog log)
		{
			bool shouldUpdate = true;

			if (datePropertyInfo.Property.Value.IsEmpty || !datePropertyInfo.Attribute.ShouldOnlyUpdateEmptyDate)
			{
				var eventDatePropertyChecker = datePropertyInfo.Property.BizObj as IEventDatePropertyChecker;
				if (eventDatePropertyChecker != null)
				{
					shouldUpdate = eventDatePropertyChecker.CanUpdateProperty(log, datePropertyInfo.Property);
				}
			}
			else
			{
				shouldUpdate = false;
			}

			return shouldUpdate;
		}
	}
}
