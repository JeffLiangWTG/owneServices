using System;

namespace Enterprise.ZArchitecture.Core
{
	[Serializable]
	public class DatabaseConnectionClosedException : Exception
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception name")]
		public DatabaseConnectionClosedException() : base("Database connection closed")
		{
		}

#if NETFRAMEWORK
		protected DatabaseConnectionClosedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
