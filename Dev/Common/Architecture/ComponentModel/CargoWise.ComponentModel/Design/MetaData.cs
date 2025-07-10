using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Helper to locate meta-data property values.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
	public abstract class MetaData
	{
		public static object GetNullValue(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			return GetMetaData(component, property, MetaDataTypes.Null);
		}

		public static bool GetReadOnly(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			var result = GetMetaData(component, property, MetaDataTypes.ReadOnly);
			if (result != null && !(result is bool))
			{
				result = TypeDescriptor.GetConverter(result.GetType()).ConvertTo(result, typeof(bool));
			}
			return result != null && (bool)result;
		}

		public static bool GetReadOnlyExcludingMethodProvider(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			var result = GetMetaData(component, property, MetaDataTypes.ReadOnly, true);
			return result != null && (bool)result;
		}

		public static int GetMaxLength(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			var result = GetMetaData(component, property, MetaDataTypes.MaxLength);
			return result != null ? (int)result : 0;
		}

		public static bool GetPassword(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			var result = GetMetaData(component, property, MetaDataTypes.Password);
			return result != null && (bool)result;
		}

		public static int GetDateTimeOffsetPlaces(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			var result = GetMetaData(component, property, MetaDataTypes.DateTimeOffsetPlaces);
			return result != null ? (int)result : 0;
		}

		public static int GetDecimalPlaces(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			var result = GetMetaData(component, property, MetaDataTypes.DecimalPlaces);
			return result != null ? (int)result : 0;
		}

		public static int GetDecimalPrecision(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			var result = GetMetaData(component, property, MetaDataTypes.DecimalPrecision);
			return result != null ? (int)result : 0;
		}

		public static IEnumerable GetListDataSource(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			return (IEnumerable)GetMetaData(component, property, MetaDataTypes.ListDataSource);
		}

		#region Notifications

		public static IEnumerable<INotification> GetNotifications(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			return (IEnumerable<INotification>)GetMetaData(component, property, MetaDataTypes.Notifications);
		}

		public static bool HasNotifications(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			var notifications = GetNotifications(component, property);
			var enumerator = notifications != null ? notifications.GetEnumerator() : null;
			return enumerator != null && enumerator.MoveNext();
		}

		public static bool HasNotifications(object component, PropertyDescriptor property, INotificationType type)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			var notifications = GetNotifications(component, property);
			var typeNotifications = notifications != null ? notifications.GetNotifications(type) : null;
			var enumerator = typeNotifications != null ? typeNotifications.GetEnumerator() : null;
			return enumerator != null && enumerator.MoveNext();
		}

		#endregion

		#region GetDescription

		public static IDescription GetDescription(object component, PropertyDescriptor property)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			return (IDescription)GetMetaData(component, property, MetaDataTypes.Description);
		}

		public static string GetDescriptionOfMaxLength(object component, PropertyDescriptor property, int lessThanLength)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			string result = null;
			var description = MetaData.GetDescription(component, property);
			if (description != null)
			{
				for (int i = description.Count - 1; i >= 0; i--)
				{
					result = description.GetDescription(i, CultureInfo.CurrentCulture);
					if (result.Length <= lessThanLength)
					{
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region IsEmptyValue

		public static bool IsEmptyValue(object component, PropertyDescriptor property)
		{
			Argument.NotNull(property, nameof(property)); // Suggested By ReviewBot 
			var empty = MetaData.GetNullValue(component, property);
			var value = property.GetValue(component);
			var list = value as IList;
			return (list != null && list.Count == 0) || value == null || value is DBNull || object.Equals(empty, value) || value.ToString().Length == 0;
		}

		#endregion

		#region GetMetaData / GetMetaDataProperty

		public static object GetMetaData(Type componentType, string metaDataTypeId)
		{
			return MetaDataValueMemberLocator.GetMetaDataValue(componentType, metaDataTypeId);
		}

		public static object GetMetaData(object component, PropertyDescriptor property, string metaDataTypeId)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			return GetMetaData(component, property, metaDataTypeId, false);
		}

		public static object GetMetaData(object component, PropertyDescriptor property, string metaDataTypeId, bool excludeMethodProvider)
		{
			if (!(component != null || property != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(component));
			}

			object result;

			var componentType = component == null ? property.ComponentType : component.GetType();

			if (property == null)
			{
				result = MetaDataValueMemberLocator.GetMetaDataValue(componentType, metaDataTypeId);
			}
			else
			{
				var metaDataProperty = GetMetaDataProperty(componentType, property, metaDataTypeId, excludeMethodProvider);
				if (metaDataProperty != null)
				{
					result = metaDataProperty.GetValueForMetaData(component);
				}
				else
				{
					var type = MetaDataType.GetMetaDataType(metaDataTypeId);
					result = type.DefaultValue;
				}
			}
			return result;
		}

		public static KPropertyDescriptor GetMetaDataProperty(Type componentType, PropertyDescriptor property, string metaDataTypeId)
		{
			return GetMetaDataProperty(componentType, property, metaDataTypeId, false);
		}

		public static KPropertyDescriptor GetMetaDataProperty(Type componentType, PropertyDescriptor property, string metaDataTypeId, bool excludeMethodProvider)
		{
			KPropertyDescriptor result = null;

			var castedProperty = property as KPropertyDescriptor;
			if (castedProperty != null)
			{
				var excludeMetaDataMemberAttribute = (ExcludeMetaDataMemberAttribute)castedProperty.Attributes[typeof(ExcludeMetaDataMemberAttribute)];
				if (excludeMetaDataMemberAttribute != null && excludeMetaDataMemberAttribute.MetaDataTypeId == metaDataTypeId)
				{
					return null;
				}

				var collection = castedProperty.Collection as PropertyDescriptorCollectionWithMetaData;
				if (collection != null)
				{
					result = collection.GetMetaDataProperty(componentType, castedProperty, metaDataTypeId, excludeMethodProvider);
				}
			}

			return result;
		}

		#endregion
	}
}
