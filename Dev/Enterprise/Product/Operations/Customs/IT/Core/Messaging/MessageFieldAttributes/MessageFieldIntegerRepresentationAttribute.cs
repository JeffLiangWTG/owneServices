using System;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldIntegerRepresentationAttribute : MessageFieldRepresentationAttribute
{
	public MessageFieldIntegerRepresentationAttribute(int length, bool isFixedLength) : base(length, isFixedLength)
	{
	}

	public override ZString SerializeValue(IZType value)
	{
		var result = GetCastedValue<ZInt>(value).ToString(ZString.Empty, CultureInfo.InvariantCulture);
		return IsFixedLength ? result.PadLeft(Length, ZeroChar) : result;
	}

	const char ZeroChar = '0';
}
