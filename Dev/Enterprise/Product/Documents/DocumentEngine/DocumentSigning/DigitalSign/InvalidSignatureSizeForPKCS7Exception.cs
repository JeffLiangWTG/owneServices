using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class InvalidSignatureSizeForPKCS7Exception : ZException
	{
		public InvalidSignatureSizeForPKCS7Exception(string message, string errorContext = null) : base(message, errorContext)
		{
		}

#if NETFRAMEWORK
		protected InvalidSignatureSizeForPKCS7Exception(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}
