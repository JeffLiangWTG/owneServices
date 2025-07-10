using System;

namespace Enterprise.ZArchitecture.GlowInterop
{
	[Serializable]
	public class TokenGenerationException : Exception
	{
		public TokenGenerationException(string message)
			: base(message)
		{
		}

		public TokenGenerationException(string message, Exception ex)
			: base(message, ex)
		{
		}

#if NETFRAMEWORK
		public TokenGenerationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
