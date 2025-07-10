using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldSignedIntegerRepresentationAttribute : MessageFieldRepresentationAttribute
{
	[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
	public MessageFieldSignedIntegerRepresentationAttribute(int length, bool isFixedLength) : base(length, isFixedLength)
	{
	}

	public override ZString SerializeValue(IZType value)
	{
		var castedValue = GetCastedValue<ZInt>(value);

		var valueIsNegative = castedValue < 0;
		var actualLength = valueIsNegative ? Length - 1 : Length;

		var result = Math.Abs(castedValue).ToString(ZString.Empty, CultureInfo.InvariantCulture);

		result = IsFixedLength ? result.PadLeft(actualLength, ZeroChar) : result;
		return valueIsNegative ? result.Insert(0, NegativeSign) : result;
	}

	const char ZeroChar = '0';
	const string NegativeSign = "-";
}
