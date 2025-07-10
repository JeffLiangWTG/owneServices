using System;

namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	[Serializable]
	public class RegenException : Exception
	{
		public RegenException(string message)
			: base(message)
		{
		}

		public RegenException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected RegenException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
