using System;

namespace CargoWise.Data
{
	[Serializable]
	public class UpgradeException : Exception
	{
		public UpgradeException(string message) : base(message)
		{
		}

		public UpgradeException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected UpgradeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
