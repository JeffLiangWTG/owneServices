using System;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldBoolRepresentationAttribute : MessageFieldRepresentationAttribute
{
	public MessageFieldBoolRepresentationAttribute()
		: base(1, true)
	{
	}

	public override ZString SerializeValue(IZType value) => GetCastedValue<ZBool>(value) ? TrueValue : FalseValue;

	const string TrueValue = "1";
	const string FalseValue = "0";
}
