using System;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class CouldNotReadNextRunTimeException : RunnerInternalException
	{
		public CouldNotReadNextRunTimeException() : this(ExceptionMessage)
		{
		}

		public CouldNotReadNextRunTimeException(string message) : base(message)
		{
		}

		public CouldNotReadNextRunTimeException(string message, Exception exception) : base(message, exception)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected CouldNotReadNextRunTimeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public CouldNotReadNextRunTimeException(Exception exception) : this(ExceptionMessage, exception)
		{
		}

		const string ExceptionMessage = "Could not read next run time.";
	}
}
