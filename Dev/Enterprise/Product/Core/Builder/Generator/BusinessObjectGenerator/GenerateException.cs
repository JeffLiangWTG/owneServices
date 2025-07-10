using System;

namespace Enterprise.Builder.Generator
{
	[Serializable]
	public class GenerateException : Exception
	{
		public GenerateException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected GenerateException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}