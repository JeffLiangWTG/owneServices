using System;
using System.Collections.Generic;

namespace Enterprise.Metadata.Business.Extension
{
	public static class TypeExtension
	{
		public static Attribute GetFirstAttributeInHierarchy<T>(this Type type, Type stopBefore = null) where T : Attribute
		{
			Attribute attribute;

			do
			{
				attribute = type.GetFirstAttribute<T>();
				if (attribute != null)
				{
					return attribute;
				}

				type = type.BaseType;
			}
			while (!ReachTheEnd(type, stopBefore));

			return null;
		}

		public static Type[] GetInterfacesInHierarchy(this Type type, string interfacePrefix, string interfaceSuffix, Type stopBefore = null)
		{
			var result = new List<Type>();

			do
			{
				var interfaceName = string.Format("{0}{1}{2}", interfacePrefix, type.Name, interfaceSuffix);
				var foundInterface = type.GetInterface(interfaceName);
				if (foundInterface != null)
				{
					result.Add(foundInterface);
				}

				type = type.BaseType;
			}
			while (!ReachTheEnd(type, stopBefore));

			return result.ToArray();
		}

		static bool ReachTheEnd(Type type, Type end)
		{
			return type == end || type == null;
		}

		public static Attribute GetFirstAttribute<T>(this Type type) where T : Attribute
		{
			var attributes = type.GetCustomAttributes(typeof(T), false);
			if (attributes.Length > 0)
			{
				return (T)attributes[0];
			}

			return null;
		}
	}
}
