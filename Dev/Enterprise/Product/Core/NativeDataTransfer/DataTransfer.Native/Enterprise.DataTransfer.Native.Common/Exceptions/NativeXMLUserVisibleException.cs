using System;
using CargoWise.Common;

namespace Enterprise.DataTransfer.Native
{
	/// <summary>
	/// Used wherever a full human readable explanation of an issue is given.
	/// Exceptions of this type will not be reported back to CargoWise, but 
	/// will be reported to the user using the message text only.
	/// 
	/// DO NOT USE UNLESS THE MESSAGE YOU PASS IN IS GOOD ENOUGH TO SHOW 
	/// TO A USER AND YOU ARE HAPPY FOR THE CALL STACK TO BE THROWN AWAY.
	/// 
	/// PS: If an exception subclasses this type, it must also pass in a user 
	/// friendly message whenever it is thrown.
	/// </summary>
	[Serializable]
	[ExceptionVisibility(ExceptionVisibility.User)]
	public class NativeXMLUserVisibleException : InvalidOperationException
	{
		public NativeXMLUserVisibleException() : base() { }
		public NativeXMLUserVisibleException(string message) : base(message) { }
#if NETFRAMEWORK
		protected NativeXMLUserVisibleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
		public NativeXMLUserVisibleException(string message, Exception innerException) : base(message, innerException) { }
	}
}
