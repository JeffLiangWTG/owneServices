using System;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ValueParsers
{
	abstract class ValueParser
	{
		protected ValueParser(XmlReader reader)
		{
			this.reader = reader;
		}

		protected readonly XmlReader reader;

		protected virtual object TryParseAndValidate(PropertyInfo propertyInfo, Type targetType, string sourceValue, int? lineNumber, string elementFullName, bool isAttribute)
		{
			throw new InvalidOperationException();
		}
		protected virtual object TryParseAndValidate(PropertyInfo propertyInfo, Type targetType, object sourceValue, int? lineNumber, string elementFullName, bool isAttribute)
		{
			throw new InvalidOperationException();
		}

		internal abstract bool HandlesType(Type targetType);

		internal virtual object Process(PropertyInfo propertyInfo, object targetDataObject, Type targetType, object value, int? lineNumber, string elementFullName = "", bool isAttribute = false)
		{
			var stringValue = value as string;

			object validatedResult;

			if (stringValue != null)
			{
				validatedResult = TryParseAndValidate(propertyInfo, targetType, stringValue, lineNumber, elementFullName, isAttribute);
			}
			else
			{
				validatedResult = TryParseAndValidate(propertyInfo, targetType, value, lineNumber, elementFullName, isAttribute);
			}

			SetValue(propertyInfo, targetDataObject, validatedResult);

			return validatedResult;
		}

		internal static void SetValue(PropertyInfo propertyInfo, object targetDataObject, object value)
		{
			if (value != null)
			{
				propertyInfo.SetValue(targetDataObject, value, null);
			}
		}
	}
}
