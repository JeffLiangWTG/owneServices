using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public enum EstimateActual
	{
		MilestoneEstimateOnly,
		Estimate,
		Actual,
	}

	/// <summary>
	/// Apply this attribute to a property to ensure an StmALog record exists with SL_EventTime containing the
	/// property's value.
	/// WARNING: Use of this property means an INSERT into dbo.StmALog when a date is first populated or changed.
	///          When the property value is changed, an additional SELECT is performed to see if the StmALog
	///          already exists, so that the existing event log can be cancelled.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public sealed class EventDatePropertyAttribute : Attribute
	{
		public EventDatePropertyAttribute(string eventType, EstimateActual estimateActual, bool shouldOnlyUpdateEmptyDate = false)
		{
			EventType = eventType;
			EstimateActual = estimateActual;
			ShouldOnlyUpdateEmptyDate = shouldOnlyUpdateEmptyDate;
		}

		public readonly ZString EventType;
		public readonly EstimateActual EstimateActual;
		public readonly bool ShouldOnlyUpdateEmptyDate;

		ZString PropertyName { get; set; }

		public static IEnumerable<EventDatePropertyInfo> FindPropertyInfos(BusinessObject parent, Event eventType, EstimateActual estimateActual)
		{
			Argument.NotNull(parent, "parent");
			var results = new HashSet<EventDatePropertyInfo>(new EventDatePropertyInfoComparer());

			if (eventType != null)
			{
				var attributes = GetAttributesForTypeCached(parent.GetType())
					.Where(a => a.EventType == eventType.Code && a.EstimateActual == estimateActual);

				foreach (var attribute in attributes)
				{
					var propertyInfo = parent.ZPropertyInfoHash.GetPropertySafe(attribute.PropertyName);
					if (propertyInfo != null)
					{
						results.Add(new EventDatePropertyInfo { Attribute = attribute, Property = propertyInfo });
					}
				}
			}

			return results;
		}

		#region Implementation

		static Dictionary<Type, EventDatePropertyAttribute[]> CachedAttributes
		{
			get { return cachedAttributes ?? (cachedAttributes = new Dictionary<Type, EventDatePropertyAttribute[]>()); }
		}
		[ThreadStatic]
		static Dictionary<Type, EventDatePropertyAttribute[]> cachedAttributes;

		static EventDatePropertyAttribute[] GetAttributesForTypeCached(Type type)
		{
			EventDatePropertyAttribute[] result = null;
			lock (CachedAttributes)
			{
				CachedAttributes.TryGetValue(type, out result);
				if (result == null)
				{
					result = GetAttributesForType(type);
					CachedAttributes[type] = result;
				}
			}

			return result;
		}

		static EventDatePropertyAttribute[] GetAttributesForType(Type type)
		{
			var result = new List<EventDatePropertyAttribute>();

			Type currentType = type;
			while (currentType != typeof(object))
			{
				foreach (PropertyInfo property in currentType.GetProperties())
				{
					foreach (EventDatePropertyAttribute attribute in property.GetCustomAttributes(typeof(EventDatePropertyAttribute), false))
					{
						attribute.PropertyName = property.Name;
						result.Add(attribute);
					}
				}
				currentType = currentType.BaseType;
			}

			return result.ToArray();
		}

		#endregion

		#region Types

		public class EventDatePropertyInfo
		{
			public EventDatePropertyAttribute Attribute { get; set; }
			public ZPropertyInfo Property { get; set; }
		}

		class EventDatePropertyInfoComparer : IEqualityComparer<EventDatePropertyInfo>
		{
			public bool Equals(EventDatePropertyInfo x, EventDatePropertyInfo y)
			{
				return x.Property.Equals(y.Property);
			}

			public int GetHashCode(EventDatePropertyInfo obj)
			{
				return obj.Property.GetHashCode();
			}
		}

		#endregion
	}
}
