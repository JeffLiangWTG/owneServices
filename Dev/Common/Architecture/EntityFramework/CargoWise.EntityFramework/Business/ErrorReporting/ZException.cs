using System;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ZException : Exception
	{
#if NETFRAMEWORK
		protected ZException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public ZException(string message, string errorContext = null) : base(message)
		{
			this.ErrorContext = errorContext;
		}

		public ZException(string message, Exception inner, string errorContext = null) : base(message, inner)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;
	}
}
