using System;
using CargoWise.Bi.Development.Common;

namespace CargoWise.Bi.Development.SchemaSync
{
	[Serializable]
	public class BiDatabaseSyncException : BiAutomationException
	{
		public BiDatabaseSyncException(string message)
			: base(message)
		{
		}

		public BiDatabaseSyncException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected BiDatabaseSyncException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
