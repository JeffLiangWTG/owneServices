using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Common.Collections;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Locates value and meta-data PropertyDescriptors for bindable controls and components.
	/// </summary>
	public class BindableComponentMetaDataPropertyLocator
	{
		protected BindableComponentMetaDataPropertyLocator(Type bindableComponentType)
		{
			Argument.NotNull(bindableComponentType, nameof(bindableComponentType));
			BindableComponentType = bindableComponentType;
		}

		public static BindableComponentMetaDataPropertyLocator GetInstance(Type bindableComponentType)
		{
			Argument.NotNull(bindableComponentType, nameof(bindableComponentType)); // Suggested By ReviewBot 
			if (instances == null)
			{
				instances = new LRUCache<Type, BindableComponentMetaDataPropertyLocator>();
			}

			var result = instances[bindableComponentType];
			if (result == null)
			{
				result = new BindableComponentMetaDataPropertyLocator(bindableComponentType);
				instances.Add(bindableComponentType, result);
			}
			return result;
		}
		[ThreadStatic]
		static LRUCache<Type, BindableComponentMetaDataPropertyLocator> instances;

		/// <summary>
		/// Get the control meta data property accessor for the given meta-data type for the default bindable property.
		/// </summary>
		public static PropertyDescriptor GetDefaultMetaDataProperty(Type controlType, string metaDataTypeId)
		{
			Argument.NotNull(controlType, nameof(controlType));
			Argument.NotNull(metaDataTypeId, nameof(metaDataTypeId)); // Suggested By ReviewBot 
			PropertyDescriptor result = null;
			var properties = GetInstance(controlType).DefaultControlMetaDataProperties;
			properties.TryGetValue(metaDataTypeId, out result);
			return result;
		}

		/// <summary>
		/// Get the PropertyDescriptor on the control that accesses the value being edited.
		/// </summary>
		public PropertyDescriptor DefaultBindingProperty
		{
			get
			{
				if (!defaultBindingPropertyPopulated)
				{
					var attributes = TypeDescriptor.GetAttributes(BindableComponentType);
					var valuePropertyAttribute = (DefaultBindingPropertyAttribute)attributes[typeof(DefaultBindingPropertyAttribute)];
					var valuePropertyName = valuePropertyAttribute == null ? null : valuePropertyAttribute.Name;
					defaultBindingProperty = valuePropertyName == null ? null : TypeDescriptor.GetProperties(BindableComponentType)[valuePropertyName];
					defaultBindingPropertyPopulated = true;
				}
				return defaultBindingProperty;
			}
		}
		PropertyDescriptor defaultBindingProperty;
		bool defaultBindingPropertyPopulated;

		/// <summary>
		/// Get the control meta data property accessors for the default bindable property.
		/// </summary>
		public IDictionary<string, PropertyDescriptor> DefaultControlMetaDataProperties
		{
			get
			{
				if (defaultControlMetaDataProperties == null)
				{
					var controlValueProperty = DefaultBindingProperty ?? TypeDescriptor.GetProperties(BindableComponentType)["DataSource"];

					if (controlValueProperty == null)
					{
						defaultControlMetaDataProperties = new Dictionary<string, PropertyDescriptor>();
					}
					else
					{
						defaultControlMetaDataProperties = GetControlMetaDataProperties(controlValueProperty);
					}
				}
				return defaultControlMetaDataProperties;
			}
		}
		IDictionary<string, PropertyDescriptor> defaultControlMetaDataProperties;

		/// <summary>
		/// Get the control meta data property accessors for a bindable property.
		/// </summary>
		public IDictionary<string, PropertyDescriptor> GetControlMetaDataProperties(PropertyDescriptor controlValueProperty)
		{
			Argument.NotNull(controlValueProperty, nameof(controlValueProperty));

			IDictionary<string, PropertyDescriptor> result = null;
			if (!MetaControlDataProperties.TryGetValue(controlValueProperty, out result))
			{
				lock (MetaControlDataProperties)
				{
					if (!MetaControlDataProperties.TryGetValue(controlValueProperty, out result))
					{
						result = GetControlMetaDataPropertiesUncached(controlValueProperty);
						MetaControlDataProperties[controlValueProperty] = result;
					}
				}
			}
			return result;
		}
		readonly Dictionary<PropertyDescriptor, IDictionary<string, PropertyDescriptor>> MetaControlDataProperties = new Dictionary<PropertyDescriptor, IDictionary<string, PropertyDescriptor>>();

		/// <summary>
		/// Get whether a control property requires Binding.FormattingEnabled to be set to true
		/// when bound to a data source.
		/// </summary>
		public BindingOptionsAttribute GetBindingOptions(PropertyDescriptor property)
		{
			Argument.NotNull(property, nameof(property)); // Suggested By ReviewBot 
			BindingOptionsAttribute result;
			if (!BindingOptions.TryGetValue(property, out result))
			{
				var attributes = property.Attributes;
				var attribute = (BindingOptionsAttribute)attributes[typeof(BindingOptionsAttribute)];
				result = attribute ?? BindingOptionsAttribute.Default;
				BindingOptions[property] = result;
			}
			return result;
		}
		readonly Dictionary<PropertyDescriptor, BindingOptionsAttribute> BindingOptions = new Dictionary<PropertyDescriptor, BindingOptionsAttribute>();

		#region Implementation

		readonly Type BindableComponentType;

		IDictionary<string, PropertyDescriptor> GetControlMetaDataPropertiesUncached(PropertyDescriptor valueProperty)
		{
			Argument.NotNull(valueProperty, nameof(valueProperty));

			var propertyDescriptorCollection = TypeDescriptor.GetProperties(BindableComponentType);
			var result = new Dictionary<string, PropertyDescriptor>();
			foreach (BindingMetaDataPropertyAttribute attribute in valueProperty.GetAttributesAllowMultiple(typeof(BindingMetaDataPropertyAttribute)))
			{
				if (attribute.Enabled)
				{
					if (result.ContainsKey(attribute.MetaDataTypeId))
					{
						throw new InvalidOperationException(
							"You can't apply " +
							nameof(BindingMetaDataPropertyAttribute) +
							" with the same meta data identifier to more than 1 property (on " +
							BindableComponentType.FullName + ")");
					}

					PropertyDescriptor metaDataProperty = null;
					try
					{
						metaDataProperty = propertyDescriptorCollection[attribute.MetaDataPropertyName];
					}
					catch (NullReferenceException)
					{
						propertyDescriptorCollection = GetNewProperties(propertyDescriptorCollection);
						metaDataProperty = propertyDescriptorCollection[attribute.MetaDataPropertyName];
					}

					if (metaDataProperty == null)
					{
						throw new InvalidOperationException("Could not find control meta-data property '" + attribute.MetaDataPropertyName + "'");
					}
					result.Add(attribute.MetaDataTypeId, metaDataProperty);
				}
			}
			return result;
		}

		PropertyDescriptorCollection GetNewProperties(PropertyDescriptorCollection props)
		{
			PropertyDescriptor[] propArray = new PropertyDescriptor[props.Count];
			for (int i = 0; i < props.Count; i++)
			{
				propArray[i] = TypeDescriptor.CreateProperty(props[i].ComponentType, props[i], new CategoryAttribute(props[i].PropertyType.Name));
			}
			return new PropertyDescriptorCollection(propArray);
		}

		#endregion
	}
}
