using System;

namespace Enterprise.Integration.Compliance
{
	[Flags]
	public enum TransactionTypeOfUse
	{
		INV = 1 << 0,
		CRD = 1 << 1,
		ADJ = 1 << 2,
		ALL = INV | CRD | ADJ
	}
}
