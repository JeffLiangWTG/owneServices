using System;
using System.Collections;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap
{
	public class MCRDataReflectorFilter : DocDataReflectorFilter
	{
		public override bool IsCollection(Type type)
		{
			return base.IsCollection(type) || IsIEnumerable(type);
		}

		static bool IsIEnumerable(Type type)
		{
			return !IsStringType(type) && typeof(IEnumerable).IsAssignableFrom(type);
		}

		static bool IsStringType(Type type)
		{
			return typeof(string).IsAssignableFrom(type);
		}
	}
}
