using System;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class BucketSizeWebException : Exception
	{
		public BucketSizeWebException(string message)
			: base(message)
		{
		}

		public BucketSizeWebException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected BucketSizeWebException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
