using System;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public abstract class ZTypeBaseParser<T> : XmlTypeBaseParser<T>, IZTypeBaseParser where T : struct, IZType
	{
		public new IZType TryParseAndValidate(string incomingValue, Type targetType, string valueSourceDescription, ISimpleLogger logger)
		{
			return (IZType)base.TryParseAndValidate(incomingValue, targetType, valueSourceDescription, logger);
		}
	}
}