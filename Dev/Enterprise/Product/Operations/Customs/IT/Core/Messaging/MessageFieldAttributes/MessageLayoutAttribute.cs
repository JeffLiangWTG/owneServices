using System;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageLayoutAttribute : Attribute
{
	int _position;
	public int Order { get; set; }

	public int Position
	{
		get => _position;
		set
		{
			if (value < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(value), "Position must be greater than 0.");
			}
			_position = value;
		}
	}
}
