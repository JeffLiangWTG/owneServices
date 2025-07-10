using System;
using CargoWise.Common;

namespace Enterprise.PrintProcessing
{
	[ExceptionVisibility(ExceptionVisibility.User), Serializable]
	public class EDocsOffLineException : Exception
	{
		public EDocsOffLineException()
		{
		}

		public EDocsOffLineException(string message)
			: base(message)
		{
		}

		public EDocsOffLineException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected EDocsOffLineException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
