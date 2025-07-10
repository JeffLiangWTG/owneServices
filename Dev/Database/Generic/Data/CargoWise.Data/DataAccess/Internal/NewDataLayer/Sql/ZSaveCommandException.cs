using System;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ZSaveCommandException : Exception
	{
		public ZSaveCommandException(Exception innerException, Guid errorPk, bool isConcurrencyError, bool isTriggerException = false)
			: base(innerException.Message, innerException)
		{
			ErrorPk = errorPk;
			IsConcurrencyError = isConcurrencyError;
			IsTriggerException = isTriggerException;
		}

#if NETFRAMEWORK
		protected ZSaveCommandException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public Guid ErrorPk { get; private set; }
		public bool IsConcurrencyError { get; private set; }
		public bool IsTriggerException { get; private set; }
	}
}
