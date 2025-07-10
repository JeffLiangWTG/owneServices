namespace CargoWise.Bi.Common
{
	using System;

	[Serializable]
	public class BiServiceLockException : Exception
	{
		public BiServiceLockException(string message) : base(message) { }

#if NETFRAMEWORK
		protected BiServiceLockException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
