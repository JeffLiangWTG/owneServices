using System;

namespace Enterprise.DataTransfer.Native.Common.Exceptions
{
	[Serializable]
	class ResultNotFoundException : NativeXMLUserVisibleException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public ResultNotFoundException(string tableName)
			: base(string.Format("No record found in table {0}", tableName))
		{
		}

#if NETFRAMEWORK
		protected ResultNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
