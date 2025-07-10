using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	internal class ColumnSettingXMLInvalidException : Exception
	{
		internal ColumnSettingXMLInvalidException()
			: base("")
		{
		}

#if NETFRAMEWORK
		protected ColumnSettingXMLInvalidException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
