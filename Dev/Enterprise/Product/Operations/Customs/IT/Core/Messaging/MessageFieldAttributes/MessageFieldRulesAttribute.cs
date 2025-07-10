using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldRulesAttribute : Attribute
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public string[] Rules { get; }

	public MessageFieldRulesAttribute(params string[] rules)
	{
		Rules = rules;
	}
}
