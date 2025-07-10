using System;
using System.Collections;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO
{
	public static class Extensions
	{
		internal static int GetMaxLength(this PropertyInfo propertyInfo)
		{
			var maxLengthAttributes = propertyInfo.GetCustomAttributes(typeof(MaxLengthAttribute), false);
			if (maxLengthAttributes.Length == 0)
			{
				throw new XmlProcessingException(propertyInfo, "All ZString properties on DataObjects must have the MaxLengthAttribute applied.");
			}

			return (maxLengthAttributes[0] as MaxLengthAttribute).MaxLength;
		}

		internal static bool IsGenericAndATypeUsedForCollections(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition().IsATypeUsedForCollections();
		}

		internal static bool IsATypeUsedForCollections(this Type type)
		{
			return typeof(IList).IsAssignableFrom(type);
		}

#if DEBUG	

		public static bool IsGenericAndATypeUsedForCollectionsForTesting(this Type type)
		{
			return type.IsGenericAndATypeUsedForCollections();
		}

		public static bool ShouldHaveMaxLengthDefinedForTesting(this Type type)
		{
			return type == typeof(ZString) || type == typeof(ZCodeMappedZString) || type == typeof(ZString?) || type == typeof(ZCodeMappedZString?);
		}

#endif
	}
}
