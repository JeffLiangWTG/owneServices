using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldImportRulesAttribute : Attribute
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public string[] Rules { get; }

	public MessageFieldImportRulesAttribute(params string[] rules)
	{
		Rules = rules;
	}
}
