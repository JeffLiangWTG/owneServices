using System;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop.ClientLink
{
	[Serializable]
	public class UnreachableCodeException : Exception
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception message")]
		public UnreachableCodeException()
			: this("This program location is thought to be unreachable.")
		{
		}
		public UnreachableCodeException(string message)
			: base(message)
		{
		}

		public UnreachableCodeException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected UnreachableCodeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
