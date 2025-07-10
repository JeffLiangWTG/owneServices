using System;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class CannotAddToCollectionException : Exception
	{
#if NETFRAMEWORK
		protected CannotAddToCollectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public CannotAddToCollectionException(string message) : base(message)
		{
		}

		public CannotAddToCollectionException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
