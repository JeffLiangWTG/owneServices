using System;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	[Serializable]
	public class PowerBiException : Exception
	{
		public PowerBiException(string message) : base(message)
		{ }

		public PowerBiException(string message, Exception ex) : base(message, ex)
		{ }

#if NETFRAMEWORK
		protected PowerBiException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}
