using System;

namespace Enterprise.DocumentScanning.Integration
{
	[Serializable]
	public class CanNotCreateSDDatabaseException : Exception
	{
		public CanNotCreateSDDatabaseException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected CanNotCreateSDDatabaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
