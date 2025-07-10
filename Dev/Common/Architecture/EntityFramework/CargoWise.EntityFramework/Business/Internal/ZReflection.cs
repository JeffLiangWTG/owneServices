using System;
using System.Reflection;

namespace CargoWise.EntityFramework
{
	public sealed class ZReflection
	{
		ZReflection()
		{
		}

		public static PropertyInfo GetPropertyIncludingNew(object instance, string propertyName)
		{
			return GetPropertyIncludingNew(instance.GetType(), propertyName, DefaultBindingAttr);
		}

		public static PropertyInfo GetPropertyIncludingNew(object instance, string propertyName, BindingFlags bindingAttr)
		{
			return GetPropertyIncludingNew(instance.GetType(), propertyName, bindingAttr);
		}

		public static PropertyInfo GetPropertyIncludingNew(Type type, string propertyName)
		{
			return GetPropertyIncludingNew(type, propertyName, DefaultBindingAttr);
		}

		public static PropertyInfo GetPropertyIncludingNew(Type type, string propertyName, BindingFlags bindingAttr)
		{
			PropertyInfo result = null;
			Type currentType = type;

			while (result == null && currentType != null)
			{
				result = currentType.GetProperty(propertyName, bindingAttr | BindingFlags.DeclaredOnly);
				currentType = currentType.BaseType;
			}

			return result;
		}

		const BindingFlags DefaultBindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
	}
}
