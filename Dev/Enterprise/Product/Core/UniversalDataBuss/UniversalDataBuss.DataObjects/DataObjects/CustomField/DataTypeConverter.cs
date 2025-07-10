using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	public class DataTypeConverter : EnumConverter<DataType, Type>
	{
		protected override Type[] GetCodes()
		{
			return new Type[]
			{
				typeof(ZBool),
				typeof(ZByte),
				typeof(ZDateTime),
				typeof(ZDecimal),
				typeof(ZInt),
				typeof(ZShort),
				typeof(ZString),
				typeof(ZDateTimeOffset),
				typeof(ZGeography),
				typeof(ZTime)
			};
		}

		protected override DataType[] GetEnumValues()
		{
			return new DataType[]
			{
				DataType.Boolean,
				DataType.Byte,
				DataType.DateTime,
				DataType.Decimal,
				DataType.Integer,
				DataType.Short,
				DataType.String,
				DataType.DateTimeOffset,
				DataType.Geography,
				DataType.Time
			};
		}

		protected override Type GetEmptyValue()
		{
			throw new InvalidOperationException("Cannot pass null into GetCode(). Supported DataType values are: " + string.Join(", ", GetEnumValues().Select(value => { return value.ToString(); }).ToArray()) + ".");
		}
	}
}
