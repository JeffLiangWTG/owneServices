using System;

namespace Enterprise.DataTransfer.Native.Common.Exceptions
{
	[Serializable]
	public class PropertyNotExistException : NativeXMLUserVisibleException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message is safe")]
		public PropertyNotExistException(string propertyName)
			: base("Property:[ " + propertyName + "] is not set")
		{
		}

#if NETFRAMEWORK
		protected PropertyNotExistException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
