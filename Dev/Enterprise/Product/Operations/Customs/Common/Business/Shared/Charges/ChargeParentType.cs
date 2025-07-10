using System;

namespace Enterprise.Customs.Common
{
	[Flags]
	public enum ChargeParentTypes
	{
		GroupInvoice = 1,
		Invoice = 2,
		InvoiceLine = 4
	}
}
