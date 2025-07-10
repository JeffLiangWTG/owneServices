using System;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldStringRepresentationAttribute : MessageFieldRepresentationAttribute
{
	public CharType CharType { get; }

	public MessageFieldStringRepresentationAttribute(CharType charType, int length, bool isFixedLength) : base(length, isFixedLength)
	{
		if (!Enum.IsDefined(typeof(CharType), charType))
		{
			throw new ArgumentOutOfRangeException(nameof(charType), "CharType must be a valid value.");
		}
		CharType = charType;
	}

	public override ZString SerializeValue(IZType value)
	{
		var castedValue = GetCastedValue<ZString>(value);
		return IsFixedLength ? castedValue.PadRight(Length) : castedValue;
	}
}
