using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldInternationalRoadTransportsRulesAttribute : Attribute
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public string[] Rules { get; }

	public MessageFieldInternationalRoadTransportsRulesAttribute(params string[] rules)
	{
		Rules = rules;
	}
}
