using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.DataMapping
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ImportInitializationMethodNameAttribute : Attribute
	{
		public ImportInitializationMethodNameAttribute(string methodName)
		{
			InitializationMethodName = methodName;
		}

		public string InitializationMethodName { get; private set; }

		public static bool HasAttributeApplied(object component, string propertyName)
		{
			PropertyInfo propertyInfo = GetPropertyInfo(component, propertyName);
			if (propertyInfo != null)
			{
				try
				{
					return (propertyInfo.GetCustomAttributes(typeof(ImportInitializationMethodNameAttribute), true).Any());
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}
					return false;
				}
			}
			return false;
		}

		public static MethodInfo GetInitializationMethod(object component, string propertyName)
		{
			PropertyInfo propertyInfo = GetPropertyInfo(component, propertyName);
			if (propertyInfo != null && propertyInfo.GetValue(component, null) == null)
			{
				try
				{
					var attribute = (ImportInitializationMethodNameAttribute)propertyInfo.GetCustomAttributes(typeof(ImportInitializationMethodNameAttribute), true)
						.FirstOrDefault();

					return GetMethodInfo(component, attribute);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}
					return null;
				}
			}
			return null;
		}

		static MethodInfo GetMethodInfo(object component, ImportInitializationMethodNameAttribute attribute)
		{
			MethodInfo methodInfo = null;
			if (component != null && attribute != null)
			{
				try
				{
					methodInfo = component.GetType().GetMethod(attribute.InitializationMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				}
				catch (AmbiguousMatchException)
				{
					// see if it was becasause a derived type declares a method that hides an
					// inherited method with the same name, by using the new modifier
					Type componentType = component.GetType();

					while (methodInfo == null && componentType != null)
					{
						methodInfo = componentType.GetMethod(attribute.InitializationMethodName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						componentType = componentType.BaseType;
					}
				}
			}

			return methodInfo;
		}

		static PropertyInfo GetPropertyInfo(object component, string propertyName)
		{
			PropertyInfo propertyInfo = null;
			if (component != null && propertyName != null)
			{
				try
				{
					propertyInfo = component.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				}
				catch (AmbiguousMatchException)
				{
					// see if it was becasause a derived type declares a property that hides an
					// inherited property with the same name, by using the new modifier
					Type componentType = component.GetType();

					while (propertyInfo == null && componentType != null)
					{
						propertyInfo = componentType.GetProperty(propertyName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						componentType = componentType.BaseType;
					}
				}
			}
			return propertyInfo;
		}
	}
}
