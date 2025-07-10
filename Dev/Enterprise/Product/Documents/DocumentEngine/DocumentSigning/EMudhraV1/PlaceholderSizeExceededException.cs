using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class PlaceholderSizeExceededException : ZException
	{
		public PlaceholderSizeExceededException(string message, string errorContext = null) : base(message, errorContext)
		{
		}

#if NETFRAMEWORK
		protected PlaceholderSizeExceededException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}
