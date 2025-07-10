using System;

namespace Enterprise.DbUpgrader.Shared
{
	[Serializable]
	public class UpgradeBlockedException : DbUpgraderException
	{
		public UpgradeBlockedException()
		{
		}

		public UpgradeBlockedException(string message) : base(message)
		{
		}

		public UpgradeBlockedException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected UpgradeBlockedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
