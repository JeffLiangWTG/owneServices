using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Extensions
{
	public static class IAddInfoWithSyncPropertySupporterExtension
	{
		public static string GetStringRepresentation(this IZType value)
		{
			string result = "";

			if (!value.IsDefault)
			{
				if (value is ZDateTime)
				{
					ZDateTime dateTime = (ZDateTime)value;
					result = dateTime.IsValid ? dateTime.SqlFormat : ZString.Empty;
				}
				else if (value is ZDate)
				{
					var date = (ZDate)value;
					result = date.IsValid ? date.ToZDateTime().ToISO8601ShortDateString() : string.Empty;
				}
				else if (value is ZDateTimeOffset)
				{
					var dateTime = (ZDateTimeOffset)value;
					result = dateTime.IsValid ? dateTime.SqlFormat : ZString.Empty;
				}
				else if (value is ZDecimal)
				{
					ZDecimal vDecimal = (ZDecimal)value;
					result = vDecimal.ToString(vDecimal.DecimalPlaces);
				}
				else
				{
					result = value.ToString();
				}
			}
			return result;
		}

		public static (string addInfoName, Type addInfoValueType, ZPropertyInfo info, string fastSearchName)[] GetSyncAddInfos(this IAddInfoWithSyncPropertySupporter supporter)
		{
			(string addInfoName, Type addInfoValueType, ZPropertyInfo info, string fastSearchName)[] result = null;
			if (supporter != null)
			{
				var syncAddInfos = new Dictionary<string, (Type addInfoValueType, ZPropertyInfo info, string fastSearchName)>();
				var supporterType = supporter.GetType();
				foreach (var mapping in GetBizObjSyncPropertyMapping(supporterType))
				{
					var info = supporter.ZPropertyInfoHash.GetPropertySafe(mapping.Key);
					if (info != null)
					{
						syncAddInfos.Add(mapping.Value.addInfoName, (mapping.Value.addInfoValueType, info, mapping.Value.fastSearchName));
					}
				}
				result = syncAddInfos.Select(x => (x.Key, x.Value.addInfoValueType, x.Value.info, x.Value.fastSearchName)).ToArray();
			}
			return result;
		}

		static IDictionary<string, (string addInfoName, Type addInfoValueType, string fastSearchName)> GetBizObjSyncPropertyMapping(Type bizObjType)
		{
			if (!BizObjSyncPropertyMapping.TryGetValue(bizObjType, out var result))
			{
				result = new Dictionary<string, (string addInfoName, Type addInfoValueType, string fastSearchName)>();
				foreach (var property in bizObjType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
				{
					var attribute = property.GetCustomAttribute<AddInfoSyncPropertyAttribute>();
					if (attribute != null)
					{
						result.Add(property.Name, (attribute.AddInfoName, attribute.AddInfoValueType, attribute.FastSearchName));
					}
				}
				BizObjSyncPropertyMapping.Add(bizObjType, result);
			}

			return result;
		}

		static IDictionary<Type, IDictionary<string, (string addInfoName, Type addInfoValueType, string fastSearchName)>> BizObjSyncPropertyMapping => bizObjSyncPropertyMapping ?? (bizObjSyncPropertyMapping = new Dictionary<Type, IDictionary<string, (string addInfoName, Type addInfoValueType, string fastSearchName)>>());
		[ThreadStatic]
		static IDictionary<Type, IDictionary<string, (string addInfoName, Type addInfoValueType, string fastSearchName)>> bizObjSyncPropertyMapping;
	}
}
