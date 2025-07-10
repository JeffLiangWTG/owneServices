using System;
using System.Reflection;
using System.Text;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ValueParsers
{
	sealed class EnumValueParser : ValueParser
	{
		internal EnumValueParser(XmlReader reader) : base(reader) { }

		internal sealed override bool HandlesType(Type targetType)
		{
			return targetType.IsEnum;
		}

		protected sealed override object TryParseAndValidate(PropertyInfo propertyInfo, Type enumType, string sourceValue, int? lineNumber, string elementFullName, bool isAttribute)
		{
			if (IsDefinedCaseInsensitive(enumType, sourceValue))
			{
				return Enum.Parse(enumType, sourceValue, true);
			}
			else
			{
				reader.AddWarning(lineNumber, ElementPath.FullName, (new StringBuilder()).Append(" - Invalid value [").Append(sourceValue).Append("], valid values are [").Append(string.Join(", ", Enum.GetNames(enumType)) + "].").ToString());
				return null;
			}
		}

		static bool IsDefinedCaseInsensitive(Type enumType, string sourceValue)
		{
			foreach (var value in Enum.GetNames(enumType))
			{
				if (value.Equals(sourceValue, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}
	}
}
