using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public static class BusinessObjectHelper
	{
		#region GetNonDefaultValueForZType

		public static IZType GetNonDefaultValueForZType(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.PropertyType == typeof(ZString))
			{
				return new ZString(propertyInfo.HumanReadableName).SubstringSafe(0, propertyInfo.MaxLength);
			}

			if (propertyInfo.PropertyType == typeof(ZDateTime))
			{
				if (propertyInfo.Name.Contains((NoResString)"Duration"))
				{
					return new ZInt(120).GetDateTimeFromMinutes();
				}

				return ZDateTime.Now;
			}

			return GetNonDefaultValueForZType(propertyInfo.PropertyType);
		}

		public static IZType GetNonDefaultValueForZType(Type type)
		{
			if (type == typeof(ZString))
			{
				return new ZString(ZGuid.NewZGuid().ToString()).SubstringSafe(0, 3);
			}

			if (type == typeof(ZDateTime))
			{
				return ZDateTime.Now;
			}

			if (type == typeof(ZDecimal))
			{
				return new ZDecimal(10m);
			}

			if (type == typeof(ZInt))
			{
				return new ZInt(10);
			}

			if (type == typeof(ZShort))
			{
				return new ZShort(10);
			}

			if (type == typeof(ZByte))
			{
				return new ZByte(10);
			}

			if (type == typeof(ZBool))
			{
				return ZBool.True;
			}

			return null;
		}

		#endregion
	}
}
