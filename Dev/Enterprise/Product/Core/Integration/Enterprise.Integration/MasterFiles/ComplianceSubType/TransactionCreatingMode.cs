using System;

namespace Enterprise.Integration.Compliance
{
	[Flags]
	public enum TransactionCreatingMode
	{
		Original = 1 << 0,
		Reversal = 1 << 1,
		Amending = 1 << 2,
		All = Original | Reversal | Amending,
	}
}
