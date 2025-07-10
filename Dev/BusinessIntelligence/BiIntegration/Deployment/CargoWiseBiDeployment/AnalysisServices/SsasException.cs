using System;

namespace CargoWise.Bi.Deployment.AnalysisServices
{
	[Serializable]
	public class SsasException : Exception
	{
		public SsasException(string message) : base(message)
		{ }

		public SsasException(string message, Exception ex) : base(message, ex)
		{ }

#if NETFRAMEWORK
		protected SsasException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}
