using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules
{
	[Serializable]
	public class LimitedRunException : Exception
	{
		public LimitedRunException()
		{
		}

		public LimitedRunException(string message) : base(message)
		{
		}

		public LimitedRunException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected LimitedRunException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
