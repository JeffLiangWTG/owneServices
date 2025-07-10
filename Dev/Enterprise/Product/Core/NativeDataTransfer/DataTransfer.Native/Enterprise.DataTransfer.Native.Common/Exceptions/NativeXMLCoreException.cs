using System;

namespace Enterprise.DataTransfer.Native.Common.Exceptions
{
	/// <summary>
	/// Subclasses of this will not be shown to the user, and should be used for all programmatic failures within the 
	/// Native XML Codeset.
	/// 
	/// If you WANT a human readable message to be shown to the user, use NativeXMLUserVisibleException instead.
	/// </summary>
	[Serializable]
	public class NativeXMLCoreException : InvalidOperationException
	{
		public NativeXMLCoreException() : base() { }
		public NativeXMLCoreException(string message) : base(message) { }
		protected NativeXMLCoreException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		protected NativeXMLCoreException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
