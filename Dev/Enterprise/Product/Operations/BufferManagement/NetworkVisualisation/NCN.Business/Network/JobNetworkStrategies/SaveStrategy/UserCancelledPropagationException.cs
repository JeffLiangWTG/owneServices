using System;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[Serializable]
	public class UserCancelledPropagationException : Exception
	{
		public UserCancelledPropagationException()
		{
		}

#if NETFRAMEWORK
		protected UserCancelledPropagationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
