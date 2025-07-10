using System;

namespace Enterprise.DataTransfer.Native.DB.Exception
{
	[Serializable]
	public class NoColumnFoundException : InvalidOperationException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		public NoColumnFoundException(string tableName, string columnName)
			: this(string.Format("Column with name {0} not found on table {1}.", columnName, tableName))
		{
		}

		public NoColumnFoundException(string msg)
			: base(msg)
		{
		}

#if NETFRAMEWORK
		protected NoColumnFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}