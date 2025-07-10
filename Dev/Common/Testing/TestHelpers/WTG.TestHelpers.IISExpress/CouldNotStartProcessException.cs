using System;

namespace WTG.TestHelpers.IISExpress
{
	[Serializable]
	public class CouldNotStartProcessException : Exception
	{
		public CouldNotStartProcessException()
		{
		}

		public CouldNotStartProcessException(string message)
			: base(message)
		{
		}

		public CouldNotStartProcessException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected CouldNotStartProcessException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
