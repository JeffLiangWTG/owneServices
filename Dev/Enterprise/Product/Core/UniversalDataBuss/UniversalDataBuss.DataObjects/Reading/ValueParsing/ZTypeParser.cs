using System;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZTypeParser
	{
		public IZType TryParseAndValidate(string incomingValue, Type targetType, string valueSourceDescription, ISimpleLogger logger)
		{
			foreach (var parser in parsers)
			{
				if (parser.HandlesType(targetType))
				{
					return parser.TryParseAndValidate(incomingValue, targetType, valueSourceDescription, logger);
				}
			}
			throw new InvalidPropertyTypeException();
		}

		readonly IZTypeBaseParser[] parsers = new IZTypeBaseParser[]
			{
				new ZStringParser(),
				new ZIntParser(),
				new ZDecimalParser(),
				new ZDateParser(),
				new ZDateTimeParser(),
				new ZDateTimeOffsetParser(),
				new ZBoolParser(),
				new ZShortParser(),
				new ZLongParser(),
				new ZByteParser(),
				new ZBlobParser(),
				new ZGeographyParser(),
				new ZTimeParser(),
			};
	}
}
