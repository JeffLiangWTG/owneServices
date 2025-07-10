using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	public static class PropertyDescriptorExtensions
	{
		/// <summary>
		/// Get the IExtenderProvider object given the property from an extended component.
		/// </summary>
		public static IExtenderProvider GetExtenderProvider(this PropertyDescriptor property)
		{
			var wrappedProperty = property as KPropertyDescriptor;
			if (wrappedProperty != null && wrappedProperty.Inner != null)
			{
				property = wrappedProperty.Inner;
			}

			if (property != null)
			{
				property = property.UnwrapFromVSDesignerPropertyDescriptor();
			}
			var providerField = GetProviderFieldOfExtendedPropertyDescriptor(property);
			IExtenderProvider result = null;
			if (providerField != null && providerField.DeclaringType != null && providerField.DeclaringType.IsInstanceOfType(property))
			{
				result = (IExtenderProvider)providerField.GetValue(property);
			}
			return result;
		}

		/// <summary>
		/// Get whether the given property is a simple ReflectPropertyDescriptor that gets it's
		/// value from an instance property on a class.
		/// </summary>
		public static bool IsReflectPropertyDescriptor(this PropertyDescriptor property)
		{
			return property is ReflectPropertyDescriptor || (property != null && property.GetType().Name == "ReflectPropertyDescriptor");
		}

		/// <summary>
		/// Unwraps and returns final component property descriptor.
		/// </summary>
		/// <param name="propertyDescriptor">Design-time property descriptor.</param>
		/// <returns>Final property descriptor.</returns>
		/// <remarks>
		/// VS2010 workaround:
		///   incomming PropertyDescriptor is Microsoft.VisualStudio.Shell.Design.VsTargetFrameworkPropertyDescriptor
		///   result is System.ComponentModel.ExtendedPropertyDescriptor
		/// </remarks>
		public static PropertyDescriptor UnwrapFromVSDesignerPropertyDescriptor(this PropertyDescriptor propertyDescriptor)
		{
			Argument.NotNull(propertyDescriptor, nameof(propertyDescriptor)); // Suggested By ReviewBot 
			if (propertyDescriptor.GetType().Name == "VsTargetFrameworkPropertyDescriptor")
			{
				var runtimePropertyField = propertyDescriptor.GetType().GetField("_runtimeProperty", BindingFlags.Instance | BindingFlags.NonPublic);
				if (runtimePropertyField != null)
				{
					propertyDescriptor = (PropertyDescriptor)runtimePropertyField.GetValue(propertyDescriptor) ?? propertyDescriptor;
				}
			}
			return propertyDescriptor;
		}

		#region HasSetter

		/// <summary>
		/// Get whether the PropertyDescriptor is settable.
		/// </summary>
		public static bool HasSetter(this PropertyDescriptor property)
		{
			var result = true;
			var kproperty = property as KPropertyDescriptor;
			if (kproperty != null)
			{
				result = kproperty.HasSetter();
			}
			else if (property != null && property.IsReflectPropertyDescriptor() && property.ComponentType != null)
			{
				var currentType = property.ComponentType;
				PropertyInfo propertyInfo = null;
				do
				{
					propertyInfo = currentType.GetProperty(property.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					currentType = currentType.BaseType;
				}
				while (currentType != null && (propertyInfo == null || propertyInfo.GetSetMethod(true) == null || HasExplicitlyNonPublicSetter(propertyInfo)));

				result = (propertyInfo != null && propertyInfo.GetSetMethod(true) != null);
			}
			return result;
		}

		static bool HasExplicitlyNonPublicSetter(PropertyInfo property)
		{
			Argument.NotNull(property, nameof(property)); // Suggested By ReviewBot 
			var getMethod = property.GetGetMethod(true);
			var setMethod = property.GetSetMethod(true);
			return getMethod != null && setMethod != null && getMethod.IsPublic && !setMethod.IsPublic;
		}

		#endregion

		#region GetAttributeFromMostSpecificComponentType

		/// <summary>
		/// Get the attribute from the property's most specific component type.
		/// </summary>
		public static Attribute GetAttributeFromMostSpecificComponentType(this PropertyDescriptor property, Type attributeType)
		{
			Argument.NotNull(property, nameof(property));
			var possibleResult = GetAttributesAllowMultiple(property, attributeType).GetEnumerator();
			return possibleResult.MoveNext() ? possibleResult.Current : null;
		}

		#endregion

		#region GetAttributesAllowMultiple

		/// <summary>
		/// Enumerates over the attributes of a given type returned from a PropertyDescriptor.
		/// </summary>
		public static IEnumerable<Attribute> GetAttributesAllowMultiple(this PropertyDescriptor property, Type attributeType)
		{
			Argument.NotNull(property, nameof(property));
			if (property is KPropertyDescriptor)
			{
				foreach (var attribute in (property as KPropertyDescriptor).GetAttributesAllowMultiple(attributeType))
				{
					yield return attribute;
				}
			}
			else if (IsReflectPropertyDescriptor(property))
			{
				foreach (var attribute in AttributeUtil.FindAttributes(
					property.ComponentType,
					property.Name,
					property.PropertyType,
					attributeType,
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					yield return attribute;
				}
			}
			else
			{
				var attribute = property.Attributes[attributeType];
				if (attribute != null)
				{
					yield return attribute;
				}
			}
		}

		#endregion

		#region Implementation

		static FieldInfo GetProviderFieldOfExtendedPropertyDescriptor(PropertyDescriptor property)
		{
			if (providerFieldOfExtendedPropertyDescriptor != null)
			{
				return providerFieldOfExtendedPropertyDescriptor;
			}

			if (property.GetType().FullName.StartsWith("System.ComponentModel.Extend", StringComparison.Ordinal))
			{
#if NETFRAMEWORK
			providerFieldOfExtendedPropertyDescriptor = property.GetType().GetField("provider", BindingFlags.NonPublic | BindingFlags.Instance);
#else
			providerFieldOfExtendedPropertyDescriptor = property.GetType().GetField("_provider", BindingFlags.NonPublic | BindingFlags.Instance);
#endif
			}
			return providerFieldOfExtendedPropertyDescriptor;
		}

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static FieldInfo providerFieldOfExtendedPropertyDescriptor;

#endregion
	}
}
