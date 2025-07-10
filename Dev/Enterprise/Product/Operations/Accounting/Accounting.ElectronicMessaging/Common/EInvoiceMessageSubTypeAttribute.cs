using System;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = false)]
	public sealed class EInvoiceMessageSubTypeAttribute : Attribute
	{
	}
}
