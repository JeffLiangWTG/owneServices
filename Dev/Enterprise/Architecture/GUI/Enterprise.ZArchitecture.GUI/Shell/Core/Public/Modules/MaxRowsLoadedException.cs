using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules
{
	[Serializable]
	public class MaxRowsLoadedException : Exception
	{
		public MaxRowsLoadedException()
		{
		}

		public MaxRowsLoadedException(string message) : base(message)
		{
		}

		public MaxRowsLoadedException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected MaxRowsLoadedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
