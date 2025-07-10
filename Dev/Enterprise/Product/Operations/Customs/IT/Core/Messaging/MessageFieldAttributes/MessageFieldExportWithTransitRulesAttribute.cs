using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldExportWithTransitRulesAttribute : Attribute
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public string[] Rules { get; }

	public MessageFieldExportWithTransitRulesAttribute(params string[] rules)
	{
		Rules = rules;
	}
}
