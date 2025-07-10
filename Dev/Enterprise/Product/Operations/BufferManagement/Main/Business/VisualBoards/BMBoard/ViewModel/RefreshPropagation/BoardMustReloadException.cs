using System;

namespace Enterprise.BufferManagement.Business
{
	[Serializable]
	public class BoardMustReloadException : Exception
	{
		public BoardMustReloadException()
		{
		}

#if NETFRAMEWORK
		protected BoardMustReloadException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
