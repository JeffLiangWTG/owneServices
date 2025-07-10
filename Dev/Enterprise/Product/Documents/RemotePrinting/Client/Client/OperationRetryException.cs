using System;

namespace Enterprise.RemotePrinting.Client
{
	[Serializable]
	public class OperationRetryException : Exception
	{
		public OperationRetryException(string operationName, Exception ex) : base(ex.Message, ex)
		{
			OperationName = operationName;
		}

#if NETFRAMEWORK
		protected OperationRetryException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif

		public string OperationName { get; }
	}
}
