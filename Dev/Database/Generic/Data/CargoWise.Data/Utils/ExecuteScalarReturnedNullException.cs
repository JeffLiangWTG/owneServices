using System;
using CargoWise.Common;

namespace CargoWise.Data.Utils
{
	[Serializable]
	public class ExecuteScalarReturnedNullException : ArgumentException, ICriticalException
	{
		public ExecuteScalarReturnedNullException()
			: this("Statement returned a null value")
		{
		}

		public ExecuteScalarReturnedNullException(string message, Exception innerException = null)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected ExecuteScalarReturnedNullException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public bool IsCriticalException => true;
	}
}
