using System.Reflection;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class ReflectionExtensions
	{
		public static IZType TryConvertValueToPropertyType(this PropertyInfo propertyInfo, ZString sourceValue, ISimpleLogger logger, string valueSourceDescription)
		{
			return new ZTypeParser().TryParseAndValidate(sourceValue, propertyInfo.PropertyType, valueSourceDescription, logger);
		}
	}
}
