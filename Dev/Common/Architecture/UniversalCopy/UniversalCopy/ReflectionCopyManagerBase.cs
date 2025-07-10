using System;
using System.Reflection;

namespace CargoWise.UniversalCopy
{
	/// <summary>
	/// Base reflection-driven universal copy manager.
	/// </summary>
	public abstract class ReflectionCopyManagerBase : CopyManager
	{
		protected override object GetPropertyValueCore(object source, string propertyName)
		{
			var propertyInfo = GetPropertyInfo(source, propertyName);
			return propertyInfo != null ? propertyInfo.GetValue(source, Array.Empty<object>()) : null;
		}

		protected override void SetPropertyValueCore(object target, string propertyName, object value)
		{
			var propertyInfo = GetPropertyInfo(target, propertyName);
			if (propertyInfo != null && propertyInfo.CanWrite)
			{
				propertyInfo.SetValue(target, value, Array.Empty<object>());
			}
		}

		protected override Type GetPropertyTypeCore(object source, string propertyName)
		{
			var propertyInfo = GetPropertyInfo(source, propertyName);
			return propertyInfo != null ? propertyInfo.PropertyType : null;
		}

		protected static PropertyInfo GetPropertyInfo(object component, string propertyName)
		{
			if (component != null && !string.IsNullOrEmpty(propertyName))
			{
				Type componentType = component.GetType();
				while (componentType != null && componentType != typeof(object))
				{
					PropertyInfo info = componentType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
					if (info != null)
					{
						return info;
					}

					componentType = componentType.BaseType;
				}
			}

			return null;
		}
	}
}
