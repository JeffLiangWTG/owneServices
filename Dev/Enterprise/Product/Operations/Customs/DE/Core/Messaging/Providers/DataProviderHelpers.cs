using System;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public static class DataProviderHelpers
	{
		public static ZString XmlEnumToString<TEnum>(this TEnum value) where TEnum : struct, IConvertible
		{
			var result = ZString.Empty;

			var type = typeof(TEnum);
			if (type.IsEnum)
			{
				var name = Enum.GetName(type, value);
				if (name != null)
				{
					var attribute = type.GetField(name).GetCustomAttribute<XmlEnumAttribute>(false);
					result = attribute?.Name ?? name;
				}
			}

			return result;
		}
	}
}
