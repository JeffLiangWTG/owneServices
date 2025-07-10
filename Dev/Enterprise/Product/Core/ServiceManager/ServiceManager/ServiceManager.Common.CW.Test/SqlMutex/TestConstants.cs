using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Semaphores.Common.Test
{
	internal class TestConstants : MarshalByRefObject
	{
		public const string Category = "~UT";
		public const string UserCode = "TST";

		public const string TransactionLockOwner = "Transaction";

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public const int TenSecondsLockTimeOut = 10000;
	}
}
