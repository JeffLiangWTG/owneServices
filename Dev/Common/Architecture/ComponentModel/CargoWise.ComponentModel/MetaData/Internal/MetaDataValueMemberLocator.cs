using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Common.Testing;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Helper class to locate the meta-data values and accessor members from attributes.
	/// </summary>
	[ImmutableObject(true)] // and THREAD SAFE
	[EditorBrowsable(EditorBrowsableState.Never)]
	class MetaDataValueMemberLocator
	{
		const string DotString = ".";
		const string PlusString = "+";

		#region GetInstance

		protected MetaDataValueMemberLocator(PropertyDescriptor property, string metaDataTypeId)
		{
			Argument.NotNull(property, nameof(property));
			PropertyDescriptor = property;
			MetaDataTypeId = metaDataTypeId;
		}

		public static MetaDataValueMemberLocator GetInstance(PropertyDescriptor property, string metaDataTypeId)
		{
			Argument.NotNull(property, nameof(property));
			var locatorsForProperty = Locators[property];
			if (locatorsForProperty == null)
			{
				locatorsForProperty = new List<MetaDataValueMemberLocator>(5);
				Locators.Add(property, locatorsForProperty);
			}
			else
			{
				foreach (var locator in locatorsForProperty)
				{
					if (locator.MetaDataTypeId == metaDataTypeId)
					{
						return locator;
					}
				}
			}
			locatorsForProperty = new List<MetaDataValueMemberLocator>(locatorsForProperty);
			var metaDataValueMemberLocator = new MetaDataValueMemberLocator(property, metaDataTypeId);
			locatorsForProperty.Add(metaDataValueMemberLocator);
			Locators.Add(property, locatorsForProperty);
			return metaDataValueMemberLocator;
		}

		static LRUCache<PropertyDescriptor, List<MetaDataValueMemberLocator>> Locators
		{
			get
			{
				if (locators == null)
				{
					locators = CreateLocatorsCacheForThisThread();
				}
				return locators;
			}
		}
		[ThreadStatic]
		static LRUCache<PropertyDescriptor, List<MetaDataValueMemberLocator>> locators;

		static LRUCache<PropertyDescriptor, List<MetaDataValueMemberLocator>> CreateLocatorsCacheForThisThread()
		{
			return new LRUCache<PropertyDescriptor, List<MetaDataValueMemberLocator>>(new PropertyDescriptorEqualityComparer(), PropertyDescriptorCollectionFactory.NumberOfTypesToStringReferenceCache);
		}

		class PropertyDescriptorEqualityComparer : EqualityComparer<PropertyDescriptor>
		{
			public override bool Equals(PropertyDescriptor x, PropertyDescriptor y)
			{
				return x != null && y != null && x.ComponentType == y.ComponentType && x.Name == y.Name;
			}

			public override int GetHashCode(PropertyDescriptor obj)
			{
				return obj != null ? obj.GetHashCode() : -1;
			}
		}

		#endregion

		#region GetMetaDataValue

		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<ComponentTypeAndMetaDataType, MetaDataValueHolder> metaDataComponentTypeValues = new LRUCache<ComponentTypeAndMetaDataType, MetaDataValueHolder>();

		public static object GetMetaDataValue(Type componentType, string metaDataTypeId)
		{
			var key = new ComponentTypeAndMetaDataType(componentType, metaDataTypeId);
			var result = metaDataComponentTypeValues[key];
			if (result == null)
			{
				result = new MetaDataValueHolder();
				var found = false;
				var attributes = AttributeUtil.FindAttributes(componentType, typeof(MetaDataBaseAttribute));
				foreach (MetaDataBaseAttribute attribute in attributes)
				{
					if (attribute != null && attribute.ProvidesMetaDataValue(metaDataTypeId))
					{
						result.Value = attribute.GetMetaDataValue(metaDataTypeId);
						found = true;
					}
				}
				if (!found)
				{
					var metaDataType = MetaDataType.GetMetaDataType(metaDataTypeId);
					result.Value = metaDataType.DefaultValue;
				}
				metaDataComponentTypeValues.Add(key, result);
			}
			return result.Value;
		}

		class ComponentTypeAndMetaDataType
		{
			public ComponentTypeAndMetaDataType(Type componentType, string metaDataTypeId)
			{
				ComponentType = componentType;
				MetaDataTypeId = metaDataTypeId;
			}

			public override bool Equals(object obj)
			{
				var rhs = obj as ComponentTypeAndMetaDataType;
				return rhs != null && ComponentType == rhs.ComponentType && MetaDataTypeId == rhs.MetaDataTypeId;
			}

			public override int GetHashCode()
			{
				return ComponentType != null && MetaDataTypeId != null ? ComponentType.GetHashCode() ^ MetaDataTypeId.GetHashCode() : -1;
			}

			readonly Type ComponentType;
			readonly string MetaDataTypeId;
		}

		class MetaDataValueHolder
		{
			public object Value;
		}

		#endregion

		/// <summary>
		/// Get the PropertyDescriptor this instance relates to.
		/// </summary>
		public PropertyDescriptor PropertyDescriptor { get; private set; }

		/// <summary>
		/// Get the meta-data type id this instance relates to.
		/// </summary>
		public string MetaDataTypeId { get; private set; }

		/// <summary>
		/// Is the meta-data type with the given ID specified on the given property?
		/// </summary>
		public bool IsSpecified
		{
			get { return MetaDataMembers.Length > 0 || MetaDataValues.Length > 0; }
		}

		#region MetaDataValues

		/// <summary>
		/// Get an array of the attribute-specified values of a meta-data type. This will only
		/// return > 1 value if the meta-data is configured incorrectly.
		/// </summary>
		public object[] MetaDataValues
		{
			get
			{
				return metaDataValues ?? (metaDataValues = FindMetaDataValues(true));
			}
		}
		object[] metaDataValues;

		object[] FindMetaDataValues(bool lookOnDataSourceList)
		{
			var result = new List<object>();
			foreach (var attribute in Attributes)
			{
				var readOnlyAttribute = attribute as ReadOnlyAttribute;
				var metaDataAttribute = attribute as MetaDataBaseAttribute;
				if (metaDataAttribute != null && metaDataAttribute.ProvidesMetaDataValue(MetaDataTypeId))
				{
					result.Add(metaDataAttribute.GetMetaDataValue(MetaDataTypeId));
				}
				else if (readOnlyAttribute != null && MetaDataTypeId == MetaDataTypes.ReadOnly)
				{
					result.Add(readOnlyAttribute.IsReadOnly);
				}
			}

			if (result.Count == 0 && lookOnDataSourceList)
			{
				var metaDataTypes = MetaDataType.GetRegisteredMetaDataTypes();
				foreach (var metaDataType in metaDataTypes)
				{
					if (typeof(IList).IsAssignableFrom(metaDataType.DataType))
					{
						result.AddRange(FindMetaDataValuesOnDataSourceList(PropertyDescriptor, MetaDataTypeId, metaDataType.Id));
					}
				}
			}
			return result.Count > 0 ? result.ToArray() : emptyArray;
		}

		[ThreadSafe]
		static readonly object[] emptyArray = Array.Empty<object>();

		static object[] FindMetaDataValuesOnDataSourceList(PropertyDescriptor property, string metaDataTypeId, string listMetaDataTypeId)
		{
			Argument.NotNull(property, nameof(property));
			var result = new List<object>();
			var elementType = MetaDataValueMemberLocator.GetInstance(property, listMetaDataTypeId).ListElementType;
			if (elementType != null)
			{
				foreach (var attribute in GetAttributesForType(elementType, metaDataTypeId))
				{
					var metaDataAttribute = attribute as MetaDataBaseAttribute;
					if (metaDataAttribute != null && metaDataAttribute.ProvidesMetaDataValue(metaDataTypeId))
					{
						result.Add(metaDataAttribute.GetMetaDataValue(metaDataTypeId));
					}
				}
			}
			return result.ToArray();
		}

		#endregion

		#region MetaDataMembers

		/// <summary>
		/// Get an array of the attribute-specified members specifying a meta-data type. This
		/// will only return > 1 value if the meta-data is configured incorrectly.
		/// </summary>
		public string[] MetaDataMembers
		{
			get
			{
				if (metaDataMembers == null)
				{
					var list = new List<string>();
					foreach (var attribute in Attributes)
					{
						var metaDataAttribute = attribute as MetaDataBaseAttribute;
						if (metaDataAttribute != null && metaDataAttribute.ProvidesMetaDataMember(MetaDataTypeId))
						{
							var metaDataMember = metaDataAttribute.GetMetaDataMember(MetaDataTypeId);
							if (metaDataMember != null)
							{
								metaDataMember = metaDataMember.Replace('.', '+');
								list.Add(metaDataMember);
							}
						}
					}
					if (list.Count == 0)
					{
						AddDefaultMetaDataMemberIfExist(list);
					}
					metaDataMembers = list.Count > 0 ? list.ToArray() : emptyStringArray;
				}
				return metaDataMembers;
			}
		}
		string[] metaDataMembers;

		[ThreadSafe]
		static readonly string[] emptyStringArray = Array.Empty<string>();

		void AddDefaultMetaDataMemberIfExist(List<string> dataMembers)
		{
			Argument.NotNull(dataMembers, nameof(dataMembers));
			var metaDataType = MetaDataType.TryGetMetaDataType(MetaDataTypeId);
			if (metaDataType != null)
			{
				var memberName = string.Concat(PropertyDescriptor.Name, "_", MetaDataTypeId);
				var properties = GetAllProperties();
				if (properties != null)
				{
					var metaDataProperty = properties[memberName];
					if (metaDataProperty != null && metaDataProperty.PropertyType == metaDataType.DataType)
					{
						dataMembers.Add(memberName);
					}
				}
			}
		}

		#endregion

		#region IsMetaDataMemberMoreSpecificThanValue

		public bool IsMetaDataMemberMoreSpecificThanValue
		{
			get
			{
				if (isMetaDataMemberMoreSpecificThanValue == null)
				{
					isMetaDataMemberMoreSpecificThanValue = IsMetaDataMemberMoreSpecificThanValueCore();
				}
				return isMetaDataMemberMoreSpecificThanValue.Value;
			}
		}
		bool? isMetaDataMemberMoreSpecificThanValue;

		bool IsMetaDataMemberMoreSpecificThanValueCore()
		{
			var properties = GetAllProperties();
			if (properties != null && properties[PropertyDescriptor.Name + "_" + MetaDataTypeId] != null)
			{
				return true;
			}
			foreach (var attribute in Attributes)
			{
				var metaDataAttribute = attribute as MetaDataBaseAttribute;
				if (metaDataAttribute != null && metaDataAttribute.ProvidesMetaDataMember(MetaDataTypeId))
				{
					return true;
				}
				if (ProvidesMetaData(attribute, MetaDataTypeId) && (metaDataAttribute == null || metaDataAttribute.ProvidesMetaDataValue(MetaDataTypeId)))
				{
					return false;
				}
			}
			return false;
		}

		#endregion

		#region ListElementType

		/// <summary>
		/// Find the element type of the collection returned by the list meta-data type.
		/// </summary>
		public Type ListElementType
		{
			get
			{
				if (listElementType == null)
				{
					listElementType = typeof(object);
					if (MetaDataMembers.Length == 1)
					{
						var listMember = MetaDataMembers[0];
						var allProperties = GetAllProperties();
						if (allProperties != null)
						{
							var listProperty = allProperties[listMember];
							if (listProperty != null)
							{
								var collectionType = listProperty.PropertyType;
								if (collectionType != null)
								{
									listElementType = ListUtil.GetListElementType(collectionType);
								}
							}
						}
					}

					var listValues = FindMetaDataValues(false);
					if (listValues.Length > 0)
					{
						var listValue = (IList)listValues[0];
						if (listValue != null)
						{
							listElementType = ListUtil.GetListElementType(listValue.GetType());
						}
					}
				}
				return listElementType;
			}
		}
		Type listElementType;

		#endregion

		#region Implementation

		KPropertyDescriptorCollection GetAllProperties()
		{
			KPropertyDescriptorCollection result = null;
			var kPropertyDescriptor = PropertyDescriptor as KPropertyDescriptor;
			if (kPropertyDescriptor != null && kPropertyDescriptor.Collection != null)
			{
				result = kPropertyDescriptor.Collection.AllProperties;
			}
			else if (PropertyDescriptor.ComponentType != null)
			{
				result = KPropertyDescriptorCollection.FromType(PropertyDescriptor.ComponentType, true);
			}
			return result;
		}

		Attribute[] Attributes
		{
			get
			{
				if (attributes == null)
				{
					var list = new List<Attribute>();
					list.AddRange(AttributesOnProperty);
					if (list.Count == 0)
					{
						list.AddRange(GetAttributesForType(PropertyDescriptor.PropertyType, MetaDataTypeId));
					}
					attributes = list.ToArray();
				}
				return attributes;
			}
		}
		Attribute[] attributes;

		IEnumerable<Attribute> AttributesOnProperty
		{
			get
			{
				var found = false;
				var current = PropertyDescriptor.ComponentType;
				while (!found && current != null)
				{
					var property = current.GetProperty(PropertyDescriptor.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					if (property != null)
					{
						foreach (Attribute attribute in property.GetCustomAttributes(typeof(Attribute), false))
						{
							if (ProvidesMetaData(attribute, MetaDataTypeId))
							{
								yield return attribute;
								found = true;
							}
						}
					}
					current = current.BaseType;
				}
				if (!found && !IsThisPropertyBelongsToChild(PropertyDescriptor))
				{
					foreach (Attribute attribute in PropertyDescriptor.Attributes)
					{
						if (ProvidesMetaData(attribute, MetaDataTypeId))
						{
							yield return attribute;
						}
					}
				}
			}
		}

		bool IsThisPropertyBelongsToChild(PropertyDescriptor propertyDescriptor) => propertyDescriptor.Name.Contains(DotString) || propertyDescriptor.Name.Contains(PlusString);

		static IEnumerable<Attribute> GetAttributesForType(Type type, string metaDataTypeId)
		{
			var found = false;
			var current = type;
			while (!found && current != null)
			{
				foreach (Attribute attribute in AttributeUtil.FindAttributes(current, typeof(Attribute)))
				{
					if (ProvidesMetaData(attribute, metaDataTypeId))
					{
						yield return attribute;
						found = true;
					}
				}
				current = current.BaseType;
			}
		}

		static bool ProvidesMetaData(Attribute attribute, string metaDataTypeId)
		{
			var metaDataAttribute = attribute as MetaDataBaseAttribute;
			var readOnlyAttribute = attribute as ReadOnlyAttribute;
			return
				(metaDataAttribute != null && metaDataAttribute.ProvidesMetaData(metaDataTypeId)) ||
				(readOnlyAttribute != null && metaDataTypeId == MetaDataTypes.ReadOnly);
		}

		#endregion
	}
}
