using System;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class AllocationCodeFormatException : Exception
	{
		public AllocationCodeFormatException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected AllocationCodeFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
