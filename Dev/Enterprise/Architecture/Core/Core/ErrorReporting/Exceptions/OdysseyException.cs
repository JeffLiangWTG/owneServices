using System;

namespace Enterprise.ZArchitecture.Environment
{
	[Serializable]
	public class OdysseyException : ApplicationException
	{
#if NETFRAMEWORK
		protected OdysseyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public OdysseyException(string errorMessage)
			: base(errorMessage)
		{
		}

		public OdysseyException(string errorMessage, Exception innerException)
			: base(errorMessage, innerException)
		{
		}
	}
}
