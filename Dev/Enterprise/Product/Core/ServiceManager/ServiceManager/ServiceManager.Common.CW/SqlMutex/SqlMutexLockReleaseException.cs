using System;

namespace ServiceManager.Common.CW
{
	[Serializable]
	public class SqlMutexLockReleaseException : Exception
	{
		public SqlMutexLockReleaseException() : this(ExceptionMessage)
		{
		}

		public SqlMutexLockReleaseException(string message) : base(message)
		{
		}

		public SqlMutexLockReleaseException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected SqlMutexLockReleaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		internal SqlMutexLockReleaseException(int errorNumber) : this($"{ExceptionMessage} Error number: [{errorNumber}].") // exception message
		{
			ErrorNumber = errorNumber;
		}

		internal SqlMutexLockReleaseException(Exception exception) : this(ExceptionMessage, exception)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		const string ExceptionMessage = "Error during lock release.";
		public int? ErrorNumber { get; }
	}
}
