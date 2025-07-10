using System;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public abstract class XmlTypeBaseParser<T> : IXmlTypeBaseParser where T : struct
	{
		public bool HandlesType(Type targetType)
		{
			return targetType == typeof(T) || targetType == typeof(T?);
		}

		public object TryParseAndValidate(string incomingValue, Type targetType, string valueSourceDescription, ISimpleLogger logger)
		{
			if (string.IsNullOrEmpty(incomingValue) && IsValidWhenEmpty(targetType))
			{
				return new T();
			}

			if (TryParse(incomingValue, out T typedResult))
			{
				return typedResult;
			}
			else
			{
				logger.Log(LogType.Warning, Res.GetString("744cdb5b-748c-4b2e-9c70-caf201f409dd", "{0} - Invalid value [{1}]. Value must be a valid {2}.", valueSourceDescription, incomingValue, ValidValueDescription));
				return null;
			}
		}

		protected virtual bool IsValidWhenEmpty(Type targetType)
		{
			return true;
		}

		protected abstract bool TryParse(string sourceValue, out T typedResult);

		protected abstract string ValidValueDescription { get; }
	}
}