using System;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(Enterprise.Core.Constants.CountryCodes))]

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	[CodeAlive("Soon to be used")]
	public static class Extensions
	{
		public static ZString XmlEnumToString<TEnum>(this TEnum value) where TEnum : struct, IConvertible
		{
			ZString result = ZString.Empty;
			Type typeFromHandle = typeof(TEnum);
			if (typeFromHandle.IsEnum)
			{
				string name = Enum.GetName(typeFromHandle, value);
				if (name != null)
				{
					result = (typeFromHandle.GetField(name).GetCustomAttribute<XmlEnumAttribute>(inherit: false)?.Name ?? name);
				}
			}

			return result;
		}

		public static ZString GetCountryPrefix(this string countryRegNo)
		{
			var countryPrefix = new ZString(countryRegNo).Left(2).ToUpperInvariant();
			if (countryPrefix == "EL")
			{
				countryPrefix = Core.Constants.CountryCodes.Greece;
			}
			return countryPrefix;
		}

		public static ZString RemoveCountryPrefix(this string countryRegNo) => new ZString(countryRegNo).SubstringSafe(2);

		public static ZString GetAddress(string streetName, string streetNumber) => streetName + " " + streetNumber;
	}
}

