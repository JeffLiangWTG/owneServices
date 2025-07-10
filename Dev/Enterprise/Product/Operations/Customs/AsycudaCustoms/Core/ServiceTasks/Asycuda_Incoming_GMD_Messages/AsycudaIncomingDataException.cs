using System;

namespace Enterprise.Customs.AsycudaCustoms.ServiceTasks
{
	[Serializable]
	public class AsycudaIncomingDataException : Exception
	{
		public AsycudaIncomingDataException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected AsycudaIncomingDataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
