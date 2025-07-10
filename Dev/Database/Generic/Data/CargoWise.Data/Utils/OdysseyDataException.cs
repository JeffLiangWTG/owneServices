#region SuppressResourceStringsCheckRegion

using System;
using CargoWise.Common;

namespace CargoWise.Data
{
	[Serializable]
	public class OdysseyDataException : ApplicationException
	{
		public OdysseyDataException(string message)
			: base(message)
		{
		}

		public OdysseyDataException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected OdysseyDataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	public enum OdysseyDataErrorType
	{
		NotSpecified = 0,
		TransactionRolledBack,
		NoTransactionToCommit
	}

	[Serializable]
	public class TransactionException : OdysseyDataException
	{
		public OdysseyDataErrorType ErrorType { get; }

		public TransactionException(string message)
			: base(message)
		{
		}

		public TransactionException(string message, OdysseyDataErrorType errorType)
			: base(message)
		{
			ErrorType = errorType;
		}

#if NETFRAMEWORK
		protected TransactionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class LoginException : OdysseyDataException, ICriticalException
	{
		public LoginException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected LoginException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
	}
#endif

		bool ICriticalException.IsCriticalException
		{
			get
			{
				return true;
			}
		}
	}

	[Serializable]
	public class DatabaseMissingException : OdysseyDataException
	{
		public DatabaseMissingException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected DatabaseMissingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	internal class CloseReopenWhileConnectingException : InvalidOperationException
	{
		public CloseReopenWhileConnectingException()
			: base(CloseReopenWhileConnectingError)
		{
		}

#if NETFRAMEWORK
		protected CloseReopenWhileConnectingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		const string CloseReopenWhileConnectingError = "Cannot close and reopen connection in process of opening.";
	}
}

#endregion
