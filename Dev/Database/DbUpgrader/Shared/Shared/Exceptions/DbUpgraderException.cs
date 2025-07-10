using System;

namespace Enterprise.DbUpgrader.Shared
{
	[Serializable]
	public abstract class DbUpgraderException : Exception
	{
		protected DbUpgraderException()
		{
		}

		protected DbUpgraderException(string message) : base(message)
		{
		}

		protected DbUpgraderException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DbUpgraderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
