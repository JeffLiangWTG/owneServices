using System;

namespace Enterprise.DataTransfer.Native.Common.Exceptions
{
	[Serializable]
	class PropertyNotDefinedException : NativeXMLUserVisibleException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public PropertyNotDefinedException(string propertyName)
			: base(string.Format("Property with name {0} not defined.", propertyName))
		{
		}

#if NETFRAMEWORK
		protected PropertyNotDefinedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
