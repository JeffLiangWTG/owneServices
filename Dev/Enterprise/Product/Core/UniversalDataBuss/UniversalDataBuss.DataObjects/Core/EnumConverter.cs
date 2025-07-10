using System;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public abstract class EnumConverter<T, U>
		where T : struct
	{
		protected EnumConverter()
		{
			this.Codes = GetCodes();
			this.Values = GetEnumValues();
		}

		readonly U[] Codes;
		readonly T[] Values;

		protected abstract U[] GetCodes();
		protected abstract T[] GetEnumValues();

		public T? ToEnumValue(U code) => ToEnumValueCore(code);

		protected virtual T? ToEnumValueCore(U code)
		{
			for (int index = 0; index < Codes.Length; index++)
			{
				if (Codes[index].Equals(code))
				{
					return Values[index];
				}
			}

			return null;
		}

		public U FromEnumValue(T? value) => FromEnumValueCore(value);

		protected virtual U FromEnumValueCore(T? value)
		{
			if (!value.HasValue)
			{
				return GetEmptyValue();
			}

			T typedValue = value.Value;
			for (int index = 0; index < Values.Length; index++)
			{
				if (Values[index].Equals(typedValue))
				{
					return Codes[index];
				}
			}

			throw new InvalidOperationException("Unhandled value [" + value.ToString() + "] sent for conversion");
		}

		protected abstract U GetEmptyValue();
	}

	public abstract class EnumConverter<T> : EnumConverter<T, ZString> where T : struct
	{
		protected override T? ToEnumValueCore(ZString code)
		{
			if (!code.IsEmpty)
			{
				return base.ToEnumValueCore(code);
			}

			return null;
		}

		protected override ZString GetEmptyValue()
		{
			return ZString.Empty;
		}
	}
}
