using System;

namespace Enterprise.DataTransfer.Native.Common.Exceptions
{
	[Serializable]
	public class RowNotFoundException : NativeXMLUserVisibleException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public RowNotFoundException(string tableName, Guid pk) : base(string.Format("Cannot find row in table [{0}] using PK [{1}].", tableName, pk.ToString())) { }
#if NETFRAMEWORK
		protected RowNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
