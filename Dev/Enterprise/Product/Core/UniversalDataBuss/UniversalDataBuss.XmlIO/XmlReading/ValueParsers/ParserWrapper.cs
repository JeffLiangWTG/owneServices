using System;
using System.Reflection;
using Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing;
using Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ValueParsers
{
	sealed class ParserWrapper<T> : ValueParser where T : IXmlTypeBaseParser, new()
	{
		readonly IXmlTypeBaseParser baseParser = new T();

		internal ParserWrapper(XmlReader reader) : base(reader) { }

		internal sealed override bool HandlesType(Type targetType)
		{
			return baseParser.HandlesType(targetType);
		}

		protected sealed override object TryParseAndValidate(PropertyInfo propertyInfo, Type targetType, string sourceValue, int? lineNumber, string elementFullName, bool isAttribute)
		{
			string valueSourceDescription = FormattableString.Invariant($"Line {lineNumber ?? reader.CurrentLineNumber}: {reader.CurrentElementParser.GetFullName()}");

			return baseParser.TryParseAndValidate(sourceValue, targetType, valueSourceDescription, reader.Logger);
		}
	}
}
