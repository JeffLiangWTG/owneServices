using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ValueParsers
{
	sealed class ZStringValueParser : ValueParser
	{
		readonly StringInterner stringInterner = new StringInterner();
		readonly Type[] expectedTypes = { typeof(ZString), typeof(ZCodeMappedZString) };

		internal ZStringValueParser(XmlReader reader) : base(reader) { }

		internal override bool HandlesType(Type targetType) => expectedTypes.Contains(targetType);

		internal override object Process(PropertyInfo propertyInfo, object targetDataObject, Type targetType, object stringValue, int? lineNumber, string elementFullName = "", bool isAttribute = false)
		{
			var value = (string)stringValue;

			value = UnEscapeContent(value, targetType, isAttribute);

			value = StripWhiteSpaces(propertyInfo, value, lineNumber);

			value = TrimWhiteSpaces(propertyInfo, value);

			value = stringInterner.InternValue(value);

			object resultValue;
			if (reader.QueueForCodeMappingWhereApplicable(targetDataObject, propertyInfo, value, DataFieldValidatorAndSetter, isAttribute))
			{
				resultValue = ConvertToTargetType(targetType, value);
				SetValue(propertyInfo, targetDataObject, resultValue);
			}
			else
			{
				resultValue = base.Process(propertyInfo, targetDataObject, targetType, value, lineNumber, string.Empty, isAttribute);
			}

			return resultValue;
		}

		protected override object TryParseAndValidate(PropertyInfo propertyInfo, Type targetType, string sourceValue, int? lineNumber, string elementFullName, bool isAttribute)
		{
			return TryParseAndValidate(reader, propertyInfo, targetType, sourceValue, lineNumber, elementFullName, isAttribute);
		}

		static object TryParseAndValidate(XmlReader reader, PropertyInfo propertyInfo, Type targetType, string sourceValue, int? lineNumber, string elementFullName, bool isAttribute)
		{
			var valueLength = sourceValue.Length;
			var maxLength = reader.GetMaxLengthCached(propertyInfo);
			if (valueLength > maxLength)
			{
				if (isAttribute)
				{
					reader.AddWarning(lineNumber, ElementPath.FullName, " - Attribute [" + propertyInfo.Name + "] exceeded its maximum length of " + maxLength + " characters. " + valueLength + " characters were found.");
				}
				else
				{
					var message = " exceeded its maximum length of " + maxLength + " characters. " + valueLength + " characters were found.";
					if (string.IsNullOrEmpty(elementFullName))
					{
						reader.AddWarning(lineNumber, ElementPath.FullName, message);
					}
					else
					{
						reader.AddWarning(lineNumber, elementFullName, message);
					}
				}

				sourceValue = IsExcludedProperty(propertyInfo) ? sourceValue : sourceValue.Substring(0, maxLength);
			}

			return ConvertToTargetType(targetType, sourceValue);
		}

		static object DataFieldValidatorAndSetter(IXmlReader reader, object targetDataObject, PropertyInfo propertyInfo, string originalValue, string mappedValue, int? lineNumber, string elementFullName, bool isAttribute)
		{
			bool isMappedValueUsed = !string.IsNullOrEmpty(mappedValue);
			var targetType = propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;
			var validatedValue = TryParseAndValidate(reader as XmlReader, propertyInfo, targetType, isMappedValueUsed ? mappedValue : originalValue, lineNumber, elementFullName, isAttribute);
			if (isMappedValueUsed && validatedValue is ZCodeMappedZString)
			{
				validatedValue = ExchangeSourceAndMappedValues((ZCodeMappedZString)validatedValue, originalValue);
			}

			SetValue(propertyInfo, targetDataObject, validatedValue);

			return validatedValue;
		}

		static bool IsExcludedProperty(PropertyInfo info)
		{
			return info.Name == "OrganizationCode";
		}

		string UnEscapeContent(string sourceValue, Type targetType, bool isAttribute)
		{
			if (HandlesType(targetType))
			{
				var unEscapingRegex = isAttribute ? XmlRegexes.AttributeContentEscapes : XmlRegexes.ElementContentEscapes;
				return unEscapingRegex.Replace(sourceValue, ReplaceRegexMatches);
			}

			return sourceValue;
		}

		static string ReplaceRegexMatches(Match match)
		{
			var matchContent = match.Groups["content"].Value;
			switch (matchContent)
			{
				case "lt":
					return "<";
				case "gt":
					return ">";
				case "amp":
					return "&";
				case "quot":
					return @"""";
			}

			return match.Value;
		}

		static object ConvertToTargetType(Type targetType, string value)
		{
			if (targetType == typeof(ZString))
			{
				return new ZString(value);
			}
			if (targetType == typeof(ZCodeMappedZString))
			{
				return new ZCodeMappedZString(value);
			}

			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} type is not supported", targetType), nameof(targetType));
		}
		static ZCodeMappedZString ExchangeSourceAndMappedValues(ZCodeMappedZString codeMappedZStringWithMappedValueInSource, string originalValue) => new ZCodeMappedZString(originalValue) { MappedValue = codeMappedZStringWithMappedValueInSource.SourceValue };

		string StripWhiteSpaces(PropertyInfo propertyInfo, string sourceValue, int? lineNumber)
		{
			var result = sourceValue;
			var maxLengthAttribute = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();
			if (maxLengthAttribute != null && sourceValue.Length > 0)
			{
				var allowLineControlWhiteSpaceAttribute = propertyInfo.GetCustomAttribute<AllowLineControlWhiteSpaceAttribute>();
				var maxLength = maxLengthAttribute.MaxLength;
				if (maxLength <= 200 || allowLineControlWhiteSpaceAttribute == null)
				{
					var containsWhiteSpace = false;
					var characters = sourceValue.ToList();
					char lastCharacter = ' ';
					for (var index = 0; index < characters.Count;)
					{
						if (IsWhiteSpace(characters[index]))
						{
							containsWhiteSpace = true;
							if (index == 0 || lastCharacter == ' ')
							{
								characters.RemoveAt(index);
								continue;
							}

							characters[index] = ' ';
						}

						lastCharacter = characters[index];
						index++;
					}

					if (containsWhiteSpace)
					{
						reader.AddWarning(lineNumber, ElementPath.FullName, Res.GetString("d9c81d37-b65e-42b5-9911-78eae07cc1c8", "The field does not accept carriage returns but a carriage return was detected in the XML. All carriage returns have been converted to a white space."));
					}

					result = new string(characters.ToArray()).TrimEnd();
				}
			}

			return result;
		}

		string TrimWhiteSpaces(PropertyInfo propertyInfo, string sourceValue)
		{
			var result = sourceValue;
			var trimWhiteSpaceAttribute = propertyInfo.GetCustomAttribute<TrimWhiteSpaceAttribute>();
			if (trimWhiteSpaceAttribute != null)
			{
				result = sourceValue.TrimEnd();
			}
			return result;
		}

		static bool IsWhiteSpace(char character)
		{
			return character == '\r' || character == '\n' || character == '\t';
		}
	}
}
