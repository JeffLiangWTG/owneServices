using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Semaphores.Common
{
	[Serializable]
	public abstract class SemaphoreHandleException : Exception
	{
		internal SemaphoreHandleException(SqlException ex)
			: base(ex.Message, ex)
		{
			Argument.NotNull(ex, nameof(ex));
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected SemaphoreHandleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public static Exception NewException(SqlException ex)
		{
			Argument.NotNull(ex, nameof(ex));

			Exception result = ex;
			DbErrorMatch errorMatch = new DbErrorMatch(ex);

			switch (errorMatch.ExceptionType)
			{
				case DbErrorType.GeneralUserException:
					result = MatchUserDefinedException(ex);
					break;

				case DbErrorType.CannotInsertDuplicateUniqueIndexKey:
					string indexName = errorMatch.GetIndexNameIfUniqueIndexViolation();
					if (indexName == StmServiceSemaphoreSchema.Constants.Indexes.FK_UC__SS_SV_SS_ServiceClass_SS_LockInfo)
					{
						result = new SemaphoreUniqueIndexViolationException(ex);
					}
					break;

				case DbErrorType.DeadlockError:
					result = new SemaphoreTransactionDeadlockException(ex);
					break;
			}

			return result;
		}

		static Exception MatchUserDefinedException(SqlException ex)
		{
			Argument.NotNull(ex, nameof(ex));

			Exception result = ex;

			if (ex.Message.StartsWith(MaxNumberOfHandlesException, StringComparison.OrdinalIgnoreCase))
			{
				result = new SemaphoreReachedMaxAllowedHandlesException(ex);
			}
			else if (ex.Message.StartsWith(CreateSemaphoreWithNoTransactionException, StringComparison.OrdinalIgnoreCase))
			{
				result = new SemaphoreMustBeCreatedInTransactionException(ex);
			}
			else if (ex.Message.StartsWith(CreateSemaphoreWithInvalidSessionId, StringComparison.OrdinalIgnoreCase))
			{
				result = new SemaphoreInvalidSessionIdException(ex);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		internal const string MaxNumberOfHandlesException = "Handle not created, semaphore has reached max quantity of concurrent handles";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		const string CreateSemaphoreWithNoTransactionException = "Inserting Semaphore Handle must run in a transaction context";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		const string CreateSemaphoreWithInvalidSessionId = "Cannot create semaphore handle with an invalid session ID";
	}

	[Serializable]
	public class SemaphoreReachedMaxAllowedHandlesException : SemaphoreHandleException
	{
		public SemaphoreReachedMaxAllowedHandlesException(SqlException ex)
			: base(ex)
		{
			Argument.NotNull(ex, nameof(ex));
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected SemaphoreReachedMaxAllowedHandlesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class SemaphoreUniqueIndexViolationException : SemaphoreHandleException
	{
		public SemaphoreUniqueIndexViolationException(SqlException ex)
			: base(ex)
		{
			Argument.NotNull(ex, nameof(ex));
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected SemaphoreUniqueIndexViolationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class SemaphoreMustBeCreatedInTransactionException : SemaphoreHandleException
	{
		public SemaphoreMustBeCreatedInTransactionException(SqlException ex)
			: base(ex)
		{
			Argument.NotNull(ex, nameof(ex));
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected SemaphoreMustBeCreatedInTransactionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class SemaphoreInvalidSessionIdException : SemaphoreHandleException
	{
		public SemaphoreInvalidSessionIdException(SqlException ex)
			: base(ex)
		{
			Argument.NotNull(ex, nameof(ex));
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected SemaphoreInvalidSessionIdException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class SemaphoreTransactionDeadlockException : SemaphoreHandleException
	{
		public SemaphoreTransactionDeadlockException(SqlException ex)
			: base(ex)
		{
			Argument.NotNull(ex, nameof(ex));
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected SemaphoreTransactionDeadlockException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class HeartbeatIsNotAliveException : Exception
	{
		public HeartbeatIsNotAliveException(Exception ex)
			: base(ex.Message, ex)
		{
			Argument.NotNull(ex, nameof(ex));
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected HeartbeatIsNotAliveException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
