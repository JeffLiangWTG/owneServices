using System;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	/// <summary>
	/// Environmental problem from OnlinePreSchemaUpgrader.
	/// </summary>
	[Serializable]
	public class OnlinePreSchemaUpgraderEnvironmentException : DbUpgraderException
	{
		public OnlinePreSchemaUpgraderEnvironmentException(string message) : base(message)
		{
		}

		public OnlinePreSchemaUpgraderEnvironmentException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected OnlinePreSchemaUpgraderEnvironmentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		internal const string DisableStatisticsExceptionMessage = "Problem trying to disable database statistics update.";
	}
}
