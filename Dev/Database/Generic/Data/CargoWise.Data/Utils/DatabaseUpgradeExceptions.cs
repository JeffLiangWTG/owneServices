using System;
using System.Diagnostics;
using CargoWise.Common.ErrorManagement;

namespace CargoWise.Data
{
	// -------------------------- WARNING --------------------------
	// PLEASE DO NOT ADD ANY CALL THAT MIGHT HIT DATABASE EXPLICITLY OR IMPLICITLY IN EXCEPTION TYPES BELOW, e.g. Res.GetString
	// -------------------------- WARNING --------------------------

	[Serializable]
	public class DatabaseUpgradeExceptionCaughtException : Exception, IWithRootCauseStackTrace
	{
		public DatabaseUpgradeExceptionCaughtException(StackTrace originalStackTrace, Exception ex)
			: base("DatabaseUpgradeException has been thrown multiple times.\r\nCheck the call stack for any exception handling that swallows a DatabaseUpgradeException.", ex) // Developer only exception message
		{
			rootCauseStackTrace = originalStackTrace;
		}

#if NETFRAMEWORK
		protected DatabaseUpgradeExceptionCaughtException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public StackTrace RootCauseStackTrace => rootCauseStackTrace;

		[NonSerialized]
		readonly StackTrace rootCauseStackTrace;
	}
}
