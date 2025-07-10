using System;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public interface IZTypeBaseParser : IXmlTypeBaseParser
	{
		new IZType TryParseAndValidate(string incomingValue, Type targetType, string valueSourceDescription, ISimpleLogger logger);
	}
}
