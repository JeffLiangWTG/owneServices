using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	[Flags]
	public enum PropertyInfoTypes
	{
		All = 0xffff,
		NonWrapping = 2, // for example OH_Code
		Wrapping = 1, // for example MiscServ+OM_PropertyInfo or OH_DeclaredWrappingPropertyReturningAMiscServProperty
	}

	public class ZPropertyInfoHashtable : IEnumerable
	{
		protected internal ZPropertyInfoHashtable(BusinessObject bizObj)
		{
			if (object.ReferenceEquals(bizObj, null))
			{
				throw new ArgumentNullException(nameof(bizObj));
			}
			this.businessObject = bizObj;
		}

		public virtual ZPropertyInfo this[string propertyName]
		{
			get { return GetProperty(propertyName, true); }
		}

		public virtual ZPropertyInfo GetPropertySafe(string propertyName)
		{
			return GetProperty(propertyName, false);
		}

		ZPropertyInfo GetProperty(string propertyName, bool throwOnError)
		{
			ZPropertyInfo result = null;
			PropertyDescriptor infoPropertyDescriptor;
			PropertyNamesToPropertyDescriptors.TryGetValue(propertyName, out infoPropertyDescriptor);

			if (infoPropertyDescriptor == null)
			{
				infoPropertyDescriptor = businessObject.GetProperties()[propertyName + "Info"];
				if (infoPropertyDescriptor == null)
				{
					if (throwOnError)
					{
						throw new ArgumentException("PropertyInfo for '" + propertyName + "' does not exist on object of type '" + businessObject.GetType().FullName + "'");
					}
					return null;
				}
				result = (ZPropertyInfo)infoPropertyDescriptor.GetValue(businessObject);

				PropertyDescriptorCollection properties = ZCustomTypeDescriptor.GetProperties(businessObject.GetType());
				AddInfoPropertyToDictionary(PropertyNamesToPropertyDescriptors, properties, infoPropertyDescriptor);
				if (result is ZWrappedPropertyInfo)
				{
					AddInfoPropertyToDictionary(WrappingOnlyPropertyNamesToPropertyDescriptors, properties, infoPropertyDescriptor);
				}
			}
			else
			{
				result = (ZPropertyInfo)infoPropertyDescriptor.GetValue(businessObject);
			}
			return result;
		}

		public bool ContainsKey(string key)
		{
			return GetPropertySafe(key) != null;
		}

		public int Count
		{
			get { return PropertyNamesToPropertyDescriptors.Count; }
		}

		#region GetPropertyInfos

		public IEnumerable GetPropertyInfos(PropertyInfoTypes filter)
		{
			IEnumerable<PropertyDescriptor> properties = filter == PropertyInfoTypes.Wrapping ? WrappingOnlyPropertyNamesToPropertyDescriptors.Values : PropertyNamesToPropertyDescriptors.Values;
			properties = new List<PropertyDescriptor>(properties); //so that if the original Dictionary is modified by an insertion the enumeration doesn't throw
			foreach (PropertyDescriptor propertyDescriptor in properties)
			{
				ZPropertyInfo info = ZPropertyInfoFromPropertyDescriptor(propertyDescriptor);
				if (Matches(info, filter))
				{
					yield return info;
				}
			}
		}

		static bool Matches(ZPropertyInfo property, PropertyInfoTypes filter)
		{
			return
				((filter & PropertyInfoTypes.Wrapping) == PropertyInfoTypes.Wrapping && property is ZWrappedPropertyInfo) ||
				((filter & PropertyInfoTypes.NonWrapping) == PropertyInfoTypes.NonWrapping && !(property is ZWrappedPropertyInfo));
		}

		ZPropertyInfo ZPropertyInfoFromPropertyDescriptor(PropertyDescriptor propertyDescriptor)
		{
			ZPropertyInfo result;
			try
			{
				result = (ZPropertyInfo)propertyDescriptor.GetValue(businessObject);
				if (result == null)
				{
					throw new InvalidOperationException(propertyDescriptor.Name + " returned null for BusinessObject type: " + businessObject.GetType().FullName);
				}
			}
			catch (TargetException ex)
			{
				string message = (NoResString)"BizObj: <" + businessObject.GetType().FullName + (NoResString)">, Property.ComponentType: <" + propertyDescriptor.ComponentType + (NoResString)">, Property.Name <" + propertyDescriptor.Name + (NoResString)">.";
				throw new TargetException(message, ex);
			}
			catch (InvalidCastException ex)
			{
				string message = (NoResString)"BizObj: <" + businessObject.GetType().FullName + (NoResString)">, Property.ComponentType: <" + propertyDescriptor.ComponentType + (NoResString)">, Property.Name <" + propertyDescriptor.Name + (NoResString)">.";
				throw new InvalidCastException(message, ex);
			}
			return result;
		}

		#endregion

		#region PropertyNamesToPropertyDescriptors

		protected readonly BusinessObject businessObject;
		IDictionary<string, PropertyDescriptor> propertyNamesToPropertyDescriptors;

		static LRUCache<Type, IDictionary<string, PropertyDescriptor>> PropertyNamesToPropertyDescriptorsByType
		{
			get { return propertyNamesToPropertyDescriptorsByType ?? (propertyNamesToPropertyDescriptorsByType = new LRUCache<Type, IDictionary<string, PropertyDescriptor>>(PropertyDescriptorCollectionFactory.NumberOfTypesToStringReferenceCache)); }
		}
		[ThreadStatic]
		static LRUCache<Type, IDictionary<string, PropertyDescriptor>> propertyNamesToPropertyDescriptorsByType;

		protected IDictionary<string, PropertyDescriptor> PropertyNamesToPropertyDescriptors
		{
			get
			{
				if (propertyNamesToPropertyDescriptors == null)
				{
					propertyNamesToPropertyDescriptors = NewPropertyNamesToPropertyDescriptorsDictionary();
				}
				return propertyNamesToPropertyDescriptors;
			}
		}

		protected virtual IDictionary<string, PropertyDescriptor> NewPropertyNamesToPropertyDescriptorsDictionary()
		{
			return GetPropertyNamesToPropertyDescriptors(businessObject.GetType());
		}

		static IDictionary<string, PropertyDescriptor> GetPropertyNamesToPropertyDescriptors(Type bizObjType)
		{
			IDictionary<string, PropertyDescriptor> result;
			if (!PropertyNamesToPropertyDescriptorsByType.TryGetValue(bizObjType, out result))
			{
				PropertyDescriptorCollection propertiesAsDotNetCollection = ZCustomTypeDescriptor.GetProperties(bizObjType, true);
				result = new Dictionary<string, PropertyDescriptor>(propertiesAsDotNetCollection.Count);
				foreach (PropertyDescriptor property in propertiesAsDotNetCollection)
				{
					if (property == null)
					{
						ErrorReporter.ReportOnce("AddInfoPropertyToDictionary_PropertyIsNull", "Property is null in AddInfoPropertyToDictionary() for component of type " + bizObjType.FullName);
					}
					else if (property.Name == null)
					{
						ErrorReporter.ReportOnce("AddInfoPropertyToDictionary_PropertyNameIsNull", "Property.Name is null in AddInfoPropertyToDictionary() for component of type " + bizObjType.FullName);
					}
					else
					{
						AddInfoPropertyToDictionary(result, propertiesAsDotNetCollection, property);
					}
				}
				PropertyNamesToPropertyDescriptorsByType.Add(bizObjType, result);
			}
			return result;
		}

		static void AddInfoPropertyToDictionary(IDictionary<string, PropertyDescriptor> dictionary, PropertyDescriptorCollection collection, PropertyDescriptor property)
		{
			if (typeof(ZPropertyInfo).IsAssignableFrom(property.PropertyType))
			{
				string boundPropertyName = property.Name.Remove(property.Name.Length - 4, 4); // remove "Info"
				string internedBoundPropertyName = string.Intern(boundPropertyName);
				if (collection[internedBoundPropertyName] != null)
				{
					dictionary[internedBoundPropertyName] = property;
				}
			}
		}

		#endregion

		#region WrappedOnlyPropertyNamesToPropertyDescriptors

		protected IDictionary<string, PropertyDescriptor> WrappingOnlyPropertyNamesToPropertyDescriptors
		{
			get { return GetWrappingOnlyPropertyNamesToPropertyDescriptors(PropertyNamesToPropertyDescriptors); }
		}

		IDictionary<string, PropertyDescriptor> GetWrappingOnlyPropertyNamesToPropertyDescriptors(IDictionary<string, PropertyDescriptor> properties)
		{
			IDictionary<string, PropertyDescriptor> result = WrappedOnlyPropertyNamesToPropertyDescriptorsByType[properties];
			if (result == null)
			{
				result = new Dictionary<string, PropertyDescriptor>();
				foreach (KeyValuePair<string, PropertyDescriptor> property in properties)
				{
					ZPropertyInfo propertyInfo = ZPropertyInfoFromPropertyDescriptor(property.Value);
					if (propertyInfo is ZWrappedPropertyInfo)
					{
						result.Add(property.Key, property.Value);
					}
				}
				WrappedOnlyPropertyNamesToPropertyDescriptorsByType.Add(properties, result);
			}
			return result;
		}

		static LRUCache<IDictionary<string, PropertyDescriptor>, IDictionary<string, PropertyDescriptor>> WrappedOnlyPropertyNamesToPropertyDescriptorsByType
		{
			get { return wrappedOnlyPropertyNamesToPropertyDescriptorsByType ?? (wrappedOnlyPropertyNamesToPropertyDescriptorsByType = new LRUCache<IDictionary<string, PropertyDescriptor>, IDictionary<string, PropertyDescriptor>>(PropertyDescriptorCollectionFactory.NumberOfTypesToStringReferenceCache)); }
		}

		[ThreadStatic]
		static LRUCache<IDictionary<string, PropertyDescriptor>, IDictionary<string, PropertyDescriptor>> wrappedOnlyPropertyNamesToPropertyDescriptorsByType;

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetPropertyInfos(PropertyInfoTypes.All).GetEnumerator();
		}

		#endregion
	}
}
