using System;

namespace Enterprise.DbUpgrader.Startup
{
	[Serializable]
	public class ServerRequirementsNotMetException : Exception
	{
		public ServerRequirementsNotMetException(string message) : base(message)
		{ }

		public ServerRequirementsNotMetException(string message, Exception ex) : base(message, ex)
		{ }

#if NETFRAMEWORK
		protected ServerRequirementsNotMetException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}
