using System;

namespace CargoWise.Types
{
	[Serializable]
	public class OperationOnInvalidZTypeException : Exception
	{
		public OperationOnInvalidZTypeException()
		{
		}

		public OperationOnInvalidZTypeException(string message)
			: base(message)
		{
		}

		public OperationOnInvalidZTypeException(string message, Exception ex)
			: base(message, ex)
		{
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected OperationOnInvalidZTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
