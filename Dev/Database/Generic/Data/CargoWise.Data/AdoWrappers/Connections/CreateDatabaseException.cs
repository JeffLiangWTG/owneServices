using System;

namespace CargoWise.Data
{
	[Serializable]
	public class CreateDatabaseException : Exception
	{
		public CreateDatabaseException(string message) : base(message)
		{
		}

		public CreateDatabaseException(string message, Exception ex) : base(message, ex)
		{
		}

#if NETFRAMEWORK
		protected CreateDatabaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
