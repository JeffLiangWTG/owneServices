using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public enum ZCalcEditPropertyType
	{
		Byte,
		Decimal,
		Int,
		Short,
		Long
	}

	public static class ZCalcEditPropertyTypeConverter
	{
		public static Type Convert(ZCalcEditPropertyType value)
		{
			switch (value)
			{
				case ZCalcEditPropertyType.Byte:
					return typeof(ZByte);
				case ZCalcEditPropertyType.Decimal:
					return typeof(ZDecimal);
				case ZCalcEditPropertyType.Int:
					return typeof(ZInt);
				case ZCalcEditPropertyType.Short:
					return typeof(ZShort);
				case ZCalcEditPropertyType.Long:
					return typeof(ZLong);
				default:
					throw new ArgumentOutOfRangeException(nameof(value));
			}
		}

		public static ZCalcEditPropertyType Convert(Type value)
		{
			if (value == typeof(ZDecimal))
			{
				return ZCalcEditPropertyType.Decimal;
			}

			if (value == typeof(ZInt))
			{
				return ZCalcEditPropertyType.Int;
			}

			if (value == typeof(ZShort))
			{
				return ZCalcEditPropertyType.Short;
			}

			if (value == typeof(ZByte))
			{
				return ZCalcEditPropertyType.Byte;
			}

			if (value == typeof(ZLong))
			{
				return ZCalcEditPropertyType.Long;
			}

			throw new ArgumentException(value.FullName + " is not a valid value.");
		}
	}
}
