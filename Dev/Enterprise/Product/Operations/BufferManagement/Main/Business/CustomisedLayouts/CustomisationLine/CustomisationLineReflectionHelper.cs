using System;
using System.Reflection;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public static class CustomisationLineReflectionHelper
	{
		public static PropertyInfo GetPropertyInfo(Type targetType, string propertyName)
		{
			var result = targetType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			if (result != null)
			{
				return result;
			}

			if (targetType == typeof(BusinessObject) || targetType == typeof(object) || targetType.BaseType == null)
			{
				return null;
			}

			return GetPropertyInfo(targetType.BaseType, propertyName);
		}
	}
}
