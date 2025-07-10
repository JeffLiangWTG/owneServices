using System;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class IgnoreSerializationIfNullAttribute : Attribute
{
}
