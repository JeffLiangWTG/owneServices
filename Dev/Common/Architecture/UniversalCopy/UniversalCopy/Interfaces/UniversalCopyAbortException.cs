using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace CargoWise.UniversalCopy
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), Serializable]
	public class UniversalCopyAbortException : Exception
	{
		public UniversalCopyAbortException(string message) : base(message) { }

		public UniversalCopyAbortException(string message, Exception innerException) : base(message, innerException) { }

#if NETFRAMEWORK
		protected UniversalCopyAbortException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
	}
}
