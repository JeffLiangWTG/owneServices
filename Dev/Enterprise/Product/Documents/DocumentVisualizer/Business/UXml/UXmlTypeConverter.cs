using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class UXmlTypeConverter : ITypeConverter
	{
		public Try<object> Convert(object value, Type targetType)
		{
			if (targetType == typeof(ZCodeMappedZString?))
			{
				return Try<object>.Success(new ZCodeMappedZString(System.Convert.ToString(value, CultureInfo.InvariantCulture)));
			}

			if (targetType == typeof(UXmlDateTime?) && value is string svalue)
			{
				if (ZDateTimeOffset.StringHasOffsetInformation(svalue))
				{
					var tryConvertToDateTimeOffset = Convert(svalue, typeof(ZDateTimeOffset));

					if (tryConvertToDateTimeOffset.IsFaulted
						|| !(tryConvertToDateTimeOffset.Value is ZDateTimeOffset dateTimeOffset))
					{
						return tryConvertToDateTimeOffset;
					}

					return Try<object>.Success(new UXmlDateTime(dateTimeOffset));
				}
				else
				{
					var tryConvertToDateTime = Convert(svalue, typeof(ZDateTime));

					if (tryConvertToDateTime.IsFaulted
						|| !(tryConvertToDateTime.Value is ZDateTime dateTime))
					{
						return tryConvertToDateTime;
					}

					return Try<object>.Success(new UXmlDateTime(dateTime));
				}
			}

			targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

			return converter.Value.Convert(value, targetType);
		}

		readonly Lazy<DefaultTypeConverter> converter = new Lazy<DefaultTypeConverter>(() => new DefaultTypeConverter());
	}
}
