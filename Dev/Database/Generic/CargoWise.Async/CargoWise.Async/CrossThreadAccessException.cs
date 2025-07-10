using System;

namespace CargoWise.Async
{
	[Serializable]
	public class CrossThreadAccessException : Exception
	{
		public CrossThreadAccessException()
			: base()
		{
		}

		public CrossThreadAccessException(string message)
			: base(message)
		{
		}

		public CrossThreadAccessException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected CrossThreadAccessException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
