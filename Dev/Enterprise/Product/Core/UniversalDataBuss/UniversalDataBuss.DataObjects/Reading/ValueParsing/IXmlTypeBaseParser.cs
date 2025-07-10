using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public interface IXmlTypeBaseParser
	{
		bool HandlesType(Type targetType);
		object TryParseAndValidate(string incomingValue, Type targetType, string valueSourceDescription, ISimpleLogger logger);
	}
}
