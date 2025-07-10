using System;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	[Serializable]
	public class InvalidDocumentWrapperParameterException : ArgumentException
	{
		public InvalidDocumentWrapperParameterException(string msg)
			: base(msg)
		{
		}

		public InvalidDocumentWrapperParameterException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

#if NETFRAMEWORK
		protected InvalidDocumentWrapperParameterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
