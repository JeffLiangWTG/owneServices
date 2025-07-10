using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A collection for KPropertyDescriptor objects with lazy-created meta-data accessing KPropertyDescriptorS.
	/// </summary>
	public class PropertyDescriptorCollectionWithMetaData : KPropertyDescriptorCollection
	{
		#region Constants

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Postfix character")]
		const string ExcludingMethodProviderPropertyNamePostfix = "\u9C7C";// "__excludingMethodProvider"; this REDUCES memory usage (code for fish - just because)
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Separator character")]
		public const string MetaDataPropertyNameSeparator = "\u611B";// __metadataproperty__"; this REDUCES memory usage (code for love - just because)

		#endregion

		#region Factory Method

		protected internal PropertyDescriptorCollectionWithMetaData(Type componentType, bool includePrivate)
			: base(componentType, includePrivate)
		{
			Argument.NotNull(componentType, nameof(componentType));
		}

		public new static PropertyDescriptorCollectionWithMetaData FromType(Type componentType)
		{
			Argument.NotNull(componentType, nameof(componentType));
			return FromType(componentType, false);
		}

		public new static PropertyDescriptorCollectionWithMetaData FromType(Type componentType, bool includePrivate)
		{
			Argument.NotNull(componentType, nameof(componentType));
			return factory.FromType(componentType, includePrivate);
		}

		static readonly PropertyDescriptorCollectionFactory<PropertyDescriptorCollectionWithMetaData> factory = new PropertyDescriptorCollectionFactory<PropertyDescriptorCollectionWithMetaData>(delegate(Type componentType)
		{
			return new PropertyDescriptorCollectionWithMetaData(componentType, false);
		});

		#endregion

		#region KPropertyDescriptorCollection Overrides

		protected override PropertyDescriptor FindCore(string name, bool ignoreCase)
		{
			var result = base.FindCore(name, ignoreCase);
			if (result == null)
			{
				result = FindOrCreatePropertyNotInCollection(name, ignoreCase);
				if (result != null)
				{
					lock (addMutex)
					{
						Add(result);
					}
				}
			}
			return result;
		}

		protected override KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollectionCore(Type componentType, bool includePrivate)
		{
			return new PropertyDescriptorCollectionWithMetaData(componentType, includePrivate);
		}

		protected override KPropertyDescriptorCollection GetPropertyDescriptorCollectionForComponentTypeCore(Type componentType, bool includePrivate)
		{
			return FromType(componentType, includePrivate);
		}

		protected internal override PropertyDescriptor NewPropertyDescriptor(PropertyDescriptor reflectedProperty)
		{
			PropertyDescriptor result = null;
			if (reflectedProperty != null)
			{
				result = new KPropertyDescriptor(this, reflectedProperty);
			}
			return result;
		}

		protected override bool ShouldEnumerateOver(PropertyDescriptor property)
		{
			return !property.Name.Contains(MetaDataPropertyNameSeparator);
		}

		readonly object addMutex = new object();

		#endregion

		#region GetMetaDataPropertyName / GetMetaDataProperty

		protected internal string GetMetaDataPropertyName(PropertyDescriptor property, string metaDataTypeId, bool excludeMethodProvider)
		{
			Argument.NotNull(property, nameof(property)); // Suggested By ReviewBot 
			string result;
			if (property.ComponentType != null && property.ComponentType != ComponentType)
			{
				var collectionForMetaData = (PropertyDescriptorCollectionWithMetaData)GetPropertyDescriptorCollectionForComponentType(property.ComponentType, IncludePrivate);
				result = collectionForMetaData.GetMetaDataPropertyName(property, metaDataTypeId, excludeMethodProvider);
			}
			else
			{
				var members = MetaDataValueMemberLocator.GetInstance(property, metaDataTypeId).MetaDataMembers;
				result = property.Name + MetaDataPropertyNameSeparator + metaDataTypeId;

				if (members.Length > 0 && SupportsStraightThroughMetaDataAccessor(property, members[0], metaDataTypeId))
				{
					result = members[0];
				}
			}
			return result + (excludeMethodProvider ? ExcludingMethodProviderPropertyNamePostfix : "");
		}

		public KPropertyDescriptor GetMetaDataProperty(Type componentType, KPropertyDescriptor property, string metaDataTypeId)
		{
			Argument.NotNull(property, nameof(property));
			return GetMetaDataProperty(componentType, property, metaDataTypeId, false);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal exception message")]
		public KPropertyDescriptor GetMetaDataProperty(Type componentType, KPropertyDescriptor property, string metaDataTypeId, bool excludeMethodProvider)
		{
			Argument.NotNull(property, nameof(property)); // Suggested By ReviewBot 

			var key = new MetaDataKey(componentType, metaDataTypeId, excludeMethodProvider);
			var resultObj = property.UserData[key];
			var result = resultObj as KPropertyDescriptor;

			if (resultObj == null)
			{
				if (componentType == ComponentType)
				{
					result = GetMetaDataPropertyFromThisComponentType(property, metaDataTypeId, excludeMethodProvider);
				}
				else if (componentType != null && property.ComponentType != null && property.ComponentType.IsAssignableFrom(componentType))
				{
					result = GetMetaDataPropertyFromSubclassComponentType(componentType, property, metaDataTypeId, excludeMethodProvider);
				}
				else
				{
					string exceptionMessage = "componentType must be a sub-class of prop.ComponentType. componentType={0}, property.Name={1}, property.ComponentType={2}";
					exceptionMessage = string.Format(exceptionMessage, componentType, property.Name, property.ComponentType);
					throw new ArgumentException(exceptionMessage, nameof(componentType));
				}

				property.UserData[key] = result == null ? DBNull.Value : result;
			}
			return result;
		}

		KPropertyDescriptor GetMetaDataPropertyFromThisComponentType(KPropertyDescriptor property, string metaDataTypeId, bool excludeMethodProvider)
		{
			Argument.NotNull(property, nameof(property)); // Suggested By ReviewBot
			if (property.Collection != null)
			{
				var metaDataPropertyName = GetMetaDataPropertyName(property, metaDataTypeId, excludeMethodProvider);
				return (KPropertyDescriptor)property.Collection[metaDataPropertyName];
			}
			return null;
		}

		KPropertyDescriptor GetMetaDataPropertyFromSubclassComponentType(Type componentType, KPropertyDescriptor property, string metaDataTypeId, bool excludeMethodProvider)
		{
			Argument.NotNull(property, nameof(property)); // Suggested By ReviewBot 
			Argument.NotNull(componentType, nameof(componentType));
			var propertiesForActualComponentType = (PropertyDescriptorCollectionWithMetaData)GetPropertyDescriptorCollectionForComponentType(componentType, IncludePrivate);

			KPropertyDescriptor result;

			var propertyOnSubclass = (KPropertyDescriptor)propertiesForActualComponentType[property.Name];
			if (propertyOnSubclass == null)
			{
				result = GetMetaDataPropertyFromThisComponentType(property, metaDataTypeId, excludeMethodProvider);
			}
			else
			{
				result = propertiesForActualComponentType.GetMetaDataProperty(componentType, propertyOnSubclass, metaDataTypeId, excludeMethodProvider);
			}
			return result;
		}

		struct MetaDataKey
		{
			public MetaDataKey(Type componentType, string metaDataTypeId, bool excludingMethodProvider)
			{
				ComponentType = componentType;
				MetaDataTypeId = metaDataTypeId;
				ExcludingMethodProvider = excludingMethodProvider;
				hashCode = ComponentType != null && MetaDataTypeId != null ? ComponentType.GetHashCode() ^ MetaDataTypeId.GetHashCode() : -1;
			}

			public override bool Equals(object obj)
			{
				return obj is MetaDataKey rhs &&
					ComponentType == rhs.ComponentType &&
					MetaDataTypeId == rhs.MetaDataTypeId &&
					ExcludingMethodProvider == rhs.ExcludingMethodProvider;
			}

			public override int GetHashCode() => hashCode;

			readonly int hashCode;
			readonly Type ComponentType;
			readonly string MetaDataTypeId;
			readonly bool ExcludingMethodProvider;
		}

		#endregion

		#region NewMetaDataProperty / NewMetaDataPropertyFromAttributes

		PropertyDescriptor NewMetaDataProperty(PropertyDescriptor property, string metaDataTypeId, string metaDataPropertyName, bool excludeMethodProvider)
		{
			var result = excludeMethodProvider ? null : FindMethodProvidedProperty(property, metaDataTypeId);
			if (result == null && property != null)
			{
				var metaDataAccessorFromAttributes = FindAttributeSpecifiedMetaDataPropertyDescriptor(property, metaDataTypeId);
				result = NewMetaDataPropertyCore(property, metaDataTypeId, metaDataPropertyName, metaDataAccessorFromAttributes);
			}
			return result;
		}

		protected virtual PropertyDescriptor NewMetaDataPropertyCore(PropertyDescriptor property, string metaDataTypeId, string metaDataPropertyName, PropertyDescriptor metaDataAccessorFromAttributes)
		{
			var result = metaDataAccessorFromAttributes;
			if (metaDataTypeId == MetaDataTypes.ReadOnly && metaDataPropertyName != null && property != null)
			{
				result = new IsReadOnlyPropertyDescriptor(this, metaDataPropertyName, property, result);
			}
			else if (property != null && metaDataTypeId == MetaDataTypes.Notifications && NotificationsPropertyDescriptorWithAttributedNotificationLogic.RequiresThisMetaDataPropertyDescriptor(property))
			{
				if (result != null)
				{
					result = new NotificationsPropertyDescriptorWithAttributedNotificationLogic(this, property, metaDataPropertyName, result);
				}
				else
				{
					result = new NotificationsPropertyDescriptorWithAttributedNotificationLogic(this, property, metaDataPropertyName);
				}
			}
			else if (result != null && metaDataPropertyName != result.Name)
			{
				result = new MetaDataAccessorPropertyDescriptor(this, result, metaDataPropertyName);
			}
			return result;
		}

		class MetaDataAccessorPropertyDescriptor : KPropertyDescriptor
		{
			public MetaDataAccessorPropertyDescriptor(PropertyDescriptorCollectionWithMetaData collection, PropertyDescriptor inner, string name)
				: base(collection, inner)
			{
				Argument.NotNull(inner, nameof(inner));
				this.name = name;
			}

			public override string Name
			{
				get { return name; }
			}

			readonly string name;
		}

		class WrappedMetadataPropertyDescriptor : MetaDataAccessorPropertyDescriptor, IWrappingPropertyDescriptor
		{
			public WrappedMetadataPropertyDescriptor(PropertyDescriptorCollectionWithMetaData collection, Type componentType, string name, MetaDataType metaDataType, PropertyDescriptor inner, PropertyDescriptor outer)
				: base(collection, inner, name)
			{
				Argument.NotNull(inner, nameof(inner));
				Argument.NotNull(metaDataType, nameof(metaDataType));
				Outer = outer;
				this.componentType = componentType;
				this.metaDataType = metaDataType;
			}

			public PropertyDescriptor Outer { get; private set; }
			readonly Type componentType;
			readonly MetaDataType metaDataType;

			protected override object GetValueCore(object component)
			{
				var outerComponent = component != null && component != DBNull.Value ? Outer.GetValue(component) : null;
				return Inner != null && outerComponent != null && outerComponent != DBNull.Value ? Inner.GetValue(outerComponent) : metaDataType.DefaultValue;
			}

			public override Type PropertyType
			{
				get { return metaDataType.DataType; }
			}

			public override Type ComponentType
			{
				get { return componentType; }
			}
		}

		#endregion

		#region SupportsStraightThroughMetaDataAccessor / FindOrCreatePropertyNotInCollection

		protected virtual bool SupportsStraightThroughMetaDataAccessor(PropertyDescriptor property, string member, string metaDataTypeId)
		{
			var isMemberPublic = base[member] != null;
			return isMemberPublic && (metaDataTypeId != MetaDataTypes.Notifications || (property != null && !NotificationsPropertyDescriptorWithAttributedNotificationLogic.RequiresThisMetaDataPropertyDescriptor(property)));
		}

		protected virtual PropertyDescriptor FindOrCreatePropertyNotInCollection(string name, bool ignoreCase)
		{
			Argument.NotNull(name, nameof(name));
			PropertyDescriptor property;
			string metaDataId;
			bool excludeMethodProvider;

			PropertyDescriptor result = null;
			if (ParsePropertyName(name, ignoreCase, out property, out metaDataId, out excludeMethodProvider))
			{
				var metaDataPropertyName = property != null ? GetMetaDataPropertyName(property, metaDataId, excludeMethodProvider) : null;
				result = NewMetaDataProperty(property, metaDataId, metaDataPropertyName, excludeMethodProvider);
			}
			return result;
		}

		bool ParsePropertyName(string name, bool ignoreCase, out PropertyDescriptor property, out string metaDataId, out bool excludeMethodProvider)
		{
			Argument.NotNull(name, nameof(name)); // Suggested By ReviewBot 
			excludeMethodProvider = name.EndsWith(ExcludingMethodProviderPropertyNamePostfix, StringComparison.Ordinal);
			metaDataId = null;
			property = null;
			metaDataId = null;

			var nameAndMetaData = excludeMethodProvider ? name.Substring(0, name.Length - ExcludingMethodProviderPropertyNamePostfix.Length) : name;
			var index = nameAndMetaData.IndexOf(MetaDataPropertyNameSeparator, StringComparison.Ordinal);
			var result = false;
			if (index != -1)
			{
				metaDataId = nameAndMetaData.Substring(index + MetaDataPropertyNameSeparator.Length);
				if (ignoreCase)
				{
					metaDataId = FindMetaDataIdIgnoreCase(metaDataId);
				}
				property = Find(nameAndMetaData.Substring(0, index), ignoreCase);
				result = property != null && metaDataId != null;
			}
			return result;
		}

		#endregion

		#region Implementation

		PropertyDescriptor FindAttributeSpecifiedMetaDataPropertyDescriptor(PropertyDescriptor property, string metaDataTypeId)
		{
			Argument.NotNull(property, nameof(property));

			var wrappingProperty = property as WrappingPropertyDescriptor;
			if (wrappingProperty != null && wrappingProperty.Inner != null && !ReferenceEquals(wrappingProperty.Inner, property))
			{
				var innerMetadataProperty = FindAttributeSpecifiedMetaDataPropertyDescriptor(wrappingProperty.Inner, metaDataTypeId);
				if (innerMetadataProperty != null)
				{
					var propertyName = GetMetaDataPropertyName(property, metaDataTypeId, false);
					var metaDataType = MetaDataType.GetMetaDataType(metaDataTypeId);
					return new WrappedMetadataPropertyDescriptor(this, property.ComponentType, propertyName, metaDataType, innerMetadataProperty, wrappingProperty.Outer);
				}
			}
			else
			{
				var locator = MetaDataValueMemberLocator.GetInstance(property, metaDataTypeId);
				var values = locator.MetaDataValues;
				var members = locator.MetaDataMembers;

				PropertyDescriptor result = null;
				if (values.Length > 0 && values[0] != null && !(locator.IsMetaDataMemberMoreSpecificThanValue && members.Length > 0))
				{
					var propertyName = GetMetaDataPropertyName(property, metaDataTypeId, false);
					result = new ConstantValuePropertyDescriptor(this, propertyName, property.ComponentType, values[0]);
				}
				else if (members.Length > 0 && property.ComponentType != null)
				{
					var properties = property.ComponentType == ComponentType ? AllProperties : FromType(property.ComponentType).AllPropertiesSafe();
					result = properties != null ? properties[members[0]] : null;
				}
				return result;
			}

			return null;
		}

		PropertyDescriptor FindMethodProvidedProperty(PropertyDescriptor property, string metaDataTypeId)
		{
			if (property != null)
			{
				var wrappingProperty = property as WrappingPropertyDescriptor;
				if (wrappingProperty != null && !ReferenceEquals(wrappingProperty.Inner, property))
				{
					var innerMetadataProperty = FindMethodProvidedProperty(wrappingProperty.Inner, metaDataTypeId);
					if (innerMetadataProperty != null)
					{
						var propertyName = GetMetaDataPropertyName(property, metaDataTypeId, false);
						var metaDataType = MetaDataType.GetMetaDataType(metaDataTypeId);
						return new WrappedMetadataPropertyDescriptor(this, property.ComponentType, propertyName, metaDataType, innerMetadataProperty, wrappingProperty.Outer);
					}
				}
				else if (property.ComponentType != null)
				{
					var componentType = property.ComponentType;
					var attribute = ProvideMetaDataPropertyAttribute.FindAttribute(componentType, metaDataTypeId);
					if (attribute != null)
					{
						var propertyName = GetMetaDataPropertyName(property, metaDataTypeId, false);
						var metaDataType = MetaDataType.GetMetaDataType(metaDataTypeId);
						return new MethodProvidedPropertyDescriptor(this, componentType, propertyName, attribute.PropertyName, property, metaDataType);
					}
				}
			}

			return null;
		}

		static string FindMetaDataIdIgnoreCase(string metaDataTypeId)
		{
			var metaDataTypes = MetaDataType.GetRegisteredMetaDataTypes();
			foreach (var metaDataType in metaDataTypes)
			{
				if (metaDataType.Id != null && metaDataType.Id.Equals(metaDataTypeId, StringComparison.OrdinalIgnoreCase))
				{
					return metaDataType.Id;
				}
			}
			return null;
		}

		#endregion
	}
}
