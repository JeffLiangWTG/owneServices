using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enterprise.ReflectionTest.Utilities
{
	internal static class TestAttributesHelper
	{
		/// <summary>
		/// specific to ITestsGroupOfClasses - handles arrays of Type and string only
		/// </summary>
		static object ToRunnable(object o, Type objectType)
		{
			if (o is Type type)
			{
				return Type.GetType(type.AssemblyQualifiedName, true);
			}
			else if (o is IEnumerable<CustomAttributeTypedArgument> col)
			{
				object[] a = col.Select(item => ToRunnable(item.Value, objectType)).ToArray();
				Array result = Array.CreateInstance(objectType.GetElementType(), a.Length);

				Array.Copy(a, result, a.Length);

				return result;
			}
			else
			{
				return o;
			}
		}
	}
}
