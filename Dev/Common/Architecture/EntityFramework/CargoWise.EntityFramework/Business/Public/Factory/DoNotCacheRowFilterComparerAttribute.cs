using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DoNotCacheRowFilterComparerAttribute : Attribute
	{
		public static bool HasAttribute(Type type)
		{
			bool result = false;

			if (!Types.TryGetValue(type, out result))
			{
				result = type.GetCustomAttributes(typeof(DoNotCacheRowFilterComparerAttribute), true).Length > 0;
				Types.Add(type, result);
			}

			return result;
		}

		static Dictionary<Type, bool> Types
		{
			get { return types ?? (types = new Dictionary<Type, bool>()); }
		}

		[ThreadStatic]
		static Dictionary<Type, bool> types;
	}
}
