using CargoWise.Types;

namespace Enterprise.Messaging.Business.AWB
{
	#region Value Types

	public enum ValueType
	{
		Value,
		Special,
		LineIdentifier,
		ColumnIdentifier
	}

	public enum CharType
	{
		Text,
		Numeric,
		NumericWithDecimal,
		Alpha,
		AlphaNumeric,
		Special,
		EmailUserNameV16,
		EmailDomainNameV16
	}

	#endregion

	#region Format

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:Override equals and operator equals on value types")]
	public struct Format
	{
		public Format(int maxLength, int minLength, CharType charType)
		{
			this.MaxLength = maxLength;
			this.MinLength = minLength;
			this.CharType = charType;
		}

		public Format(int length, CharType charType)
			: this(length, length, charType)
		{
		}

		public readonly int MaxLength;
		public readonly int MinLength;
		public readonly CharType CharType;

		public override bool Equals(object obj)
		{
			var result = false;
			if (obj is Format format)
			{
				result = format.MaxLength == MaxLength && format.MinLength == MinLength && format.CharType == CharType;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return MaxLength.GetHashCode() ^ MinLength.GetHashCode() ^ CharType.GetHashCode();
		}

		public static bool operator ==(Format obj1, Format obj2)
		{
			return obj1.Equals(obj2);
		}

		public static bool operator !=(Format obj1, Format obj2)
		{
			return !obj1.Equals(obj2);
		}
	}

	#endregion

	public class ValueElement : Element
	{
		public ValueElement(StatusType status, Format format, ZString value, ValueType valueType)
			: base(status)
		{
			this.ValueType = valueType;
			this.Format = format;
			this.Value = value;
		}

		public ValueElement(Format format, ZString value)
			: this(StatusType.Mandatory, format, value, ValueType.Value)
		{
		}

		public ValueElement(Format format, ZString value, ValueType valueType)
			: this(StatusType.Mandatory, format, value, valueType)
		{
		}

		public override string ToString()
		{
			return FormatValue(Value);
		}

		public static class CharTypes
		{
			public const string Numeric = "0123456789";
			public const string NumericWithDecimal = Numeric + ".";
			public const string Alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			public const string AlphaNumeric = Alpha + Numeric;
			public const string Text = Alpha + Numeric + ". -";
			const string SpecialCharacters = "!\"#$%&'()*+,:;<=>?[\\]^_`{|}~";
			public const string EmailUserNameV16 = Text + SpecialCharacters;
			public const string EmailDomainNameV16 = AlphaNumeric + ".-:[]";
		}

		ZString FormatValue(ZString value)
		{
			var result = value.ToUpper();

			switch (Format.CharType)
			{
				case CharType.Alpha:
					result = result.KeepChars(CharTypes.Alpha);
					break;
				case CharType.Numeric:
					result = result.KeepChars(CharTypes.Numeric);
					break;
				case CharType.AlphaNumeric:
					result = result.KeepChars(CharTypes.AlphaNumeric);
					break;
				case CharType.NumericWithDecimal:
					result = result.KeepChars(CharTypes.NumericWithDecimal);
					break;
				case CharType.Text:
					result = result.KeepChars(CharTypes.Text, " ");
					break;
				case CharType.EmailUserNameV16:
					result = result.KeepChars(CharTypes.EmailUserNameV16);
					break;
				case CharType.EmailDomainNameV16:
					result = result.KeepChars(CharTypes.EmailDomainNameV16);
					break;
			}
			if (Format.CharType != CharType.Special)
			{
				result = result.Left(Format.MaxLength);
			}
			else
			{
				result = value;
			}

			return result;
		}

		public string ToStringValueTypes()
		{
			string result = "";
			if (IsValueType)
			{
				if (Format.CharType == CharType.Numeric || Format.CharType == CharType.NumericWithDecimal)
				{
					if (Value.Length != 0 && Value != "0")
					{
						result = ToString();
					}
				}
				else
				{
					result = ToString();
				}
			}
			return result;
		}

		public bool IsValueType
		{
			get { return ValueType == ValueType.Value; }
		}

		public readonly ValueType ValueType;
		public ZString Value;
		readonly Format Format;
	}
}
