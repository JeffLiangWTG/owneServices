using System;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class MaxLengthExceededException : Exception
	{
		public MaxLengthExceededException(int maxLength)
		{
			MaxLength = maxLength;
		}

		public MaxLengthExceededException(string message, int maxLength)
			: base(message)
		{
			MaxLength = maxLength;
		}

#if NETFRAMEWORK
		protected MaxLengthExceededException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		protected MaxLengthExceededException(string message, int maxLength, Exception innerException)
			: base(message, innerException)
		{
			MaxLength = maxLength;
		}

		public int MaxLength { get; }
	}
}
