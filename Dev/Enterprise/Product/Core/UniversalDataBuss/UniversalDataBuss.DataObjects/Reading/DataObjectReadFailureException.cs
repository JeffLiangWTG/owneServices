using System;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[Serializable]
	public class DataObjectReadFailureException : Exception
	{
		public DataObjectReadFailureException() : base() { }
		public DataObjectReadFailureException(string message) : base(message) { }
		public DataObjectReadFailureException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		protected DataObjectReadFailureException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}

