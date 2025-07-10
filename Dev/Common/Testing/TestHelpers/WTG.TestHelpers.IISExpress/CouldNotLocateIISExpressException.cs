using System;

namespace WTG.TestHelpers.IISExpress
{
	[Serializable]
	public class CouldNotLocateIISExpressException : Exception
	{
		public CouldNotLocateIISExpressException()
		{
		}

		public CouldNotLocateIISExpressException(string message)
			: base(message)
		{
		}

		public CouldNotLocateIISExpressException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected CouldNotLocateIISExpressException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
