using System;
using System.Text;
using System.Xml;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class SimpleTypeFormatter
	{
		public static string FormatForMultiTypedStringElementTargetInDataObject(object value)
		{
			return GetFormattedValue(value, null, false, false);
		}

		public static string GetFormattedValueAsAttributeForWritingToXml(string name, object value, Func<int> getMaxLength)
		{
			if (!(value is IZType zTypedValue) || (zTypedValue.IsValid && !zTypedValue.IsEmpty))
			{
				return FormattableString.Invariant($@" {name}=""{GetFormattedValue(value, getMaxLength, true, formatForXml: true)}""");
			}

			return null;
		}

		public static string GetFormattedValueForWritingToXml(object value, Func<int> getMaxLength)
		{
			return GetFormattedValue(value, getMaxLength, false, true);
		}

		static string GetFormattedValue(object value, Func<int> getMaxLength, bool isAttribute, bool formatForXml)
		{
			var valueType = value.GetType();
			if (valueType == typeof(ZString) || valueType == typeof(ZCodeMappedZString))
			{
				return formatForXml ? FormatStringEscapingCharactersAndTrimToLength(value.ToString(), getMaxLength(), isAttribute) : value.ToString();
			}
			if (valueType == typeof(ZBool))
			{
				return XmlConvert.ToString((ZBool)value);
			}
			if (valueType == typeof(ZDateTime))
			{
				return FormatDateTime((ZDateTime)value);
			}
			if (valueType == typeof(ZDate))
			{
				return FormatDate((ZDate)value);
			}
			if (valueType == typeof(ZDateTimeOffset))
			{
				return FormatDateTimeOffset((ZDateTimeOffset)value);
			}
			if (valueType == typeof(UXmlDateTime))
			{
				return FormatXmlDateTime((UXmlDateTime)value);
			}
			if (valueType == typeof(ZTime))
			{
				return FormatTime((ZTime)value);
			}
			if (valueType == typeof(ZGeography))
			{
				return FormatGeography((ZGeography)value);
			}
			if (valueType == typeof(ZDecimal))
			{
				return XmlConvert.ToString((ZDecimal)value);
			}
			if (valueType == typeof(ZBlob))
			{
				return System.Convert.ToBase64String((System.Byte[])((ZBlob)value));
			}
			if (valueType == typeof(TimeSpan))
			{
				return XmlConvert.ToString((TimeSpan)value);
			}
			if (valueType == typeof(SubStreamableStream))
			{
				return ((SubStreamableStream)value).ConvertToBase64StringAndLeaveStreamOpen();
			}
			return value.ToString();
		}

		static string FormatDate(ZDate dateValue)
		{
			if (dateValue.IsValid && !dateValue.IsEmpty)
			{
				return XmlConvert.ToString(dateValue.ToDateTime(), "yyyy-MM-dd");
			}
			else
			{
				return string.Empty;
			}
		}

		static string FormatDateTime(ZDateTime dateTimeValue)
		{
			if (dateTimeValue.IsValid && !dateTimeValue.IsEmpty)
			{
				return XmlConvert.ToString(dateTimeValue.ToDateTime(), XmlDateTimeSerializationMode.Unspecified);
			}
			else
			{
				return string.Empty;
			}
		}

		static string FormatDateTimeOffset(ZDateTimeOffset dateTimeOffsetValue)
		{
			if (dateTimeOffsetValue.IsValid && !dateTimeOffsetValue.IsEmpty)
			{
				return XmlConvert.ToString(dateTimeOffsetValue.ToDateTimeOffset(), "yyyy-MM-ddTHH:mm:ss.fffzzzzzzz");
			}
			else
			{
				return string.Empty;
			}
		}

		static string FormatXmlDateTime(UXmlDateTime xmlDateTimeValue)
		{
			var type = xmlDateTimeValue.GetDateType();

			if (type == typeof(ZDateTime))
			{
				return FormatDateTime(xmlDateTimeValue);
			}
			else if (type == typeof(ZDateTimeOffset))
			{
				return FormatDateTimeOffset(xmlDateTimeValue);
			}
			else
			{
				return string.Empty;
			}
		}

		static string FormatTime(ZTime timeValue)
		{
			if (timeValue.IsValid && !timeValue.IsEmpty)
			{
				return XmlConvert.ToString(timeValue.ToDateTime(), ZTime.TimeFormat);
			}
			else
			{
				return string.Empty;
			}
		}

		static string FormatGeography(ZGeography geographyValue)
		{
			if (geographyValue.IsValid && !geographyValue.IsEmpty)
			{
				return geographyValue.ToString();
			}
			else
			{
				return string.Empty;
			}
		}

		static string FormatStringEscapingCharactersAndTrimToLength(string input, int maxLength, bool isAttribute)
		{
			var characters = input.Length > maxLength
				? input.ToCharArray(0, maxLength)
				: input.ToCharArray();

			var result = new StringBuilder();
			foreach (var character in characters)
			{
				switch (character)
				{
					case '<':
						result.Append("&lt;");
						break;
					case '>':
						result.Append("&gt;");
						break;
					case '&':
						result.Append("&amp;");
						break;
					case '"':
						if (isAttribute)
						{
							result.Append("&quot;");
						}
						else
						{
							result.Append(character);
						}
						break;
					default:
						result.Append(character);
						break;
				}
			}

			return result.ToString();
		}
	}
}
