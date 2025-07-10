using System;

namespace Enterprise.Customs.IE.Messaging;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class XlsxFieldAttribute : Attribute
{
	public XlsxFieldAttribute(int order, string displayName)
	{
		Order = order;
		DisplayName = displayName;
	}

	public int Order { get; }
	public string DisplayName { get; }
}
