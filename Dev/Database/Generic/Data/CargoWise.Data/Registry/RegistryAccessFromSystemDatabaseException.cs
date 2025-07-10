using System;

namespace CargoWise.Data
{
	[Serializable]
	class RegistryAccessFromSystemDatabaseException : InvalidOperationException
	{
		public RegistryAccessFromSystemDatabaseException(string currentDb) : base(FormattableString.Invariant($"{ErrorMsgPrefix} ({currentDb})."))
		{
		}

#if NETFRAMEWORK
		protected RegistryAccessFromSystemDatabaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		const string ErrorMsgPrefix = "Attempt to access Registry whilst connected to a system database";
	}
}
