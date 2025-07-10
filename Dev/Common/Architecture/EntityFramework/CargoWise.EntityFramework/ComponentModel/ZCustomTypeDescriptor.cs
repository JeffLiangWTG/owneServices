using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Common.Collections;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	[IncludeOnlyNamedPropertiesForPropertyDescriptorReflection]
	public abstract class ZCustomTypeDescriptor : ICustomTypeDescriptor, ITypedList
	{
		#region GetProperties

		KPropertyDescriptorCollection properties;
		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<Type, KPropertyDescriptorCollection> instances = new LRUCache<Type, KPropertyDescriptorCollection>(PropertyDescriptorCollectionFactory.NumberOfTypesToStringReferenceCache);

		public static void RemoveFromCache(Type componentType)
		{
			if (componentType != null)
			{
				instances.Remove(componentType);
			}
		}

		public static KPropertyDescriptorCollection GetProperties(Type componentType)
		{
			return GetProperties(componentType, false);
		}

		internal static KPropertyDescriptorCollection GetProperties(Type componentType, bool ignoreTypeDecider)
		{
			Type bizOType = ignoreTypeDecider ? componentType : TypeDecider.GetTypeForBinding(componentType);
			KPropertyDescriptorCollection result = instances[bizOType];

			if (result == null)
			{
				result = GetNewCollectionFromAttribute(bizOType);
				if (result == null)
				{
					result = PropertyDescriptorCollectionWithWrappingProperties.FromType(bizOType);
				}
				instances.Add(bizOType, result);
			}

			return result;
		}

		protected internal KPropertyDescriptorCollection GetProperties()
		{
			if (properties == null)
			{
				var dynamicBusinessObject = this as IDynamicBusinessObject;
				properties =
					dynamicBusinessObject != null ?
					new DynamicBusinessObjectPropertyDescriptorCollection(dynamicBusinessObject) :
					GetProperties(GetType(), true);
			}
			return properties;
		}

		internal void RefreshDynamicBusinessObjectPropertyDescriptorCollection()
		{
			if (properties is DynamicBusinessObjectPropertyDescriptorCollection dynamicPropertyDescriptorCollection)
			{
				dynamicPropertyDescriptorCollection.RefreshDynamicPropertyDescriptorCollectionFromBusinessObject(this as IDynamicBusinessObject);
			}
		}

		#endregion

		#region GetNewCollectionFromAttribute

		static KPropertyDescriptorCollection GetNewCollectionFromAttribute(Type componentType)
		{
			KPropertyDescriptorCollection result = null;
			PropertyDescriptorCollectionAttribute attribute = (PropertyDescriptorCollectionAttribute)TypeDescriptor.GetAttributes(componentType)[typeof(PropertyDescriptorCollectionAttribute)];
			if (attribute != null)
			{
				MethodInfo fromTypeMethod = attribute.CollectionType.GetMethod("FromType",
					BindingFlags.Public | BindingFlags.Static,
					null,
					new Type[] { typeof(Type) },
					null);

				if (fromTypeMethod != null)
				{
					result = (KPropertyDescriptorCollection)fromTypeMethod.Invoke(null, new object[] { componentType });
				}
				else
				{
					throw new InvalidOperationException("PropertyDescriptorCollection type: " + attribute.CollectionType.FullName + " should have static method FromType(Type componentType).");
				}
			}

			return result;
		}

		#endregion

		#region ICustomTypeDescriptor

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return GetProperties();
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return GetProperties();
		}

		#region Delegating to System.ComponentModel.TypeDescriptor

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this, true);
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(this, true);
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(this, true);
		}

		#endregion

		#endregion

		#region ITypedList

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			return GetItemProperties(listAccessors);
		}

		protected virtual PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			Type componentType = GetTypeFromListAccessors(listAccessors);
			Type listComponentType = null;

			if (typeof(IBusinessObjectCollection).IsAssignableFrom(componentType))
			{
				listComponentType = BusinessObjectCollection.GetElementTypeFromCollectionType(componentType);
			}
			else if (componentType.Name.EndsWith((NoResString)"Collection") && typeof(IList).IsAssignableFrom(componentType))
			{
				listComponentType = ListUtil.GetListElementType(componentType);
			}

			if (listComponentType != null)
			{
				componentType = listComponentType;
			}

			if (componentType == null)
			{
				throw new Exception("Could not derive component type");
			}

			if (typeof(IDynamicBusinessObject).IsAssignableFrom(componentType))
			{
				IDynamicBusinessObject dynamicBusinessObject = null;
				if (componentType.Equals(GetType()))
				{
					dynamicBusinessObject = (IDynamicBusinessObject)this;
				}
				else if (listAccessors != null && listAccessors.Length == 1 && typeof(IDynamicBusinessObjectCollection).IsAssignableFrom(listAccessors[0].PropertyType))
				{
					IDynamicBusinessObjectCollection collection = listAccessors[0].GetValue(this) as IDynamicBusinessObjectCollection;
					if (collection != null)
					{
						dynamicBusinessObject = collection.Template;
					}
				}
				else if (typeof(IDynamicBusinessObjectCollection).IsAssignableFrom(GetType()))
				{
					dynamicBusinessObject = ((IDynamicBusinessObjectCollection)this).Template;
				}

				if (dynamicBusinessObject != null)
				{
					return new DynamicBusinessObjectPropertyDescriptorCollection(dynamicBusinessObject);
				}
			}

			return GetProperties(componentType);
		}

		string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
		{
			return "BusinessObjectCollection";
		}

		Type GetTypeFromListAccessors(PropertyDescriptor[] listAccessors)
		{
			bool childListAccessor = (listAccessors == null || listAccessors.Length == 0);
			return childListAccessor ? GetType() : listAccessors[listAccessors.Length - 1].PropertyType;
		}

		#endregion
	}
}
