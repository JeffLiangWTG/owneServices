using System;

namespace CargoWise.Data
{
	[Serializable]
	public class UpgradeManagerException : UpgradeException
	{
		public UpgradeManagerException() : this(UpgradeExceptionMessage)
		{
		}

		public UpgradeManagerException(string message) : base(message)
		{
		}

		public UpgradeManagerException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public UpgradeManagerException(Exception exception) : this(UpgradeExceptionMessage, exception)
		{
		}

#if NETFRAMEWORK
		protected UpgradeManagerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		const string UpgradeExceptionMessage = "Upgrade manager exception.";
	}
}
