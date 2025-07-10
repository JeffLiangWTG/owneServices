using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public abstract class MessageFieldRepresentationAttribute : Attribute
{
	public int Length { get; }
	public bool IsFixedLength { get; }

	public bool IsCapitalOnly { get; set; }

	protected MessageFieldRepresentationAttribute(int length, bool isFixedLength)
	{
		if (length < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than 0");
		}

		Length = length;
		IsFixedLength = isFixedLength;
	}

	public abstract ZString SerializeValue(IZType value);

	protected T GetCastedValue<T>(IZType value)
		where T : struct, IZType
	{
		Argument.NotNull(value, nameof(value));
		if (!(value is T))
		{
			throw new InvalidCastException(FormattableString.Invariant($"The expected type is: {typeof(T).Name}, but was: {value.GetType().Name}"));
		}
		return (T)value;
	}
}
