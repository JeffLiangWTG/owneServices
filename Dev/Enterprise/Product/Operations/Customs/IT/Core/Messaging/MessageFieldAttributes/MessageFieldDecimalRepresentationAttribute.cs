using System;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldDecimalRepresentationAttribute : MessageFieldRepresentationAttribute
{
	public int IntegerPartLength { get; }
	public int DecimalPartLength { get; }
	public bool IsDecimalPartFixedLength { get; }

	public MessageFieldDecimalRepresentationAttribute(int integerPartLength, int decimalPartLength, bool isFixedLength, bool isDecimalPartFixedLength)
		: base(integerPartLength + 1 + decimalPartLength, isFixedLength)
	{
		if (integerPartLength < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(integerPartLength), "Integer part length must be greater than 0");
		}
		IntegerPartLength = integerPartLength;

		if (decimalPartLength < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(decimalPartLength), "Decimal part length must be greater than 0.");
		}
		DecimalPartLength = decimalPartLength;
		IsDecimalPartFixedLength = isDecimalPartFixedLength;
	}
	public MessageFieldDecimalRepresentationAttribute(int integerPartLength, int decimalPartLength, bool isFixedLength)
		: this(integerPartLength, decimalPartLength, isFixedLength, isFixedLength)
	{
	}

	public override ZString SerializeValue(IZType value)
	{
		ZString integerPartFormatStringSegment;
		ZString decimalFormatStringSegment;
		if (IsFixedLength)
		{
			integerPartFormatStringSegment = new string(ZeroChar, IntegerPartLength);
			decimalFormatStringSegment = new string(ZeroChar, DecimalPartLength);
		}
		else if (IsDecimalPartFixedLength)
		{
			integerPartFormatStringSegment = new string(ZeroChar, 1);
			decimalFormatStringSegment = new string(ZeroChar, DecimalPartLength);
		}
		else
		{
			integerPartFormatStringSegment = new string(ZeroChar, 1);
			decimalFormatStringSegment = new string(HashChar, DecimalPartLength);
		}
		return GetCastedValue<ZDecimal>(value).ToString(FormattableString.Invariant($"{integerPartFormatStringSegment}.{decimalFormatStringSegment}"), CultureInfo.InvariantCulture);
	}

	const char ZeroChar = '0';
	const char HashChar = '#';
}
