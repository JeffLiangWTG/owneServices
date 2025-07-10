using System;

namespace Enterprise.Services.OperationalActions.Business
{
	[Serializable]
	public sealed class RunActionException : Exception
	{
		public RunActionException() { }

		public RunActionException(string message)
			: base(message) { }

		public RunActionException(string message, Exception innerException)
			: base(message, innerException) { }

#if NETFRAMEWORK
		RunActionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
#endif
	}
}
