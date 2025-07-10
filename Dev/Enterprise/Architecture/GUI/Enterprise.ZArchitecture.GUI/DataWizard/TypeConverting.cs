using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	internal static class TypeConverting
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "type friendly name")]
		public static string GetPropertyTypeFriendlyName(BusinessObject newObj, string propertyName)
		{
			var property = newObj.ZPropertyInfoHash.GetPropertySafe(propertyName);
			if (property == null)
			{
				return null;
			}

			var typeDictionary = new Dictionary<Type, string>
			{
				{ typeof(ZString), "String" },
				{ typeof(ZDecimal), "Decimal" },
				{ typeof(ZInt), "Integer" },
				{ typeof(ZShort), "Short" },
				{ typeof(ZByte), "Byte" },
				{ typeof(ZBool), "Boolean" },
				{ typeof(ZDateTime), "DateTime" }
			};

			if (typeDictionary.TryGetValue(property.PropertyType, out var typeFriendlyName))
			{
				return typeFriendlyName;
			}

			return ZString.Empty;
		}

		public static object ConvertToType(string stringData, string propertyName, BusinessObject businessObject)
		{
			var propertyInfo = businessObject.ZPropertyInfoHash.GetPropertySafe(propertyName);
			var propType = businessObject[propertyName].GetType();
			var typeConverter = TypeDescriptor.GetConverter(propType);
			var result = typeConverter.ConvertFromString(stringData);
			if (propertyInfo != null && propertyInfo.SupportsMaxLength && propertyInfo.MaxLength > 0 && (result is ZString || result is string))
			{
				ZString str = result.ToString();
				if (str.Length > propertyInfo.MaxLength)
				{
					result = str.Substring(0, propertyInfo.MaxLength);
				}
			}

			return result;
		}
	}
}
