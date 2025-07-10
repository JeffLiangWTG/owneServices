using System;

namespace CargoWise.Data
{
	[Serializable]
	public class OnlineDataTransformationException : UpgradeException
	{
		public OnlineDataTransformationException() : this(nameof(OnlineDataTransformationException))
		{
		}

		public OnlineDataTransformationException(string message) : base(message)
		{
		}

		public OnlineDataTransformationException(Exception innerException) : this(nameof(OnlineDataTransformationException), innerException)
		{
		}

		public OnlineDataTransformationException(string taskUserDescription, Exception innerException) : base(taskUserDescription, innerException)
		{
		}

#if NETFRAMEWORK
		protected OnlineDataTransformationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
