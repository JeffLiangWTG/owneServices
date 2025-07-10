using System;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing;
using Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ValueParsers
{
	sealed class UXmlDateTimeValueParser : ValueParser
	{
		readonly Type[] expectedTypes = { typeof(UXmlDateTime), typeof(UXmlDateTime?) };

		internal UXmlDateTimeValueParser(XmlReader reader) : base(reader) { }

		internal override bool HandlesType(Type targetType) => expectedTypes.Contains(targetType);

		protected override object TryParseAndValidate(PropertyInfo propertyInfo, Type targetType, string sourceValue, int? lineNumber, string elementFullName, bool isAttribute)
		{
			string valueSourceDescription = FormattableString.Invariant($"Line {lineNumber ?? reader.CurrentLineNumber}: {reader.CurrentElementParser.GetFullName()}");

			if (ZDateTimeOffset.StringHasOffsetInformation(sourceValue))
			{
				var result = offsetParser.TryParseAndValidate(sourceValue, typeof(ZDateTimeOffset), valueSourceDescription, reader.Logger);
				if (result != null)
				{
					return new UXmlDateTime((ZDateTimeOffset)result);
				}
			}
			else
			{
				var result = dtParser.TryParseAndValidate(sourceValue, typeof(ZDateTime), valueSourceDescription, reader.Logger);
				if (result != null)
				{
					return new UXmlDateTime((ZDateTime)result);
				}
			}

			return null;
		}

		readonly ZDateTimeParser dtParser = new ZDateTimeParser();
		readonly ZDateTimeOffsetParser offsetParser = new ZDateTimeOffsetParser();
	}
}
