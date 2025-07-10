using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class PropertyComparer : IComparer<BusinessObject>, IComparer
	{
		public PropertyComparer(Type businessObjectType, string propertyName, ListSortDirection direction)
		{
			Argument.NotNull(businessObjectType, "businessObjectType");
			Argument.NotNull(propertyName, "propertyName");

			this.PropertyDescriptor = ((PropertyDescriptorCollection)ZCustomTypeDescriptor.GetProperties(businessObjectType))[propertyName];
			if (PropertyDescriptor == null)
			{
				throw new ArgumentException("Could not find property " + propertyName + " on business object type " + businessObjectType.FullName);
			}
			this.Direction = direction;
		}

		public PropertyComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
		{
			Argument.NotNull(propertyDescriptor, "propertyDescriptor");
			this.PropertyDescriptor = propertyDescriptor;
			this.Direction = direction;
		}
		internal Dictionary<BusinessObject, IComparable> cachedValues;

		public readonly PropertyDescriptor PropertyDescriptor;
		public readonly ListSortDirection Direction;

		int factoryVersion;

		internal protected virtual IComparable GetPropertyValueFromObject(BusinessObject businessObject)
		{
			try
			{
				IComparable result = null;

				var factory = businessObject.Factory;
				if (factory == null)
				{
					result = (IComparable)PropertyDescriptor.GetValue(businessObject);
				}
				else
				{
					if (cachedValues == null || factory.CacheVersion != factoryVersion)
					{
						cachedValues = new Dictionary<BusinessObject, IComparable>();
						factoryVersion = factory.CacheVersion;
					}

					if (!cachedValues.TryGetValue(businessObject, out result))
					{
						result = (IComparable)PropertyDescriptor.GetValue(businessObject);
						cachedValues[businessObject] = result;
					}
				}

				return result;
			}
			catch (InvalidCastException ex)
			{
				ex.Source = PropertyDescriptor.Name;
				throw;
			}
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			PropertyComparer rhs = obj as PropertyComparer;
			return
				rhs != null &&
				PropertyDescriptor == rhs.PropertyDescriptor &&
				Direction == rhs.Direction;
		}

		public override int GetHashCode()
		{
			return PropertyDescriptor.GetHashCode();
		}

		#endregion

		#region IComparer Members

		public virtual int Compare(BusinessObject x, BusinessObject y)
		{
			IComparable valueFromX = GetPropertyValueFromObject(x);
			IComparable valueFromY = GetPropertyValueFromObject(y);

			if (valueFromX == null && valueFromY == null)
			{
				return 0;
			}
			else if (valueFromX == null)
			{
				return (Direction == ListSortDirection.Ascending) ? -1 : 1;
			}
			else if (valueFromY == null)
			{
				return (Direction == ListSortDirection.Ascending) ? 1 : -1;
			}
			else
			{
				try
				{
					return Compare(valueFromX, valueFromY);
				}
				catch (ArgumentException ex)
				{
					var message = string.Format((NoResString)"Comparing valueFromX:{0}=[{1}] with valueFromY:{2}=[{3}] for property {4}.{5} ({6})",
						valueFromX.GetType().Name, valueFromX, valueFromY.GetType().Name, valueFromY, PropertyDescriptor.ComponentType.FullName, PropertyDescriptor.Name, PropertyDescriptor.PropertyType.Name);
					ErrorReporter.ReportOnce(message, ex);

					return Compare(valueFromX.ToString(), valueFromY.ToString());
				}
			}
		}

		int Compare(IComparable x, IComparable y)
		{
			return Direction == ListSortDirection.Ascending ? x.CompareTo(y) : y.CompareTo(x);
		}

		int IComparer.Compare(object x, object y)
		{
			if ((x == null || x is BusinessObject) && (y == null || y is BusinessObject))
			{
				return Compare((BusinessObject)x, (BusinessObject)y);
			}
			else
			{
				throw new ArgumentException("Invalid arguments (x, y)");
			}
		}

		#endregion
	}
}
