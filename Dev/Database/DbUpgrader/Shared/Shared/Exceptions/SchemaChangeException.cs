using System;

namespace Enterprise.DbUpgrader.Shared
{
	[Serializable]
	public class SchemaChangeException : DbUpgraderException
	{
		public SchemaChangeException()
		{
		}

		public SchemaChangeException(string message) : base(message)
		{
		}

		public SchemaChangeException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected SchemaChangeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
